using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Excel=Microsoft.Office.Interop.Excel;

namespace activeWindow
{
    public interface ITestcase
    {
        Boolean CanAuto();
        Boolean InitialTestcase(Excel.Worksheet sheet,int row);
        Boolean insertCases(Excel.Worksheet sheet, int row, int deep);
        String GetScriptName();
        String ToScript();
        String Optimize();
        void setLogger(ILog log);

    }
}
