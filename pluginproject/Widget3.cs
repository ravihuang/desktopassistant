using System;
using System.Runtime.InteropServices;
namespace Widgets.FirstSet
{
    public class Widget3 : activeWindow.ITestcase
	{

        #region ITestcase ≥…‘±
        public bool CanAuto()
        {
            return true;
        }
        public Boolean insertPairwiseCase(Excel.Worksheet sheet, int row) {
            return false;
        }
        public bool InitialTestcase(Excel.Worksheet sheet, int row)
        {
            return true;
        }
        public string ToScript()
        {
            return "Widget1 script";
        }
        public string GetScriptName()
        {

            return "test.tcl";
        }
        public  string Optimize()
        {
            return "todo Widget1 Optimize";
        }
        public override string ToString()
        {
            return ".tcl";
        }
        public void setLogger(activeWindow.ILog log)
        {
            
        }
        #endregion
    }
}
