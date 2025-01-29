using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Commands.Utils;
using ek24.Dtos;
using ek24.RequestHandling;
using ek24.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;


namespace ek24.UI;

public class EK24Modify_ViewModel : INotifyPropertyChanged
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

    #region PPTIES/VALUES FOR COMBO BOXES
    public List<string> BrandItems { get; } = EKBrands.all_brand_names;
    public List<string> _ekTypeItems { get; set; } = [];
    public List<string> EKTypeItems
    {
        get => _ekTypeItems;
        set
        {
            if (_ekTypeItems == value) return;
            _ekTypeItems = value;
            OnPropertyChanged(nameof(EKTypeItems));
        }
    }
    public List<string> _ekCategoryItems { get; set; }
    public List<string> EKCategoryItems
    {
        get => _ekCategoryItems;
        set
        {
            if (_ekCategoryItems == value) return;
            _ekCategoryItems = value;
            OnPropertyChanged(nameof(EKCategoryItems));
        }
    }
    public List<EK_SKU> _ekSKUItems { get; set; }
    public List<EK_SKU> EKSKUItems
    {
        get => _ekSKUItems;
        set
        {
            if (_ekSKUItems == value) return;
            _ekSKUItems = value;
            OnPropertyChanged(nameof(EKSKUItems));
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
    private List<string> _vendorStyleNames { get; set; }
    public List<string> VendorStyleWithIdItems
    {
        get => _vendorStyleNames;
        set
        {
            if (_vendorStyleNames == value) return;
            _vendorStyleNames = value;
            OnPropertyChanged(nameof(VendorStyleWithIdItems));
        }
    }

    private ObservableCollection<Vendor_Style_With_Id> _filteredVendorStyles;
    public ObservableCollection<Vendor_Style_With_Id> FilteredVendorStyles
    {
        get => _filteredVendorStyles;
        set
        {
            if (value == _filteredVendorStyles) return;
            _filteredVendorStyles = value;
            OnPropertyChanged(nameof(FilteredVendorStyles));
        }
    }

    private List<string> _filtered_vendor_finishes { get; set; } = [];
    public List<string> FilteredVendorFinishes
    {
        get => _filtered_vendor_finishes;
        set
        {
            if (value == _filtered_vendor_finishes) return;
            _filtered_vendor_finishes = value;
            OnPropertyChanged(nameof(FilteredVendorFinishes));
        }
    }

    public class ModificationItem
    {
        public string Name { get; set; }

        public bool IsChecked { get; set; }
    }
    private ObservableCollection<ModificationItem> _availableModifications;
    public ObservableCollection<ModificationItem> AvailableModifications
    {
        get => _availableModifications;
        set
        {
            if (value == _availableModifications) return;
            _availableModifications = value;
            OnPropertyChanged(nameof(AvailableModifications));
        }
    }
    #endregion

    #region HELPER Functions to filter item sources
    public void filter_ekType_items()// based on: SelectedBrand
    {
        List<string> newEktypeItems = new List<string>(); // Reset the Category Items

        //var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads
        var ekCaseworkSymbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols; // Gets created when document loads

        foreach (var ekSymbol in ekCaseworkSymbols)
        {
            if (ekSymbol.EKBrand != SelectedBrand) continue;

            if (ekSymbol.EKType == "" || newEktypeItems.Contains(ekSymbol.EKType)) continue; // no empty or repeated entries
            newEktypeItems.Add(ekSymbol.EKType);
        }
        newEktypeItems.Sort();
        EKTypeItems = newEktypeItems;   // Now it triggers the binding ppty
        return;
    }
    public void filter_ekCategory_items() //based on: SelectedBrand, SelectedCategory
    {
        var temp_EKCategoryItems = new List<string>(); // Reset the Configuration Items

        //var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads
        var ekCaseworkSymbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols; // Gets created when document loads

        // Chosen:: Type
        if (SelectedEKType != null && SelectedEKType != "")
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand
                if (ekSymbol.EKType != SelectedEKType) continue; // Same as Chosen EKType

                if (ekSymbol.EKCategory == "" || temp_EKCategoryItems.Contains(ekSymbol.EKCategory)) continue; // no empty or repeated entries
                temp_EKCategoryItems.Add(ekSymbol.EKCategory);
            }
        }
        // EKTYpe is not chosen or left empty
        else
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand

                if (ekSymbol.EKCategory == "" || temp_EKCategoryItems.Contains(ekSymbol.EKCategory)) continue; // no empty or repeated entries
                temp_EKCategoryItems.Add(ekSymbol.EKCategory);
            }
        }
        temp_EKCategoryItems.Sort();
        EKCategoryItems = temp_EKCategoryItems;   // Now it triggers the binding ppty
        return;
    }
    public void filter_sku_items()  // based on: SelectedBrand, SelectedCategory, SelectedConfiguration
    {
        var temp_EKSKUItems = new List<EK_SKU>(); // Reset the SKU_Items list

        //var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads
        var ekCaseworkSymbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols; // Gets created when document loads

        // Chosen:: Type & Category 
        if (SelectedEKType != null && SelectedEKType != "" && SelectedEKCategory != null && SelectedEKCategory != "")
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand
                if (ekSymbol.EKType != SelectedEKType) continue; // Same as Chosen EKType
                if (ekSymbol.EKCategory != SelectedEKCategory) continue; // Same as Chosen EKCategory

                // Shouldn't need to do the following because all SKU must be unique
                //if (ekSymbol.EKSKU == null || ekSymbol.EKSKU.TypeName == "" || temp_EKSKUItems.Contains(ekSymbol.EKSKU)) continue;

                temp_EKSKUItems.Add(ekSymbol.EKSKU);
            }
        }
        // Chosen:: Type
        else if (SelectedEKType != null && SelectedEKType != "")
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand
                if (ekSymbol.EKType != SelectedEKType) continue; // Same as Chosen EKType

                // Shouldn't need to do the following because all SKU must be unique
                //if (ekSymbol.EKSKU == null || ekSymbol.EKSKU.TypeName == "" || EKSKUItems.Contains(ekSymbol.EKSKU)) continue;

                temp_EKSKUItems.Add(ekSymbol.EKSKU);
            }
        }
        // Chosen:: Category
        else if (SelectedEKCategory != null && SelectedEKCategory != "")
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand
                if (ekSymbol.EKCategory != SelectedEKCategory) continue; // Same as Chosen EKCategory

                // Shouldn't need to do the following because all SKU must be unique
                //if (ekSymbol.EKSKU == null || ekSymbol.EKSKU.TypeName == "" || temp_EKSKUItems.Contains(ekSymbol.EKSKU)) continue;

                temp_EKSKUItems.Add(ekSymbol.EKSKU);
            }
        }
        // Only Brand is Chosen
        else
        {
            foreach (var ekSymbol in ekCaseworkSymbols)
            {
                if (ekSymbol.EKBrand != SelectedBrand) continue; // Same as Chosen brand

                // Shouldn't need to do the following because all SKU must be unique
                //if (ekSymbol.EKSKU == null || ekSymbol.EKSKU.TypeName == "" || temp_EKSKUItems.Contains(ekSymbol.EKSKU)) continue;

                temp_EKSKUItems.Add(ekSymbol.EKSKU);
            }

        }

        // Sort by TypeName, then by VendorNotes
        temp_EKSKUItems = temp_EKSKUItems
            .OrderBy(sku => sku.TypeName)
            .ThenBy(sku => sku.VendorNotes)
            .ToList();
        EKSKUItems = temp_EKSKUItems;   // Now it triggers the binding ppty
        return;
    }

    public void filter_vendor_styles()
    {
        // Grab your entire hardcoded list
        List<string> all_names = hardcoded_vendorStyleNames;

        // Check if the user has selected a brand
        if (string.IsNullOrWhiteSpace(SelectedBrand)) return;

        List<string> temp_filteredVendorStyles = new List<string>();

        // Filter for names that start with "<brand> - "
        temp_filteredVendorStyles = all_names
            .Where(name => name.StartsWith(SelectedBrand + " - ", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // If no matches, handle it gracefully (optional)
        if (temp_filteredVendorStyles.Count == 0)
        {
            TaskDialog.Show("INFO", $"No vendor styles found for brand '{SelectedBrand}'.");
        }
        VendorStyleWithIdItems = temp_filteredVendorStyles; // set the ppty that triggers UI
    }
    #endregion

    #region Chosen Revit Family Symbol & CURRENT SELECTION FROM REVIT UI

    private static FamilySymbol _chosenRevitFamilySymbol { get; set; }
    public static FamilySymbol ChosenRevitFamilySymbol
    {
        get => _chosenRevitFamilySymbol;
        set
        {
            if (_chosenRevitFamilySymbol == value) return;
            _chosenRevitFamilySymbol = value;
            OnStaticPropertyChanged(nameof(EKFamilySymbol));
        }
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
            //OnPropertyChanged(nameof(CanUpdateVendorStyleButton));

            // Reset dependent dropdowns
            //SelectedEKType = null;
            //SelectedEKCategory = null;
            //SelectedSKU = null;

            // Set the Dependent dropdown items
            filter_ekType_items();
            filter_ekCategory_items();
            filter_sku_items();
        }
    }

    private string _selectedEKType { get; set; }
    public string SelectedEKType
    {
        get => _selectedEKType;
        set
        {
            if (_selectedEKType == value) return;
            _selectedEKType = value;

            // Reset dependent dropdowns
            //SelectedEKCategory = null;
            //SelectedSKU = null;

            // Set the Dependent dropdown items
            filter_ekCategory_items();
            filter_sku_items();
            OnPropertyChanged(nameof(SelectedEKType));
            //OnPropertyChanged(nameof(CanUpdateVendorStyleButton));
        }
    }

    private string _selectedEKCategory { get; set; }
    public string SelectedEKCategory
    {
        get => _selectedEKCategory;
        set
        {
            if (_selectedEKCategory == value) return;
            _selectedEKCategory = value;
            OnPropertyChanged(nameof(SelectedEKCategory));
            //OnPropertyChanged(nameof(CanUpdateVendorStyleButton));

            // Reset dependent dropdowns
            //SelectedSKU = null;

            // Set the Dependent dropdown items
            filter_sku_items();
        }
    }

    private EK_SKU _selectedSKU { get; set; }
    public EK_SKU SelectedSKU
    {
        get => _selectedSKU;
        set
        {
            if (_selectedSKU != value)
            {
                _selectedSKU = value;
                OnPropertyChanged(nameof(SelectedSKU));
                OnPropertyChanged(nameof(CanUpdateButton));
                ChosenRevitFamilySymbol = SelectedSKU?.RevitFamilySymbol;
            }
        }
    }

    // This is the static ppty used by command to execute transaction, we have to keep 'SelectedVendorStyle' as instance ppty
    // and that is why we have this 'ChosenVendorStyle' as static ppty
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
            OnPropertyChanged(nameof(CanUpdateFinishDropDown));

            ChosenVendorStyle = SelectedVendorStyle;
            update_modify_tab_finish_based_on_selected_style();
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
            OnPropertyChanged(nameof(CanUpdateStyleFinishButton));
            ChosenVendorFinish = SelectedVendorFinish;
        }
    }

    public static List<ModificationItem> ChosenModifications;
    #endregion


    public int EK_Selection_Count
    {
        get
        {
            if (APP.Global_State.Current_Project_State == null) return 0;
            return APP.Global_State.Current_Project_State.EKSelectionCount;
        }
        set
        {
            APP.Global_State.Current_Project_State.EKSelectionCount = value;
            OnPropertyChanged(nameof(CanUpdateButton));
            //OnPropertyChanged(nameof(CanUpdateVendorStyleButton));
        }
    }
    public bool CanUpdateButton => SelectedSKU != null && EK_Selection_Count > 0;
    //public bool CanUpdateCabinetStyleSection { get; set; }
    public bool CanUpdateStyleDropDown { get; set; }
    public bool CanUpdateFinishDropDown => SelectedVendorStyle != null && EK_Selection_Count == 1;
    public bool CanUpdateStyleFinishButton => SelectedVendorFinish != null && EK_Selection_Count == 1;

    public bool CanUpdateModifications { get; set; }


    #region HANDLE CURRENT_PROJECT_STATE
    private EK_Project_State _currentProjectState;
    public Selection Current_Project_Revit_Selection
    {
        get => APP.Global_State.Current_Project_State.EKCurrentProjectSelection;
        set => APP.Global_State.Current_Project_State.EKCurrentProjectSelection = value;
    }

    /// <summary>
    /// Fires whenever a property *within* the Current_Project_State changes,
    /// e.g., EKSelectionCount or EKCurrentProjectSelection.
    /// </summary>
    private void CurrentProjectState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Project_State.EKSelectionCount))
        {
            OnPropertyChanged(nameof(EK_Selection_Count));
        }
        else if (e.PropertyName == nameof(EK_Project_State.EKCurrentProjectSelection))
        {
            OnPropertyChanged(nameof(Current_Project_Revit_Selection));
            // Update/Modify UI based on current selection
            update_modify_tab_cabinet_based_on_current_selection();
            update_modify_tab_stylefinish_based_on_current_selection();
            update_modify_tab_customizations_based_on_current_selection();
            filter_vendor_styles();
        }
    }

    /// <summary>
    /// Fires whenever any property on the Global_State changes,
    /// e.g., Current_Project_State is replaced with a new one.
    /// </summary>
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
            OnPropertyChanged(nameof(EK_Selection_Count));
            OnPropertyChanged(nameof(Current_Project_Revit_Selection));
        }
    }
    #endregion

    #region Update UI BASED ON CURRENT SELECTION
    void update_modify_tab_customizations_based_on_current_selection()  // Runs when Current-Selection is updated
    {
        Document document = APP.Global_State.Current_Project_State.Document;
        Selection selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;

        // Selecition is ONLY 1 eagle cabinet
        if (selection == null || selection.GetElementIds().Count != 1)
        {
            AvailableModifications?.Clear();
            CanUpdateModifications = false;
            OnPropertyChanged(nameof(CanUpdateModifications));
            return;
        }

        // Get the singly selected family instance
        FamilyInstance familyInstance = document.GetElement(selection.GetElementIds().First()) as FamilyInstance;

        // Check if the singly selected family instance is a eagle cabinet
        if (!FilterAllCabinets.IsInstanceAEagleCabinet(familyInstance))
        {
            AvailableModifications?.Clear();
            CanUpdateModifications = false;
            OnPropertyChanged(nameof(CanUpdateModifications));
            return;
        }

        // NOW GRAB THE EXISTING CUSTOMIZATIONS for the current family instance
        var availableModifications = get_cabinet_modificatinos(familyInstance);
        if (availableModifications == null)
        {
            AvailableModifications?.Clear();
            CanUpdateModifications = false;
            OnPropertyChanged(nameof(CanUpdateModifications));
            return;
        }
        AvailableModifications = availableModifications;
        CanUpdateModifications = true;
        OnPropertyChanged(nameof(CanUpdateModifications));
    }

    ObservableCollection<ModificationItem> get_cabinet_modificatinos(FamilyInstance familyInstance)
    {
        Document doc = APP.Global_State.Current_Project_State.Document;

        // This is a type param
        Parameter available_modifications_param = familyInstance.Symbol.LookupParameter("Vendor_Available_Modifications");
        if (available_modifications_param == null) return null;

        string available_modifications_value = available_modifications_param.AsValueString();
        if (string.IsNullOrEmpty(available_modifications_value)) return null;

        // Split available modifications
        List<string> available_modifications_strings = new List<string>(
            available_modifications_value.Split(',', (char)StringSplitOptions.RemoveEmptyEntries)
        );

        var availableModifications = new ObservableCollection<ModificationItem>();

        foreach (var mod_string in available_modifications_strings)
        {
            availableModifications.Add(new ModificationItem { Name = mod_string.Trim(), IsChecked = false });
        }

        // Retrieve selected modifications
        Parameter param = familyInstance.LookupParameter("Vendor_Modifications"); // This is a instance param, that is set a project param, set using global param
        if (param == null) return availableModifications; // No selected mods, return available list

        string value_string = param.AsValueString();
        if (string.IsNullOrEmpty(value_string)) return availableModifications;

        List<string> chosen_modification_strings = new List<string>(
            value_string.Split(',', (char)StringSplitOptions.RemoveEmptyEntries)
        );

        // Mark available modifications that are selected
        foreach (var mod in availableModifications)
        {
            if (chosen_modification_strings.Contains(mod.Name))
            {
                mod.IsChecked = true;
            }
        }

        return availableModifications;
    }


    void update_modify_tab_stylefinish_based_on_current_selection()  // Runs when Current-Selection is updated
    {
        // NOTE: As of now, style&finish can only be modified for 1 cabinet

        Document document = APP.Global_State.Current_Project_State.Document;
        Selection selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;

        // Selecition is ONLY 1 eagle cabinet
        if (selection == null || selection.GetElementIds().Count != 1)
        {
            SelectedVendorStyle = null;
            SelectedVendorFinish = null;
            CanUpdateStyleDropDown = false;
            OnPropertyChanged(nameof(CanUpdateStyleDropDown));
            return;
        }

        // Get the singly selected family instance
        FamilyInstance familyInstance = document.GetElement(selection.GetElementIds().First()) as FamilyInstance;

        // Check if the singly selected family instance is a eagle cabinet
        if (!FilterAllCabinets.IsInstanceAEagleCabinet(familyInstance))
        {
            SelectedVendorStyle = null;
            SelectedVendorFinish = null;
            CanUpdateStyleDropDown = false;
            OnPropertyChanged(nameof(CanUpdateStyleDropDown));
            return;
        }

        CanUpdateStyleDropDown = true;
        OnPropertyChanged(nameof(CanUpdateStyleDropDown));

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
            Element e = document.GetElement(eid);
            names.Add(e.Name);
            map_name_eid[e.Name] = eid;
        }

        // Filter VendorStyles that belogn to current Brand

        // 1. Get the current brand from your family instance.
        Parameter manufacturer_param = familyInstance.Symbol.LookupParameter("Manufacturer");
        string currentBrand = manufacturer_param.AsValueString();

        /*
        // 2. Map the user‐friendly brand names to the prefixes used in your Dictionary keys.
        string brandPrefix = currentBrand switch
        {
            "Aristokraft" => "Aristokraft",
            "Yorktowne Classic" => "YTC",
            "Yorktowne Historic" => "YTH",
            "Eclipse" => "Eclipse",
            _ => "" // or handle unexpected brands appropriately
        };

        // 3. Filter your dictionary so only those whose Key begins with the correct prefix remain.
        var filteredMap = map_name_eid
            .Where(kvp => kvp.Key.StartsWith(brandPrefix))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // 4. At this point, `filteredMap` contains only those entries that match the brand.
        // Update Combo Boxes
        // Build the list/collection
        FilteredVendorStyles = new ObservableCollection<Vendor_Style_With_Id>(
            filteredMap.Select(kvp => new Vendor_Style_With_Id(kvp.Key, kvp.Value))
        );
        */

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
        FilteredVendorStyles = temp_filtered_vendor_styles;
    }

    void update_modify_tab_finish_based_on_selected_style()  // Runs when Current-Selection is updated
    {
        // 5. Based on the SelectedVendor Style set the available finishes
        List<string> temp_filtered_vendor_finishes = [];
        if (SelectedVendorStyle == null) return;
        for (int i = 0; i < 5; i++)
        {
            temp_filtered_vendor_finishes.Add(SelectedVendorStyle.Vendor_Style_Name.ToString() + " Material " + i.ToString());
        }

        FilteredVendorFinishes = temp_filtered_vendor_finishes;
    }

    void update_modify_tab_cabinet_based_on_current_selection()  // Runs when Current-Selection is updated
    {
        // When selection is empty, clear the UI
        if (Current_Project_Revit_Selection.GetElementIds().Count == 0)
        {
            SelectedBrand = null;
            return;
        }

        // Get current documents Family Symbols
        //var map_RevitFamilySymbol_EKFamilySymbol = APP.Global_State.Current_Project_State.Map_RevitFamilySymbol_EKFamilySymbol;
        var map_RevitFamilySymbolId_EKFamilySymbol = APP.Global_State.Current_Project_State.Map_RevitFamilySymbolId_EKFamilySymbol;
        var all_ekCaseworkSymbols = APP.Global_State.Current_Project_State.EKCaseworkSymbols;

        // Get the current Document & Selection
        Document document = APP.Global_State.Current_Project_State.Document;
        Selection selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;

        // Get the selected element IDs
        ICollection<ElementId> selectedIds = selection.GetElementIds();

        // NOTE: Ensure all selected elements are family instances
        // 1. Filter for FamilyInstance elements from the selected IDs
        List<FamilyInstance> familyInstances = selectedIds
            .Select(id => document.GetElement(id))
            .OfType<FamilyInstance>()   // Only FamilyInstances
            .ToList();

        // 2. Map each FamilyInstance's symbol to its EKFamilySymbol
        //List<EKFamilySymbol> eKFamilySymbols = familyInstances
        //    .Select(fi => fi.Symbol)  // get the FamilySymbol from the FamilyInstance
        //    .Where(symbol => map_RevitFamilySymbol_EKFamilySymbol.ContainsKey(symbol))
        //    .Select(symbol => map_RevitFamilySymbol_EKFamilySymbol[symbol])
        //    .Distinct()   // avoid duplicates
        //    .ToList();

        List<EKFamilySymbol> ekFamilySymbols = new List<EKFamilySymbol>();
        foreach (var familyInstance in familyInstances)
        {
            EKFamilySymbol ekFamilySymbol;
            map_RevitFamilySymbolId_EKFamilySymbol.TryGetValue(familyInstance.Symbol.Id, out ekFamilySymbol);
            if (ekFamilySymbol != null)
            {
                ekFamilySymbols.Add(ekFamilySymbol);
            }
        }

        // NOTE: all selected family instances must be also in ekFamilySymbols
        if (familyInstances.Count != ekFamilySymbols.Count)
        {
            SelectedBrand = null;
            return;
        }

        //bool all_selected_elements_are_same_ekbrand = false;
        //bool all_selected_elements_are_same_ektype = false;
        //bool all_selected_elements_are_same_ekcategory = false;
        //bool all_selected_elements_are_same_sku = false;

        // Fill up all boolean values based on the 'ekFamilySymbols'
        // Only run checks if the list is not empty
        if (ekFamilySymbols.Any())
        {
            if (ekFamilySymbols.Select(x => x.EKBrand).Distinct().Count() != 1)
            {
                SelectedBrand = null;
                SelectedEKType = null;
                SelectedEKCategory = null;
                SelectedSKU = null;
                return;
            }
            SelectedBrand = ekFamilySymbols.First().EKBrand;

            if (ekFamilySymbols.Select(x => x.EKType).Distinct().Count() != 1)
            {
                SelectedEKType = null;
                SelectedEKCategory = null;
                SelectedSKU = null;
                return;
            }
            SelectedEKType = ekFamilySymbols.First().EKType;

            if (ekFamilySymbols.Select(x => x.EKCategory).Distinct().Count() != 1)
            {
                SelectedEKCategory = null;
                SelectedSKU = null;
                return;
            }
            SelectedEKCategory = ekFamilySymbols.First().EKCategory;

            if (ekFamilySymbols.Select(x => x.EKSKU).Distinct().Count() != 1)
            {
                SelectedSKU = null;
                return;
            }
            SelectedSKU = ekFamilySymbols.First().EKSKU;
        }
        //Debug.WriteLine("Here");
    }
    #endregion

    #region Constructor
    public EK24Modify_ViewModel()
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

        SelectedBrand = null;
        SelectedEKType = null;
        SelectedEKCategory = null;
        SelectedSKU = null;
        Command_Reset_SelectedBrand = new RelayCommand(Handle_Command_Reset_SelectedBrand);
        Command_Reset_SelectedEKType = new RelayCommand(Handle_Command_Reset_SelectedEKType);
        Command_Reset_SelectedEKCategory = new RelayCommand(Handle_Command_Reset_SelectedEKCategory);
        Command_Reset_SelectedSKU = new RelayCommand(Handle_Command_Reset_SelectedSKU);
        Command_UpdateFamily = new RelayCommand(Handle_Command_UpdateFamily);
        Command_CreateNewFamily = new RelayCommand(Handle_Command_CreateNewFamily);

        Command_UpdateVendorStyleVendorFinish = new RelayCommand(Handle_Command_UpdateVendorStyleVendorFinish);

        Command_UpdateModifications = new RelayCommand(Handle_Command_UpdateModifications);
    }
    #endregion

    #region Helper FN's: Reset Combo box selections
    public ICommand Command_Reset_SelectedBrand { get; }
    public void Handle_Command_Reset_SelectedBrand()
    {
        SelectedBrand = null;
    }
    public ICommand Command_Reset_SelectedEKType { get; }
    public void Handle_Command_Reset_SelectedEKType()
    {
        SelectedEKType = null;
    }
    public ICommand Command_Reset_SelectedEKCategory { get; }
    public void Handle_Command_Reset_SelectedEKCategory()
    {
        SelectedEKCategory = null;
    }
    public ICommand Command_Reset_SelectedSKU { get; }
    public void Handle_Command_Reset_SelectedSKU()
    {
        SelectedSKU = null;
    }
    #endregion

    #region Relay Commands for Modify Tab
    public ICommand Command_UpdateFamily { get; }
    public ICommand Command_CreateNewFamily { get; }
    public void Handle_Command_CreateNewFamily()
    {
        Debug.WriteLine("Chosen symbol is", ChosenRevitFamilySymbol);
        APP.RequestHandler.RequestType = RequestType.Modify_CreateNewFamilyType;
        APP.ExternalEvent?.Raise();
    }
    public void Handle_Command_UpdateFamily()
    {
        Debug.WriteLine("Update Current selected family instances to new family symbol");
        APP.RequestHandler.RequestType = RequestType.Modify_UpdateNewFamilyType;
        APP.ExternalEvent?.Raise();
    }
    public ICommand Command_UpdateVendorStyleVendorFinish { get; }
    public void Handle_Command_UpdateVendorStyleVendorFinish()
    {
        if (SelectedBrand == null)
        {
            TaskDialog.Show("ERROR", "All instances must belong to same Brand");
            return;
        }

        Debug.WriteLine("UPDATE Instance Param Value");
        APP.RequestHandler.RequestType = RequestType.Modify_UpdateVendoryStyleFinish;
        APP.ExternalEvent?.Raise();
    }

    public ICommand Command_UpdateModifications { get; }
    public void Handle_Command_UpdateModifications()
    {
        // set this static property for the utility class to access it
        ChosenModifications = AvailableModifications.Where(mod => mod.IsChecked).ToList();

        Debug.WriteLine("UPDATE Instance - Modifications value");
        APP.RequestHandler.RequestType = RequestType.Modify_UpdateVendoryModifications;
        APP.ExternalEvent?.Raise();
    }

    #endregion

}

