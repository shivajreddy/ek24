using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI.Selection;
using ek24.UI.ViewModels.Manage;
using System.IO;
using ek24.Utils;


namespace ek24.Commands;


public class ExportToExcel
{

    public static void ExportQuantitiesToExcel(UIApplication app)
    {
        Document doc = app.ActiveUIDocument.Document;

        string current_project_path = doc.PathName;

        // Collect all casework and millwork family instances that have a type parameter of 'Vendor_Name'
        var collector = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilyInstance))
            .OfCategory(BuiltInCategory.OST_Casework);


        var data = new List<(string Brand, string FamilyName, string TypeName, string Notes, int count)>();

        // Dictionary to keep track of the unique entries and their counts
        var groupedData = new Dictionary<(string Brand, string FamilyName, string TypeName, string Notes), int>();

        foreach (FamilyInstance instance in collector)
        {
            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;
            string typeName = instance.Symbol.Name;

            // Check if the 'Vendor_Name' parameter exists and is of type string
            Parameter vendorNameParam = instance.Symbol.LookupParameter("Vendor_Name");
            if (vendorNameParam != null && vendorNameParam.StorageType == StorageType.String)
            {
                string vendorNameValue = vendorNameParam.AsValueString();

                // Check if the 'Vendor_Notes' parameter exists
                Parameter notesParam = instance.Symbol.LookupParameter("Vendor_Notes");
                string notesValue = notesParam != null ? notesParam.AsString() : "";

                // Create a key for grouping
                var key = (vendorNameValue, familyName, typeName, notesValue);

                // Increment the count if the key already exists, otherwise add it with a count of 1
                if (groupedData.ContainsKey(key))
                {
                    groupedData[key]++;
                }
                else
                {
                    groupedData[key] = 1;
                }
            }
        }

        // Convert the dictionary to the final list with the count included
        data = groupedData.Select(entry => (entry.Key.Brand, entry.Key.FamilyName, entry.Key.TypeName, entry.Key.Notes, entry.Value)).ToList();

        // Now 'data' contains the unique combinations of brand, family name, type name, and notes with their respective counts


        // Export to Excel
        //var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //var filePath = Path.Combine(desktopPath, "LindenIII_Purchase_List.xlsx");

        // Export to Excel
        // Change the file extension to .xlsx
        string filePath = Path.ChangeExtension(current_project_path, ".xlsx");

        var excelExporter = new ExcelExporter(filePath);
        //excelExporter.ExportToExcel(data);
        excelExporter.ExportCabinetDataToExcel(data);

        TaskDialog.Show("Successfully Exported", $"File Exported to: {filePath}");
    }


}
