using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.RequestHandling;
using ek24.UI.Commands;
using System;
using System.Collections.Generic;
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

    #region All 6 Combo Box Values
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
                OnPropertyChanged(nameof(CanUpdate));
                ChosenRevitFamilySymbol = SelectedSKU?.RevitFamilySymbol;
            }
        }
    }

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
    /*
    private static Vendor_Style_With_Id _selectedVendorStyleWithId { get; set; }
    public static Vendor_Style_With_Id SelectedVendorStyleWithId
    {
        get => _selectedVendorStyleWithId;
        set
        {
            if (_selectedVendorStyleWithId == value) return;
            _selectedVendorStyleWithId = value;
            OnStaticPropertyChanged(nameof(SelectedVendorStyleWithId));
        }
    }
    */

    #endregion

    private EK_Project_State _currentProjectState;

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
            OnPropertyChanged(nameof(CanUpdate));
        }
    }
    public bool CanUpdate => SelectedSKU != null && EK_Selection_Count > 0;

    public Selection Current_Project_Revit_Selection
    {
        get => APP.Global_State.Current_Project_State.EKCurrentProjectSelection;
        set => APP.Global_State.Current_Project_State.EKCurrentProjectSelection = value;
    }

    // Handle the ppties that change on Global State
    /*
    private void CurrentProjectState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Project_State.EKSelectionCount))
        {
            OnPropertyChanged(nameof(EK_Selection_Count));
        }
        if (e.PropertyName == nameof(EK_Project_State.EKCurrentProjectSelection))
        {
            OnPropertyChanged(nameof(Current_Project_Revit_Selection));
        }
    }

    // Don't forget to unsubscribe in a Dispose method
    public void Dispose()
    {
        if (_currentProjectState != null)
        {
            _currentProjectState.PropertyChanged -= CurrentProjectState_PropertyChanged;
        }
    }
    */

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
            update_modify_ui_based_on_selection();
            filter_vendor_styles();
        }
    }


    /*
    private void CurrentProjectState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // `Current_Project_State` is changed | this has to be notified from where it is coming from
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State))
        {
            OnPropertyChanged(nameof(EK_Selection_Count));
        }
        // `Current_Project_State.EKSelectionCount` is changed
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State.EKSelectionCount))
        {
            OnPropertyChanged(nameof(EK_Selection_Count));
        }
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State.EKCurrentProjectSelection))
        {
            OnPropertyChanged(nameof(Current_Project_Revit_Selection));
            update_modify_ui_based_on_selection();
        }
    }
    */

    void update_modify_ui_based_on_selection()  // Runs when Current-Selection is updated
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

    #region Relay Commands for UPDATE & CREATE
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
    #endregion

}

