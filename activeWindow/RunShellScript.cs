using System;
using System.Collections.Generic;
using System.Text;

namespace activeWindow
{
    class RunShellScript
    {
        void run() {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            string strCmdLine="dir";
            //strCmdLine = "/C regenresx " + textBox1.Text + " " + textBox2.Text;
            System.Diagnostics.Process.Start("CMD.exe", strCmdLine);

        
        }
    }
}
