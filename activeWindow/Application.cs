using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace activeWindow
{
    class Application
    {
        [STAThread]
        public static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new TCAssistant());
            Console.Write("llll");
        }
    }
}
