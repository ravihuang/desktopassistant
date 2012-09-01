using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Configuration;
using Excel;
using System.Collections;
namespace activeWindow
{
    public partial class TCAssistant : Form,ILog
    {
        
        Excel.Application app;
        bool showHelp = false;
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        
        public TCAssistant()
        {
         //   File.Delete("temp.txt");
            InitializeComponent();
            textBox1.Text = ConfigurationManager.AppSettings["save_path"];
            showHelp = ConfigurationManager.AppSettings["show_help"] == "true";
            this.cbDeep.SelectedIndex = 1;
            this.cbOs.SelectedIndex = 0;
            initTestcaseBox();

            foreach (string tc in PluginBroker.List(typeof(ITestcase)))
            {
                this.cbScriptType.Items.Add(PluginBroker.Load(tc, typeof(ITestcase)));
            }
            this.cbScriptType.SelectedIndex = 0;
        }
        
        public void saveConfig(String key,String value) {
            config.AppSettings.Settings.Remove("key");
            config.AppSettings.Settings.Add("key", value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private Boolean createDir(String path) {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Exists)
                return true;
            try
            {
                if (DialogResult.OK == MessageBox.Show(
                                  "所选文件夹不存在，是否创建?",
                                  "注意！",
                                  MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Asterisk))
                {
                    this.AppendLine("生成文件夹"+path);
                    dir.Create();
                    return true;
                }                    
                
            }
            catch (IOException ex)
            {
                MessageBox.Show("所选子目录非法：" + path);                      
            }
            return false;
           
        }
        private bool initTestcaseBox() {
            if (app == null)
            {
                try
                {
                    app = Marshal.GetActiveObject("Excel.Application") as Excel.Application;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("请打开用例文档先！");
                    return false;
                }

            }

            if (app.ActiveWorkbook == null)
            {
               return false;
            }

            tbTestcase.Text = app.ActiveWorkbook.Name;
            return true;
        
        }
        private void bGen_Click(object sender, EventArgs e)
        {
            bGen.Enabled = false;
            
            if (!initTestcaseBox() || !createDir(textBox1.Text))
            {                
                bGen.Enabled = true;
                return;
            }

            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;            

            Excel.Range selection = app.Application.Selection as Excel.Range;

            DefaultTC itc = (DefaultTC)this.cbScriptType.SelectedItem;
            int cnt = 0;
            for (int i = 0; i < selection.Count; i++)
            {
                int row=selection.Row + i;
                if (!itc.InitialTestcase(sheet, row))
                {
                    this.AppendLine("Line" + row + "不是用例");
                    continue;
                }
                if (!itc.CanAuto() && DialogResult.OK != MessageBox.Show(
                                  "Line" + row + "用例不能自动化，是否继续?",
                                  "注意！",
                                  MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Asterisk))
                {
                    this.AppendLine("Line" + row + "不能自动化！");
                    continue;                
                }
                
                String fname = System.IO.Path.Combine(textBox1.Text, itc.GetScriptName());
                FileInfo fi = new FileInfo(fname);                

                if (fi.Exists)
                {    
                     DialogResult rst=MessageBox.Show(
                                      "文件" + fi.FullName + "已经存在，是否覆盖?",
                                      "注意！",
                                      MessageBoxButtons.YesNoCancel,
                                      MessageBoxIcon.Asterisk);
                     if (DialogResult.Yes == rst)
                     {
                        fi.Delete();
                     }
                     else if(DialogResult.No == rst)
                     {
                        continue;
                     }
                     else if(DialogResult.Cancel == rst)
                     {
                         bGen.Enabled = true;
                         return;
                     }                
                }
                UTF8Encoding utf8 = new UTF8Encoding(false,false);
                itc.walkSubItemTestCase(sheet, selection.Row);
                StreamWriter w = new StreamWriter(fi.Create(), Encoding.GetEncoding("GBK"));
                w.WriteLine(itc.ToScript());
                w.Flush();
                w.Close();
                this.AppendLine("生成脚本："+itc.GetScriptName());
                cnt++;
            }
            saveConfig("save_path", textBox1.Text);
            if(cnt==0)
                MessageBox.Show("请选择用例先！");
            bGen.Enabled = true;
        }

        private void bOpt_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = @textBox3.Text;
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            textBox3.Text = folderBrowserDialog1.SelectedPath;            
        }

