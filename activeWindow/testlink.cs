using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;

namespace activeWindow
{
    class testlink
    {   
        testsuite[] tree = new testsuite[5];
        uint externalid = 0;
        uint[] node_order = new uint[5];
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        string apikey;
        string uri;
        ILog log;
        public testlink(ILog log)
        {
            this.log = log;
            tree[0] = new testsuite();
            apikey = ConfigurationManager.AppSettings["tl_apikey"];
            uri = ConfigurationManager.AppSettings["tl_xmlrpc_url"];
        //  testsuite template = testsuite.LoadFromFile(Environment.CurrentDirectory + "\\template.xml");
        }

        public testsuite addItems(int deep, string id, string summary)
        {
            testsuite ts = new testsuite();
            ts.name = id;
            ts.details = summary;
            ts.node_order = ++node_order[deep];
            tree[deep-1].Items.Add(ts);
            tree[deep] = ts;

            for (int i = deep+1; i < 5; i++) {
                node_order[i] = 0;
            }
            return ts;
        }

        
        public string saveToFile(string path)
        {
            if (path == null || path.Length == 0)
                path = Environment.CurrentDirectory;
            testsuite ts = tree[0];
            List<object> items = ts.Items;
            
            for (int i = 0; i < items.Count; i++)
            {
                ts.Items = new List<object>();
                string name=((testsuite)items[i]).name;
                ts.Items.Add(items[i]);
                name=name.Replace("/","").Replace("\\", "");
                name = path + "/" + i + "_" + name + "_" + this.tc_cnt((testsuite)items[i]) + ".xml";
                log.AppendLine(name);
                ts.SaveToFile(name);
            }
            return path;
        }
        public int tc_cnt(testsuite ts)
        {
            List<object> items=ts.Items;
            int cnt=0;
            for (int i = 0; i < items.Count; i++) {
                if (items[i] is testsuite)
                    cnt += tc_cnt((testsuite)items[i]);
                else 
                {
                    cnt++;                
                }
            }
            return cnt;
        }
        public int addTestcases(Excel.Worksheet sheet,DefaultTC itc, int row)
        {
            itc.InitialTestcase(sheet, row);            
           // testsuite ts=addItems(4,itc.GetCell(DefaultTC.colName.FEATUREID),itc.GetCell(DefaultTC.colName.FEATURE_DESC));
            
            string detail = itc.GetCell(DefaultTC.colName.FEATUREID)+":"+itc.GetCell(DefaultTC.colName.CASE_DESC);

            testsuite ts = addItems(4, itc.GetCell(DefaultTC.colName.FEATURE_DESC), detail);
            
            string preset = itc.GetCell(DefaultTC.colName.PRESET);
            List<string> steps = itc.getSteps();
            List<string> exp = itc.getExpRsts();
            string req = itc.GetCell(DefaultTC.colName.DS_ID);
            bool can_auto = itc.GetCell(DefaultTC.colName.CAN_AUTO).ToLower().Equals("true");
            string importance = itc.GetCell(DefaultTC.colName.PRIORITY).ToLower();
            string testtype=itc.GetCell(DefaultTC.colName.TYPE);

            uint order = 0;
            while (true)
            {
                row++;                
                string c0 = itc.GetSubCase(row, DefaultTC.subColName.ID0);
                string c1 = itc.GetSubCase(row,DefaultTC.subColName.ID);

                if (c0.Length != 0||c1.Length==0)
                {
                    row--;                    
                    return row;
                }

                string c2 = itc.GetSubCase(row,DefaultTC.subColName.SAMPLES);
                testcase tc = new testcase();
                tc.name = c1;                
                tc.externalid = externalid++;
                tc.internalid = externalid+"";
                tc.node_order = order++;
                tc.version = 1;
                tc.summary = c2;
                tc.preconditions = preset;
                tc.execution_type = (uint)(can_auto ? 2 : 1);
                if (importance.Length>5)
                    tc.importance = Convert.ToUInt32(importance.Substring(5,1));
                else tc.importance = 1;
                custom_field test_type=new custom_field();
                test_type.name="test_type";
                test_type.value=testtype;
                tc.custom_fields.Add(test_type);

                //todo 需求没有处理
                //string reqs= itc.GetSubCase(row,DefaultTC.subColName.REQ);                
                //requirement rq=new requirement();

                //tc.requirements.Add

                for (int i = 0; i < steps.Count; i++) {
                    step s = new step();
                    s.actions = steps[i];
                    s.expectedresults=exp[i];
                    s.step_number=(uint)(i+1);
                    s.execution_type = tc.execution_type;
                    tc.steps.Add(s);                
                }

                ts.Items.Add(tc);                
            }
        }        
    }
}
