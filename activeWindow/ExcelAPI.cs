using System;
using System.Collections.Generic;
using System.Text;

namespace activeWindow
{
    class ExcelAPI
    {
        public void insert() {
            Excel.Worksheet ws = (Excel.Worksheet)ThisApplication.Sheets[1];
            Excel.Range rng = (Excel.Range)ws.Cells[2, 2];

            Excel.Range row = rng.EntireRow;

            row.Insert(Excel.XlInsertShiftDirection.xlShiftDown, false);

        
        }
    }
}
