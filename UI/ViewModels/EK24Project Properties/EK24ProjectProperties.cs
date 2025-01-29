namespace ek24.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.Commands.Utils;
using ek24.Dtos;
using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;
using ek24.UI.ViewModels.ChangeBrand;
using ek24.UI.ViewModels.Manage;
using ek24.UI.Views.Manage;
using ek24.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;


public class EK24ProjectProperties_ViewModel : INotifyPropertyChanged
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    private static void OnStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Properties to bind to UI
    public List<string> BrandItems { get; } = EKBrands.all_brand_names;
    public bool canUpdateKitchenBrandButton => EKProjectKitchenBrand != "" && SelectedBrand != "" && SelectedBrand != EKProjectKitchenBrand;

    private Brand _chosenBrandFromDropdown;
    public Brand ChosenBrandFromDropdown
    {
        get => _chosenBrandFromDropdown;
        set
        {
            if (_chosenBrandFromDropdown != value)
            {
                _chosenBrandFromDropdown = value;
                OnPropertyChanged(nameof(ChosenBrandFromDropdown));
                OnPropertyChanged(nameof(EKProjectKitchenBrand)); // Notify for BrandName if needed
            }
        }
    }

    public string EKProjectKitchenBrand
    {
        get
        {
            if (APP.Global_State.Current_Project_State == null || APP.Global_State.Current_Project_State.EKProjectKitchenBrand == null) return "";
            return APP.Global_State.Current_Project_State.EKProjectKitchenBrand;
        }
        set
        {
            APP.Global_State.Current_Project_State.EKProjectKitchenBrand = value;
        }
    }

    // HARD CODED FOR NOW
    private List<string> hardcoded_vendorStyleNames = [
            "Aristokraft - Sinclair",
            "Aristokraft - Benton",
            "Aristokraft - Brellin",
            "Aristokraft - Winstead",
            "Yorktowne Classic - Henning",
            "Yorktowne Classic - Stillwater",
            "Yorktowne Classic - Fillmore",
            "Eclipse - Metropolitan",
            "Yorktowne Historic - Corsica",
            "Yorktowne Historic - Langdon"
        ];
    private List<string> _vendorStyleNameForKitchenBrand { get; set; }
    public List<string> VendorStyleNamesForKitchenBrand
    {
        get => _vendorStyleNameForKitchenBrand;
        set
        {
            if (_vendorStyleNameForKitchenBrand == value) return;
            _vendorStyleNameForKitchenBrand = value;
            OnPropertyChanged(nameof(VendorStyleNamesForKitchenBrand));
        }
    }

    private ObservableCollection<Vendor_Style_With_Id> _VendorStyles;
    public ObservableCollection<Vendor_Style_With_Id> VendorStyles
    {
        get => _VendorStyles;
        set
        {
            if (value == _VendorStyles) return;
            _VendorStyles = value;
            OnPropertyChanged(nameof(VendorStyles));
        }
    }

    /*
    private static string _selectedVendorStyle { get; set; }
    public static string SelectedVendorStyle
    {
        get => _selectedVendorStyle;
        set
        {
            if (_selectedVendorStyle == value) return;
            _selectedVendorStyle = value;
            OnStaticPropertyChanged(nameof(SelectedVendorStyle));
        }
    }
    private static string _selectedVendorFinish { get; set; }
    public static string SelectedVendorFinish
    {
        get => _selectedVendorFinish;
        set
        {
            if (_selectedVendorFinish == value) return;
            _selectedVendorFinish = value;
            OnStaticPropertyChanged(nameof(SelectedVendorFinish));
        }
    }
    */

    public static Vendor_Style_With_Id ChosenVendorStyle;
    public static string ChosenVendorFinish;
    private Vendor_Style_With_Id _selectedVendorStyle;
    public Vendor_Style_With_Id SelectedVendorStyle
    {
        get => _selectedVendorStyle;
        set
        {
            _selectedVendorStyle = value;
            OnPropertyChanged(nameof(SelectedVendorStyle));
            OnPropertyChanged(nameof(SelectedVendorFinish));

            ChosenVendorStyle = SelectedVendorStyle;
        }
    }

    private string _selectedVendorFinish { get; set; }
    public string SelectedVendorFinish
    {
        get => _selectedVendorFinish;
        set
        {
            if (_selectedVendorFinish == value) return;
            _selectedVendorFinish = value;
            OnPropertyChanged(nameof(SelectedVendorFinish));
            ChosenVendorFinish = SelectedVendorFinish;
        }
    }

    #endregion

    #region HELPER Function to filter item sources
    public void filter_vendor_styles()
    {
        // Grab your entire hardcoded list
        List<string> all_names = hardcoded_vendorStyleNames;

        // Check if the user has selected a brand
        if (string.IsNullOrWhiteSpace(EKProjectKitchenBrand)) return;

        // Get one of the cabinet family instance for the current project, and use that family-instance
        // for getting the VendorStyle param, which in turn gives the Element Id's
        // Get all cabinet instances
        Document doc = APP.Global_State.Current_Project_State.Document;
        List<FamilyInstance> cabinet_instances = FilterAllCabinets.FilterProjectForEagleCabinets(doc);
        FamilyInstance familyInstance = cabinet_instances.First();  // Grab just one of the EKCabinets(this is not supposed to be glass cabinet)

        // Grab all the possible Element Id's (Look at `Update_Project_Style_Finish_Utility.UpdateStyleParam` for more comments/documentation)
        Parameter vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
        if (!Category.IsBuiltInCategory(vendor_style_param.Definition.GetDataType()))
        {
            TaskDialog.Show("ERROR", "Vendor_Style's data type is not a built in category");
            return;
        }
        ISet<ElementId> all_possible_familytype_parameter_values_set = familyInstance.Symbol.Family.GetFamilyTypeParameterValues(vendor_style_param.Id);

        if (all_possible_familytype_parameter_values_set.Count == 0)
        {
            TaskDialog.Show("ERROR", "One of the chosen istance doesnt have Vendor-Style param or Vendor-Finish Param");
            return;
        }

        Dictionary<string, ElementId> map_name_eid = new Dictionary<string, ElementId>();
        HashSet<string> names = [];

        foreach (var eid in all_possible_familytype_parameter_values_set)
        {
            Element e = doc.GetElement(eid);
            names.Add(e.Name);
            map_name_eid[e.Name] = eid;
        }
        // Filter VendorStyles that belogn to current Brand

        // 1. Get the current brand from your family instance.
        Parameter manufacturer_param = familyInstance.Symbol.LookupParameter("Manufacturer");
        string currentBrand = manufacturer_param.AsValueString();
        // 2. Map each brand to the possible prefix strings that might appear in the dictionary
        Dictionary<string, string[]> brandToPrefixes = new Dictionary<string, string[]>
        {
            { "Aristokraft",         new[] { "Aristokraft" } },
            { "Yorktowne Classic",   new[] { "YTC", "Yorktowne Classic" } },
            { "Yorktowne Historic",  new[] { "YTH", "Yorktowne Historic" } },
            { "Eclipse",             new[] { "Eclipse" } }
        };

        if (!brandToPrefixes.TryGetValue(currentBrand, out string[] possiblePrefixes))
        {
            // Handle unexpected brand, for example:
            possiblePrefixes = Array.Empty<string>();
        }

        // 3. Filter the dictionary so only those whose Key starts with ANY of the possible prefixes remain.
        var filteredMap = map_name_eid
            .Where(kvp => possiblePrefixes.Any(prefix => kvp.Key.StartsWith(prefix)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // 4. Build the list/collection for the combo box
        ObservableCollection<Vendor_Style_With_Id> temp_filtered_vendor_styles = new ObservableCollection<Vendor_Style_With_Id>(
                    filteredMap.Select(kvp => new Vendor_Style_With_Id(kvp.Key, kvp.Value))
                );

        // Set the available styles & finishes that user can apply to the singly selected cabinet
        VendorStyles = temp_filtered_vendor_styles;

        /*
        List<string> temp_filteredVendorStyles = new List<string>();
        List<Vendor_Style_With_Id> temp_filtered_vendor_styles = new List<Vendor_Style_With_Id>();

        // Filter for names that start with "<brand> - "
        temp_filteredVendorStyles = all_names
            .Where(name => name.StartsWith(EKProjectKitchenBrand + " - ", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // If no matches, handle it gracefully (optional)
        if (temp_filteredVendorStyles.Count == 0)
        {
            TaskDialog.Show("INFO", $"No vendor styles found for brand '{SelectedBrand}'.");
        }
        VendorStyleNamesForKitchenBrand = temp_filteredVendorStyles; // set the ppty that triggers UI
        */
    }
    #endregion

    #region Selected Item for ComboBoxes
    private string _selectedBrand { get; set; }
    public string SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (_selectedBrand == value) return;
            _selectedBrand = value;
            OnPropertyChanged(nameof(SelectedBrand));
            OnPropertyChanged(nameof(canUpdateKitchenBrandButton));
        }
    }
    #endregion

    #region GLOBAL STATE PROPERTY CHANGE HANDLERS
    private EK_Project_State _currentProjectState;
    private void GlobalState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State))
        {
            // Unsubscribe from the old project state
            if (_currentProjectState != null)
            {
                _currentProjectState.PropertyChanged -= CurrentProjectState_PropertyChanged;
            }

            // Subscribe to the new project state
            _currentProjectState = APP.Global_State.Current_Project_State;
            if (_currentProjectState != null)
            {
                _currentProjectState.PropertyChanged += CurrentProjectState_PropertyChanged;
            }

            // Force the UI to reevaluate these properties, 
            // in case the new project state is entirely different
            OnPropertyChanged(nameof(EKProjectKitchenBrand));
        }
    }
    private void CurrentProjectState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Project_State.EKProjectKitchenBrand))
        {
            OnPropertyChanged(nameof(EKProjectKitchenBrand));
            filter_vendor_styles();
        }
    }
    #endregion

    #region Relay Commands
    public ICommand Command_UpdateKitchenBrand { get; }
    public void Handle_Command_UpdateKitchenBrand()
    {
        Update_ProjectKitchenBrand_Utility.target_project_kitchenBrand = SelectedBrand;

        Debug.WriteLine("UPDATE Instance Param Value");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_UpdateKitchenBrand;
        APP.ExternalEvent?.Raise();

    }
    public ICommand Command_UpdateVendorStyleFinish { get; }
    public void Handle_Command_UpdateVendorStyleFinish()
    {
        Debug.WriteLine("UPDATE STYLE & FINISH");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_UpdateStyleAndFinish;
        APP.ExternalEvent?.Raise();     // invokes Update_Project_Style_Finish_Utility.change_style_finish(app);
    }
    public ICommand Command_ExportDrawingsToPdf { get; }
    public void Handle_Command_ExportDrawingsToPdf()
    {
        Debug.WriteLine("Export Drawings to PDF");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_ExportDrawingsToPDF;
        APP.ExternalEvent?.Raise();     // invokes Export_Drawings_To_PDF_Utility.PrintDocument(app);
    }
    public ICommand Command_ExportToExcel { get; }
    public void Handle_Command_ExportToExcel()
    {
        Debug.WriteLine("Export Drawings to PDF");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_ExportToExcel;
        APP.ExternalEvent?.Raise();     // invokes Export_To_Excel_Utility.ExportQuantitiesToExcel(app);



    }
    #endregion

    #region Constructor
    public EK24ProjectProperties_ViewModel()
    {
        // THESE 2 THINGS ARE ACTUALLY TRIGGERING THE ONPROPERTY CHANGE CALLS
        // 1) Listen to changes on the Global State itself (especially Current_Project_State)
        APP.Global_State.PropertyChanged += GlobalState_PropertyChanged;
        // 2) Subscribe to the *current* project state's changes
        _currentProjectState = APP.Global_State.Current_Project_State;
        if (_currentProjectState != null)
        {
            _currentProjectState.PropertyChanged += CurrentProjectState_PropertyChanged;
        }

        Command_UpdateKitchenBrand = new RelayCommand(Handle_Command_UpdateKitchenBrand);
        Command_UpdateVendorStyleFinish = new RelayCommand(Handle_Command_UpdateVendorStyleFinish);
        Command_ExportDrawingsToPdf = new RelayCommand(Handle_Command_ExportDrawingsToPdf);
        Command_ExportToExcel = new RelayCommand(Handle_Command_ExportToExcel);
    }
    #endregion
}


