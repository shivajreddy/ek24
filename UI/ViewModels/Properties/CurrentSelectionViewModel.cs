using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using ek24.RequestHandling;
using ek24.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ek24.UI.ViewModels.Properties;


public class CurrentSelectionViewModel : INotifyPropertyChanged
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


    public static bool SelectionIsCaseWorkOnly { get; set; }
    public static bool SelectionIsOneInstance { get; set; }


    public static List<FamilySymbol> _currentSelectionFamilySymbols { get; set; }
    public static List<FamilySymbol> CurrentSelectionFamilySymbols
    {
        get => _currentSelectionFamilySymbols;
        set
        {
            if (_currentSelectionFamilySymbols != value)
            {
                _currentSelectionFamilySymbols = value;
                OnStaticPropertyChanged(nameof(CurrentSelectionFamilySymbols));
            }
        }
    }

    // Add a property to hold the currently selected FamilyInstance
    private static FamilyInstance _selectedFamilyInstance;
    public static FamilyInstance SelectedFamilyInstance
    {
        get => _selectedFamilyInstance;
        set
        {
            if (_selectedFamilyInstance != value)
            {
                _selectedFamilyInstance = value;
                OnStaticPropertyChanged(nameof(SelectedFamilyInstance));
            }
        }
    }

    public static List<(string, string)> _availableCabinetTypes { get; set; }
    public static List<(string, string)> AvailableCabinetTypes
    {
        get => _availableCabinetTypes;
        set
        {
            if (_availableCabinetTypes != value)
            {
                _availableCabinetTypes = value;
                OnStaticPropertyChanged(nameof(AvailableCabinetTypes));
            }
        }
    }
    public static (string, string) _chosenCabinetType { get; set; }
    public static (string, string) ChosenCabinetType
    {
        get => _chosenCabinetType;
        set
        {
            if (_chosenCabinetType != value)
            {
                _chosenCabinetType = value;
                OnStaticPropertyChanged(nameof(ChosenCabinetType));
            }
        }
    }


    public static string _chosenFamilySymbol { get; set; }
    public static string ChosenFamilySymbol
    {
        get => _chosenFamilySymbol;
        set
        {
            if (_chosenFamilySymbol != value)
            {
                _chosenFamilySymbol = value;
                OnStaticPropertyChanged(nameof(ChosenFamilySymbol));
            }
        }
    }


    /// <summary>
    /// Sets all the properties in the view model that will be used by the view.
    /// Is executed by the selection-event handler function
    /// </summary>
    public static void SyncViewModelWithRevit(Selection currentSelection, Document doc)
    {
        var selectedIds = currentSelection.GetElementIds();

        // Check if all selected elements are Casework or Millwork
        SelectionIsCaseWorkOnly = AllElementsAreCaseworkMillWork(selectedIds, doc);

        // Check if there is exactly one instance selected
        SelectionIsOneInstance = selectedIds.Count() == 1;

        if (SelectionIsCaseWorkOnly && SelectionIsOneInstance)
        {
            // Get the selected element
            Element selectedElement = doc.GetElement(selectedIds.First());
            if (selectedElement is FamilyInstance familyInstance)
            {
                // Save the selected FamilyInstance
                SelectedFamilyInstance = familyInstance;

                // Clear existing FamilyTypes
                CurrentSelectionFamilySymbols.Clear();

                // Get the FamilySymbol associated with the selected FamilyInstance
                FamilySymbol symbol = familyInstance.Symbol;

                // Add the associated FamilyTypes to the FamilyTypes collection
                CurrentSelectionFamilySymbols.Add(symbol);

                // Update AvailableCabinetTypes with the names of the available FamilySymbols and their Vendor_Notes values
                AvailableCabinetTypes = symbol.Family
                    .GetFamilySymbolIds()
                    .Select(id => doc.GetElement(id) as FamilySymbol)
                    .Where(familySymbol => familySymbol != null)
                    .Select(familySymbol => (
                        familySymbol.Name,
                        GetParameterValue(familySymbol, "Vendor_Notes") ?? string.Empty))
                    .ToList();
            }
        }
        else
        {
            // If not casework or not a single instance, clear the AvailableCabinetTypes
            AvailableCabinetTypes = new List<(string, string)>();
        }
    }
    private static string GetParameterValue(Element element, string parameterName)
    {
        // Find the parameter by name
        Parameter param = element.LookupParameter(parameterName);

        // If the parameter is found and has a value, return it as a string; otherwise, return null
        return param != null && param.HasValue ? param.AsString() : null;
    }

    private static bool AllElementsAreCaseworkMillWork(ICollection<ElementId> allElementIds, Document doc)
    {
        //return doc != null && allElementIds != null && allElementIds.Count != 0 && allElementIds.All(elementId => doc.GetElement(elementId).Category.Name == "Casework");
        return doc != null
            && allElementIds != null
            && allElementIds.Count != 0
            && allElementIds.All(elementId =>
            {
                var element = doc.GetElement(elementId);
                return element != null
                       && element.Category != null
                       && element.Category.Name == "Casework";
            });
    }

    public ICommand UpdateTypeCommand { get; }

    public CurrentSelectionViewModel()
    {
        CurrentSelectionFamilySymbols = new List<FamilySymbol>();

        UpdateTypeCommand = new RelayCommand(HandleUpdateType);
    }
    private void HandleUpdateType()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.RevitNew_UpdateCabinetType;
        APP.ExternalEvent?.Raise();
    }


}
