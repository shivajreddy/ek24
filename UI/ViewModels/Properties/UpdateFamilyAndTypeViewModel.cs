using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ek24.UI.ViewModels.Properties;


public class UpdateFamilyAndTypeViewModel : INotifyPropertyChanged
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


    private static Selection _currentSelection { get; set; }
    public static Selection CurrentSelection
    {
        get => _currentSelection;
        set
        {
            if (_currentSelection != value)
            {
                _currentSelection = value;
                OnStaticPropertyChanged(nameof(CurrentSelection));
                //UpdateSelectionProperties();
            }

        }
    }

    private static int _totalSelections;
    public static int TotalSelections
    {
        get => _totalSelections;
        set
        {
            if (_totalSelections != value)
            {
                _totalSelections = value;
                OnStaticPropertyChanged(nameof(TotalSelections));
            }
        }
    }

    private static bool _isGridVisible;
    public static bool IsGridVisible
    {
        get => _isGridVisible;
        set
        {
            if (_isGridVisible != value)
            {
                _isGridVisible = value;
                OnStaticPropertyChanged(nameof(IsGridVisible));
            }
        }
    }

    private static bool _isTypeComboBoxEnabled;
    public static bool IsTypeComboBoxEnabled
    {
        get => _isTypeComboBoxEnabled;
        set
        {
            if (_isTypeComboBoxEnabled != value)
            {
                _isTypeComboBoxEnabled = value;
                OnStaticPropertyChanged(nameof(IsTypeComboBoxEnabled));
            }
        }
    }

    private static ObservableCollection<string> _availableTypes;
    public static ObservableCollection<string> AvailableTypes
    {
        get => _availableTypes;
        set
        {
            if (_availableTypes != value)
            {
                _availableTypes = value;
                OnStaticPropertyChanged(nameof(AvailableTypes));
            }
        }
    }

    private static ObservableCollection<string> _availableFamilyTypes;
    public static ObservableCollection<string> AvailableFamilyTypes
    {
        get => _availableFamilyTypes;
        set
        {
            if (_availableFamilyTypes != value)
            {
                _availableFamilyTypes = value;
                OnStaticPropertyChanged(nameof(AvailableFamilyTypes));
            }
        }
    }

    public UpdateFamilyAndTypeViewModel()
    {
        AvailableTypes = new ObservableCollection<string>();
        AvailableFamilyTypes = new ObservableCollection<string>();
    }

    //private static void UpdateSelectionProperties()
    public static void UpdateSelectionProperties(Document doc)
    {
        if (CurrentSelection == null)
        {
            TotalSelections = 0;
            IsGridVisible = false;
            return;
        }

        var selectedIds = CurrentSelection.GetElementIds();
        TotalSelections = selectedIds.Count;

        var caseworkInstances = selectedIds
            .Select(id => doc.GetElement(id))
            .Where(e => e is FamilyInstance && (e as FamilyInstance).Symbol.Family.FamilyCategory.Name == "Casework")
            .Cast<FamilyInstance>()
            .ToList();

        IsGridVisible = caseworkInstances.Any();
        IsTypeComboBoxEnabled = caseworkInstances.Count == 1;

        if (IsTypeComboBoxEnabled)
        {
            var instance = caseworkInstances.First();
            AvailableTypes = new ObservableCollection<string>(
                instance.Symbol.Family.GetFamilySymbolIds()
                    .Select(id => doc.GetElement(id).Name)
            );
        }
        else
        {
            AvailableTypes.Clear();
        }

        if (caseworkInstances.Any())
        {
            var allCaseworkFamilyTypes = caseworkInstances
                .SelectMany(i => i.Symbol.Family.GetFamilySymbolIds())
                .Distinct()
                .Select(id => doc.GetElement(id).Name);
            AvailableFamilyTypes = new ObservableCollection<string>(allCaseworkFamilyTypes);
        }
        else
        {
            AvailableFamilyTypes.Clear();
        }
    }


    //public List<(string, string)> AvailableFamilyTypes { get; set; }
    //public List<string> AvailableTypes { get; set; }


    //public UpdateFamilyAndTypeViewModel()
    //{
    //}

}
