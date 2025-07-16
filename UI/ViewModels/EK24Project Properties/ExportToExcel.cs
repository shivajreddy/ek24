using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
using ek24.UI.Views.Manage;
using ek24.Utils;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ek24;


/// <summary>
/// UTILITY CLASS: EXPORT TO EXCEL
/// </summary>

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class ExportToExcel : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        //throw new NotImplementedException();
        HandleExportToExcelButtonClick(commandData.Application);
        return Result.Succeeded;
    }

    // TEMPLATE FILE LOCATION
    //public static string TemplateFolder = "T:\\50_DESIGN DATA\\TRAINING\\EK24 Addin\\";
    //public static string TemplatePath = TemplateFolder + "EK24_PO_TEMPLATE_V1.xlsx";
    public static string TemplateFolder = "\\\\tecserver01\\TECdata\\TECData\\50_DESIGN DATA\\TRAINING\\EK24 Addin\\";
    public static string TemplatePath = TemplateFolder + "EK24_PO_TEMPLATE_V1.xlsx";
    //public static string TemplatePath = "\\\\tecserver01\\TECdata\\TECData\\50_DESIGN DATA\\TRAINING\\EK24 Addin\\EK24_PO_TEMPLATE_V1.xlsx";

    // GLOBAL VARIABLES
    public string ChosenDesignOptionName { get; set; } = string.Empty;
    public string FilePath { get; set; } // Path of the newly created Excel file using the TemplatePath

    // Entry point for handling the 'EXPORT P.O. TO EXCEL' button click
    public void HandleExportToExcelButtonClick(UIApplication app)
    {
        // Set the license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or Commercial if you have a commercial license

        Document doc = app.ActiveUIDocument.Document;

        // 1. Setup the Excel File
        AskUserToChooseDesignOption(doc);
        SetUpExcelFile(doc.PathName);

        // 2. Handle All worksheets (collect+transform data, then push to excel)
        CollectTranformData_And_PushToExcel_cabinets();
        CollectTranformData_And_PushToExcel_trim();
        CollectTranformData_And_PushToExcel_others();
    }


    // WORKSHEET 1: Cabinets
    public void CollectTranformData_And_PushToExcel_cabinets()
    {
        string workSheetName = "CABINETS"; // should bein in template file
        int row = 2;
        int col = 1;
        int no_of_rows_between_tables = 4; // used as gap

        // Collect, Transform, Sanitize Data ----------------
        string[] ek_BASE_CabinetFamilyNamePrefixes = {
           "Aristokraft-B-",
           "Eclipse-B-",
           "Eclipse",
           "YTC-B-",
           "YTH-B-",
       };
        object[][] base_cabinets = CollectAndTransformdata_cabinets(ek_BASE_CabinetFamilyNamePrefixes, APP.Global_State.Current_Project_State.Document);
        // Push Final Transformed Data to Excel ----------------
        PushTableDataToExcel(workSheetName, row, col, "BASE CABINETS", base_cabinets);
        row += (base_cabinets.Length + no_of_rows_between_tables);

        // Collect, Transform, Sanitize Data ----------------
        string[] ek_WALL_CabinetFamilyNamePrefixes = {
           "Aristokraft-W-",
           "Eclipse-W-",
           "Eclipse",
           "YTC-W-",
           "YTH-W-",
       };
        object[][] wall_cabinets = CollectAndTransformdata_cabinets(ek_WALL_CabinetFamilyNamePrefixes, APP.Global_State.Current_Project_State.Document);
        // Push Final Transformed Data to Excel ----------------
        PushTableDataToExcel(workSheetName, row, col, "WALL CABINETS", wall_cabinets);
        row += (wall_cabinets.Length + no_of_rows_between_tables);

        // Collect, Transform, Sanitize Data ----------------
        string[] ek_TALL_CabinetFamilyNamePrefixes = {
           "Aristokraft-T-",
           "Eclipse-T-",
           "Eclipse",
           "YTC-T-",
           "YTH-T-"
       };
        object[][] tall_cabinets = CollectAndTransformdata_cabinets(ek_TALL_CabinetFamilyNamePrefixes, APP.Global_State.Current_Project_State.Document);
        // Push Final Transformed Data to Excel ----------------
        PushTableDataToExcel(workSheetName, row, col, "TALL CABINETS", tall_cabinets);
        row += (tall_cabinets.Length + no_of_rows_between_tables);
    }
    // HELPER FOR - WORKSHEET 1: CABINETS
    public object[][] CollectAndTransformdata_cabinets(string[] ekCabinetFamilyNamePrefixes, Document doc)
    {
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);

        // Filter all the cabinet instances
        FilteredElementCollector caseWorkCollector = caseworkCollector.OfCategory(BuiltInCategory.OST_Casework);

        // Filter for FamilyInstance elements of 'Casework' category
        ICollection<Element> caseworkFamilyInstances = caseWorkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        // Filter for cabinet instances with the prefixes
        List<FamilyInstance> ekCabinetInstances = new List<FamilyInstance>();

        foreach (Element element in caseworkFamilyInstances)
        {
            FamilyInstance instance = element as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            string design_option_val = GetInstanceParameterValue(instance, "Design Option");
            // sanitize the value because it includes the design option set also
            if (design_option_val != "Main Model")
            {
                int index = design_option_val.IndexOf(" : ");
                //string restOfString = index != -1 ? design_option_val.Substring(index + 3) : "";
                string restOfString = index != -1 ? design_option_val.Substring(index + 3) : "Main Model";
                design_option_val = restOfString;
            }

            // Check if the family name starts with any of the prefixes
            if (ekCabinetFamilyNamePrefixes.Any(familyName.StartsWith) && design_option_val == ChosenDesignOptionName)
            {
                // Add the matching instance to the list
                ekCabinetInstances.Add(instance);
            }
        }
        // Convert instances into structured cabinet data
        List<EKCabinetTableDataModel> cabinetData = ConvertFamilyInstancesToEKCabinetDataModels(ekCabinetInstances);

        // Initialize the result list
        //List<string[]> result = new List<string[]>();
        List<object[]> result = new List<Object[]>();

        // Add headers
        result.Add(new string[] { "Brand", "SKU", "Configuration", "Notes", "Style", "Species", "Finish", "Features", "Hinge Location", "BaseCost", "Cost With Features", "Count" });

        // Add the data rows
        foreach (var cabinet in cabinetData)
        {
            result.Add(new object[]
            {
            cabinet.Brand,
            cabinet.BrandSKU,
            cabinet.Configuration,
            cabinet.Notes,
            cabinet.VendorStyle,
            cabinet.VendorSpecies,
            cabinet.VendorFinish,
            //cabinet.Count.ToString()  // Convert Count (int) to string
            cabinet.Features,
            cabinet.HingleLocation,
            cabinet.BaseCost,
            cabinet.CostWithFeatures,
            cabinet.Count
            });
        }

        return result.ToArray();
    }
    // HELPER FOR - WORKSHEET 1: CABINETS
    public static List<EKCabinetTableDataModel> ConvertFamilyInstancesToEKCabinetDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var cabinetDataModelsHashMap = new Dictionary<string, EKCabinetTableDataModel>();

        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Manufacturer");
            var brand_sku = instance.Symbol.Name;   // the type name
            var configuration = GetTypeParameterValue(instance, "Family Name");
            // sanitize configuration
            int firstDash = configuration.IndexOf('-');
            int secondDash = configuration.IndexOf('-', firstDash + 1);
            if (secondDash != -1 && secondDash + 1 < configuration.Length)
            {
                configuration = configuration.Substring(secondDash + 1); // Use Substring instead of range operator
            }
            var notes = GetTypeParameterValue(instance, "Vendor_Notes");
            var vendor_style = GetInstanceParameterValue(instance, "Vendor_Style");
            // sanitize vendor-style
            int dashIndex = vendor_style.LastIndexOf('-');
            if (dashIndex != -1 && dashIndex + 1 < vendor_style.Length)
            {
                vendor_style = vendor_style.Substring(dashIndex + 1).Trim(); // Extract and trim spaces
            }
            var vendor_species_and_finish = GetInstanceParameterValue(instance, "Vendor_Finish");
            string vendor_species;
            string vendor_finish;
            // empty value for material is '<By Category>' so ignore that
            if (vendor_species_and_finish == "<By Category>")
            {
                vendor_species = "";
                vendor_finish = "";
            }
            else
            {
                var parts = vendor_species_and_finish.Split('-');
                vendor_species = (parts.Length > 0) ? parts[0] : "";
                vendor_finish = (parts.Length > 1) ? parts[1] : "";
            }

            // Use Environment.NewLine instead of \n for Excel-compatible line breaks
            var features = string.Join(Environment.NewLine, EKProgram.style_properties_map[vendor_style.ToLower()]);
            //var features = string.Join("\n", EKProgram.style_properties_map[vendor_style.ToLower()]);

            var hinge_location = "";
            if (configuration.ToLower().Contains("two door"))
            {
                hinge_location = "L&R";
            }
            else
            {
                string handle_location_str = "LEFT";    // default location
                // 1.1: check user selected option
                if (GetInstanceParameterValue(instance, "Handle_Position_One") == "Yes")     // user chose LEFT
                {
                    //1.2: check if instance was mirrored
                    handle_location_str = instance.Mirrored ? "RIGHT" : "LEFT";
                }
                else   // user chose RIGHT
                {
                    //1.2: check if instance was mirrored
                    handle_location_str = instance.Mirrored ? "LEFT" : "RIGHT";
                }
                hinge_location = handle_location_str == "LEFT" ? "R" : "L"; // hinge is on the opposite side of handle
            }
            double cost = instance.Symbol.LookupParameter("Cost").AsDouble();
            double cost_with_features = cost * 1.2;  // hack: later get the logic of cost depending on chosen features

            // Create a unique key by concatenating relevant fields
            string key = $"{brand}|{brand_sku}|{configuration}|{notes}|{vendor_style}|{vendor_species_and_finish}";

            // Same row item already exists, just increase the Count property 
            if (cabinetDataModelsHashMap.ContainsKey(key))
            {
                cabinetDataModelsHashMap[key].Count++;

            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                cabinetDataModelsHashMap[key] = new EKCabinetTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    BrandSKU = brand_sku,
                    Configuration = configuration,
                    Notes = notes,
                    VendorStyle = vendor_style,
                    VendorSpecies = vendor_species,
                    VendorFinish = vendor_finish,
                    Features = features,
                    HingleLocation = hinge_location,
                    BaseCost = cost,
                    CostWithFeatures = cost_with_features,
                    Count = 1
                };
            }
        }
        return cabinetDataModelsHashMap.Values.ToList();
    }


    // WORKSHEET 2: Trim
    public void CollectTranformData_And_PushToExcel_trim()
    {
        string workSheetName = "TRIM"; // should bein in template file
        int row = 2;
        int col = 1;
        int no_of_rows_between_tables = 4; // used as gap

        object[][] crown_modling_table = CollectTranformData_Trim_CrowMolding(APP.Global_State.Current_Project_State.Document);
        PushTableDataToExcel(workSheetName, row, col, "CROWN MODLING", crown_modling_table);
        row += (crown_modling_table.Length + no_of_rows_between_tables);

        // call other trim materials helper methods
    }
    // HELPER - WORKSHEET 2: Trim
    public object[][] CollectTranformData_Trim_CrowMolding(Document doc)
    {
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);
        // Filter all the cabinet instances
        FilteredElementCollector caseWorkCollector = caseworkCollector.OfCategory(BuiltInCategory.OST_Casework);

        // Filter for FamilyInstance elements of 'Casework' category
        ICollection<Element> caseworkFamilyInstances = caseWorkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

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

        // Filter for crown modling instances with the prefixes
        List<FamilyInstance> crownMolding_instances = new List<FamilyInstance>();

        foreach (Element element in caseworkFamilyInstances)
        {
            FamilyInstance instance = element as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            string design_option_val = GetInstanceParameterValue(instance, "Design Option");
            // sanitize the value because it includes the design option set also
            if (design_option_val != "Main Model")
            {
                int index = design_option_val.IndexOf(" : ");
                string restOfString = index != -1 ? design_option_val.Substring(index + 3) : "";
                design_option_val = restOfString;
            }

            // Check if the family name starts with any of the prefixes
            if (ekCrownMoldingFamilyPrefixes.Any(familyName.StartsWith) && design_option_val == ChosenDesignOptionName)
            {
                // Add the matching instance to the list
                crownMolding_instances.Add(instance);
            }
        }

        // Convert instances into structured EKCrownMolding data
        List<EKCrownMoldingTableDataModel> crown_modling_data = ConvertFamilyInstancesToEKCrownMoldingDataModels(crownMolding_instances);

        // Initialize the result list
        List<string[]> result = new List<string[]>();

        // Add headers
        result.Add(new string[] { "Brand", "SKU", "Notes", "Length", "Count" });

        // Add the data rows
        foreach (var crown_molding in crown_modling_data)
        {
            result.Add(new string[]
            {
            crown_molding.Brand,
            crown_molding.SKU,
            crown_molding.Notes,
            crown_molding.Length,
            crown_molding.Count.ToString()  // Convert Count (int) to string
            });
        }
        return result.ToArray();
    }
    // HELPER - WORKSHEET 2: Trim
    public static List<EKCrownMoldingTableDataModel> ConvertFamilyInstancesToEKCrownMoldingDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var crownmodlingDataModelsHashMap = new Dictionary<string, EKCrownMoldingTableDataModel>();
        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Manufacturer");
            var sku = GetTypeParameterValue(instance, "Vendor_SKU");
            var notes = GetTypeParameterValue(instance, "Vendor_Notes");
            var length = GetTypeParameterValue(instance, "Length");

            // Create a unique key by concatenating relevant fields
            string key = $"{brand}|{sku}|{notes}|{length}";

            // Same row item already exists, just increase the Count property 
            if (crownmodlingDataModelsHashMap.ContainsKey(key))
            {
                crownmodlingDataModelsHashMap[key].Count++;
            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                crownmodlingDataModelsHashMap[key] = new EKCrownMoldingTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    SKU = sku,
                    Notes = notes,
                    Length = length,
                    Count = 1
                };
            }
        }
        return crownmodlingDataModelsHashMap.Values.ToList();
    }


    // WORKSHEET 3: Others
    public void CollectTranformData_And_PushToExcel_others() { }
    // Ask user to choose the Design Option if there is any by showing the design option picker dialog
    public void AskUserToChooseDesignOption(Document doc)
    {
        FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);

        // Get all design options in the project
        var designOptions = designOptionsCollector
            .OfCategory(BuiltInCategory.OST_DesignOptions)
            .Cast<DesignOption>()
            .Select(option => option.Name)
            .ToList();

        // Project only has Main Model, it has NO design options
        if (designOptions.Count == 0)
        {
            ChosenDesignOptionName = "Main Model";
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
                string design_option_val = designOptionWindow.SelectedDesignOption;
                if (design_option_val.EndsWith(" <primary>"))
                {
                    ChosenDesignOptionName = design_option_val.Substring(0, design_option_val.Length - "  <primary>".Length);
                }
            }
            else
            {
                TaskDialog.Show("Export Canceled", "No design option was chosen. Export process has been canceled.");
                return; // Exit the method since no design option was chosen
            }

            // If the User didn't choose a design option.
            if (ChosenDesignOptionName == null)
            {
                ChosenDesignOptionName = "Main Model";
            }
        }
    }

    // Creates the Excel File, using the template file
    public void SetUpExcelFile(string revit_project_path)
    {
        try
        {
            // Check if template folder exists
            if (!Directory.Exists(TemplateFolder))
            {
                TaskDialog.Show("ERROR", $"Template folder: {TemplateFolder} not found or inaccessible");
                throw new DirectoryNotFoundException($"Template folder not found: {TemplateFolder}");
            }

            // Check if template file exists
            if (!File.Exists(TemplatePath))
            {
                TaskDialog.Show("ERROR", $"Template file: {TemplatePath} not found");
                throw new FileNotFoundException("Template file not found", TemplatePath);
            }

            // Define destination path
            string newpath = CreateFilePath(revit_project_path);

            // Ensure destination directory exists
            string destDir = Path.GetDirectoryName(newpath);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            FilePath = newpath; // update global variable, for other fn's to use

            // Create a new excel file using the template path
            File.Copy(TemplatePath, FilePath, false);
        }
        catch (DirectoryNotFoundException ex)
        {
            TaskDialog.Show("Directory Error", $"Could not find directory: {ex.Message}\nPlease check if the T: drive is properly mapped.");
            throw;
        }
        catch (FileNotFoundException ex)
        {
            TaskDialog.Show("File Error", $"Could not find file: {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            TaskDialog.Show("IO Error", $"Error copying file: {ex.Message}");
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            TaskDialog.Show("Access Error", $"Access denied: {ex.Message}\nPlease check file permissions.");
            throw;
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Unexpected Error", $"An unexpected error occurred: {ex.Message}");
            throw;
        }
    }

    // Helper FN: given the revit project path, creates a full path of the new excel file including versioning
    public string CreateFilePath(string currentPath)
    {
        /* logic
         current_path will be something like  C:\Users\sreddy\Desktop\SR-Linden III-V30.rvt
         i want the result to be C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V1.xlsx
         and also if there is a file name actually ends with something like -PO_V? i would want the new name to be the next version
         so if we have to keep increasing the version number until there is no clash
         so say if the given current path is C:\Users\sreddy\Desktop\SR-Linden III-V30.rvt, but the folder C:\Users\sreddy\Desktop\ has
         already has
         C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V1.xlsx 
         C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V2.xlsx
         C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V3.xlsx
         C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V4.xlsx
         then the result should be C:\Users\sreddy\Desktop\SR-Linden III-V30-PO_V5.xlsx
        */

        // Get the directory and file name (without extension)
        string directory = Path.GetDirectoryName(currentPath) ?? "";
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(currentPath);
        string newFileNameBase = fileNameWithoutExt + "-PO_V";
        string newFilePath;
        int version = 1;

        // Regex to find existing versions (e.g., "-PO_V1.xlsx")
        Regex regex = new Regex(Regex.Escape(newFileNameBase) + @"(\d+)\.xlsx$", RegexOptions.IgnoreCase);

        // Get all files in the directory that match the pattern
        string[] existingFiles = Directory.GetFiles(directory, newFileNameBase + "*.xlsx");

        // Find the highest version number
        foreach (string file in existingFiles)
        {
            Match match = regex.Match(Path.GetFileName(file));
            if (match.Success && int.TryParse(match.Groups[1].Value, out int foundVersion))
            {
                version = Math.Max(version, foundVersion + 1);
            }
        }

        // Construct the final file name with the next available version
        newFilePath = Path.Combine(directory, $"{newFileNameBase}{version}.xlsx");

        return newFilePath;
    }

    // Pushes table data to the Excel-file at FilePath
    // NOTE: THIS WILL THROW ERROR IF 
    public void PushTableDataToExcel(string workSheetName, int startRow, int startCol, string tableName, object[][] table)
    {
        if (table == null || table.Length == 0)
        {
            throw new ArgumentException("Table data cannot be empty.");
        }

        FileInfo fileInfo = new FileInfo(FilePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("Excel file not found.", FilePath);
        }

        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            // Find if the 'workSheetName' exists
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(workSheetName, StringComparison.OrdinalIgnoreCase));
            if (worksheet == null)
            {
                TaskDialog.Show("ERROR while Pushing data to Excel File", $"File doesn't have a worksheet: {workSheetName}");
                throw new Exception("ERROR while Pushing data to Excel File");
            }

            int numRows = table.Length;
            int numCols = table[0].Length;

            // Insert table name (if provided)
            if (!string.IsNullOrEmpty(tableName))
            {
                var titleCell = worksheet.Cells[startRow, startCol];
                titleCell.Value = tableName;
                titleCell.Style.Font.Size = 14; // Set the font size
                var row = worksheet.Row(startRow);
                row.Style.Fill.PatternType = ExcelFillStyle.Solid;
                row.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#dae9f8")); // Example hex color
                startRow++; // Move down to where the table starts
            }

            // Insert table headers (assuming the first row in `table` is headers)
            for (int col = 0; col < numCols; col++)
            {
                worksheet.Cells[startRow, startCol + col].Value = table[0][col]?.ToString();
                worksheet.Cells[startRow, startCol + col].Style.Font.Bold = true;
                worksheet.Cells[startRow, startCol + col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[startRow, startCol + col].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#f2f2f2"));
            }

            // Insert data into the worksheet
            for (int row = 1; row < numRows; row++) // Start from index 1 as index 0 is headers
            {
                for (int col = 0; col < numCols; col++)
                {
                    worksheet.Cells[startRow + row, startCol + col].Value = table[row][col];
                }
            }

            // Define the table range
            string tableRange = $"{worksheet.Cells[startRow, startCol].Address}:{worksheet.Cells[startRow + numRows - 1, startCol + numCols - 1].Address}";
            //var excelTable = worksheet.Tables.Add(worksheet.Cells[tableRange], tableName ?? "Table1");
            Func<string, string> ReplaceSpaces = s => s.Replace(" ", "_");
            tableName = ReplaceSpaces(tableName);
            var excelTable = worksheet.Tables.Add(worksheet.Cells[tableRange], tableName ?? "Table1"); // shouldn't contain spaces

            // Create or get a custom table style
            //string Custom_Style_Name_For_PO = "EK_Table_Style";
            //string Custom_Style_Name_For_PO = "Table Style Medium 1";
            string Custom_Style_Name_For_PO = "Custom_EK_TableStyle";

            // Apply a built-in table style
            excelTable.StyleName = Custom_Style_Name_For_PO;

            package.Save();
            Debug.WriteLine($"Data pushed successfully to {FilePath}");
        }
    }

    public void PushTableDataToExcel_old(string workSheetName, int startRow, int startCol, string tableName, object[][] table)
    {
        if (table == null || table.Length == 0)
        {
            throw new ArgumentException("Table data cannot be empty.");
        }

        FileInfo fileInfo = new FileInfo(FilePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("Excel file not found.", FilePath);
        }

        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            // NOTE: THE BELOW LINE IS IN INFINITE LOOP (when used by raising through event handler, doesn't get stuck when executed through ExternaalCommand
            // Find if the 'workSheetName' exists if not throw error (because we want to strictly control worksheets through template file)
            bool worksheet_exists = package.Workbook.Worksheets.Any(ws => ws.Name.Equals(workSheetName, StringComparison.OrdinalIgnoreCase));
            if (!worksheet_exists)
            {
                TaskDialog.Show("ERROR while Pushing data to Excel File", $"File doesn't have a worksheet: {workSheetName}");
                throw new Exception("ERROR while Pushing data to Excel File");
            }

            ExcelWorksheet worksheet = package.Workbook.Worksheets[workSheetName];

            // Add Table Heading if it exists
            // Apply formatting to the table heading
            using (ExcelRange table_title_row = worksheet.Cells[startRow, startCol, startRow, startCol + 50]) // using 50 as width of row for now
            {
                table_title_row.Style.Font.Size = 14;                // Font size 18
                table_title_row.Style.Fill.PatternType = ExcelFillStyle.Solid;
                table_title_row.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#d0d0d0")); // Gray background
            }
            if (!string.IsNullOrEmpty(tableName))
            {
                worksheet.Cells[startRow, startCol].Value = tableName; // add table name
                startRow++; // move the row-cell location, col stays the same
            }

            int numRows = table.Length;
            int numCols = table[0].Length;

            // Insert data into the worksheet
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    object cellValue = table[row][col];

                    // Convert cellValue to string safely
                    string cellText = cellValue?.ToString() ?? string.Empty;

                    // Try parsing the value as an integer
                    if (int.TryParse(cellText, out int intValue))
                    {
                        worksheet.Cells[startRow + row, startCol + col].Value = intValue; // Set as integer
                    }
                    else if (double.TryParse(cellText, out double doubleValue))
                    {
                        worksheet.Cells[startRow + row, startCol + col].Value = doubleValue; // Set as double
                    }
                    else
                    {
                        worksheet.Cells[startRow + row, startCol + col].Value = cellValue; // Set as string
                    }
                }
            }

            // Apply formatting to the first row of the table
            using (ExcelRange table_header_row = worksheet.Cells[startRow, startCol, startRow, startCol + 50])
            {
                table_header_row.Style.Font.Bold = true;               // Bold text
                table_header_row.Style.Font.Size = 12;                // Font size 18
                table_header_row.Style.Fill.PatternType = ExcelFillStyle.Solid;
                table_header_row.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#f2f2f2")); // Gray background
            }


            package.Save();
            Debug.WriteLine($"Data pushed successfully to {FilePath}");
        }
    }


    // HELPER FN: get Type parametre data of a family instance
    private static string GetTypeParameterValue(FamilyInstance instance, string typeParamName)
    {
        var type = instance.Symbol;
        var typeParam = type.LookupParameter(typeParamName);

        // Param is not found or has no value
        if (typeParam == null || !typeParam.HasValue)
        {
            return string.Empty;
        }
        if (typeParam.AsString() != null)
        {
            return typeParam.AsString();
        }
        if (typeParam.AsValueString() != null)
        {
            return typeParam.AsValueString();
        }
        return string.Empty;
    }

    // HELPER FN: get instance parametre data of a family instance
    private static string GetInstanceParameterValue(FamilyInstance familyInstance, string instanceParamName)
    {
        var instanceParam = familyInstance.LookupParameter(instanceParamName);

        // Param is not found or has no value
        if (instanceParam == null || !instanceParam.HasValue)
        {
            return string.Empty;
        }
        if (instanceParam.AsString() != null)
        {
            return instanceParam.AsString();
        }
        if (instanceParam.AsValueString() != null)
        {
            return instanceParam.AsValueString();
        }
        return string.Empty;
    }

    public static void foo(Document doc)
    {
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


        // Part 2: Filter cabinet data models based on chosen design option and export to Excel

        string chosenDesignOptionName = "alskdjaklsdjaklsdjfalksdfjaklsdj";

        // Filter for cabinet-data-models with chosen design option
        // Any element in the "Main Model" is included in every design option
        List<EKCabinetTableDataModel> filteredEKCabinetDataModels = cabinetDataModels
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


public static class EKProgram
{
    public static Dictionary<string, List<string>> style_properties_map = new Dictionary<string, List<string>>
    {
        { "sinclair", new List<string> { "partial overlay", "flat panel", "furniture board boxes", "stapled butt joint drawers", "3/4 extension drawers", "standard drawer glides", "standard hinges" } },
        { "benton", new List<string> { "partial overlay", "flat panel", "furniture board boxes", "stapled butt joint drawers", "3/4 extension drawers", "standard drawer glides", "standard hinges" } },
        { "brellin", new List<string> { "full overlay", "flat panel", "furniture board boxes", "stapled butt joint drawers", "3/4 extension drawers", "standard drawer glides", "standard hinges" } },
        { "winstead", new List<string> { "full overlay", "flat panel", "furniture board boxes", "stapled butt joint drawers", "3/4 extension drawers", "standard drawer glides", "standard hinges" } },
        { "henning", new List<string> { "full overlay", "flat panel", "dovetail drawers", "soft close doors + drawers", "full extension undermount glides", "metal shelf supports", "veneer center panel" } },
        { "stillwater", new List<string> { "full overlay", "flat panel", "dovetail drawers", "soft close doors + drawers", "full extension undermount glides", "metal shelf supports", "veneer center panel" } },
        { "fillmore", new List<string> { "full overlay", "raised panel", "dovetail drawers", "soft close doors + drawers", "full extension undermount glides", "metal shelf supports", "veneer center panel" } },
        { "corsica", new List<string> { "inset (beaded or flush)", "flat panel", "dovetail drawers", "soft close doors + drawers", "5-piece drawer fronts", "concealed hinges", "reversed raised panel doors", "plywood box", "wall cabinets 13\" deep" } },
        { "langdon", new List<string> { "inset (beaded or flush)", "flat panel", "dovetail drawers", "soft close doors + drawers", "5-piece drawer fronts", "concealed hinges", "reversed raised panel doors", "plywood box", "wall cabinets 13\" deep" } }
    };

    // Copied from the python node inside the dynamo file
    public static List<string> materials_brand_style_species_finish = [
     "Aristokraft-Sinclair-Birch-Arid", "Aristokraft-Sinclair-Birch-Flagstone", "Aristokraft-Sinclair-Birch-Natural", "Aristokraft-Sinclair-Birch-Quill", "Aristokraft-Sinclair-Birch-Saddle", "Aristokraft-Sinclair-Birch-Umber",
     "Aristokraft-Benton-Birch-Arid", "Aristokraft-Benton-Birch-Flagstone", "Aristokraft-Benton-Birch-Natural", "Aristokraft-Benton-Birch-Quill", "Aristokraft-Benton-Birch-Saddle", "Aristokraft-Benton-Birch-Umber",
     "Aristokraft-Brellin-Purestyle-Colada", "Aristokraft-Brellin-Purestyle-Frost", "Aristokraft-Brellin-Purestyle-Stone Gray", "Aristokraft-Brellin-Purestyle-White",
     "Aristokraft-Winstead-Birch-Arid", "Aristokraft-Winstead-Birch-Flagstone", "Aristokraft-Winstead-Birch-Natural", "Aristokraft-Winstead-Birch-Quill", "Aristokraft-Winstead-Birch-Saddle", "Aristokraft-Winstead-Birch-Umber",
     "Aristokraft-Winstead-Neutral Paint-Colada", "Aristokraft-Winstead-Neutral Paint-Frost", "Aristokraft-Winstead-Neutral Paint-Stone Gray", "Aristokraft-Winstead-Neutral Paint-White",

     "Yorktowne Classic-Henning-Maple-Amaretto", "Yorktowne Classic-Henning-Maple-Biscotti", "Yorktowne Classic-Henning-Maple-Eagle Rock", "Yorktowne Classic-Henning-Maple-Espresso", "Yorktowne Classic-Henning-Maple-Ginger Snap", "Yorktowne Classic-Henning-Maple-Onyx",
     "Yorktowne Classic-Henning-Neutral Paint-Chai Latte", "Yorktowne Classic-Henning-Neutral Paint-Gray Owl", "Yorktowne Classic-Henning-Neutral Paint-Irish Crème", "Yorktowne Classic-Henning-Neutral Paint-Macchiato",  "Yorktowne Classic-Henning-Neutral Paint-Sea Salt",
     "Yorktowne Classic-Henning-Rich Paint-Carriage Black", "Yorktowne Classic-Henning-Rich Paint-Celeste", "Yorktowne Classic-Henning-Rich Paint-Earl Gray", "Yorktowne Classic-Henning-Rich Paint-Eucalyptus", "Yorktowne Classic-Henning-Rich Paint-Frappe", "Yorktowne Classic-Henning-Rich Paint-Gale",
     "Yorktowne Classic-Stillwater-Maple-Amaretto", "Yorktowne Classic-Stillwater-Maple-Biscotti", "Yorktowne Classic-Stillwater-Maple-Eagle Rock", "Yorktowne Classic-Stillwater-Maple-Espresso", "Yorktowne Classic-Stillwater-Maple-Ginger Snap", "Yorktowne Classic-Stillwater-Maple-Onyx",
     "Yorktowne Classic-Stillwater-Neutral Paint-Chai Latte", "Yorktowne Classic-Stillwater-Neutral Paint-Gray Owl", "Yorktowne Classic-Stillwater-Neutral Paint-Irish Crème", "Yorktowne Classic-Stillwater-Neutral Paint-Macchiato",  "Yorktowne Classic-Stillwater-Neutral Paint-Sea Salt",
     "Yorktowne Classic-Stillwater-Rich Paint-Carriage Black", "Yorktowne Classic-Stillwater-Rich Paint-Celeste", "Yorktowne Classic-Stillwater-Rich Paint-Earl Gray", "Yorktowne Classic-Stillwater-Rich Paint-Eucalyptus", "Yorktowne Classic-Stillwater-Rich Paint-Frappe", "Yorktowne Classic-Stillwater-Rich Paint-Gale",
     "Yorktowne Classic-Fillmore-Maple-Biscotti", "Yorktowne Classic-Fillmore-Maple-Eagle Rock", "Yorktowne Classic-Fillmore-Maple-Espresso", "Yorktowne Classic-Fillmore-Maple-Ginger Snap", "Yorktowne Classic-Fillmore-Maple-Onyx",
     "Yorktowne Classic-Fillmore-Neutral Paint-Chai Latte", "Yorktowne Classic-Fillmore-Neutral Paint-Gray Owl", "Yorktowne Classic-Fillmore-Neutral Paint-Irish Crème", "Yorktowne Classic-Fillmore-Neutral Paint-Macchiato",  "Yorktowne Classic-Fillmore-Neutral Paint-Sea Salt",
     "Yorktowne Classic-Fillmore-Rich Paint-Carriage Black", "Yorktowne Classic-Fillmore-Rich Paint-Celeste", "Yorktowne Classic-Fillmore-Rich Paint-Earl Gray", "Yorktowne Classic-Fillmore-Rich Paint-Eucalyptus", "Yorktowne Classic-Fillmore-Rich Paint-Frappe", "Yorktowne Classic-Fillmore-Rich Paint-Gale",

     "Eclipse-Metropolitan-HighGlossAcrylic-Bianco", "Eclipse-Metropolitan-HighGlossAcrylic-Bigio", "Eclipse-Metropolitan-HighGlossAcrylic-Cubanite", "Eclipse-Metropolitan-HighGlossAcrylic-Gabbiano",
     "Eclipse-Metropolitan-MatteAcrylic-Ash Velvet", "Eclipse-Metropolitan-MatteAcrylic-Carbon Velvet", "Eclipse-Metropolitan-MatteAcrylic-Charcoal Velvet", "Eclipse-Metropolitan-MatteAcrylic-White Velvet",
     "Eclipse-Metropolitan-ThermallyFusedLaminates-Kirsche", "Eclipse-Metropolitan-ThermallyFusedLaminates-Morning Fog", "Eclipse-Metropolitan-ThermallyFusedLaminates-Natural Elm", "Eclipse-Metropolitan-ThermallyFusedLaminates-Takase Teak",

     "Yorktowne Historic-Corsica-Maple-Amaretto", "Yorktowne Historic-Corsica-Maple-Biscotti", "Yorktowne Historic-Corsica-Maple-Eagle Rock", "Yorktowne Historic-Corsica-Maple-Espresso", "Yorktowne Historic-Corsica-Maple-Ginger Snap", "Yorktowne Historic-Corsica-Maple-Onyx",
     "Yorktowne Historic-Corsica-Neutral Paint-Chai Latte", "Yorktowne Historic-Corsica-Neutral Paint-Gray Owl", "Yorktowne Historic-Corsica-Neutral Paint-Irish Crème", "Yorktowne Historic-Corsica-Neutral Paint-Macchiato",  "Yorktowne Historic-Corsica-Neutral Paint-Sea Salt",
     "Yorktowne Historic-Corsica-Rich Paint-Carriage Black", "Yorktowne Historic-Corsica-Rich Paint-Celeste", "Yorktowne Historic-Corsica-Rich Paint-Coffee Break", "Yorktowne Historic-Corsica-Rich Paint-Earl Gray", "Yorktowne Historic-Corsica-Rich Paint-Eucalyptus", "Yorktowne Historic-Corsica-Rich Paint-Frappe", "Yorktowne Historic-Corsica-Rich Paint-Gale", "Yorktowne Historic-Corsica-Rich Paint-Tidal",
     "Yorktowne Historic-Langdon-Maple-Amaretto", "Yorktowne Historic-Langdon-Maple-Biscotti", "Yorktowne Historic-Langdon-Maple-Eagle Rock", "Yorktowne Historic-Langdon-Maple-Espresso", "Yorktowne Historic-Langdon-Maple-Ginger Snap", "Yorktowne Historic-Langdon-Maple-Onyx",
     "Yorktowne Historic-Langdon-Neutral Paint-Chai Latte", "Yorktowne Historic-Langdon-Neutral Paint-Gray Owl", "Yorktowne Historic-Langdon-Neutral Paint-Irish Crème", "Yorktowne Historic-Langdon-Neutral Paint-Macchiato",  "Yorktowne Historic-Langdon-Neutral Paint-Sea Salt",
     "Yorktowne Historic-Langdon-Rich Paint-Carriage Black", "Yorktowne Historic-Langdon-Rich Paint-Celeste", "Yorktowne Historic-Langdon-Rich Paint-Coffee Break", "Yorktowne Historic-Langdon-Rich Paint-Earl Gray", "Yorktowne Historic-Langdon-Rich Paint-Eucalyptus", "Yorktowne Historic-Langdon-Rich Paint-Frappe", "Yorktowne Historic-Langdon-Rich Paint-Gale", "Yorktowne Historic-Langdon-Rich Paint-Tidal"
    ];

    // Function to extract species and finish 
    public static List<string> get_species_finish(string brand, string style)
    {
        List<string> result = new List<string>();

        foreach (string item in materials_brand_style_species_finish)
        {
            var parts = item.Split('-');
            if (parts[0] == brand && parts[1] == style) // only get the items that have matching brand&style
            {
                result.Add(parts[2] + "-" + parts[3]);
            }
        }
        return result;
    }
}

