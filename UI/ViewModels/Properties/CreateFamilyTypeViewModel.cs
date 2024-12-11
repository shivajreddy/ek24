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

    // This comes from the Revit utility class that deserializes the Resource data
    // before create UI instance
    public List<FamilyGroup> FamilyGroups { get; set; } = RevitFamilyGroups.FamilyGroups;

    public static List<EKFamily> _familys { get; set; }
    public static List<EKFamily> Familys
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

    public static List<EKFamilyType> _familyTypes { get; set; }
    public static List<EKFamilyType> FamilyTypes
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

    public static EKFamily _selectedFamily { get; set; }
    public static EKFamily SelectedFamily
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


    public static EKFamilyType _selectedFamilyType { get; set; }
    public static EKFamilyType SelectedFamilyType
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
            Familys = new List<EKFamily>();
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
            FamilyTypes = new List<EKFamilyType>();
        }
    }


    public ICommand CreateNewFamilyCommand { get; }
    private void HandleCreateNewFamilyCommand()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.Properties_CreateNewFamilyAndType;
        APP.ExternalEvent?.Raise();
    }

    public CreateFamilyTypeViewModel()
    {
        SelectedFamilyGroup = null;
        SelectedFamily = null;
        SelectedFamilyType = null;
        Familys = new List<EKFamily>(); // Initialize Familys to an empty list
        CreateNewFamilyCommand = new RelayCommand(HandleCreateNewFamilyCommand);
    }


}
