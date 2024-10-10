using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
using ek24.UI.Views.Manage;
using ek24.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ek24.Commands.Utils;
class FilterAllCabinets
{
    public static void ExportQuantitiesToExcel(UIApplication app)
    {
        Document doc = app.ActiveUIDocument.Document;

        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);
        FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);

        // Filter all the cabinet instances
        FilteredElementCollector caseWorkCollector = caseworkCollector.OfCategory(BuiltInCategory.OST_Casework);

        // Filter for FamilyInstance elements of 'Casework' category
        ICollection<Element> caseworkFamilyInstances = caseWorkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        string[] ekCabinetFamilyNamePrefixes = {
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

        string[] ekCrownMoldingFamilyPrefixes = [
            //"Aristokraft-ShM",
            //"Eclipse-ShM",
            "YTC-ShM",
            //"YTH-ShM",

            "Aristokraft-BM",
            //"Eclipse-BM",
            "YTC-BM",
            "YTH-BM",

            "Aristokraft-CM",
            //"Eclipse-CM",
            "YTC-CM",
            "YTH-CM",

            //"Aristokraft-SM",
            //"Eclipse-SM",
            "YTC-SM",
            //"YTH-SM",

        ];

        // Exact matches
        string[] ekSidePanelFamilyPrefixes = [
            "EK_EndPanel_Flat",
            "EK_DecorativePanel_ONE_DOOR"
        ];

        // Exact matches
        string[] ekFillerStripFamilyPrefixes = [
            "Corner_FillerStrip_Generic",
            "FillerStrip_Generic",
        ];

        // Filter for cabinet instances with the prefixes
        List<FamilyInstance> ekCabinetInstances = new List<FamilyInstance>();

        List<FamilyInstance> ekCrownMoldingInstances = new List<FamilyInstance>();
        List<FamilyInstance> ekSidePanelInstances = new List<FamilyInstance>();
        List<FamilyInstance> ekFillerStripInstances = new List<FamilyInstance>();

        foreach (Element element in caseworkFamilyInstances)
        {
            FamilyInstance instance = element as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            // Check if the family name starts with any of the prefixes
            if (ekCabinetFamilyNamePrefixes.Any(familyName.StartsWith))
            {
                // Add the matching instance to the list
                ekCabinetInstances.Add(instance);
            }

            else if (ekCrownMoldingFamilyPrefixes.Any(familyName.StartsWith))
            {
                ekCrownMoldingInstances.Add(instance);
            }
            else if (ekSidePanelFamilyPrefixes.Contains(familyName))
            {
                ekSidePanelInstances.Add(instance);
            }
            else if (ekSidePanelFamilyPrefixes.Contains(familyName))
            {
                ekFillerStripInstances.Add(instance);
            }
        }

        // Extract type & instance params for each of the instances, and convert them to data model
        var cabinetDataModels = CabinetsExportDataModel.ConvertEKCabinetInstancesToEKCabinetDataModels(ekCabinetInstances);

        var crownMoldingDataModels = CabinetsExportDataModel.ConvertEKCrownMoldingInstancesToEKCrownMoldingDataModels(ekCrownMoldingInstances);
        var sidePanelDataModels = CabinetsExportDataModel.ConvertEKSidePanelInstancesToEKCrownMoldingDataModels(ekSidePanelInstances);
        var fillerStripDataModels = CabinetsExportDataModel.ConvertEKFillerStripInstancesToEKCrownMoldingDataModels(ekFillerStripInstances);


        // Part 1: Show the design option picker dialog

        // Declare the variable before the dialog is shown
        string chosenDesignOptionName = string.Empty;

        // Get all design options in the project
        var designOptions = designOptionsCollector
            .OfCategory(BuiltInCategory.OST_DesignOptions)
            .Cast<DesignOption>()
            .Select(option => option.Name)
            .ToList();

        // Project only has Main Model, it has NO design options
        if (designOptions.Count == 0)
        {
            chosenDesignOptionName = "Main Model";
        }
        // Project has design options
        else
        {
            // Let the user also choose Main Model as a valid design option
            // Revit objects that belong to the 'Main Model' will have the value of 'Design Option'
            // property set to null. This should also be handled at 'ConvertInstancesToDataModels'.
            designOptions.Add("Main Model");

            // Create and show the design option picker window
            var designOptionWindow = new DesignOptionPicker(designOptions);
            bool? dialogResult = designOptionWindow.ShowDialog();

            if (dialogResult == true)
            {
                // Set the chosen design option name based on user's selection
                chosenDesignOptionName = designOptionWindow.SelectedDesignOption;
            }
            else
            {
                TaskDialog.Show("Export Canceled", "No design option was chosen. Export process has been canceled.");
                return; // Exit the method since no design option was chosen
            }

            // If the User didn't choose a design option.
            if (chosenDesignOptionName == null)
            {
                chosenDesignOptionName = "Main Model";
            }

        }


        // Part 2: Filter cabinet data models based on chosen design option and export to Excel

        // Filter for cabinet-data-models with chosen design option
        // Any element in the "Main Model" is included in every design option
        List<EKCabinetDataModel> filteredEKCabinetDataModels = cabinetDataModels
            .Where(model => model.DesignOption == chosenDesignOptionName || model.DesignOption == "Main Model")
            .ToList();

        // Open File Dialog in the project's directory
        var currentProjectPathWithExtension = doc.PathName;

        if (currentProjectPathWithExtension == "")
        {
            TaskDialog.Show("Can't Continue:", "Must be in a project that is saved");
            return;
        }

        var currentProjectPath = Path.GetDirectoryName(currentProjectPathWithExtension);

        // Use SaveFileDialog to choose the file location and name
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
                excelExporter.ExportCabinetDataToExcel(chosenDesignOptionName, filteredEKCabinetDataModels);

                // export data type 2
                //excelExporter.ExportCabinetDataToExcel(chosenDesignOptionName, crownMoldingDataModels);
                // export data type 3
                //excelExporter.ExportCabinetDataToExcel(chosenDesignOptionName, sidePanelDataModels);
                // export data type 4
                //excelExporter.ExportCabinetDataToExcel(chosenDesignOptionName, fillerStripDataModels);



                TaskDialog.Show("Successfully Exported", $"File Exported to: {filePath} \n Chosen Design Option: {chosenDesignOptionName}");
            }
            else
            {
                TaskDialog.Show("Export Canceled", "No file name was chosen. Export process has been canceled.");
            }
        }

    }

}
