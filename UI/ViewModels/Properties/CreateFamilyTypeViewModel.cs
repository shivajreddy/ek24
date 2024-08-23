using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Properties;
using ek24.UI.Models.Revit;



namespace ek24.UI.ViewModels.Properties;


public class CreateFamilyTypeViewModel : INotifyPropertyChanged
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

    // This comes from the Revit utility class that deserializes the Resource data
    // before create UI instance
    public List<FamilyGroup> FamilyGroups { get; set; } = RevitFamilyGroups.FamilyGroups;

    public static List<Family> _familys { get; set; }
    public static List<Family> Familys
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

    public static List<FamilyType> _familyTypes { get; set; }
    public static List<FamilyType> FamilyTypes
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

    public static Family _selectedFamily { get; set; }
    public static Family SelectedFamily
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


    public static FamilyType _selectedFamilyType { get; set; }
    public static FamilyType SelectedFamilyType
    {
        get => _selectedFamilyType;
        set
        {
            if (_selectedFamilyType != value)
            {
                _selectedFamilyType = value;
                OnStaticPropertyChanged($"{nameof(SelectedFamilyType)}");
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
            Familys = new List<Family>();
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
            FamilyTypes = new List<FamilyType>();
        }
    }


    public ICommand CreateNewFamilyCommand { get; }
    private void OnCreateNewFamily()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.Properties_FamilyAndType;
        APP.ExternalEvent?.Raise();
    }

    public CreateFamilyTypeViewModel()
    {
        SelectedFamilyGroup = null;
        SelectedFamily = null;
        SelectedFamilyType = null;
        Familys = new List<Family>(); // Initialize Familys to an empty list
        CreateNewFamilyCommand = new RelayCommand(OnCreateNewFamily);
    }


}
