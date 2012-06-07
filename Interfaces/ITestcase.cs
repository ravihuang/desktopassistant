using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace activeWindow
{
    public interface ITestcase
    {
        Boolean CanAuto();
        Boolean InitialTestcase(Excel.Worksheet sheet,int row);        
        Boolean insertPairwiseCase(Excel.Worksheet sheet, int row);
        String GetScriptName();
        String ToScript();
        String Optimize();
        void setLogger(ILog log);

    }
}
