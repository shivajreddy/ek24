using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Services;


namespace ek24.UI.ViewModels.Properties;

public class TypeParamsViewModel : INotifyPropertyChanged
{
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
    private static ObservableCollection<FamilyInstance> _selectedCabinetFamilyInstances;
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

    public static (string, string) _chosenCabinetType { get; set; } = ("", "");
    public static (string, string) ChosenCabinetType
    {
        get => _chosenCabinetType;
        set
        {
            _chosenCabinetType = value;
            OnStaticPropertyChanged(nameof(ChosenCabinetType));

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


        }
        // 2: Multipel cabinets with different types selected
        else
        {

            // prop-3
            //ChosenCabinetType = ("Varies", "Varies");
            ChosenCabinetTypeText = "Varies";
            //ChosenCabinetType = new List<string> { "varies", "varies" };

        }

        // 3: Update the observable property to hold the selected cabinet instance
        foreach (var cabinetInstance in cabinetInstances)
        {

            // prop-2
            SelectedCabinetFamilyInstances.Add(cabinetInstance);
        }

        // 3. Show the available types for the selected cabinet instance
        // /*
        AvailableCabinetTypes = cabinetInstances.First().Symbol.Family
            .GetFamilySymbolIds()
            .Select(id => doc.GetElement(id) as FamilySymbol)
            .Where(familySymbol => familySymbol != null)
            .Select(familySymbol => (
                familySymbol.Name,
                GetParameterValue(familySymbol, "Vendor_Notes") ?? string.Empty))
            .ToList();
        // */

        /*
        AvailableCabinetTypes = cabinetInstances.First().Symbol.Family
            .GetFamilySymbolIds()
            .Select(id => doc.GetElement(id) as FamilySymbol)
            .Where(familySymbol => familySymbol != null)
            .Select(familySymbol => familySymbol.Name)
            .ToList();
        // */

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

    public TypeParamsViewModel()
    {
        SelectedCabinetFamilyInstances = new ObservableCollection<FamilyInstance>();

        UpdateTypeCommand = new RelayCommand(HandleUpdateType);
    }
    private void HandleUpdateType()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.Properties_UpdateCabinetType;
        APP.ExternalEvent?.Raise();
    }

}