/// <summary>
/// UTILITY CLASS: Update Entire Project's Kitchen Brand
/// </summary>
public static class Update_ProjectKitchenBrand_Utility
{
    public static string target_project_kitchenBrand { get; set; }

    public static void change_ekKitchenBrand(UIApplication app)
    {
        if (target_project_kitchenBrand == APP.Global_State.Current_Project_State.EKProjectKitchenBrand) return;

        var current_project_state = APP.Global_State.Current_Project_State;
        var current_project_kitchenBrand = APP.Global_State.Current_Project_State.EKProjectKitchenBrand;

        Document doc = app.ActiveUIDocument.Document;

        // Call the function that changes the project parameter

        // Get the Project Information element
        ProjectInfo projectInfo = doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        using (Transaction t = new Transaction(doc, "Change Kitchen Brand"))
        {
            try
            {
                t.Start();
                // 1. Update ProjectInformation parameter
                kitchenBrandParam.Set(target_project_kitchenBrand);

                // 2. Update the Cabinet Families
                update_cabinets(doc, current_project_kitchenBrand, target_project_kitchenBrand);

                // 3. Updat view filter
                update_value_of_view_kitchenbrand_viewfilter(doc, target_project_kitchenBrand);

                t.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ERROR", $"Failed to change Kitchen Brand {ex.Message}");
                t.RollBack();
                return;
            }
        }

        // 4. Update the global state
        APP.Global_State.Current_Project_State.EKProjectKitchenBrand = target_project_kitchenBrand;
    }