/*
public class OLDCODE
{
    public List<FamilyGroup> FamilyGroups { get; set; } = RevitFamilyGroups.FamilyGroups;

    public static List<Models.Revit.EKFamily> _familys { get; set; }
    public static List<Models.Revit.EKFamily> Familys
    {
        get => _familys;
        set
        {
            if (_familys != value)
            {
                _familys = value;
                OnStaticPropertyChanged(nameof(Familys));
            }
        }
    }

    public static List<Models.Revit.EKFamilyType> _familyTypes { get; set; }
    public static List<Models.Revit.EKFamilyType> FamilyTypes
    {
        get => _familyTypes;
        set
        {
            if (_familyTypes != value)
            {
                _familyTypes = value;
                OnStaticPropertyChanged(nameof(FamilyTypes));
            }
        }
    }


    public static FamilyGroup _selectedFamilyGroup { get; set; }
    public static FamilyGroup SelectedFamilyGroup
    {
        get => _selectedFamilyGroup;
        set
        {
            if (_selectedFamilyGroup != value)
            {
                _selectedFamilyGroup = value;
                OnStaticPropertyChanged(nameof(SelectedFamilyGroup));
                UpdateFamilys();
            }
        }
    }

    public static Models.Revit.EKFamily _selectedFamily { get; set; }
    public static Models.Revit.EKFamily SelectedFamily
    {
        get => _selectedFamily;
        set
        {
            if (_selectedFamily != value)
            {
                _selectedFamily = value;
                OnStaticPropertyChanged($"{nameof(SelectedFamily)}");
                UpdateTypes();
            }
        }
    }


    public static Models.Revit.EKFamilyType _selectedFamilyType { get; set; }
    public static Models.Revit.EKFamilyType SelectedFamilyType
    {
        get => _selectedFamilyType;
        set
        {
            if (_selectedFamilyType != value)
            {
                _selectedFamilyType = value;
                //OnStaticPropertyChanged($"{nameof(SelectedFamilyType)}");
                OnStaticPropertyChanged(nameof(SelectedFamilyType));
            }
        }
    }

    private static void UpdateFamilys()
    {
        if (SelectedFamilyGroup != null)
        {
            Familys = SelectedFamilyGroup.Familys;
        }
        else
        {
            Familys = new List<Models.Revit.EKFamily>();
        }
    }

    private static void UpdateTypes()
    {
        if (SelectedFamily != null)
        {
            FamilyTypes = SelectedFamily.FamilyTypes;
        }
        else
        {
            FamilyTypes = new List<Models.Revit.EKFamilyType>();
        }
    }
    private static bool _selectionIsCabinetsOnly { get; set; } = false;
    public static bool SelectionIsCabinetsOnly
    {
        get => _selectionIsCabinetsOnly;
        set
        {
            _selectionIsCabinetsOnly = value;
            OnStaticPropertyChanged(nameof(SelectionIsCabinetsOnly));
        }
    }


    // Capture all the multiple cabinet instances that user selects
    private static ObservableCollection<FamilyInstance> _selectedCabinetFamilyInstances = new ObservableCollection<FamilyInstance>();
    public static ObservableCollection<FamilyInstance> SelectedCabinetFamilyInstances
    {
        get => _selectedCabinetFamilyInstances;
        set
        {
            _selectedCabinetFamilyInstances = value;
            OnStaticPropertyChanged(nameof(SelectedCabinetFamilyInstances));
        }
    }


    public static List<(string, string)> _availableCabinetTypes { get; set; }
    public static List<(string, string)> AvailableCabinetTypes
    {
        get => _availableCabinetTypes;
        set
        {
            _availableCabinetTypes = value;
            OnStaticPropertyChanged(nameof(AvailableCabinetTypes));
        }
    }

    // Binders for user to select the new type for the selected cabinet instances
    // (typename, vendor-notes) vendor-notes is "" if no param or value found
    private static string _chosenCabinetTypeText;
    public static string ChosenCabinetTypeText
    {
        get => _chosenCabinetTypeText;
        set
        {
            _chosenCabinetTypeText = value;
            OnStaticPropertyChanged(nameof(ChosenCabinetTypeText));
        }
    }

    public static bool _allCabinetsAreSameType { get; set; }
    public static bool AllCabinetsAreSameType
    {
        get => _allCabinetsAreSameType;
        set
        {
            _allCabinetsAreSameType = value;
            OnStaticPropertyChanged(nameof(AllCabinetsAreSameType));
        }
    }


    //public static (FamilySymbol, string) _chosenCabinetType { get; set; } = (null, "");
    //public static (FamilySymbol, string) ChosenCabinetType
    //{
    //    get => _chosenCabinetType;
    //    set
    //    {
    //        _chosenCabinetType = value;
    //        OnStaticPropertyChanged(nameof(ChosenCabinetType.ToString));

    //        if (value.Item2 == "")
    //        {
    //            ChosenCabinetTypeText = value.Item1.ToString();
    //        }
    //        else
    //        {
    //            ChosenCabinetTypeText = $"{value.Item1.ToString()} | {value.Item2}";
    //        }
    //    }
    //}


    public static (string, string) _chosenCabinetType { get; set; } = ("", "");
    public static (string, string) ChosenCabinetType
    {
        get => _chosenCabinetType;
        set
        {
            _chosenCabinetType = value;
            OnStaticPropertyChanged(nameof(ChosenCabinetType));

            //ChosenCabinetTypeText = value.Item1;

            // Update the text property
            if (value.Item2 == "")
            {
                ChosenCabinetTypeText = value.Item1;
            }
            else
            {
                ChosenCabinetTypeText = $"{value.Item1} | {value.Item2}";
            }
        }
    }

    public static string _chosenFamilySymbol { get; set; }
    public static string ChosenFamilySymbol
    {
        get => _chosenFamilySymbol;
        set
        {
            _chosenFamilySymbol = value;
            OnStaticPropertyChanged(nameof(ChosenFamilySymbol));
        }
    }


    public static void SyncCurrentSelectionWithTypeParamsViewModel(Selection currentSelection, Document doc)
    {
        SelectedCabinetFamilyInstances.Clear();

        SelectedFamilyGroup = null;
        SelectedFamily = null;
        SelectedFamilyType = null;


        var selectedIds = currentSelection.GetElementIds();
        // When deselecting anything, this will be 0, so don't check anything else
        if (selectedIds.Count == 0)
        {
            // prop-1, prop-2, prop-3
            SelectedCabinetFamilyInstances.Clear();
            ChosenCabinetType = ("", "");
            //ChosenCabinetTypeText = "";
            //ChosenCabinetType = null;
            SelectionIsCabinetsOnly = false;
            return;
        }

        // Check if all selected elements are Casework or Millwork
        List<FamilyInstance> cabinetInstances;
        bool selectionIsCabinetsOnly = AllElementsAreCabinets(selectedIds, doc, out cabinetInstances);

        // Exit if no cabinet is selected
        if (!selectionIsCabinetsOnly)
        {

            // prop-1, prop-2, prop-3
            SelectedCabinetFamilyInstances.Clear();
            ChosenCabinetType = ("", "");
            //ChosenCabinetTypeText = "";
            //ChosenCabinetType = null;
            SelectionIsCabinetsOnly = false;
            return;
        }

        // prop-1
        SelectionIsCabinetsOnly = true;

        // check if all selected elements are of the same type
        // Since multiple cabinets are selected, we show type name if all are same type
        bool allCabinetsAreSameType = cabinetInstances
            .Select(cabinetInstance => cabinetInstance.Symbol.Name)
            .Distinct()
            .Count() == 1;

        // 1: Single cabinet or Multiple cabinets of same type is selected
        if (cabinetInstances.Count == 1 || allCabinetsAreSameType)
        {
            // a. Update the current UI state to show the current selected instance type name
            var cabinetTypename = cabinetInstances.First().Symbol.Name;
            string cabinetVendorNotes = GetParameterValue(cabinetInstances.First().Symbol, "Vendor_Notes") ?? string.Empty;
            // prop-3
            ChosenCabinetType = (cabinetTypename, cabinetVendorNotes);
            //ChosenCabinetTypeText = $"{cabinetTypename} | {cabinetVendorNotes}";
            //ChosenCabinetType = new List<string> { cabinetTypename, cabinetVendorNotes };

            AllCabinetsAreSameType = true;


        }
        // 2: Multipel cabinets with different types selected
        else
        {

            // prop-3
            //ChosenCabinetType = ("Varies", "Varies");
            ChosenCabinetTypeText = "Varies";
            AllCabinetsAreSameType = false;


            //ChosenCabinetType = new List<string> { "varies", "varies" };

        }

        // 3: Update the observable property to hold the selected cabinet instance
        foreach (var cabinetInstance in cabinetInstances)
        {

            // prop-2
            SelectedCabinetFamilyInstances.Add(cabinetInstance);
        }

        // 3. Show the available types for the selected cabinet instance

        AvailableCabinetTypes = cabinetInstances.First().Symbol.Family
            .GetFamilySymbolIds()
            .Select(id => doc.GetElement(id) as FamilySymbol)
            .Where(familySymbol => familySymbol != null)
            .Select(familySymbol => (
                familySymbol.Name,
                GetParameterValue(familySymbol, "Vendor_Notes") ?? string.Empty))
            .ToList();

        //AvailableCabinetTypes = cabinetInstances.First().Symbol.Family
        //    .GetFamilySymbolIds()
        //    .Select(id => doc.GetElement(id) as FamilySymbol)
        //    .Where(familySymbol => familySymbol != null)
        //    .Select(familySymbol => familySymbol.Name)
        //    .ToList();

    }

    private static string GetParameterValue(Element element, string parameterName)
    {
        // Find the parameter by name
        Parameter param = element.LookupParameter(parameterName);

        // If the parameter is found and has a value, return it as a string; otherwise, return null
        return param != null && param.HasValue ? param.AsString() : null;
    }


    private static bool AllElementsAreCabinets(ICollection<ElementId> allElementIds, Document doc, out List<FamilyInstance> selectedCabinetFamilyInstances)
    {
        // This is the `out` argument passed to this function
        selectedCabinetFamilyInstances = new List<FamilyInstance>();

        // Null checks
        if (doc == null || allElementIds == null || allElementIds.Count == 0)
        {
            return false;
        }

        // Check if all selected IDs are family instances
        foreach (var elementId in allElementIds)
        {
            var element = doc.GetElement(elementId);
            if (element == null || !(element is FamilyInstance))
            {
                return false;
            }
        }

        string[] validPrefixes = {
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

        foreach (var elementId in allElementIds)
        {
            var element = doc.GetElement(elementId);
            if (element == null || element.Category == null || element.Category.Name != "Casework")
            {
                return false;
            }

            var familyInstance = element as FamilyInstance;
            string familyName = familyInstance.Symbol.Family.Name;

            if (!validPrefixes.Any(prefix => familyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            selectedCabinetFamilyInstances.Add(familyInstance);
        }

        return true;
    }


    public ICommand UpdateTypeCommand { get; }
    private void HandleUpdateTypeCommand()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.Properties_UpdateCabinetFamilyType;
        APP.ExternalEvent?.Raise();
    }

    // Constructor
    public EK24Modify_ViewModel()
    {

        SelectedFamilyGroup = null;
        SelectedFamily = null;
        SelectedFamilyType = null;
        Familys = new List<Models.Revit.EKFamily>(); // Initialize Familys to an empty list

        UpdateTypeCommand = new RelayCommand(HandleUpdateTypeCommand);
    }
}
// */

