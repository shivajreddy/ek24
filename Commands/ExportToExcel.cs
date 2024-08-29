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

using ek24.UI.Models.Revit;
using ek24.Utils;
using System.Windows.Forms;


namespace ek24.Commands;


public class ExportToExcel
{

    public static void ExportQuantitiesToExcel(UIApplication app)
    {
        Document doc = app.ActiveUIDocument.Document;

        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);
        FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);

        // Filter all the cabinet instances
        FilteredElementCollector caseWorkCollector = caseworkCollector.OfCategory(BuiltInCategory.OST_Casework);

        // Filter for FamilyInstance elements
        ICollection<Element> cabinetFamilyInstances = caseWorkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        string[] cabinetFamilyNamePrefixes = {
            "Aristokraft-W-",
            "Aristokraft-B-",
            "Aristokraft-T-",
            "Eclipse-W-",
            "Eclipse-B-",
            "Eclipse-T-",
            "Eclipse",
            "YTC-W-",
            "YTC-B-",
            "YTC-T-",
            "YTH-W-",
            "YTH-B-",
            "YTH-T-"
        };

        // Filter for cabinet instances with the prefixes
        List<FamilyInstance> ekCabinetInstances = new List<FamilyInstance>();

        foreach (Element element in cabinetFamilyInstances)
        {
            FamilyInstance instance = element as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            // Check if the family name starts with any of the prefixes
            if (cabinetFamilyNamePrefixes.Any(familyName.StartsWith))
            {
                // Add the matching instance to the list
                ekCabinetInstances.Add(instance);
            }
        }


        // Extract type & instance params for each of the instances, and convert them to data model
        var cabinetDataModels = CabinetsExportDataModel.ConvertInstancesToDataModels(ekCabinetInstances);

        // Get all design options in the project
        var designOptions = designOptionsCollector.OfCategory(BuiltInCategory.OST_DesignOptions).ToElements();

        // TODO: User will pick a design option, here a small window should pop up for user to select
        var chosenDesignOptionName = designOptions.FirstOrDefault().Name;

        // Filter for cabinet-data-models with chosen design option
        // Any element in the "Main Model" is included in every design option
        List<CabinetDataModel> filteredCabinetDataModels = cabinetDataModels
            .Where(model => model.DesignOption == chosenDesignOptionName || model.DesignOption == "Main Model")
            .ToList();

        // Open File Dialog in the project's directory
        var currentProjectPathWithExtension = doc.PathName;
        var currentProjectPath = Path.GetFileNameWithoutExtension(currentProjectPathWithExtension);


        // Use FolderBrowserDialog to choose the folder
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.InitialDirectory = currentProjectPath;
            saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save Excel File";
            saveFileDialog.DefaultExt = "xlsx";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                // Export to Excel
                var excelExporter = new ExcelExporter(filePath);

                excelExporter.ExportCabinetDataToExcel(chosenDesignOptionName, filteredCabinetDataModels);

                TaskDialog.Show("Successfully Exported", $"File Exported to: {filePath}");
            }
            else
            {
                TaskDialog.Show("Export Canceled", "No file name was chosen. Export process has been canceled.");
            }
        }


    }


}
