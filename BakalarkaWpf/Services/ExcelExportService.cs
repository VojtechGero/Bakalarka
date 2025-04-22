using BakalarkaWpf.Models;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.IO;

namespace BakalarkaWpf.Services;

public class ExcelExportService
{
    public void ExportToExcel(List<FileResults> results, string filePath)
    {
        using (ExcelEngine excelEngine = new ExcelEngine())
        {
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Xlsx;

            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            worksheet.Range["A1"].Text = "Dotaz";
            worksheet.Range["B1"].Text = "Cesta k s souboru";
            worksheet.Range["C1"].Text = "Počet výskytů";

            worksheet.Range["A1:C1"].CellStyle.Font.Bold = true;
            worksheet.Range["A1:C1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;

            int row = 2;
            foreach (var result in results)
            {
                worksheet.Range[$"A{row}"].Text = result.Query;
                worksheet.Range[$"B{row}"].Text = result.FilePath;
                worksheet.Range[$"C{row}"].Number = result.OccurrenceCount;
                row++;
            }

            worksheet.AutofitColumn(1);
            worksheet.AutofitColumn(2);
            worksheet.AutofitColumn(3);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                workbook.SaveAs(stream);
            }
        }
    }
}