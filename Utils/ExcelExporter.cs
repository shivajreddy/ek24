using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;


namespace ek24.Utils;


public class ExcelExporter
{

    private string FilePath { get; set; }

    public ExcelExporter(string filePath)
    {
        FilePath = filePath;
        //EagleKitchenViewModel.AppendLog("created an instance of ExcelExporter with filePath: " + filePath);
    }

    public void ExportCabinetDataToExcel(List<(string Brand, string FamilyName, string TypeName, string Notes, int Count)> data)
    {
        // Here Application is Microsoft.Office.Interop.Excel.Application
        Application excelApp = new Application();

        if (excelApp == null)
        {
            TaskDialog.Show("Failed to Export", "Excel application not found");
            return;
        }

        // Create a new Workbook
        Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

        Worksheet worksheet = workbook.Sheets[1];

        worksheet = (Worksheet)workbook.ActiveSheet;

        worksheet.Name = "E24-TakeOff";

        worksheet.Cells[1, 1] = "Brand";
        worksheet.Cells[1, 2] = "Shape";
        worksheet.Cells[1, 3] = "Brand-SKEW";
        worksheet.Cells[1, 4] = "Notes";
        worksheet.Cells[1, 5] = "Count";

        for (int i = 0; i < data.Count; i++)
        {
            worksheet.Cells[i + 3, 1] = data[i].Brand;
            worksheet.Cells[i + 3, 2] = data[i].FamilyName;
            worksheet.Cells[i + 3, 3] = data[i].TypeName;
            worksheet.Cells[i + 3, 4] = data[i].Notes;
            worksheet.Cells[i + 3, 5] = data[i].Count;
        }
        //EagleKitchenViewModel.AppendLog("Data written to 'Cabinets' workbook");

        // Save the Workbook
        workbook.SaveAs(FilePath, XlFileFormat.xlWorkbookDefault);

        //EagleKitchenViewModel.AppendLog("'Cabinets' workbook Saved");

        workbook.Close();
        excelApp.Quit();

        // Release the COM objects
        ReleaseObject(worksheet);
        ReleaseObject(workbook);
        ReleaseObject(excelApp);


    }

    public void ExportToExcel(List<(string FamilyName, string TypeName)> data)
    {
        //EagleKitchenViewModel.AppendLog("Starting 'ExportToExcel' method");

        // Here Application is Microsoft.Office.Interop.Excel.Application
        Application excelApp = new Application();

        if (excelApp == null)
        {
            //EagleKitchen.EagleKitchen.AppendLog($"{nameof(ExportToExcel)}");
            //EagleKitchenViewModel.AppendLog("Failed to create excel app");
            throw new InvalidOperationException("Excel is not properly installed.");
        }

        //EagleKitchenViewModel.AppendLog("'excelApp' created");

        // Create a new Workbook
        Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
        //EagleKitchenViewModel.AppendLog("workbook added Type.Missing");
        Worksheet worksheet = workbook.Sheets[1];
        //EagleKitchenViewModel.AppendLog("workbook sheets[1]");
        worksheet = (Worksheet)workbook.ActiveSheet;
        //EagleKitchenViewModel.AppendLog("worksheet is set to activesheet");
        worksheet.Name = "Cabinets";
        //EagleKitchenViewModel.AppendLog("worksheet name is now 'Cabinets'");

        worksheet.Cells[1, 1] = "Family Name";
        //EagleKitchenViewModel.AppendLog("'Cabinets' worksheet cells[1,1] is Family Name");
        worksheet.Cells[1, 2] = "Type Name";
        //EagleKitchenViewModel.AppendLog("'Cabinets' worksheet cells[1,2] is Type Name");

        for (int i = 0; i < data.Count; i++)
        {
            worksheet.Cells[i + 2, 1] = data[i].FamilyName;
            worksheet.Cells[i + 2, 2] = data[i].TypeName;
        }
        //EagleKitchenViewModel.AppendLog("Data written to 'Cabinets' workbook");

        // Save the Workbook
        workbook.SaveAs(FilePath, XlFileFormat.xlWorkbookDefault);

        //EagleKitchenViewModel.AppendLog("'Cabinets' workbook Saved");

        workbook.Close();
        excelApp.Quit();

        // Release the COM objects
        ReleaseObject(worksheet);
        ReleaseObject(workbook);
        ReleaseObject(excelApp);

    }


    private void ReleaseObject(object obj)
    {
        try
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            obj = null;
        }
        catch (Exception ex)
        {
            obj = null;
            throw new InvalidOperationException("Unable to release the object " + ex.ToString());
        }
        finally
        {
            GC.Collect();
        }
    }


}
