﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Configuration;
using Excel;

namespace activeWindow
{
    public abstract class DefaultTC : ITestcase
    {
        #region ITestcase 成员
        protected string[] cells = new string[(int)colName.TC_MAX];
        protected List<string> testcases;
        protected List<string> testcase_samples; 

        protected Excel.Worksheet sheet;
        protected ILog logger;
        
        protected enum colName { 
            FEATUREID=1,//索引	
            FEATURE_DESC,//描述	
            CASE_DESC,//设计描述	
            PRESET,//预置条件	
            CASE_VAR,//Pairwise样本点	
            STEP,//步骤	
            EXP_RESULT,//预期结果	
            EXE_RESULT,//执行结果	
            EXE_OWNER,//测试人
            TYPE,//测试类型	
            PRIORITY,//用例优先级	
            CATEGORY,//测试层级	
            CASE_OWNER,//作者	
            CAN_AUTO,//能否自动化	
            REASON,//原因	
            SCRIPT_NAME,//脚本名称	
            SCRIPT_OWNER,//作者	
            DES_LEVEL,//设计层级	
            RELATED_FEATURES,//相关特性	
            PRODUCT,//适用产品	
            DS_ID,//需求编号	
            RELATED_FUNC,//相关函数	
            RELATED_MODULE,//相关模块	
            BUG_ID,//问题单ID	
            DS_OWNER,//作者
            TC_MAX
        };
        public bool CanAuto()
        {
            return cells[(int)colName.CAN_AUTO].ToLower() == "true";
        }
        
        protected string GetCellValue(Worksheet sheet,string cell){
            object value=sheet.get_Range(cell, Type.Missing).Value2;
            if (value == null)
                return "";
            return value.ToString(); 
        }

        public void walkSubItemTestCase(Worksheet sheet, int row)
        {
            testcases = new List<string>();
            testcase_samples = new List<string>(); 
            string cellA = GetCellValue(sheet,"A" + row); 

            if (cellA == null||cellA.Trim().Length==0)
            {
                logger.AppendLine("所选行A列不能为空！"+row);
                return;
            }


            while (true) {
                row++;
                string cellB = GetCellValue(sheet, "B" + row);
                if (cellB == null || cellB.Trim().Length == 0)
                {
                   return;
                }
                if (cellB.Contains(cellA))
                {
                    this.testcases.Add(cellB);
                    string cellE = GetCellValue(sheet, "E" + row);
                    if (cellE == null || cellE.Trim().Length == 0)
                    {
                        cellE = "";
                        logger.AppendLine("Warn:没有可用的样本点，Row" + row);
                    }
                    this.testcase_samples.Add(cellE);
                }
                else
                    break;
            }
            if(testcases.Count==0)
                logger.AppendLine("Warn:没有生成可用的数据，Row" + row);
        }

        public bool InitialTestcase(Worksheet sheet, int row)
        {
            if (row <= 2) {
                return false;
            }
            Range range = sheet.get_Range("A" + row, "Z" + row);
            Array values = (Array)range.Cells.Value2;

            for (int i = 1; i < cells.Length; i++)
            {
                if (values.GetValue(1, i) == null)
                    cells[i]="";
                else
                    cells[i]=(string)values.GetValue(1, i).ToString().Trim();
            }
            if (cells[(int)colName.STEP] != "" && cells[(int)colName.EXP_RESULT] != "")
            {                
                walkSubItemTestCase(sheet, row);
                return true;
            }
            return false;
        }
        public Boolean insertPairwiseCase(Worksheet sheet, int row)
        {
            Range range = sheet.get_Range("A" + row, "Z" + row);
 
            Array values = (System.Array)range.Cells.Value2;
            string paras = values.GetValue(1, (int)colName.CASE_VAR).ToString();
            string id = values.GetValue(1, (int)colName.FEATUREID).ToString();

            string[] arr = paras.Trim().Split(new string[] { "\n" }, StringSplitOptions.None);
            //p2=[eva,tang]
            List<activeWindow.PairVaraible> pvlist = new List<PairVaraible>();
            for (int i = 0; i < arr.Length; i++)
            {
                PairVaraible pv = new PairVaraible();
                string[] t = arr[i].Trim().Split(new string[] { "=",",","，" }, StringSplitOptions.None);
                
                pv.values = new ArrayList();
                if (t.Length < 2) {
                    return false;
                }
                pv.name = t[0].Trim();
                t[1]=t[1].Trim();
                if (t.Length ==2)
                {
                    pv.values.Add(t[1].Substring(1, t[1].Length - 2));
                    pvlist.Add(pv);
                    continue; 
                }
                else 
                {
                    pv.values.Add(t[1].Substring(1));
                }
                for (int j = 2; j < t.Length - 1;j++ )
                    pv.values.Add(t[j]);
                
                pv.values.Add(t[t.Length-1].Trim().Substring(0,t[t.Length-1].Length-1));
                pvlist.Add(pv);                
            }

            ArrayList ll = Pairwise.go(pvlist, Math.Min(2,pvlist.Count));
            ll.TrimToSize();
            Range tmprange = sheet.get_Range("A" + (row + 1), "Z" + (row + 1));
            for (int i = 0; i < ll.Count;i++ )
                tmprange.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Type.Missing);

            for (int i = 0; i < ll.Count; i++)
            {
                String tcDesc = "";
                for (int j = 0; j < pvlist.Count; j++)
                {
                    tcDesc += (pvlist[j].name + ":" + ((ArrayList)ll[i])[j] + "\n");
                }
                sheet.Cells[row + i + 1, (int)colName.CASE_VAR] = tcDesc.Trim();
                sheet.Cells[row + i + 1, (int)colName.FEATURE_DESC] = id+"_"+i;
            }

            sheet.get_Range("A" + row, "A" + (row + ll.Count)).RowHeight =21;
            
             
            logger.Append("");

            return true;
        }
        public void setLogger(ILog log)
        {
            this.logger = log;
        }

        public abstract string ToScript();
        public virtual string GetScriptName()
        {

            return cells[(int)colName.SCRIPT_NAME];
        }

        public abstract string Optimize();
        #endregion

        
    }
}
