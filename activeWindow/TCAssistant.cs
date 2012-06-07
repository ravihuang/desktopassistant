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
namespace activeWindow
{
    public partial class TCAssistant : Form,ILog
    {
        
        Excel.Application app;
        bool showHelp = false;
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        bool treeInitiated = false;

        public TCAssistant()
        {
         //   File.Delete("temp.txt");
            InitializeComponent();
            textBox1.Text = ConfigurationManager.AppSettings["save_path"];
            showHelp = ConfigurationManager.AppSettings["show_help"] == "true";

            initTestcaseBox();

            foreach (string tc in PluginBroker.List(typeof(ITestcase)))
            {
                this.comboBox1.Items.Add(PluginBroker.Load(tc, typeof(ITestcase)));
            }
            this.comboBox1.SelectedIndex = 0;
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
                MessageBox.Show("请打开用例文档先！");
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
            
            ITestcase itc = (ITestcase)this.comboBox1.SelectedItem;
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

        private void bSave_Click(object sender, EventArgs e)
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

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (((TabControl)sender).SelectedTab.Text == "用例助手")
            {
                this.MinimumSize = SystemInformation.PrimaryMonitorSize;
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(0, 0);
                this.TopMost = false;

            }
            else
            {
                this.TopMost = true;
                this.MaximumSize = new System.Drawing.Size(550, 400);
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.Location = new System.Drawing.Point((SystemInformation.PrimaryMonitorSize.Width-550) / 2, 
                    (SystemInformation.PrimaryMonitorSize.Height-400) / 2);
            }
            
        }

        void initTree() {
            treeInitiated = true;
        
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

            ITestcase itc = (ITestcase)this.comboBox1.SelectedItem;

            if (selection.Count != 1||!itc.InitialTestcase(sheet, selection.Row)) {
                this.AppendLine("请选择1行用例，并且测试步骤字段不能为空");
                bGenTestcase.Enabled = true;
                return;
            }
            itc.insertPairwiseCase(sheet, selection.Row);

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
            //this.AppendLine("group from " + (from + 2) + " to " + (to + 1)); 
            range.Rows.Group(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            
        }
        private void bTestcaseGroup_Click(object sender, EventArgs e)
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
            ITestcase itc = (ITestcase)this.comboBox1.SelectedItem;
            itc.setLogger(this);
        }

        private void button3_Click(object sender, EventArgs e)
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

    }    

}