    public static void update_value_of_view_kitchenbrand_viewfilter(Document doc, string brandName) // Happens with in a transaction
    {
        // Define the filter name
        string filterName = "Vendor Name Filter";

        // Get Active View
        Autodesk.Revit.DB.View view = doc.ActiveView;

        // Create a list of categories that will be used for the filter
        IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Casework) };

        // Get the ElementId for the "Manufacturer" built-in parameter
        ElementId manufacturerParamId = new ElementId(BuiltInParameter.ALL_MODEL_MANUFACTURER);

        // Create the filter based on the 
        // Create a filter rule where "Manufacturer" equals "MY VALUE" (case-insensitive)
        FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateNotEqualsRule(manufacturerParamId, brandName);

        // Create an ElementParameterFilter with the rule
        ElementParameterFilter parameterFilter = new ElementParameterFilter(manufacturerRule);

        // Attempt to find an existing filter with the same name
        ParameterFilterElement paramFilter = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

        if (paramFilter == null)
        {
            TaskDialog.Show("ERROR", "There is no existing View Filter to modify");
            return;
        }

        // Update the existing filter's element filter
        paramFilter.SetElementFilter(parameterFilter);
    }

    private static void update_cabinets(Document doc, string current_kitchen_brand_name, string new_Kitchten_brand_name)
    {
        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        List<FamilyInstance> cabinets = FilterAllCabinets.FilterProjectForEagleCabinets(doc);

        var x = APP.Global_State.Current_Project_State.EKCaseworkSymbols;

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<CabinetFamilyInstance> cabinetFaimlyInstances = BrandMapper.ConvertFamilyInstaceIntoCabinetFamilyInstance(cabinets);

        // Create a StringBuilder to store the information to be displayed
        StringBuilder resultMessage = new StringBuilder();
        int match_found_count = 0, match_not_found_count = 0;
        resultMessage.Append($"Total Cabinet Instances in Project: {cabinets.Count()}\n");
        //resultMessage.Append($"Match Found:{match_found_count}   Match NOT-Found: {match_not_found_count}");

        StringBuilder result_Success_Message = new StringBuilder();
        StringBuilder result_Failed_Message = new StringBuilder();


        // Create an instance of BrandMapper
        BrandMapper mapper = new BrandMapper();

        Debug.WriteLine("Check all the cabinetFamilyInstances");

        // Iterate over the cabinet family instances and map the SKUs
        foreach (var cabinetFamilyInstance in cabinetFaimlyInstances)
        {
            // Use BrandMapper to map the SKU to the target brand
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, new_Kitchten_brand_name);

            if (!string.IsNullOrEmpty(mappedSKU))
            {
                match_found_count += 1;
                // Append the mapping information to the StringBuilder
                //resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => {new_Kitchten_brand_name}-{mappedSKU}");
                result_Success_Message.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => {new_Kitchten_brand_name}-{mappedSKU}");
            }
            else
            {
                match_not_found_count += 1;
                // If no mapping found, add a message
                //resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => XXX ");
                result_Failed_Message.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => XXX ");
            }
        }
        // Insert the totals at the top of the StringBuilder
        //resultMessage.Insert(1, $"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append($"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append(result_Success_Message);
        resultMessage.Append(result_Failed_Message);

        // Display the information using TaskDialog
        TaskDialog.Show("SKU MAPPING RESULTS", resultMessage.ToString());

        // TODO 1:
        // Map all current family-types to valid new valid-family types
        var cabinetInstancesWithTargetFamilyType = new List<Tuple<CabinetFamilyInstance, string, string>>();

        // Mapping of brand names to their respective brand codes
        Dictionary<string, string> brandCodeMapping = new Dictionary<string, string>
        {
            { "Aristokraft", "Aristokraft" },
            { "Yorktowne Classic", "YTC" },
            { "Yorktowne Historic", "YTH" },
            { "Eclipse", "Eclipse" }
        };

        foreach (var cabinetFamilyInstance in cabinetFaimlyInstances)
        {
            // Use BrandMapper to find the target SKU in the new brand
            // the value we get is the target family's type name
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, new_Kitchten_brand_name);

            // If a valid SKU is found in the new brand, add the mapping to the list
            if (!string.IsNullOrEmpty(mappedSKU))
            {
                // Create the target family name
                // All the family names are in this order : "<Brand Code>-<something>-<something>"
                // And here are the brand codes: 
                //{ "Aristokraft", "Aristokraft" },
                //{ "Yorktowne Classic", "YTC" },
                //{ "Yorktowne Historic", "YTH" },
                //{ "Eclipse", "Eclipse" }
                // so if a family name is: "YTH-B-One Door One Drawer" and the newKitchenBrandValue = Yorktowne Classic
                // then the targetFamilyName should be "YTC-B-One Door One Drawer"
                // where as if the newKitchenBrandValue = Aristokraft then the targetFamilyName should be "Aristokraft-B-One Door One Drawer"
                // Split the current family name by the first hyphen to get the components

                string targetFamilyName = "";
                string[] familyNameParts = cabinetFamilyInstance.FamilyInstance.Symbol.Family.Name.Split(new[] { '-' }, 2);
                // Get the target brand code based on the new brand name
                if (brandCodeMapping.TryGetValue(new_Kitchten_brand_name, out string targetBrandCode))
                {
                    // Construct the new target family name
                    targetFamilyName = $"{targetBrandCode}-{familyNameParts[1]}";
                }
                string targetTypeName = mappedSKU;


                // Add the pair (CabinetFamilyInstance, new family name, new type name) to the list
                cabinetInstancesWithTargetFamilyType.Add(new Tuple<CabinetFamilyInstance, string, string>(
                    cabinetFamilyInstance,
                    targetFamilyName,   // New brand name
                    mappedSKU               // Mapped SKU/type name for the new brand
                ));
            }
        }


        // TODO 2:
        // Convert all valid FamilyTypes
        ChangeFamilyTypes(doc, cabinetInstancesWithTargetFamilyType);
    }

    private static void ChangeFamilyTypes(Document doc, List<Tuple<CabinetFamilyInstance, string, string>> cabinetInstancesWithTargetFamilyType)
    {
        // StringBuilder to collect error messages
        StringBuilder errorMessages = new StringBuilder();

        // Loop through the list of cabinet instances and target family/type pairs
        foreach (var item in cabinetInstancesWithTargetFamilyType)
        {
            CabinetFamilyInstance currentInstance = item.Item1;
            string targetFamilyName = item.Item2;
            string targetTypeName = item.Item3;

            // Get the family instance to modify
            FamilyInstance familyInstance = currentInstance.FamilyInstance;

            // Find the target family symbol (family type) using family and type names
            FamilySymbol targetFamilySymbol = FindFamilySymbol(doc, targetFamilyName, targetTypeName);
            if (targetFamilySymbol == null)
            {
                // Append error message to the StringBuilder
                errorMessages.AppendLine($"Family or type not found for {targetFamilyName} - {targetTypeName}");
                continue;
            }

            // Ensure that the target family symbol is loaded and active in the project
            if (!targetFamilySymbol.IsActive)
            {
                targetFamilySymbol.Activate();
                // Document.Regenerate might be needed after activation to ensure the change takes effect
                currentInstance.FamilyInstance.Document.Regenerate();
            }

            // Change the family type for the family instance
            currentInstance.FamilyInstance.Symbol = targetFamilySymbol;
        }

        // Show the task dialog with all error messages if there are any
        if (errorMessages.Length > 0)
        {
            TaskDialog.Show("Errors", errorMessages.ToString());
        }
    }

    // Helper method to find the family symbol (family type) by family name and type name
    private static FamilySymbol FindFamilySymbol(Document doc, string familyName, string typeName)
    {
        // Iterate through all the families in the document
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(Autodesk.Revit.DB.Family));

        foreach (Autodesk.Revit.DB.Family family in collector)
        {
            if (family.Name == familyName)
            {
                foreach (ElementId symbolId in family.GetFamilySymbolIds())
                {
                    FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;
                    if (symbol.Name == typeName)
                    {
                        return symbol;
                    }
                }
            }
        }
        return null; // Return null if no matching family symbol is found
    }

}

