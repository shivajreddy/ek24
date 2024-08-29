using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;


namespace ek24.UI.ViewModels.Properties;


public class InstanceParamsViewModel : INotifyPropertyChanged
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

    private static bool _selectionIsCaseWorkOnly { get; set; }
    public static bool SelectionIsCaseWorkOnly
    {
        get => _selectionIsCaseWorkOnly;
        set
        {
            if (_selectionIsCaseWorkOnly != value)
            {
                _selectionIsCaseWorkOnly = value;
                OnStaticPropertyChanged(nameof(SelectionIsCaseWorkOnly));

            }
        }
    }

    private static bool _selectionIsOneInstance { get; set; }
    public static bool SelectionIsOneInstance
    {
        get => _selectionIsOneInstance;
        set
        {
            if (_selectionIsOneInstance != value)
            {
                _selectionIsOneInstance = value;
                OnStaticPropertyChanged(nameof(SelectionIsOneInstance));
                UpdateAvailableVendorStyles();
            }
        }
    }


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

    // Add a property to hold the currently selected Cabinet FamilyInstance
    private static FamilyInstance _selectedCabinetFamilyInstance;
    public static FamilyInstance SelectedCabinetFamilyInstance
    {
        get => _selectedCabinetFamilyInstance;
        set
        {
            if (_selectedCabinetFamilyInstance != value)
            {
                _selectedCabinetFamilyInstance = value;
                OnStaticPropertyChanged(nameof(SelectedCabinetFamilyInstance));
            }
        }
    }

    private static List<string> _availableVendorStyles;
    public static List<string> AvailableVendorStyles
    {
        get => _availableVendorStyles;
        set
        {
            if (_availableVendorStyles != value)
            {
                _availableVendorStyles = value;
                OnStaticPropertyChanged(nameof(AvailableVendorStyles));
            }
        }
    }
    public static (string, string) _chosenVendorStyleInstance { get; set; }
    public static (string, string) ChosenVendorStyleInstance
    {
        get => _chosenVendorStyleInstance;
        set
        {
            if (_chosenVendorStyleInstance != value)
            {
                _chosenVendorStyleInstance = value;
                OnStaticPropertyChanged(nameof(ChosenVendorStyleInstance));
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
    public static void SyncCurrentSelectionWithInstanceParamsViewModel(Selection currentSelection, Document doc)
    {
        var selectedIds = currentSelection.GetElementIds();

        // Check if all selected elements are Casework or Millwork
        SelectionIsCaseWorkOnly = AllElementsAreCabinets(selectedIds, doc);

        // Check if there is exactly one instance selected
        SelectionIsOneInstance = selectedIds.Count() == 1;

        if (SelectionIsCaseWorkOnly && SelectionIsOneInstance)
        {
            // Get the selected element
            Element selectedElement = doc.GetElement(selectedIds.First());
            if (selectedElement is FamilyInstance familyInstance)
            {
                // Save the selected FamilyInstance
                SelectedCabinetFamilyInstance = familyInstance;

                // Clear existing FamilyTypes
                CurrentSelectionFamilySymbols.Clear();

                // Get the FamilySymbol associated with the selected FamilyInstance
                FamilySymbol symbol = familyInstance.Symbol;

                // Add the associated FamilyTypes to the FamilyTypes collection
                CurrentSelectionFamilySymbols.Add(symbol);

                // get the brand name
                Parameter vendorNameParam = familyInstance.Symbol.LookupParameter("Vendor_Name");
                string vendorName = vendorNameParam != null ? vendorNameParam.AsValueString() : "";

                // get the list of style names availabe for this vendor-name
                List<string> availableStyleNames = new List<string>();

                /*
                 The 4 possible values that i get for the Vendor_Name parameter value
                "YORKTOWNE-HISTORIC"
                "Yorktowne_Classic"
                "Aristokraft" 
                 "Eclipse"

                 the BrandName ppy on the 4 RevitBrandData.Brands items
                 "Yorktowne Historic"
                 "Yorktowne Classic"
                 "Aristokraft"
                 "Eclipse by Shiloh"
                 The mapping of Vendor_Name parameter values to BrandName
                */
                var vendorNameToBrandNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "YORKTOWNE-HISTORIC", "Yorktowne Historic" },
                        { "Yorktowne_Classic", "Yorktowne Classic" },
                        { "Aristokraft", "Aristokraft" },
                        { "Eclipse", "Eclipse by Shiloh" }
                    };

                // Check if the vendorName is in the map
                if (!string.IsNullOrEmpty(vendorName) && vendorNameToBrandNameMap.ContainsKey(vendorName))
                {
                    // Get the corresponding BrandName
                    string brandName = vendorNameToBrandNameMap[vendorName];

                    // Find the brand with the matching BrandName
                    var matchingBrand = RevitBrandData.Brands.FirstOrDefault(b => b.BrandName.Equals(brandName, StringComparison.OrdinalIgnoreCase));

                    if (matchingBrand != null)
                    {
                        // Extract the style names
                        availableStyleNames = matchingBrand.Styles.Select(s => s.StyleName).ToList();
                    }
                }

                // Assign to AvailableVendorStyles or handle as needed
                AvailableVendorStyles = availableStyleNames;
            }
        }
        else
        {
            // No cabinet instance is selected
            SelectedCabinetFamilyInstance = null;
            AvailableVendorStyles = new List<string>();
        }
    }

    private static void UpdateAvailableVendorStyles()
    {
    }


    private static string GetParameterValue(Element element, string parameterName)
    {
        // Find the parameter by name
        Parameter param = element.LookupParameter(parameterName);

        // If the parameter is found and has a value, return it as a string; otherwise, return null
        return param != null && param.HasValue ? param.AsString() : null;
    }


    private static bool AllElementsAreCabinets(ICollection<ElementId> allElementIds, Document doc)
    {
        if (doc == null || allElementIds == null || allElementIds.Count == 0)
        {
            return false;
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
            "YTH-T-",
            };

        foreach (var elementId in allElementIds)
        {
            var element = doc.GetElement(elementId);
            if (element == null || element.Category == null)
            {
                return false;
            }

            if (element.Category.Name != "Casework")
            {
                return false;
            }

            if (!(element is FamilyInstance familyInstance))
            {
                return false;
            }

            string familyName = familyInstance.Symbol.Family.Name;
            if (!validPrefixes.Any(prefix => familyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }


    public ICommand UpdateTypeCommand { get; }

    public InstanceParamsViewModel()
    {
        CurrentSelectionFamilySymbols = new List<FamilySymbol>();

        UpdateTypeCommand = new RelayCommand(HandleUpdateType);
    }
    private void HandleUpdateType()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.Properties_UpdateCabinetType;
        APP.ExternalEvent?.Raise();
    }

}
