using System;
using System.Collections.Generic;
using System.Text;
  
namespace activeWindow
{    
    public interface ILog
    {
        void AppendLine(String s);
        void Append(String s);  
        void Clear();
    }
    class STDLog : ILog
    {
        #region ILog 成员

        public void AppendLine(string s)
        {
            Console.WriteLine(s);
        }

        public void Append(string s)
        {
            Console.Write(s);
        }

        public void Clear()
        {

        }

        #endregion
    }
}
