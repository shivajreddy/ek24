using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
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

    public void ExportCabinetDataToExcel(string worksheetName, List<CabinetDataModel> cabinetDataModels)
    {
        // Here Application is Microsoft.Office.Interop.Excel.Application
        /* Old Way
        Application excelApp = new Application();
        // */

        // /* New Way
        Type excelType = Type.GetTypeFromProgID("Excel.Application");
        dynamic excelApp = Activator.CreateInstance(excelType);
        // */



        if (excelApp == null)
        {
            TaskDialog.Show("Failed to Export", "Excel application not found");
            return;
        }

        // Create a new Workbook
        try
        {
            // Create worksheet
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            Worksheet worksheet = workbook.Sheets[1];
            worksheet = (Worksheet)workbook.ActiveSheet;
            worksheet.Name = SanitizeWorkSheetName(worksheetName);

            // Create Headers
            // Look at CabinetsExportDataModel() and both places should have all same headers
            // Part 1:
            List<string> allHeaders = new List<string>
            {
                "Design Option",
                "Brand",
                "Shape",
                "Eagle-SKEW",
                "Brand-SKEW",
                "Notes",
                "Style",
                "Species",
                "Finish",
                "Count"
            };
            foreach (var header in allHeaders)
            {
                worksheet.Cells[1, allHeaders.IndexOf(header) + 1] = header;
            }


            // Write Data
            foreach (var cabinetDataModel in cabinetDataModels)
            {
                // Part 2:
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 1] = cabinetDataModel.DesignOption;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 2] = cabinetDataModel.Brand;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 3] = cabinetDataModel.Shape;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 4] = cabinetDataModel.EagleSkew;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 5] = cabinetDataModel.BrandSkew;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 6] = cabinetDataModel.Notes;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 7] = cabinetDataModel.Style;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 8] = cabinetDataModel.Species;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 9] = cabinetDataModel.Finish;
                worksheet.Cells[cabinetDataModels.IndexOf(cabinetDataModel) + 3, 10] = cabinetDataModel.Count;
            }
            //EagleKitchenViewModel.AppendLog("Data written to 'Cabinets' workbook");

            // Save the Workbook
            // NOTE: fails if the path has special characters
            workbook.SaveAs(FilePath, XlFileFormat.xlWorkbookDefault);

            //EagleKitchenViewModel.AppendLog("'Cabinets' workbook Saved");

            workbook.Close();
            excelApp.Quit();

            // Release the COM objects
            ReleaseObject(worksheet);
            ReleaseObject(workbook);
            ReleaseObject(excelApp);
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Failed to export", ex.Message);
            return;
        }

    }
    private static string SanitizeWorkSheetName(string workSheetName)
    {
        // Limit the name to 31 characters
        if (workSheetName.Length > 31)
        {
            workSheetName = workSheetName.Substring(0, 31);
        }

        // Remove invalid characters: : \ / ? * [ or ]
        string invalidChars = new string(new char[] { ':', '\\', '/', '?', '*', '[', ']' });
        foreach (char c in invalidChars)
        {
            workSheetName = workSheetName.Replace(c.ToString(), "_");
        }

        // Replace any remaining invalid characters with an underscore (optional)
        // You could replace them with another character if you prefer
        workSheetName = System.Text.RegularExpressions.Regex.Replace(workSheetName, @"[\\/:*?[\]]", "");

        // Ensure the name is not empty
        if (string.IsNullOrWhiteSpace(workSheetName))
        {
            workSheetName = "Sheet1"; // Default name
        }

        return workSheetName;
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