        #region ILog 成员

        public void AppendLine(string s)
        {
            rtbLog.AppendText(s);
            rtbLog.AppendText("\n");
        }

        public void Append(string s)
        {
            rtbLog.AppendText(s);
        }

        public void Clear()
        {
            rtbLog.Text = "";
        }

        #endregion

        private void TCAssistant_FormClosed(object sender, FormClosedEventArgs e)
        {
            config.Save(ConfigurationSaveMode.Modified); 
        }

        private void tbTestcase_MouseEnter(object sender, EventArgs e)
        {            
            if (initTestcaseBox()&&showHelp)
                toolTip1.Show(tbTestcase.Text, (IWin32Window)sender);
        }

        private void bSelect_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = @textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            @textBox1.Text = folderBrowserDialog1.SelectedPath;

        }
        private void component_mouse_leave(object sender, EventArgs e)
        {
            toolTip1.RemoveAll();
        }
        private bool isloaded = false;
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            //this.Location = new System.Drawing.Point((SystemInformation.PrimaryMonitorSize.Width - 550) / 2,
            //        (SystemInformation.PrimaryMonitorSize.Height - 400) / 2);
            //this.MinimumSize = SystemInformation.PrimaryMonitorSize;
            //this.StartPosition = System.Windows.Forms.FormStartPosition;
            //this.MaximumSize = new System.Drawing.Size(550, 400);

            if (((TabControl)sender).SelectedTab.Text == "脚本助手")
                this.TopMost = true;
            else
                this.TopMost = false;
            if (((TabControl)sender).SelectedTab.Text == "STAF" && !isloaded)
            {                
                isloaded = true;
                if (Directory.Exists("C:\\STAF"))
                {
                    Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";C:\\STAF\\bin");
                }

