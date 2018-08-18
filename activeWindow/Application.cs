﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace activeWindow
{
    class Application
    {
        [STAThread]
        public static void Main4()
        {
            testlink tl = new testlink(null);
            //testlink.saveToFile(tl.getTestsuite(), "");
           // ts.Items.Add();
            //StreamReader str = new StreamReader(@"F:\vs\all_testsuites_u8.xml");
            //XmlSerializer xSerializer = new XmlSerializer(typeof(testsuite));
            
            //ts = (testsuite)xSerializer.Deserialize(str);
           /* foreach (GetBusinessCardCallResponseBusinessCards result in bcResponse.Items.ToList<GetBusinessCardCallResponseBusinessCards>()) 
            {
                var businessCards = result.BusinessCard.ToArray<GetBusinessCardCallResponseBusinessCardsBusinessCard>(); 
                foreach (GetBusinessCardCallResponseBusinessCardsBusinessCard bc in businessCards) { 
                    Console.Write(bc.firstName); 
                    Console.Write(" "); 
                    Console.WriteLine(bc.lastName); 
                } 
                Console.WriteLine(); 
            }*/ 
            
            //str.Close(); 
            Console.ReadLine(); 
        }
        
        public static string readFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            string xmlString = "";
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();                
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
            return xmlString;
        }

        [STAThread]
        public static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new TCAssistant());
            //new TCAssistant().button7_Click(null,null);
            Console.Write("llll");
        }
        [STAThread]
        /*static void Main2(string[] args)
        {
            Word.ApplicationClass WordApp = new Word.ApplicationClass();

            object fileName = "G:\\NFE-Series 操作手册 V1.4.doc";
            object readOnly = false;
            object isVisible = true;
            
            object missing = System.Reflection.Missing.Value;
            
            Word.Document aDoc = WordApp.Documents.Open(ref fileName,
                                    ref missing, ref readOnly, ref missing,
                                    ref missing, ref missing, ref missing,
                                    ref missing, ref missing, ref missing,
                                     ref missing, ref isVisible);
            Word.WdStatistic stat = Word.WdStatistic.wdStatisticPages;
            int num = aDoc.ComputeStatistics(stat, ref missing);            
            System.Console.WriteLine("The number of pages in doc is {0}",num);

            try
            {
                foreach (Word.Section s in aDoc.Sections)
                {
                    if (s.Index == 2)
                        continue;
                    
                    foreach (Word.Revision r in s.Range.Revisions)
                    {
                        object row = r.Range.get_Information(Word.WdInformation.wdFirstCharacterLineNumber);
                        object col = r.Range.get_Information(Word.WdInformation.wdFirstCharacterColumnNumber);
                        object page = r.Range.get_Information(Word.WdInformation.wdActiveEndPageNumber);
                        System.Console.WriteLine("in " + page + " " + row + " " + col+" ");
                        Console.WriteLine("\t" + printType(r.Type)+"/"+r.Range.Text); 
                       // r.Reject();
                    }
                }

                foreach (Word.Comment wordComment in aDoc.Comments)
                {
                    System.Console.WriteLine("Comment: "+wordComment.Range.Text);
                }
            }
            catch
            {
            }
            finally
            {
                aDoc.Close(ref missing, ref missing, ref missing);
            } 
            
            System.Console.ReadLine();
        }*/

        public static string printType(Word.WdRevisionType type)
        {
            switch (type) {
                case Word.WdRevisionType.wdNoRevision:
                    return "No revision";
                case Word.WdRevisionType.wdRevisionConflict:
                    return "Revision marked as a conflict";
                case Word.WdRevisionType.wdRevisionDelete:
                    return "Deletion";
                case Word.WdRevisionType.wdRevisionDisplayField:
                    return "Field display changed";
                case Word.WdRevisionType.wdRevisionInsert:
                    return "Insertion";
                case Word.WdRevisionType.wdRevisionParagraphNumber:
                    return "Paragraph number changed";
                case Word.WdRevisionType.wdRevisionParagraphProperty:
                    return "Paragraph property changed";
                case Word.WdRevisionType.wdRevisionProperty:
                    return "Property changed";
                case Word.WdRevisionType.wdRevisionReconcile:
                    return "Revision marked as reconciled conflict";
                case Word.WdRevisionType.wdRevisionReplace:
                    return "Replaced";
                case Word.WdRevisionType.wdRevisionSectionProperty:
                    return "Section property changed";
                case Word.WdRevisionType.wdRevisionStyle:
                    return "Style changed";
                case Word.WdRevisionType.wdRevisionStyleDefinition:
                    return "Style definition changed";
                case Word.WdRevisionType.wdRevisionTableProperty:
                    return "Table property changed";
                //case WdRevisionType.wdRevisionCellDeletion:
                //    return "Table cell deleted";
                //case WdRevisionType.wdRevisionCellInsertion:
                //    return "Table cell inserted";
                //case WdRevisionType.wdRevisionCellMerge:
                //    return "Table cells merged";
                //case WdRevisionType.wdRevisionMovedFrom:
                //    return "Content moved from";
                //case WdRevisionType.wdRevisionMovedTo:
                //    return "Content moved to";
                default:
                    return "others "+type;                                
            }
        
        }
    }
}