/// <summary>
/// UTILITY CLASS: Update Entire Project's STYLE 
/// </summary>
public static class Update_Project_Style_Finish_Utility
{

    public static void change_style_finish(UIApplication app)
    {
        Document doc = app.ActiveUIDocument.Document;

        // Get all cabinet instances
        List<FamilyInstance> cabinet_instances = FilterAllCabinets.FilterProjectForEagleCabinets(doc);

        Vendor_Style_With_Id chosen_vendor_style = EK24ProjectProperties_ViewModel.ChosenVendorStyle;

        /*
            "Aristokraft        - Sinclair"
            "Aristokraft        - Benton"
            "Aristokraft        - Brellin"
            "Aristokraft        - Winstead"
            "Yorktowne Classic  - Henning"
            "Yorktowne Classic  - Stillwater"
            "Yorktowne Classic  - Fillmore"
            "Eclipse            - Metropolitan"
            "Yorktowne Historic - Corsica"
            "Yorktowne Historic - Langdon"
        */

        using (Transaction trans = new Transaction(doc, "Update Vendor-Style & Vendor-Finish"))
        {
            trans.Start();

            int count = 0;
            // go over each instance and set the Style & Finish parameters
            foreach (var family_instance in cabinet_instances)
            {
                count++;
                //bool paramUpdateResult = UpdateStyleParam(doc, family_instance, "Yorktowne Classic - Fillmore");
                //bool paramUpdateResult = UpdateStyleParam(doc, family_instance, EK24ProjectProperties_ViewModel.ChosenVendorStyle.Revit_ElementId);

                Parameter current_vendor_style_param = family_instance.LookupParameter("Vendor_Style");

                // Determine the Element Id based on the chosen vendor style name
                string selected_style_name = chosen_vendor_style.Vendor_Style_Name;
                ElementId vendor_style_id = Get_VendorStyle_ElementId(doc, family_instance, selected_style_name);
                if (vendor_style_id == null)
                {
                    trans.RollBack();
                    TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    return;
                }
                bool paramUpdateResult = current_vendor_style_param.Set(vendor_style_id);
                if (paramUpdateResult == false)
                {
                    trans.RollBack();
                    TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    return;
                }
            }

            // TEST instance
            // ElementId test_id = new ElementId(9560558);
            //ElementId test_id = new ElementId(9560565);
            //FamilyInstance test_instance = doc.GetElement(test_id) as FamilyInstance;
            //UpdateStyleParam(doc, test_instance, "");
            //UpdateFinishParam(test_instance);
            trans.Commit();
        }

        TaskDialog.Show("SUCCESS", "UPDATED THE VENDOR-STYLE PARAM");
        return;
    }