                loadItemsToListBox("cfg\\HostList.txt", lstHostList);
                txtCommand.Text = readText("cfg\\Command.txt");
                txtCheckCondition.Text = readText("cfg\\CheckCondition.txt");
            }
        }        

        private void bGenTestcase_Click(object sender, EventArgs e)
        {
            bGenTestcase.Enabled = false;

            if (!initTestcaseBox())
            {
                bGenTestcase.Enabled = true;
                return;
            }

            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
            
            Excel.Range selection = app.Application.Selection as Excel.Range;

            DefaultTC itc = (DefaultTC)this.cbScriptType.SelectedItem;

            if (selection.Count != 1||!itc.InitialTestcase(sheet, selection.Row)) {
                this.AppendLine("请选择1行用例，并且测试步骤字段不能为空");
                bGenTestcase.Enabled = true;
                return;
            }            
            itc.insertCases(sheet, selection.Row, int.Parse(cbDeep.Items[cbDeep.SelectedIndex].ToString()));

            bGenTestcase.Enabled = true;
               
        }
        private int getGroupId(Excel.Worksheet sheet,int row) {
            if (row < 2) {
                return -1;
            }
            object rng0 = ((Range)sheet.Cells[row + 1, 1]).Value2;
           
            string s = "";
            if (rng0 == null)
            {
                object rng1 = ((Range)sheet.Cells[row + 1, 2]).Value2;
                if (rng1 == null)
                    return -1;
                s = rng1.ToString();
            }
            else {
                s = rng0.ToString();
            }
            if(s.Length==0)
                return -1;
            return s.Split(new string[] { "_", "-" }, StringSplitOptions.None).Length;
        }

        public void rangeToGrout(Excel.Worksheet sheet,int from,int to) {
            Range range = sheet.get_Range(sheet.Cells[from+2, 1], sheet.Cells[to+1, 1]);
            this.AppendLine("group from " + (from + 2) + " to " + (to + 1)); 
            range.Rows.Group(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            
        }
        private void bGroup_Click(object sender, EventArgs e)
        {           

            if (!initTestcaseBox())            
            {                
                return;
            }
 
            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
             int rows = sheet.UsedRange.Rows.Count;
             int[] id = new int[rows];
             int max_deep = 0;
             int row_begin = -1;   

             for (int i = 0; i < rows; i++)
             {
                 id[i] = getGroupId(sheet, i);
                 if (id[i] > max_deep)
                 {
                     max_deep = id[i];
                 }
                 if (id[i] > -1 && row_begin == -1)
                     row_begin = i;
                 if (id[i] > 6)
                     this.AppendLine("Warn:层级太多，row:" + i);
             }

             int used = -1;         
             for (int i = rows - 1; i > 0; i--)
            {
                if (id[i] > 0&& used < 0)
                {
                    used=i+1;                             
                    break;
                }                
            }
             Range range = sheet.get_Range(sheet.Cells[1, 1], sheet.Cells[used, 1]);
             range.Rows.Select();
             try
             {
                 range.Rows.Ungroup();
             }catch(Exception ex){}

            int begin=-1;
            Boolean start = false;
            int curpos = 0;
           // int deep = max_deep-3;
            for (int deep = max_deep; deep >1; deep--)
            {
                for (int j = 0; j < used; j++)
                {
                    if (id[j] == -1)
                    {
                        continue;
                    }

                    if (id[j] == deep - 1)
                    {
                        if (!start)
                        {
                            start = true;
                        }
                        else
                        {
                            //this.Append("a:");                        
                            rangeToGrout(sheet, begin, curpos);
                        }
                        begin = j;
                        curpos = j;
                        continue;
                    }
                    if (id[j] < deep && start)
                    {

                        if (begin != curpos)
                        {
                            //this.Append("b:");
                            rangeToGrout(sheet, begin, curpos);                            
                        }
                        start = false;
                        begin = -1;
                    }
                    if (j == used - 1 && start)
                    {
                        //this.Append("c:");
                        rangeToGrout(sheet, begin, used-1);
                        begin = -1;
                        start = false;
                    }
                    curpos = j;
                }
                
            }             
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ITestcase itc = (ITestcase)this.cbScriptType.SelectedItem;
            itc.setLogger(this);
        }

        private void bClrGrp_Click(object sender, EventArgs e)
        {      

            if (!initTestcaseBox())
            {               
                return;
            }
            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
            try
            {
                
                int rows = sheet.UsedRange.Rows.Count;
                Range range = sheet.get_Range(sheet.Cells[1, 1], sheet.Cells[rows, 1]);
                range.Rows.Select();
                int i =10;
                while(i-->0)
                    range.Rows.Ungroup();
                
            }catch(Exception ex){
               

            }
        }

        private void bDelSubTC_Click(object sender, EventArgs e)
        {
            if (!initTestcaseBox())
            {
                return;
            }
            DialogResult rst = MessageBox.Show(
                                      "删除后不可恢复，是否继续？",
                                      "注意！",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Asterisk);
            if (DialogResult.No == rst)
            {
                return;
            }

            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
            try
            {
                int rows = sheet.UsedRange.Rows.Count;
                for (int i = rows; i > 1; i--) {
                    Range tmprange = sheet.Range["A" +i, Type.Missing].EntireRow;
                    Array values = (Array)tmprange.Cells.Value2;

                    Object c1 = values.GetValue(1, 1);
                    Object c2 = values.GetValue(1, 2);
                    if (c1 == null && c2 == null)
                        continue;
                    if (c1 == null)
                        tmprange.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                }                
            }
            catch (Exception ex)
            {
                this.AppendLine(ex.StackTrace);
            }
        }

        private void bCheckStyle_Click(object sender, EventArgs e)
        {
            if (!initTestcaseBox())
            {
                return;
            }
            
            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
            try
            {
                int rows = sheet.UsedRange.Rows.Count;
                int curr_flag = 0;
                bool isok = true;
                for (int i = 3; i <rows; i++)
                {
                    Range tmprange = sheet.Range["A" + i, Type.Missing].EntireRow;
                    Array values = (Array)tmprange.Cells.Value2;
                    Object c1 = values.GetValue(1, 1);
                    Object c2 = values.GetValue(1, 2);
                    if (c1 == null)
                    {
                        if (c2 == null)
                        {
                            this.AppendLine("Warning:空白行,row " + i);
                            isok = false;
                            continue;
                        }
                        else
                        {
                            curr_flag = c2.ToString().Split(new string[] { "_" }, StringSplitOptions.None).Length;
                        }
                    }
                    if (c1 == null)
                        continue;

                    int flag = c1.ToString().Split(new string[] { "_" }, StringSplitOptions.None).Length;

                    if (flag > 5) {
                        this.AppendLine("Warning:层级大过5,row " + i);
                        isok = false;
                        
                        continue;
                    }

                    if (curr_flag <= 0)
                    {
                        curr_flag = flag;
                        continue;
                    }
                    
                    if ((flag - curr_flag) > 1)
                    {
                        this.AppendLine("Warning:缺少中间模块,row "+ i);
                        isok = false;
                    }

                    if (flag == 5) 
                    {
                        if (curr_flag == 5) {
                            this.AppendLine("Warning:没有生产用例 row " + (i-1));
                            isok = false; 
                        }
                        DefaultTC itc = (DefaultTC)this.cbScriptType.SelectedItem;
                        if (!itc.InitialTestcase(sheet, i))
                        {                            
                            isok = false;                            
                        }else if(itc.getSteps().Count!=itc.getExpRsts().Count)
                        {
                            this.AppendLine("Warning:步骤和预期结果长度不一致 row " + i);
                            isok = false;                            
                        }                                                
                    }
                    curr_flag = flag;
                }
                if (isok)
                    this.AppendLine("End!");
            }
            catch (Exception ex)
            {
                this.AppendLine(ex.StackTrace);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!initTestcaseBox())
            {
                return;
            }

            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;

            Excel.Range selection = app.Application.Selection as Excel.Range;
            DefaultTC itc = (DefaultTC)this.cbScriptType.SelectedItem;

            if (selection.Count != 1 || !itc.InitialTestcase(sheet, selection.Row))
            {
                this.AppendLine("请选择1行用例，并且测试步骤字段不能为空");
                return;
            }

            int row = selection.Row;
            List<string> tmp = itc.getSteps();
            string s="";
            for (int i = 0; i < tmp.Count; i++)
                s += (i+1)+","+tmp[i] + "\n";

            sheet.Cells[row, (int)DefaultTC.colName.STEP] = s.Trim();

            tmp = itc.getExpRsts();
            s = "";
            for (int i = 0; i < tmp.Count; i++)
                s += (i + 1) + "," + tmp[i] + "\n";

            sheet.Cells[row, (int)DefaultTC.colName.EXP_RESULT] = s.Trim();

        }

        public void bToTL_Click(object sender, EventArgs e)
        {
            if (!initTestcaseBox())
            {
                return;
            }

            Excel.Worksheet sheet = app.ActiveWorkbook.ActiveSheet as Excel.Worksheet;
            int rows = sheet.UsedRange.Rows.Count;
            testlink tl = new testlink();
            
            for (int i = 3; i < rows; i++)
            {
                Range tmprange = sheet.Range["A" + i, Type.Missing].EntireRow;
                Array values = (Array)tmprange.Cells.Value2;
                object o = values.GetValue(1, 1);
                if (o == null)
                {
                    this.AppendLine("stop on row "+i);
                    break;
                }
                string c1 = o.ToString();                
                int deep = c1.Split(new string[] { "_" }, StringSplitOptions.None).Length-1;                
                if (deep == 4)
                {
                    DefaultTC itc = (DefaultTC)this.cbScriptType.SelectedItem;
                    i=tl.addTestcases(sheet, itc, i);
                }
                else
                {
                    tl.addItems(deep, values.GetValue(1, 2).ToString(), c1);
                }                
            }

            //this.AppendLine("Save to "+tl.saveToFile(null));
            Console.WriteLine("Save to " + tl.saveToFile(null));
        }

        private void bSplit_Click(object sender, EventArgs e)
        {

        }
    }
}