using System;
using System.Collections.Generic;
using System.Text;

namespace activeWindow
{
    interface ITestcase
    {
        Boolean CanAuto();
        Boolean IsTestcase();             
        String GetScriptName();
        String ToString();
    }
}