    public static ElementId Get_VendorStyle_ElementId(Document doc, FamilyInstance familyInstance, string selected_vendor_style)
    {
        Debug.WriteLine("hi");

        // Chosen Vendor-Style & Vendor-Finish
        // string selected_vendor_finish = EK24Modify_ViewModel.SelectedVendorFinish;

        // By this point, we know all family-instances belong to same brand

        // TESTING, with just 1 item for now

        // Get the Parameters
        Parameter vendor_style_param = familyInstance.LookupParameter("Vendor_Style");

        ////// API: go to Autodesk.DB.NestedFamilyTypeReference Class, and there is the following note that expalins that these types can't be filtered like normal filtering. The note says:
        ///// ** These elements are very low-level and thus bypassed by standard element filters. However, 
        /////  it is possible to obtain a set of applicable elements of this class for a FamilyType parameter of a family by calling [!:Autodesk::Revit::DB::Family::GetFamilyTypeParameterValues] **
        /////  Revitapi > Family Class > Family Methods > GetFamilyTypeParameterValues method
        ////// API: go to Revitapi > Family Class > Family Methods > GetFamilyTypeParameterValues method

        // Some error handling fn's just to be sure
        if (!Category.IsBuiltInCategory(vendor_style_param.Definition.GetDataType()))
        {
            TaskDialog.Show("ERROR", "Vendor_Style's data type is not a built in category");
            return null;
        }

        // This the main working api, that will give the allowed values (of type ElementId) that can be set to the parameter
        // Though our parameter is an instance parameter, we have to still look at Symbol.Family as if it was a type parameter, because
        // the `GetFamilyTypeParameterValues` method is definded under 'Family', and 'Family' can be accessed from a family-instance through
        // it's `Symbol`
        ISet<ElementId> all_possible_familytype_parameter_values_set = familyInstance.Symbol.Family.GetFamilyTypeParameterValues(vendor_style_param.Id);

        if (all_possible_familytype_parameter_values_set.Count == 0)
        {
            TaskDialog.Show("ERROR", "One of the chosen istance doesnt have Vendor-Style param or Vendor-Finish Param");
            return null;
        }

        Dictionary<string, ElementId> map_name_eid = new Dictionary<string, ElementId>();

        HashSet<string> names = [];
        foreach (var eid in all_possible_familytype_parameter_values_set)
        {
            Element e = doc.GetElement(eid);
            names.Add(e.Name);
            map_name_eid[e.Name] = eid;
        }
        if (names.Count <= 0)
        {
            TaskDialog.Show("ERROR", "No Vendor-Style nested families found");
            return null;
        }

        // The CHOSEN STYLE & it's ID by the USER
        //string chosen_vendory_style = EK24ProjectProperties_ViewModel.SelectedVendorStyle;
        ///////////////////////// TODO: FOR NOW, setting this manually

        Debug.WriteLine("now update in trasaction");

        // TODO, for now deal with just style, later create a string in this format "style_name:style_finish"
        // then split this string based on ':'

        ElementId chosen_vendorstyle_eid = map_name_eid[selected_vendor_style];
        return chosen_vendorstyle_eid;

        /*
        Debug.WriteLine("now update in trasaction");

        bool updatedParamResult = false;


        //foreach (var familyInstance in current_selected_familyInstances)
        //{
        //    Parameter current_vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
        //    // Finaly: set with the 'ElementId' since that is the storage type of this param, and setting with string won't work
        //    updatedParamResult = current_vendor_style_param.Set(chosen_vendorstyle_eid);
        //    if (updatedParamResult == false)
        //    {
        //        trans.RollBack();
        //        TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
        //        return;
        //    }
        //}
        Parameter current_vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
        // Finaly: set with the 'ElementId' since that is the storage type of this param, and setting with string won't work
        updatedParamResult = current_vendor_style_param.Set(chosen_vendorstyle_eid);
        return updatedParamResult;
        */
    }

