using System;
using System.Runtime.InteropServices;
using activeWindow;
using System.Collections;
using System.Text;
namespace Widgets.SecondSet
{
    public class Widget2 : DefaultTC
    {
        private string product = "";
        private string import = "";
        private string parent = "TestFlows";
        private string footer = @"
    @BeforeClass
    public void BeforeClass(){
        super.BeforeClass()
    }

    @BeforeMethod
    public void BeforeMethod(Method m){
        super.BeforeMethod(m);
    }

    @AfterMethod
    public void AfterMethod(Method m){
        super.AfterMethod(m);
    }

    @AfterClass
    public void AfterClass(){
        super.AfterClass();	 
    }
";

        #region ITestcase 成员
        [STAThread]
        public static void Main()
        {
            Console.Write(new Widget2().ToScript());
        }
        public string genStepCode(string step,string expect,int i) {
            return "\n        log.info \"${STEP}." + i + " " + step + ",预期结果为:" + expect + "\"\n        \n        \n";
        }

        /*
         def SAMPLES=[
				["d1",
				     [用户组:"新",
					  用户:"有"
				]],
			    ["d2",
					[用户组:"新",
				     用户:"有"
				]]
			]*/	 
        private string genSamples() {
            string s = @"def SAMPLES=[
";
            string format = 
@"         [" + "\"{0}\""+@",
              [{1}"+"\""+@"
         ]],
";
            for (int i = 0; i < this.testcases.Count;i++ ) {
                string ss = this.testcase_samples[i];
                ss = ss.Replace(":", ":\"").Replace("\n", "\",\n               ");

                s += String.Format(format, testcases[i], ss);                
            }
            s = s.TrimEnd();
            s=s.Substring(0, s.Length - 1);

            return s+"\n     ]";
        }
        public override string ToScript()
        {
            //处理测试用例中的"步骤"字段
            string step = cells[(int)colName.STEP];
            string[] steps = step.Split(new char[1]{'\n'});            
            ArrayList arrStep = new ArrayList();
            for (int i = 0; i < steps.Length; i++) {
                string s = steps[i].Trim();
                if (s.Length == 0)
                    continue;
                int p = s.IndexOfAny(new char[3] { ',','，',' ' },0,4);
                if (p == -1) {
                    logger.AppendLine("Warning:step格式非法" + cells[(int)colName.FEATUREID] + "：" + s);
                    p = 0;
                }
                arrStep.Add(s.Substring(p+1).Trim());                
            }
            arrStep.TrimToSize();

            //处理测试用例中的"预期结果"字段
            string result = cells[(int)colName.EXP_RESULT];
            string[] results = result.Split(new char[1] { '\n' });
            ArrayList arrResult = new ArrayList();            
            for (int i = 0; i < results.Length; i++)
            {
                string s = results[i].Trim();
                if (s.Length == 0)
                    continue;
                int p = s.IndexOfAny(new char[3] { ',', '，', ' ' }, 0, 4);
                if (p == -1)
                {
                    logger.AppendLine("Warning:预期结果格式非法" + cells[(int)colName.FEATUREID] + "：" + s);
                    p = 0;
                }
                arrResult.Add(s.Substring(p+1).Trim());
               
            }
            arrResult.TrimToSize();
            //确认步骤和结果长度匹配
            if (arrResult.Count != arrStep.Count)
            {
                logger.AppendLine("Warning:结果和步骤长度不一致:" + cells[(int)colName.FEATUREID]);
                return null;
            }

            //生成代码段
            string desc = "        if(samples.get(\"\").contains(\"\")){\n\n        }\n\n"; ;
            for (int i = 0; i < arrResult.Count; i++)
            {
                desc += "\n        //@STEP " + arrStep[i] +
                        "\n        //@EXPECT " + arrResult[i] + genStepCode(arrStep[i].ToString(), arrResult[i].ToString(), i+1);
            }
            
            

            string template = @"/** 
*@summary " + cells[(int)colName.FEATURE_DESC] + @"
*@description " + cells[(int)colName.CASE_DESC] + @"
*
*<p>
*@pre-set " + cells[(int)colName.PRESET] + @"
*
*<p>
*@author " + cells[(int)colName.CASE_OWNER] + @"
*@since  " + DateTime.Now + @"
*@product " + cells[(int)colName.PRODUCT] + @"
*@revision 
*    日期                    修改人                       修改说明
*   
*/
import org.testng.*
import org.testng.annotations.*
import static org.testng.AssertJUnit.*;
import java.lang.reflect.*;
import com.ravi.util.*
import com.ravi.testcase.*
" + import+@"
public class " + cells[(int)colName.FEATUREID] + @" extends "+parent+"{\n    " + genSamples() + @"

    @Test(dataProvider=" + "\"Data_TestStep\",groups=["+"\"FUNC\"])"+@"
    public void TestStep(name,samples){
        log.info " + "\"${STEP}. " + cells[(int)colName.FEATURE_DESC] +
                 ":\"\n        log.info \"Testcase:$name, $samples\"\n" + desc + @"
    }

    @DataProvider(name=" + "\"Data_TestStep\")" + @"
    public Object[][] Data_TestStep(){
        return SAMPLES;
    }
"+footer+"}";
          //  byte[] buffer1 = Encoding.Default.GetBytes(template);
         //   byte[] buffer2 = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, buffer1, 0, buffer1.Length);
          // return Encoding.Default.GetString(buffer2, 0, buffer2.Length);

            return template;
        }
        public override string GetScriptName()
        {
            string def = base.GetScriptName();
            
            if(def=="")
                return cells[(int)colName.FEATUREID] + "." + ToString();
            return def;
        }
        public override string Optimize()
        {
            return "todo Widget2 Optimize";
        }
        public override string ToString()
        {
            return "groovy";
        }
        #endregion
    }
}
