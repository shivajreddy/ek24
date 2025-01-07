using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.UI.Commands;
using ek24.Utils;
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

    #region TEMPORARY DELETE LATER
    public string _latestProjectName { get; set; }
    public string LatestProjectName
    {
        get => _latestProjectName;
        set
        {
            if (_latestProjectName == value) return;
            _latestProjectName = value;
            OnPropertyChanged(nameof(LatestProjectName));
        }
    }
    #endregion

    #region All 4 Combo Box Values
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
    #endregion

    #region HELPER Functions to filter item sources
    public void filter_ekType_items()// based on: SelectedBrand
    {
        List<string> newEktypeItems = new List<string>(); // Reset the Category Items

        var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads
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

        var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads

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

        var ekCaseworkSymbols = EKEventsUtility.EKCaseworkSymbols;   // Gets created when document loads

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
    #endregion

    #region Chosen Revit Family Symbol & CURRENT SELECTION FROM REVIT UI

    public FamilySymbol _chosenRevitFamilySymbol { get; set; }
    public FamilySymbol ChosenRevitFamilySymbol
    {
        get => _chosenRevitFamilySymbol;
        set
        {
            if (_chosenRevitFamilySymbol == value) return;
            _chosenRevitFamilySymbol = value;
            OnPropertyChanged(nameof(EKFamilySymbol));
        }
    }


    #endregion

    #region Selected Item for ComboBoxes
    public string _selectedBrand { get; set; }
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

    public string _selectedEKType { get; set; }
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

    public string _selectedEKCategory { get; set; }
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

    public EK_SKU _selectedSKU { get; set; }
    public EK_SKU SelectedSKU
    {
        get => _selectedSKU;
        set
        {
            if (_selectedSKU != value)
            {
                _selectedSKU = value;
                OnPropertyChanged(nameof(SelectedSKU));
                ChosenRevitFamilySymbol = SelectedSKU?.RevitFamilySymbol;
            }
        }
    }
    #endregion

    // MAIN THING
    private readonly EK_Global_State _ek_Global_State;

    public int EK_Selection_Count
    {
        get
        {
            if (APP.global_state.Current_Project_State == null || APP.global_state.Current_Project_State.EKProjectsCurrentSelection == null) return 0;

            return APP.global_state.Current_Project_State.EKProjectsCurrentSelection.GetElementIds().Count;
        }
    }

    public Selection Current_Project_Revit_Selection
    {
        get => APP.global_state.Current_Project_State.EKProjectsCurrentSelection;
        set => APP.global_state.Current_Project_State.EKProjectsCurrentSelection = value;
    }

    // Handle the ppties that change on Global State
    private void GlobalState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State.EKProjectsCurrentSelection))
        {
            OnPropertyChanged(nameof(Current_Project_Revit_Selection));
            update_modify_ui_based_on_selection();
        }
        if (e.PropertyName == nameof(EK_Global_State.EKSelectionCount))
        {
            OnPropertyChanged(nameof(EK_Selection_Count));
        }
    }

    void update_modify_ui_based_on_selection()
    {
        // When selection is empty, clear the UI
        if (Current_Project_Revit_Selection.GetElementIds().Count == 0)
        {
            SelectedBrand = null;
        }
    }

    #region Constructor
    public EK24Modify_ViewModel()
    {
        _ek_Global_State = APP.global_state;
        _ek_Global_State.PropertyChanged += GlobalState_PropertyChanged;

        //_current_project_state = APP.global_state.current_project_state;

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
    public void Handle_Command_UpdateFamily()
    {
        Debug.WriteLine("Hello Update existing family");
    }
    public ICommand Command_CreateNewFamily { get; }
    public void Handle_Command_CreateNewFamily()
    {
        Debug.WriteLine("Chosen symbol is", ChosenRevitFamilySymbol);
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