    public static void UpdateFinishParam(FamilyInstance familyInstance)
    {
        Debug.WriteLine("hi");
    }
}

/// <summary>
/// UTILITY CLASS: EXPORT DRAWINGS TO PDF
/// </summary>
public static class Export_Drawings_To_PDF_Utility
{
    public static void PrintDocument(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Get the print manager from the document
        PrintManager printManager = doc.PrintManager;

        printManager.PrintRange = PrintRange.Select;

        // Target print settings
        string PrintSettingName = ManageViewModel.PrintSettingName;
        string ViewSheetSetName = ManageViewModel.ViewSheetSetName;

        // Apply the print setting by name
        PrintSetting printSetting = GetPrintSettingByName(doc, PrintSettingName);
        if (printSetting != null)
        {
            printManager.PrintSetup.CurrentPrintSetting = printSetting;
        }
        else
        {
            TaskDialog.Show("Couldn't Print", $"Print setting with the name '{PrintSettingName}' not found");
            return;
        }

        // Get the ViewSheetSet by name (a set of views/sheets to print)
        ViewSheetSet viewSheetSet = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheetSet))
            .Cast<ViewSheetSet>()
            .FirstOrDefault(vss => vss.Name == ViewSheetSetName);

        if (viewSheetSet == null)
        {
            TaskDialog.Show("Error", "ViewSheetSet named '" + ViewSheetSetName + "' not found.");
            return;
        }

        // Set the viewsheet set to print
        printManager.ViewSheetSetting.CurrentViewSheetSet = viewSheetSet;   // FIX ME: for some reason this line is doing the relay command infinity loop
                                                                            // i.e., execution never is reaching after this line
        printManager.CombinedFile = true;

        // Execute the print command
        printManager.SubmitPrint();

        // Make sure to save the current print setting to apply changes
        printManager.PrintSetup.Save();
    }

    public static async Task PrintDocumentAsync(UIApplication app)
    {
        await Task.Run(() =>
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            PrintManager printManager = doc.PrintManager;
            printManager.PrintRange = PrintRange.Select;

            string PrintSettingName = ManageViewModel.PrintSettingName;
            string ViewSheetSetName = ManageViewModel.ViewSheetSetName;

            PrintSetting printSetting = GetPrintSettingByName(doc, PrintSettingName);
            if (printSetting != null)
            {
                printManager.PrintSetup.CurrentPrintSetting = printSetting;
            }
            else
            {
                TaskDialog.Show("Couldn't Print", $"Print setting with the name '{PrintSettingName}' not found");
                return;
            }

            ViewSheetSet viewSheetSet = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheetSet))
                .Cast<ViewSheetSet>()
                .FirstOrDefault(vss => vss.Name == ViewSheetSetName);

            if (viewSheetSet == null)
            {
                TaskDialog.Show("Error", "ViewSheetSet named '" + ViewSheetSetName + "' not found.");
                return;
            }

            printManager.ViewSheetSetting.CurrentViewSheetSet = viewSheetSet;
            printManager.CombinedFile = true;

            printManager.SubmitPrint();
            printManager.PrintSetup.Save();
        });
    }

    public static PrintSetting GetPrintSettingByName(Document doc, string targetName)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(PrintSetting));

        foreach (PrintSetting ps in collector.ToElements())
        {
            if (ps.Name == targetName)
            {
                return ps;
            }
        }
        return null; // Return null if no matching print setting is found
    }

}


/// <summary>
/// UTILITY CLASS: EXPORT TO EXCEL
/// </summary>
public static class Export_To_Excel_Utility
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
