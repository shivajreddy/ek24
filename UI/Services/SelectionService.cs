using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using ek24.RequestHandling;
using ek24.UI.Commands;


namespace ek24.UI.Services;


/// <summary>
/// Static utility class for managing selection state.
/// </summary>
public static class SelectionService
{

    ///<summary>
    /// All properties in this class are static and can be accessed from anywhere in the application.
    /// We only need to handle static property changes, so we only need to implement the static PropertyChanged event.
    ///</summary>
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
            }
        }
    }

    //public static List<(string, string)> AvailableCabinetTypes { get; private set; } = new List<(string, string)>();
    public static List<(string, string)> _availableCabinetTypes { get; set; }
    public static List<(string, string)> AvailableCabinetTypes
    {
        get => new List<(string, string)>();
        set
        {
            if (_availableCabinetTypes != value)
            {
                _availableCabinetTypes = value;
                OnStaticPropertyChanged(nameof(AvailableCabinetTypes));
            }
        }
    }

    public static FamilyInstance SelectedFamilyInstance { get; private set; }
    public static List<FamilySymbol> CurrentSelectionFamilySymbols { get; private set; } = new List<FamilySymbol>();




    public static void SyncSelectionWithRevit(Selection currentSelection, Document doc)
    {
        var selectedIds = currentSelection.GetElementIds();

        SelectionIsCaseWorkOnly = AllElementsAreCaseworkMillWork(selectedIds, doc);
        SelectionIsOneInstance = selectedIds.Count() == 1;

        if (SelectionIsCaseWorkOnly && SelectionIsOneInstance)
        {
            Element selectedElement = doc.GetElement(selectedIds.First());
            if (selectedElement is FamilyInstance familyInstance)
            {
                SelectedFamilyInstance = familyInstance;
                CurrentSelectionFamilySymbols.Clear();
                FamilySymbol symbol = familyInstance.Symbol;
                CurrentSelectionFamilySymbols.Add(symbol);

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
            SelectedFamilyInstance = null;
            CurrentSelectionFamilySymbols.Clear();
            AvailableCabinetTypes.Clear();
        }
    }

    private static bool AllElementsAreCaseworkMillWork(ICollection<ElementId> allElementIds, Document doc)
    {
        return doc != null
            && allElementIds != null
            && allElementIds.Count != 0
            && allElementIds.All(elementId =>
            {
                var element = doc.GetElement(elementId);
                return element != null && element.Category != null && element.Category.Name == "Casework";
            });
    }

    private static string GetParameterValue(Element element, string parameterName)
    {
        Parameter param = element.LookupParameter(parameterName);
        return param != null && param.HasValue ? param.AsString() : null;
    }

}
