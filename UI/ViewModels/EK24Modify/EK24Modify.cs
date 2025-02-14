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
using System.Windows.Data;
using System.Windows.Input;


namespace ek24.UI;


// HELPER CLASS TO DEFINE THE DATA STRCTURE 
public class Vendor_Style_With_Id
{
    public string Vendor_Style_Name { get; set; }
    public ElementId Revit_ElementId { get; set; }

    //public override string ToString()
    //{
    //    return Vendor_Style_Name;
    //}

    public Vendor_Style_With_Id(string vendor_Style_Value, ElementId revit_ElementId)
    {
        Vendor_Style_Name = vendor_Style_Value;
        Revit_ElementId = revit_ElementId;
    }
}


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
    private static List<string> hardcoded_vendorStyleNames = [
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
    private static Dictionary<string, List<string>> hardcoded_vendorFinishNames = new Dictionary<string, List<string>>
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

    private ObservableCollection<Vendor_Style_With_Id> _vendorStylesFilteredByBrand;
    public ObservableCollection<Vendor_Style_With_Id> VendorStylesFilteredByBrand
    {
        get => _vendorStylesFilteredByBrand;
        set
        {
            if (value == _vendorStylesFilteredByBrand) return;
            _vendorStylesFilteredByBrand = value;
            OnPropertyChanged(nameof(VendorStylesFilteredByBrand));

            // Every time a new list of values are set for the 'VENDOR STYLE' reset the 'VENDOR FINISH' state to fresh
            //FilteredVendorFinishes.Clear();
            //CanUpdateFinishDropDown = false;
            //OnPropertyChanged(nameof(CanUpdateFinishDropDown));
        }
    }

    private ObservableCollection<string> _vendorFinishesFilteredByBrand { get; set; } = [];
    public ObservableCollection<string> VendorFinishesFilteredByBrand
    {
        get => _vendorFinishesFilteredByBrand;
        set
        {
            if (value == _vendorFinishesFilteredByBrand) return;
            _vendorFinishesFilteredByBrand = value;
            OnPropertyChanged(nameof(VendorFinishesFilteredByBrand));
        }
    }

    public class ModificationItem
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public string Category { get; set; }
    }
    // create a CollectionViewSource in your ViewModel and expose its View as a property for binding.
    private ObservableCollection<ModificationItem> _availableModifications;
    public ObservableCollection<ModificationItem> AvailableModifications
    {
        get => _availableModifications;
        set
        {
            if (value == _availableModifications) return;
            _availableModifications = value;
            OnPropertyChanged(nameof(AvailableModifications));
            // Set the AvailableModificationItemsCollectionView
            var temp_availableModificationItemsCollectionView = new ListCollectionView(value);
            temp_availableModificationItemsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            AvailableModificationItemsCollectionView = temp_availableModificationItemsCollectionView;
            OnPropertyChanged(nameof(AvailableModificationItemsCollectionView));
        }
    }
    public ICollectionView AvailableModificationItemsCollectionView { get; set; }
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
        // TODO: I think i am using 'FilteredVendorStyles' as the actual property
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
            if (value == null || value == _selectedVendorStyle)
            {
                CanUpdateFinishDropDown = false;
                OnPropertyChanged(nameof(CanUpdateFinishDropDown));
                return;
            }
            _selectedVendorStyle = value;
            ChosenVendorStyle = value;
            OnPropertyChanged(nameof(SelectedVendorStyle));

            // Based on the SelectedVendor Style set the available finishes
            // Got the below logic from 
            // ------ start ------------------------------------------
            // Initialize out parameters
            ObservableCollection<string> available_filtered_finishes = new ObservableCollection<string>();

            Dictionary<string, string[]> brandToPrefixes = new Dictionary<string, string[]>
            {
                { "Aristokraft",         new[] { "Aristokraft" } },
                { "Yorktowne Classic",   new[] { "YTC", "Yorktowne Classic" } },
                { "Yorktowne Historic",  new[] { "YTH", "Yorktowne Historic" } },
                { "Eclipse",             new[] { "Eclipse" } }
            };

            string brand_name = "";
            string normalized_style_name = null;
            string normalized_brand_style_name = null;

            // Split the style name into parts
            string current_style_name = value.Vendor_Style_Name;
            string[] parts = current_style_name.Split(new[] { " - " }, StringSplitOptions.None);

            if (parts.Length >= 2)
            {
                // Determine brand name based on known prefixes
                foreach (var kvp in brandToPrefixes)
                {
                    if (kvp.Value.Any(prefix => parts[0].Equals(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        brand_name = kvp.Key;
                        break;
                    }
                }

                // Ensure a valid brand name is found
                if (!string.IsNullOrEmpty(brand_name))
                {
                    // Extract the style name and ignore anything after the second dash
                    string style_name = parts[1];
                    normalized_style_name = $"{brand_name} - {style_name}";
                    normalized_brand_style_name = $"{brand_name.Split(' ')[0]} - {style_name}";
                }
            }

            if (normalized_style_name == null)
            {
                normalized_style_name = current_style_name;
                normalized_brand_style_name = current_style_name;
            }

            // Retrieve available finishes based on normalized style name
            if (hardcoded_vendorFinishNames.TryGetValue(normalized_style_name, out var temp_finishes))
            {
                available_filtered_finishes = new ObservableCollection<string>(temp_finishes);
            }
            // ------ end --------------------------------------------
            VendorFinishesFilteredByBrand = available_filtered_finishes;
            CanUpdateFinishDropDown = true;


            /*
            // Filter VendorFinishes based on the selected vendor style
            if (hardcoded_vendorFinishNames.TryGetValue(value.Vendor_Style_Name, out var temp_vendor_finishes))
            {
                VendorFinishesFilteredByBrand = new ObservableCollection<string>(temp_vendor_finishes);
                CanUpdateFinishDropDown = true;
                //OnPropertyChanged(nameof(CanUpdateFinishDropDown));
            }
            else
            {
                VendorFinishesFilteredByBrand = new ObservableCollection<string>(); // No finishes found for the selected style
                CanUpdateFinishDropDown = false;
                //OnPropertyChanged(nameof(CanUpdateFinishDropDown));
            }
            */

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
    public bool CanUpdateFinishDropDown { get; set; }
    //public bool CanUpdateStyleFinishButton { get; set; }
    public bool CanUpdateStyleFinishButton => SelectedVendorFinish != null && EK_Selection_Count == 1;

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
            VendorStylesFilteredByBrand?.Clear();
            CanUpdateStyleDropDown = false;
            VendorFinishesFilteredByBrand.Clear();
            CanUpdateFinishDropDown = false;
            return;
        }

        // Get the singly selected family instance
        FamilyInstance familyInstance = document.GetElement(selection.GetElementIds().First()) as FamilyInstance;

        // Check if the singly selected family instance is a eagle cabinet
        if (!FilterAllCabinets.IsInstanceAEagleCasework(familyInstance))
        {
            AvailableModifications?.Clear();
            VendorStylesFilteredByBrand?.Clear();
            CanUpdateStyleDropDown = false;
            VendorFinishesFilteredByBrand.Clear();
            CanUpdateFinishDropDown = false;
            OnPropertyChanged(nameof(CanUpdateFinishDropDown));
            return;
        }


        // NOW GRAB THE EXISTING CUSTOMIZATIONS for the current family instance
        var availableModifications = get_cabinet_modifications(familyInstance);
        if (availableModifications == null)
        {
            AvailableModifications?.Clear();
            VendorStylesFilteredByBrand?.Clear();
            CanUpdateStyleDropDown = false;
            VendorFinishesFilteredByBrand.Clear();
            CanUpdateFinishDropDown = false;
            OnPropertyChanged(nameof(CanUpdateFinishDropDown));
            return;
        }

        // DEMO
        //AvailableModifications = new ObservableCollection<ModificationItem>
        //{
        //    new ModificationItem { Category = "Cutlery Divider", Name = "CUT24", IsChecked = false },
        //    new ModificationItem { Category = "Cutlery Divider", Name = "CUT30", IsChecked = true },
        //    new ModificationItem { Category = "Peg Drawer Organizer", Name = "DPS-27", IsChecked = false }
        //};
        //CanUpdateStyleDropDown = true;
        //return;

        AvailableModifications = availableModifications;
        CanUpdateStyleDropDown = true;
    }

    // HELPER FN: Given a Family Instance, grabs the 'Vendor_Modifications' param and converts it into 'ModificationItem' type
    ObservableCollection<ModificationItem> get_cabinet_modifications(FamilyInstance familyInstance)
    {
        Document doc = APP.Global_State.Current_Project_State.Document;

        // This is a type parameter
        Parameter available_modifications_param = familyInstance.Symbol.LookupParameter("Vendor_Available_Modifications");
        if (available_modifications_param == null) return null;

        string available_modifications_value = available_modifications_param.AsValueString();
        if (string.IsNullOrEmpty(available_modifications_value)) return null;

        // Split available modifications
        List<string> available_modifications_strings = new List<string>(
            available_modifications_value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        );

        var availableModifications = new ObservableCollection<ModificationItem>();

        // Parse available modifications
        foreach (var mod_string in available_modifications_strings)
        {
            var parts = mod_string.Split(new[] { ':' }, 2); // Split into category and name
            if (parts.Length == 2)
            {
                availableModifications.Add(new ModificationItem
                {
                    Category = parts[0].Trim(), // Extract category
                    Name = parts[1].Trim(),    // Extract name
                    IsChecked = false          // Default to unchecked
                });
            }
        }

        // Retrieve selected modifications
        Parameter param = familyInstance.LookupParameter("Vendor_Modifications"); // This is an instance parameter
        if (param == null) return availableModifications; // No selected mods, return available list

        string value_string = param.AsValueString();
        if (string.IsNullOrEmpty(value_string)) return availableModifications;

        List<string> chosen_modification_strings = new List<string>(
            value_string.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        );

        // Mark available modifications that are selected
        foreach (var mod in availableModifications)
        {
            // Construct the full string in the format <category>:<name>
            string fullModString = $"{mod.Category}:{mod.Name}";

            if (chosen_modification_strings.Contains(fullModString))
            {
                mod.IsChecked = true;
            }
        }

        return availableModifications;
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


    // Updates the Modify Tab's Style and Finish dropdowns based on the current selection in the project.
    // This function is triggered whenever the current selection is updated.
    void update_modify_tab_stylefinish_based_on_current_selection()
    {
        /* :::::::::::::::::::::::::::::::::::::::: TODO ::::::::::::::::::::::::::::::::::::::::
        // FEATURE: Can apply for multiple cabinets, as long as they belong to the same brand
        // All selected instances are Eagle Cabinets and belong to same 
        // To achieve just get of Vendor-style-id type and use a string instead. and refactor the logic of updating the cabinets
        // by using this string, and individually getting the Element-id that matches the vendor-style-name-string
        */

        // Get the current document and selection from the global state
        Document document = APP.Global_State.Current_Project_State.Document;
        Selection selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;

        // Validate the selection:
        // - Ensure the selection is not null.
        // - Ensure only one element is selected (single selection).
        if (selection == null || selection.GetElementIds().Count != 1)
        {
            // If the selection is invalid, reset the Style and Finish properties and disable the Style dropdown.
            SelectedVendorStyle = null;
            SelectedVendorFinish = null;
            CanUpdateStyleDropDown = false;
            return;
        }

        // Get the singly selected family instance from the document.
        FamilyInstance familyInstance = document.GetElement(selection.GetElementIds().First()) as FamilyInstance;

        // Validate that the selected family instance is an Eagle cabinet.
        if (!FilterAllCabinets.IsInstanceAEagleCasework(familyInstance))
        {
            // If the selected instance is not an Eagle cabinet, reset the Style and Finish properties and disable the Style dropdown.
            SelectedVendorStyle = null;
            SelectedVendorFinish = null;
            CanUpdateStyleDropDown = false;
            return;
        }

        // Step 1: Update the Vendor Style dropdown
        // ---------------------------------------------------------------------------------
        // Get all filtered vendor styles for the current family instance and the currently set style.
        ObservableCollection<Vendor_Style_With_Id> temp_filteredVendorStyles = new ObservableCollection<Vendor_Style_With_Id>();
        Vendor_Style_With_Id current_style = null;
        Get_VendorStyles_Filtered_By_Brand_And_CurrentStyle(document, familyInstance, out temp_filteredVendorStyles, out current_style);

        // TODO: WHAT IF THE current_style is NULL???

        // 1.1: Set the filtered vendor styles as the data source for the Style dropdown.
        VendorStylesFilteredByBrand = temp_filteredVendorStyles;

        // 1.2: Set the selected value of the Style dropdown to the current style of the family instance.
        SelectedVendorStyle = current_style;

        // Step 2: Update the Vendor Finish dropdown
        // ---------------------------------------------------------------------------------
        // Get all filtered vendor finishes for the current family instance and the currently set finish.
        ObservableCollection<string> temp_VendorFinishesFilteredByBrand = new ObservableCollection<string>();
        string current_finish = "";
        Get_VendorFinishes_Filtered_By_Brand_And_CurrentFinish(document, familyInstance, current_style.Vendor_Style_Name, out temp_VendorFinishesFilteredByBrand, out current_finish);

        // 2.1: Set the filtered vendor finishes as the data source for the Finish dropdown.
        VendorFinishesFilteredByBrand = temp_VendorFinishesFilteredByBrand;

        // 2.2: Set the selected value of the Finish dropdown to the current finish of the family instance.
        SelectedVendorFinish = current_finish;
    }

    // HELPER FN: Given a family instance, get all possible 'Vendor_Style_With_Id' for this family instance,
    // filtered by the current brand and the current style.
    public static void Get_VendorStyles_Filtered_By_Brand_And_CurrentStyle(
        Document doc,
        FamilyInstance familyInstance,
        out ObservableCollection<Vendor_Style_With_Id> available_filtered_styles,
        out Vendor_Style_With_Id current_style)
    {
        // Initialize out parameters to default values
        available_filtered_styles = new ObservableCollection<Vendor_Style_With_Id>();
        current_style = null;

        // Check if the family instance has the 'Vendor_Style' parameter
        Parameter vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
        if (vendor_style_param == null)
        {
            // The given family instance doesn't have the 'Vendor_Style' parameter
            return;
        }

        // Ensure the 'Vendor_Style' parameter is of a BuiltInCategory data type
        if (!Category.IsBuiltInCategory(vendor_style_param.Definition.GetDataType()))
        {
            // The data type is not a BuiltInCategory
            return;
        }

        // Get all possible family type parameter values for the 'Vendor_Style' parameter
        ISet<ElementId> all_possible_familytype_parameter_values_set = familyInstance.Symbol.Family.GetFamilyTypeParameterValues(vendor_style_param.Id);
        if (all_possible_familytype_parameter_values_set.Count == 0)
        {
            // No valid family type parameter values found
            return;
        }

        // Get the current brand from the family instance
        Parameter manufacturer_param = familyInstance.Symbol.LookupParameter("Manufacturer");
        string currentBrand = manufacturer_param?.AsValueString() ?? string.Empty;

        // Map of all brands to their possible prefix strings that might appear in 'Vendor_Style' values
        Dictionary<string, string[]> brandToPrefixes = new Dictionary<string, string[]>
        {
            { "Aristokraft",         new[] { "Aristokraft" } },
            { "Yorktowne Classic",   new[] { "YTC", "Yorktowne Classic" } },
            { "Yorktowne Historic",  new[] { "YTH", "Yorktowne Historic" } },
            { "Eclipse",             new[] { "Eclipse" } }
        };

        // Get the possible prefixes for the current brand
        if (!brandToPrefixes.TryGetValue(currentBrand, out string[] possiblePrefixes))
        {
            // If the brand is not recognized, use an empty array of prefixes
            possiblePrefixes = Array.Empty<string>();
        }

        // Filter the styles based on the current brand's prefixes
        var filteredStyles = all_possible_familytype_parameter_values_set
            .Select(eid => doc.GetElement(eid)) // Get the element for each ElementId
            .Where(e => possiblePrefixes.Any(prefix => e.Name.StartsWith(prefix))) // Filter by prefix
            .Select(e => new Vendor_Style_With_Id(e.Name, e.Id)) // Create Vendor_Style_With_Id instances
            .ToList();

        // Set the available filtered styles
        available_filtered_styles = new ObservableCollection<Vendor_Style_With_Id>(filteredStyles);

        // Determine the current style (if any)
        string currentStyleName = vendor_style_param.AsValueString();

        // Extract the vendor style name from the currentStyleName (e.g., "EK_TEMPLATE_Panel_ONE_DOOR_ONE_DRAWER : Aristokraft - Sinclair" -> "Aristokraft - Sinclair")
        string vendorStyleName = currentStyleName?
            .Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries) // Split by ':'
            .Last() // Take the last part (vendor style name)
            .Trim(); // Remove leading/trailing whitespace

        // Match the extracted vendor style name to the filtered styles
        current_style = filteredStyles.FirstOrDefault(style => style.Vendor_Style_Name == vendorStyleName);
    }


    public static void Get_VendorFinishes_Filtered_By_Brand_And_CurrentFinish(
    Document doc,
    FamilyInstance familyInstance,
    string current_style_name,
    out ObservableCollection<string> available_filtered_finishes,
    out string current_finish)
    {
        // Initialize out parameters
        available_filtered_finishes = new ObservableCollection<string>();
        current_finish = null;

        Dictionary<string, string[]> brandToPrefixes = new Dictionary<string, string[]>
            {
                { "Aristokraft",         new[] { "Aristokraft" } },
                { "Yorktowne Classic",   new[] { "YTC", "Yorktowne Classic" } },
                { "Yorktowne Historic",  new[] { "YTH", "Yorktowne Historic" } },
                { "Eclipse",             new[] { "Eclipse" } }
            };

        string brand_name = "";
        string normalized_style_name = null;
        string normalized_brand_style_name = null;

        // Split the style name into parts
        string[] parts = current_style_name.Split(new[] { " - " }, StringSplitOptions.None);

        if (parts.Length >= 2)
        {
            // Determine brand name based on known prefixes
            foreach (var kvp in brandToPrefixes)
            {
                if (kvp.Value.Any(prefix => parts[0].Equals(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    brand_name = kvp.Key;
                    break;
                }
            }

            // Ensure a valid brand name is found
            if (!string.IsNullOrEmpty(brand_name))
            {
                // Extract the style name and ignore anything after the second dash
                string style_name = parts[1];
                normalized_style_name = $"{brand_name} - {style_name}";
                normalized_brand_style_name = $"{brand_name.Split(' ')[0]} - {style_name}";
            }
        }

        if (normalized_style_name == null)
        {
            normalized_style_name = current_style_name;
            normalized_brand_style_name = current_style_name;
        }

        // Retrieve available finishes based on normalized style name
        if (hardcoded_vendorFinishNames.TryGetValue(normalized_style_name, out var temp_finishes))
        {
            available_filtered_finishes = new ObservableCollection<string>(temp_finishes);
        }

        // Retrieve the current finish from the family instance
        Parameter vendor_finish_param = familyInstance.LookupParameter("Vendor_Finish");
        if (vendor_finish_param != null && vendor_finish_param.AsValueString() != null)
        {
            current_finish = vendor_finish_param.AsValueString() == "<By Category>" ? null : vendor_finish_param.AsValueString();
        }
        Debug.WriteLine("end");
    }


    /*
    public static void Get_VendorFinishes_Filtered_By_Brand_And_CurrentFinish(
    Document doc,
    FamilyInstance familyInstance,
    string current_style_name,
    out ObservableCollection<string> available_filtered_finishes,
    out string current_finish)
    {
        // Initialize out parameters to default values
        available_filtered_finishes = new ObservableCollection<string>();
        current_finish = null;

        Dictionary<string, string[]> brandToPrefixes = new Dictionary<string, string[]>
        {
            { "Aristokraft",         new[] { "Aristokraft" } },
            { "Yorktowne Classic",   new[] { "YTC", "Yorktowne Classic" } },
            { "Yorktowne Historic",  new[] { "YTH", "Yorktowne Historic" } },
            { "Eclipse",             new[] { "Eclipse" } }
        };

        // 1. Get the Brand Name
        string brand_name = null;

        // 2. Get the Normalized Style Name
        // Normalize the current_style_name to match the dictionary keys
        string normalized_style_name = null;

        string[] parts = current_style_name.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            // Extract the brand and style name
            string brand = parts[0] + " " + parts[1]; // e.g., "Yorktowne Classic" or "Aristokraft"
            string style = parts[2]; // e.g., "Henning" or "Sinclair"

            // Construct the normalized key
            normalized_style_name = $"{brand} - {style}";
        }
        else
        {
            normalized_style_name = current_style_name;
        }

        // 3. Get all available Finishes
        // Try to get the finishes for the normalized style name
        if (normalized_style_name != null && hardcoded_vendorFinishNames.TryGetValue(normalized_style_name, out var temp_finishes))
        {
            available_filtered_finishes = new ObservableCollection<string>(temp_finishes);
        }

        // 4. Get the Current Finish
        // Get the current finish from the family instance
        Parameter vendor_finish_param = familyInstance.LookupParameter("Vendor_Finish");
        if (vendor_finish_param == null || vendor_finish_param.AsValueString() == null) return;
        // Set to null if no finish is set
        current_finish = vendor_finish_param.AsValueString() == "<By Category>" ? null : vendor_finish_param.AsValueString();
    }
    */

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
        APP.RequestHandler.RequestType = RequestType.Modify_UpdateVendoryStyleFinish; // Invokes Update_Instance_VendorStyle_And_VendorFinish_Utility.UpdateVendorStyleVendorFinishForSelectedInstance
        APP.ExternalEvent?.Raise();
    }

    public ICommand Command_UpdateModifications { get; }
    public void Handle_Command_UpdateModifications()
    {
        // set this static property for the utility class to access it
        ChosenModifications = AvailableModifications.Where(mod => mod.IsChecked).ToList();

        Debug.WriteLine("UPDATE Instance - Modifications value");
        //  INVOKES::Update_Instance_Vendor_Modifications_Utility.UpdateVendorModificationsForSelectedInstance
        APP.RequestHandler.RequestType = RequestType.Modify_UpdateVendoryModifications;
        APP.ExternalEvent?.Raise();
    }
    #endregion
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
        using (Transaction trans = new Transaction(doc, "Modify Style & Finish"))
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

                // :: Set Material Finish :: 
                // Get the material
                //string target_material_name = "Yorktowne Classic - Stillwater-Maple Stain";
                string target_material_name = EK24Modify_ViewModel.ChosenVendorFinish;
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
                Parameter matParam = familyInstance
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
            trans.Commit();
        }

        TaskDialog.Show("SUCCESS", "UPDATED THE VENDOR-STYLE PARAM");
        return;
    }
}


/// <summary>
/// UTILITY CLASS: Update Selcted Cabinet Modifications parameter 
/// </summary>
public static class Update_Instance_Vendor_Modifications_Utility
{
    public static void UpdateVendorModificationsForSelectedInstance(UIApplication app)
    {
        // FOR NOW THIS FN. ALSO EXPLECTS TO UPDATE THE PARAM FOR ONLY 1 FAMILY INSTANCE
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // these all the modificaions that are current selected (all of them are checked, there won't be
        // any unchecked, because we filter them prior to setting in the function '')
        var chosen_modification_items = EK24Modify_ViewModel.ChosenModifications;

        // Convert the ModificationItem into String.
        // 
        // Combine all the modification names (just the names, since the list has IsChecked as true) into a comma-separated string

        //string final_string = string.Join(",", chosen_modification_items.Select(mod => $"{mod.Category}:{mod.Name}"));
        string final_string = string.Join(",", chosen_modification_items.Where(mod => mod.IsChecked).Select(mod => $"{mod.Category}:{mod.Name}"));

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


