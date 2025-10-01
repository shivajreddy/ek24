using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.Commands.Utils;
using ek24.Dtos;
using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;
using ek24.UI.ViewModels.ChangeBrand;
using ek24.UI.ViewModels.Manage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ek24.UI;

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
    // BRAND SECTION RELATED
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

    private string _ekProjectKitchBrand;
    public string EKProjectKitchenBrand
    {
        get
        {
            if (APP.Global_State.Current_Project_State == null || APP.Global_State.Current_Project_State.EKProjectKitchenBrand == null) return "";
            filter_vendor_styles();
            return APP.Global_State.Current_Project_State.EKProjectKitchenBrand;
        }
        set
        {
            APP.Global_State.Current_Project_State.EKProjectKitchenBrand = value;
            //filter_vendor_styles();
        }
    }

    private static string _ekProjectKitchenStyleFinish;
    public static string EKProjectKitchenStyleFinish
    {
        get => _ekProjectKitchenStyleFinish;
        set
        {
            if (_ekProjectKitchenStyleFinish == value) return;
            _ekProjectKitchenStyleFinish = value;
            OnStaticPropertyChanged(nameof(EKProjectKitchenStyleFinish));
        }
    }

    // STYLE&FINISH SECTION RELATED
    /*
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
    */
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
    // Dictionary to hold vendor styles and their corresponding finishes
    private Dictionary<string, List<string>> hardcoded_vendorFinishNames = new Dictionary<string, List<string>>
    {
        { "Aristokraft - Sinclair", new List<string> { "Birch" } },
        { "Aristokraft - Benton", new List<string> { "Birch" } },
        { "Aristokraft - Brellin", new List<string> { "Purestyle" } },
        { "Aristokraft - Winstead", new List<string> { "Birch", "Paint" } },
        { "Yorktowne Classic - Henning", new List<string> { "Maple Stain", "Maple Paint" } },
        { "Yorktowne Classic - Stillwater", new List<string> { "Maple Stain", "Maple Paint" } },
        { "Yorktowne Classic - Fillmore", new List<string> { "Maple Stain", "Maple Paint" } },
        { "Yorktowne Historic - Corsica", new List<string> { "Maple Stain", "Maple Paint", "Maple Appaloosa" } },
        { "Yorktowne Historic - Langdon", new List<string> { "Maple Stain", "Maple Paint", "Maple Appaloosa" } },
        { "Eclipse - Metropolitan", new List<string> { "Thermally Fused Laminates", "Matte Acrylic", "High Gloss Acrylic" } }
    };
    //private ObservableCollection<Vendor_Style_With_Id> _VendorStyles;
    //public ObservableCollection<Vendor_Style_With_Id> VendorStyles  // Used to bind to Combobox-VENDOR STYLE
    private ObservableCollection<string> _VendorStyles;
    public ObservableCollection<string> VendorStyles  // Used to bind to Combobox-VENDOR STYLE
    {
        get => _VendorStyles;
        set
        {
            if (value == _VendorStyles) return;
            _VendorStyles = value;
            OnPropertyChanged(nameof(VendorStyles));
        }
    }

    //public static Vendor_Style_With_Id ChosenVendorStyle;
    public static string ChosenVendorStyle;
    private string _selectedVendorStyle { get; set; }
    public string SelectedVendorStyle
    {
        get => _selectedVendorStyle;
        set
        {
            if (value == null || value == _selectedVendorStyle) return;
            _selectedVendorStyle = value;
            OnPropertyChanged(nameof(SelectedVendorStyle));
            OnPropertyChanged(nameof(SelectedVendorFinish));

            // Filter VendorFinishes based on the selected vendor style
            if (hardcoded_vendorFinishNames.TryGetValue(value, out var temp_vendor_finishes))
            {
                VendorFinishes = new ObservableCollection<string>(temp_vendor_finishes);
            }
            else
            {
                VendorFinishes = new ObservableCollection<string>(); // No finishes found for the selected style
            }

            ChosenVendorStyle = value;
        }
    }

    private ObservableCollection<string> _VendorFinishes;
    public ObservableCollection<string> VendorFinishes  // Used to bind to Combobox-VENDOR FINISH
    {
        get => _VendorFinishes;
        set
        {
            if (value == _VendorFinishes) return;
            _VendorFinishes = value;
            OnPropertyChanged(nameof(VendorFinishes));
        }
    }

    public static string ChosenVendorFinish;
    private string _selectedVendorFinish { get; set; }
    public string SelectedVendorFinish
    {
        get => _selectedVendorFinish;
        set
        {
            if (_selectedVendorFinish == value) return;
            _selectedVendorFinish = value;
            OnPropertyChanged(nameof(SelectedVendorFinish));
            ChosenVendorFinish = value;
        }
    }

    #endregion

    #region HELPER Function to filter item sources

    // 'EKProjectKitchenBrand' will be set before this fn is called. use the 'EKProjectKitchenBrand' to filter for VendorStyles
    public void filter_vendor_styles()
    {
        string brand_name = APP.Global_State.Current_Project_State.EKProjectKitchenBrand;
        // Use LINQ to filter the list of vendor styles that contain the brand name
        List<string> temp_filteredStyles = hardcoded_vendorStyleNames
            .Where(style => style.Contains(brand_name))
            .ToList();

        VendorStyles = new ObservableCollection<string>(temp_filteredStyles);
    }

    public void filter_vendor_finishes(string chosen_vendor_style)
    {
        if (hardcoded_vendorFinishNames.TryGetValue(chosen_vendor_style, out var temp_finishes))
        {
            VendorFinishes = new ObservableCollection<string>(temp_finishes);
        }
        else
        {
            VendorFinishes = new ObservableCollection<string>(); // No finishes found for the chosen style
        }
    }

    /*
    public void filter_vendor_styles_old()
    {
        // Grab your entire hardcoded list
        List<string> all_names = hardcoded_vendorStyleNames;

        // Check if the user has selected a brand
        //if (string.IsNullOrWhiteSpace(EKProjectKitchenBrand)) return;

        // Get one of the cabinet family instance for the current project, and use that family-instance
        // for getting the VendorStyle param, which in turn gives the Element Id's
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

        //// Set the available styles & finishes that user can apply to the singly selected cabinet
        //VendorStyles = temp_filtered_vendor_styles;

        //List<string> temp_filteredVendorStyles = new List<string>();
        //List<Vendor_Style_With_Id> temp_filtered_vendor_styles = new List<Vendor_Style_With_Id>();

        //// Filter for names that start with "<brand> - "
        //temp_filteredVendorStyles = all_names
        //    .Where(name => name.StartsWith(EKProjectKitchenBrand + " - ", StringComparison.OrdinalIgnoreCase))
        //    .ToList();

        //// If no matches, handle it gracefully (optional)
        //if (temp_filteredVendorStyles.Count == 0)
        //{
        //    TaskDialog.Show("INFO", $"No vendor styles found for brand '{SelectedBrand}'.");
        //}
        //VendorStyleNamesForKitchenBrand = temp_filteredVendorStyles; // set the ppty that triggers UI
    }
    */
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
            // filter_vendor_styles(); // TODO: I just commented this, bcs it runs when opening a project, and actually we don't need it when propect opens
        }
    }
    #endregion

    #region Relay Commands
    public ICommand Command_UpdateKitchenBrand { get; }
    public void Handle_Command_UpdateKitchenBrand()
    {
        Update_ProjectKitchenBrand_Utility.target_project_kitchenBrand = SelectedBrand;

        Debug.WriteLine("UPDATE KITCHEN BRAND");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_UpdateKitchenBrand;
        APP.ExternalEvent?.Raise();

    }
    public ICommand Command_UpdateVendorStyleFinish { get; }
    public void Handle_Command_UpdateVendorStyleFinish()
    {
        Debug.WriteLine("UPDATE STYLE & FINISH");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_UpdateStyleAndFinish;        // BUG: Sometimes this fails to invoke the Request Handler why?
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
    //public void Handle_Command_ExportToExcel()
    //{
    //    Debug.WriteLine("Export Drawings to PDF");
    //    APP.RequestHandler.RequestType = RequestType.ProjectProperties_ExportToExcel;
    //    APP.ExternalEvent?.Raise();     // invokes ExportToExcel.HandleExportToExcelButtonClick(app);
    //}
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
        //Command_ExportToExcel = new RelayCommand(Handle_Command_ExportToExcel);
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
        //if (target_project_kitchenBrand == APP.Global_State.Current_Project_State.EKProjectKitchenBrand) return;

        //var current_project_state = APP.Global_State.Current_Project_State;
        //var current_project_kitchenBrand = APP.Global_State.Current_Project_State.EKProjectKitchenBrand;
        Document doc = app.ActiveUIDocument.Document;

        // Get the Project Information element
        ProjectInfo projectInfo = doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");
        if (kitchenBrandParam == null) return;
        var current_project_kitchenBrand = kitchenBrandParam.AsValueString();
        if (current_project_kitchenBrand == "" || current_project_kitchenBrand == target_project_kitchenBrand) return;

        using (Transaction t = new Transaction(doc, "Change Kitchen Brand"))
        {
            try
            {
                t.Start();

                // 2. Update the Cabinet Families
                //update_cabinets(doc, current_project_kitchenBrand, target_project_kitchenBrand);
                // 3. Update the Casework Families
                update_casework_instances(doc, current_project_kitchenBrand, target_project_kitchenBrand);

                // 1. Update ProjectInformation parameter
                // ideally would do this after changing the instances, but have to rework on the below
                // cabinet&casework changing functions to not use the current project kitchenbrand and use 
                // the name from memory. so this is a todo.
                kitchenBrandParam.Set(target_project_kitchenBrand); // update in revit doc
                APP.Global_State.Current_Project_State.EKProjectKitchenBrand = target_project_kitchenBrand; // update in ek24 app's state

                // 4. Updat view filter
                update_value_of_view_kitchenbrand_viewfilter(doc, target_project_kitchenBrand);

                t.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ERROR", $"Failed to change Kitchen Brand\n Detial:{ex.Message}");
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

    private static void update_casework_instances(Document doc, string current_kitchen_brand_name, string new_Kitchten_brand_name)
    {
        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        /// Filter for all the eagle cabinetry instances, and only the casework of current kitchen brand
        /// DESCRIPTION: say if current brand is YTC, and there are casework instances of Aristokraft, when user is changing the brand to 
        /// either Aristocraft of YTH, so the user wouldn't expect the Aristokraft instances also to change to the new brand
        //List<FamilyInstance> eagle_casework = FilterEagleCabinetry.FilterProjectForEagleCasework(doc); // this would get all casework
        List<FamilyInstance> eagle_casework = FilterEagleCabinetry.FilterProjectForEagleCasework(doc, current_kitchen_brand_name);

        // All Eagle Kitchen cabinetry Family symbols in this doc
        var eagle_casework_symbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols;

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<EagleCaseworkFamilyInstance> eagleCaseworkInstances = BrandMapper.ConvertFamilyInstaceToEagleCaseworkFamilyInstance(eagle_casework);

        // Create a StringBuilder to display the matching results
        StringBuilder resultMessage = new StringBuilder();
        int match_found_count = 0, match_not_found_count = 0;
        resultMessage.Append($"Total Casework Instances in Project: {eagle_casework.Count()}\n");
        //resultMessage.Append($"Match Found:{match_found_count}   Match NOT-Found: {match_not_found_count}");
        StringBuilder result_Success_Message = new StringBuilder();
        StringBuilder result_Failed_Message = new StringBuilder();

        // Create an instance of BrandMapper
        BrandMapper mapper = new BrandMapper();

        Debug.WriteLine("Going to check all the cabinetFamilyInstances");

        // Map all current family-types to valid new valid-family types
        var cabinetInstancesWithTargetFamilyType = new List<Tuple<EagleCaseworkFamilyInstance, ElementId>>();

        // MATCH CRITERIA 1: Find the target SKU in the matrix
        // MATCH CRITERIA 2: Find a type with same 'Vendor_SKU' value as current symbol
        foreach (var eagleCaseworkInst in eagleCaseworkInstances)
        {
            bool found_criteria_1 = false;
            // MATHC CRITERIA 1
            {
                var res = BrandMapper.FindTargetFamilySymbolBrandType(doc, eagleCaseworkInst.BrandName, eagleCaseworkInst.TypeName, new_Kitchten_brand_name);
                ElementId familySymbolId = res.Item1;
                string mappedSKU = res.Item2;
                if (familySymbolId != null)
                {
                    cabinetInstancesWithTargetFamilyType.Add(new Tuple<EagleCaseworkFamilyInstance, ElementId>(
                        eagleCaseworkInst,
                        familySymbolId
                        ));

                    match_found_count += 1;
                    // Append the mapping information to the StringBuilder
                    result_Success_Message.AppendLine($"{eagleCaseworkInst.BrandName}-{eagleCaseworkInst.TypeName} => {new_Kitchten_brand_name}-{mappedSKU}");
                    found_criteria_1 = true; // mark this as true
                }
            }
            // MATHC CRITERIA 2
            bool found_creitier_2 = false;
            if (!found_criteria_1)
            {
                // Get the Vendor_SKU for the current eagleCaseworkInst item, skip if there is no param-value.
                // and find a FamilySymbol of targetbrand and same param-value
                var vendor_sku_param = eagleCaseworkInst.FamilyInstance.Symbol?.LookupParameter("Vendor_SKU");
                string vendor_sku_param_val = vendor_sku_param?.HasValue == true ? vendor_sku_param.AsString() ?? vendor_sku_param.AsValueString() ?? string.Empty : string.Empty;
                if (vendor_sku_param_val == "" || vendor_sku_param_val == string.Empty) continue;
                foreach (var eagle_casework_symbol in eagle_casework_symbols)
                {
                    if (eagle_casework_symbol.EKBrand == target_project_kitchenBrand && eagle_casework_symbol.EKSKU.VendorSKU == vendor_sku_param_val)
                    {
                        cabinetInstancesWithTargetFamilyType.Add(new Tuple<EagleCaseworkFamilyInstance, ElementId>(
                            eagleCaseworkInst,
                            eagle_casework_symbol.RevitFamilySymbolId
                            ));

                        match_found_count += 1;
                        // Append the mapping information to the StringBuilder
                        result_Success_Message.AppendLine($"{eagleCaseworkInst.BrandName}-{eagleCaseworkInst.TypeName} => {new_Kitchten_brand_name}-{vendor_sku_param_val}");

                        found_creitier_2 = true;
                    }
                }
            }
            if (!found_criteria_1 & !found_creitier_2)
            {
                match_not_found_count += 1;
                // If no mapping found, add a message
                result_Failed_Message.AppendLine($"{eagleCaseworkInst.BrandName}-{eagleCaseworkInst.TypeName} => XXX ");
            }
        }

        /// SHOW THE RESULTS TO THE USER BEFORE PERFORMING THE ACTUAL CHANGE SYMBOL OPERATION
        // Insert the totals at the top of the StringBuilder
        //resultMessage.Insert(1, $"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append($"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append(result_Success_Message);
        resultMessage.Append(result_Failed_Message);
        // Display the information using TaskDialog
        TaskDialog.Show("SKU MAPPING RESULTS", resultMessage.ToString());

        /// PERFORM THE FAMILY SYMBOL CONVERTION
        // Call the Helper function that changes the family symbols for instances
        Debug.WriteLine("Call the Helper function that changes the family symbols for instances");
        ChangeFamilyInstanceTypes(doc, cabinetInstancesWithTargetFamilyType);
        //ChangeFamilyTypes(doc, cabinetInstancesWithTargetFamilyType);
    }

    private static void update_cabinets(Document doc, string current_kitchen_brand_name, string new_Kitchten_brand_name)
    {
        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        List<FamilyInstance> eagle_cabinets = FilterEagleCabinetry.FilterProjectForEagleCabinets(doc);

        // All Eagle Kitchen cabinetry Family symbols in this doc
        var eagle_casework_symbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols;

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<EagleCaseworkFamilyInstance> cabinetFaimlyInstances = BrandMapper.ConvertFamilyInstaceToEagleCaseworkFamilyInstance(eagle_cabinets);

        // Create a StringBuilder to store the information to be displayed
        StringBuilder resultMessage = new StringBuilder();
        int match_found_count = 0, match_not_found_count = 0;
        resultMessage.Append($"Total Cabinet Instances in Project: {eagle_cabinets.Count()}\n");
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
        var cabinetInstancesWithTargetFamilyType = new List<Tuple<EagleCaseworkFamilyInstance, string, string>>();

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
                cabinetInstancesWithTargetFamilyType.Add(new Tuple<EagleCaseworkFamilyInstance, string, string>(
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

    private static void ChangeFamilyTypes(Document doc, List<Tuple<EagleCaseworkFamilyInstance, string, string>> cabinetInstancesWithTargetFamilyType)
    {
        // StringBuilder to collect error messages
        StringBuilder errorMessages = new StringBuilder();

        // Loop through the list of cabinet instances and target family/type pairs
        foreach (var item in cabinetInstancesWithTargetFamilyType)
        {
            EagleCaseworkFamilyInstance currentInstance = item.Item1;
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

    // Takes in family instances, and Id's of target FamilySymbols that each family instance should be changed to
    private static void ChangeFamilyInstanceTypes(Document doc, List<Tuple<EagleCaseworkFamilyInstance, ElementId>> pairs)
    {
        // StringBuilder to collect error messages
        StringBuilder errorMessages = new StringBuilder();

        foreach (var pair in pairs)
        {
            try
            {
                var familyInstance = pair.Item1;    // change this family instance
                var familySymbolId = pair.Item2;
                FamilySymbol targetFamilySymbol = doc.GetElement(familySymbolId) as FamilySymbol; // into this family symbol

                if (targetFamilySymbol == null)
                {
                    errorMessages.AppendLine($"Could not find FamilySymbol with Id {familySymbolId}.");
                    continue;
                }

                // Ensure that the target family symbol is loaded and active in the project
                if (!targetFamilySymbol.IsActive)
                {
                    targetFamilySymbol.Activate();
                    doc.Regenerate(); // safer than using familyInstance.FamilyInstance.Document
                }

                // Change the family type for the family instance
                familyInstance.FamilyInstance.Symbol = targetFamilySymbol;
            }
            catch (Exception ex)
            {
                // Collect detailed error info
                errorMessages.AppendLine($"Error changing type for instance {pair.Item1?.FamilyInstance?.Id}: {ex.Message}");
            }
        }

        // Show the task dialog with all error messages if there are any
        if (errorMessages.Length > 0) TaskDialog.Show("Errors", errorMessages.ToString());
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
        List<FamilyInstance> cabinet_instances = FilterEagleCabinetry.FilterProjectForEagleCabinets(doc);

        //Vendor_Style_With_Id chosen_vendor_style = EK24ProjectProperties_ViewModel.ChosenVendorStyle;
        string chosen_vendor_style = EK24ProjectProperties_ViewModel.ChosenVendorStyle;
        string chosen_vendor_finish = EK24ProjectProperties_ViewModel.ChosenVendorFinish;

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
                count++; // should equal to the length of cabinet_instances
                //bool paramUpdateResult = UpdateStyleParam(doc, family_instance, "Yorktowne Classic - Fillmore");
                //bool paramUpdateResult = UpdateStyleParam(doc, family_instance, EK24ProjectProperties_ViewModel.ChosenVendorStyle.Revit_ElementId);

                // :: Set Vendor_Style :: 
                Parameter current_vendor_style_param = family_instance.LookupParameter("Vendor_Style");

                // Determine the Element Id based on the chosen vendor style name
                //string selected_style_name = chosen_vendor_style.Vendor_Style_Name;
                string selected_style_name = chosen_vendor_style;
                //BUG: The following function might be causing this function to stop working, because the execution is not reaching outside the foreach loop
                // It works sometimes, but sometimes it doesn't
                ElementId vendor_style_id = Get_ElementId_Of_VendorStyle(doc, family_instance, selected_style_name);
                Debug.WriteLine("hi");
                if (vendor_style_id == null)
                {
                    continue;
                    //TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    //trans.RollBack();
                    //return;
                }
                bool paramUpdateResult = current_vendor_style_param.Set(vendor_style_id);
                if (paramUpdateResult == false)
                {
                    TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    trans.RollBack();
                    return;
                }

                // :: Set Material Finish :: 
                // Get the material
                //string target_material_name = "Yorktowne Classic - Stillwater-Maple Stain";
                string target_material_name = EK24ProjectProperties_ViewModel.ChosenVendorFinish;
                Material newMaterial = new FilteredElementCollector(doc)
                    .OfClass(typeof(Material))
                    .Cast<Material>()
                    .FirstOrDefault(m => m.Name == target_material_name);
                if (newMaterial == null)
                {
                    TaskDialog.Show("Error", $"Material: {target_material_name} not found.");
                    return;
                }

                // Find material parameter
                Parameter matParam = family_instance
                    .Parameters
                    .Cast<Parameter>()
                    .FirstOrDefault(p => p.Definition.Name == "Vendor_Finish");

                if (matParam != null && matParam.StorageType == StorageType.ElementId)
                {
                    matParam.Set(newMaterial.Id);
                }
                else
                {
                    TaskDialog.Show("Error", "No material parameter found.");
                    trans.RollBack();
                    return;
                }
            }
            Debug.WriteLine("end loop");

            // TEST instance
            // ElementId test_id = new ElementId(9560558);
            //ElementId test_id = new ElementId(9560565);
            //FamilyInstance test_instance = doc.GetElement(test_id) as FamilyInstance;
            //UpdateStyleParam(doc, test_instance, "");
            //UpdateFinishParam(test_instance);
            trans.Commit();
        }


        // Project Param
        // Get the Project Information element
        ProjectInfo projectInfo = doc.ProjectInformation;
        Parameter kitchenStyleParam = projectInfo.LookupParameter("KitchenStyle");

        if (kitchenStyleParam == null)
        {
            TaskDialog.Show("ERROR", "Project Parameter with name - 'KitchenStyle' not found");
            return;
        }

        Parameter kitchenFinishParam = projectInfo.LookupParameter("KitchenFinish");
        if (kitchenFinishParam == null)
        {
            TaskDialog.Show("ERROR", "Project Parameter with name - 'KitchenStyle' not found");
            return;
        }

        using (Transaction t = new Transaction(doc, "Set Kitchen Brand & Kitch Style Project Params"))
        {
            try
            {
                t.Start();
                // Update ProjectInformation parameters
                // 1. Update 'KitchenStyle' Parameter
                kitchenStyleParam.Set(chosen_vendor_style);

                // 2. Update 'KitchenFinish' Parameter
                Material newMaterial = new FilteredElementCollector(doc)
                    .OfClass(typeof(Material))
                    .Cast<Material>()
                    .FirstOrDefault(m => m.Name == chosen_vendor_finish);
                if (newMaterial == null)
                {
                    TaskDialog.Show("Error", $"Material: {chosen_vendor_finish} not found.");
                    return;
                }
                if (kitchenFinishParam != null && kitchenFinishParam.StorageType == StorageType.ElementId)
                {
                    kitchenFinishParam.Set(newMaterial.Id);
                }
                t.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ERROR", $"Failed to change Project Param 'KitchenStyle' & 'KitchenFinish' {ex.Message}");
                t.RollBack();
                return;
            }
        }

        // Update the global state
        APP.Global_State.Current_Project_State.EKProjectKitchenStyle = chosen_vendor_style;
        APP.Global_State.Current_Project_State.EKProjectKitchenFinish = chosen_vendor_finish;
        EK24ProjectProperties_ViewModel.EKProjectKitchenStyleFinish = chosen_vendor_style + " - " + chosen_vendor_finish;

        string title = "UPDATED Kitchen-Style & Kitchen-Finish";
        string message = $"KitchenStyle: {chosen_vendor_style}\nKitchenFinish: {chosen_vendor_finish}\n";
        TaskDialog.Show(title, message);
    }

    public static ElementId Get_ElementId_Of_VendorStyle(Document doc, FamilyInstance familyInstance, string selected_vendor_style)
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
        if (vendor_style_param == null) return null;

        // Some error handling fn's just to be sure
        if (!Category.IsBuiltInCategory(vendor_style_param.Definition.GetDataType()))
        {
            //TaskDialog.Show("ERROR", "Vendor_Style's data type is not a built in category");
            Debug.WriteLine("ERROR Vendor_Style's data type is not a built in category");
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