/// <summary>
/// UTILITY CLASS: Update Selcted Cabinet Modifications parameter 
/// </summary>
public static class Update_Instance_Vendor_Modifications_Utility
{
    public static void UpdateVendorModificationsForSelectedInstance(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // these all the modificaions that are current selected
        var chosen_modification_items = EK24Modify_ViewModel.ChosenModifications;

        // Combine all the modification names (just the names, since the list has IsChecked as true) into a comma-separated string
        string final_string = string.Join(",", chosen_modification_items.Where(mod => mod.IsChecked).Select(mod => mod.Name));

        // If there are no selected modifications, you can choose to return an empty string or some default value
        if (string.IsNullOrEmpty(final_string)) return;

        // Start a transaction to change the type
        using (Transaction trans = new Transaction(doc, "Update Vendor-Modifications"))
        {
            trans.Start();

            ElementId eid = uiDoc.Selection.GetElementIds().First();
            FamilyInstance familyInstance = doc.GetElement(eid) as FamilyInstance;

            Parameter modifications_param = familyInstance.LookupParameter("Vendor_Modifications");
            if (modifications_param == null)
            {
                trans.RollBack();
                return;
            }
            modifications_param.Set(final_string);

            trans.Commit();
        }


    }
}

/// <summary>
/// UTILITY CLASS: Update Selcted Cabinet Vendor-Style & Vendor-Finish
/// </summary>
public static class Update_Instance_VendorStyle_And_VendorFinish_Utility
{
    public static void UpdateVendorStyleVendorFinishForSelectedInstance(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // ////////////////////////////// TODO //////////////////////////////

        // Chosen Vendor-Style & Vendor-Finish
        Vendor_Style_With_Id chosen_vendor_style = EK24Modify_ViewModel.ChosenVendorStyle;
        string chosen_vendor_finish = EK24Modify_ViewModel.ChosenVendorFinish;

        Debug.WriteLine("now update in trasaction");

        // CURRENT SELECTION
        Selection current_selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;
        ICollection<ElementId> selectedIds = current_selection.GetElementIds();


        // NOTE: Ensure all selected elements are family instances
        // 1. Filter for FamilyInstance elements from the selected IDs
        List<FamilyInstance> current_selected_familyInstances = selectedIds
            .Select(id => doc.GetElement(id))
            .OfType<FamilyInstance>()   // Only FamilyInstances
            .ToList();


        bool updatedParamResult = false;

        // Start a transaction to change the type
        using (Transaction trans = new Transaction(doc, "Change Cabinet SKU"))
        {
            trans.Start();

            foreach (FamilyInstance familyInstance in current_selected_familyInstances)
            {
                if (familyInstance == null)
                {
                    TaskDialog.Show("Error", "Invalid family instance provided.");
                }
                // Update parameter
                Parameter current_vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
                // Finaly: set with the 'ElementId' since that is the storage type of this param, and setting with string won't work
                updatedParamResult = current_vendor_style_param.Set(chosen_vendor_style.Revit_ElementId);
                if (updatedParamResult == false)
                {
                    trans.RollBack();
                    TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    return;
                }
            }
            trans.Commit();
        }

        TaskDialog.Show("SUCCESS", "UPDATED THE VENDOR-STYLE PARAM");
        return;
    }
}

