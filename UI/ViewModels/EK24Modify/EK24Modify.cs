using ek24.Dtos;
using System.Collections.Generic;
using System.ComponentModel;


namespace ek24.UI;

public class EK24Modify_ViewModel : INotifyPropertyChanged
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    private static void OnStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    // TODO : TEST WITH HARD CODE OPTIONS, FIX LATER
    // Static lists exposed as public properties
    public List<string> BrandItems { get; } = EKBrands.all_brand_names;
    public List<string> CategoryItems { get; set; } = new List<string> { "C1-A", "C1-B", "C1-C", "C1-D" };
    public List<string> ConfigurationItems { get; set; } = new List<string> { "C2-A", "C2-B", "C2-C", "C2-D" };
    public List<string> SKUItems { get; set; } = new List<string> { "SKU-A", "SKU-B", "SKU-C", "SKU-D" };
    //public List<string> Category1Items { get; set; } = [];
    //public List<string> Category2Items { get; set; } = [];
    //public List<string> SKUItems { get; set; } = [];

    // TODO: ALL the getters should look at all EKFamilySymbols and filter available optiosn based on 
    // the previous selections
    public static string _selectedBrand { get; set; }
    public static string SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (_selectedBrand != value)
            {
                _selectedBrand = value;
                OnStaticPropertyChanged(nameof(SelectedBrand));
            }
        }
    }

    public static string _selectedCategory { get; set; }
    public static string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnStaticPropertyChanged(nameof(SelectedCategory));
                //UpdateFamilys();
            }
        }
    }

    public static string _selectedConfiguration { get; set; }
    public static string SelectedConfiguration
    {
        get => _selectedConfiguration;
        set
        {
            if (_selectedConfiguration != value)
            {
                _selectedConfiguration = value;
                OnStaticPropertyChanged(nameof(SelectedConfiguration));
                //UpdateFamilys();
            }
        }
    }

    public static string _selectedSKU { get; set; }
    public static string SelectedSKU
    {
        get => _selectedSKU;
        set
        {
            if (_selectedSKU != value)
            {
                _selectedSKU = value;
                OnStaticPropertyChanged(nameof(SelectedSKU));
                //UpdateFamilys();
            }
        }
    }


    // Constructor
    public EK24Modify_ViewModel()
    {
        SelectedBrand = null;
        SelectedCategory = null;
        SelectedConfiguration = null;
        SelectedSKU = null;
    }
}


/*
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
*/

