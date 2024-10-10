using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ek24.UI.ViewModels.ChangeBrand;
using System.Collections.ObjectModel;
using ek24.UI.Commands;

namespace ek24.UI.ViewModels.ChangeBrand;

public class ChangeBrandViewModel : INotifyPropertyChanged
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

    private Document doc;

    // This comes from the Revit utility class that deserializes the Resource data
    // before create UI instance
    private static ObservableCollection<Brand> _kitchenBrands = new ObservableCollection<Brand>(RevitBrandData.Brands);


    public static ObservableCollection<Brand> KitchenBrands
    {
        get => _kitchenBrands;
        set
        {
            _kitchenBrands = value;
            OnStaticPropertyChanged(nameof(KitchenBrands));
        }
    }

    private Brand _chosenBrand;
    public Brand ChosenBrand
    {
        get => _chosenBrand;
        set
        {
            if (_chosenBrand != value)
            {
                _chosenBrand = value;
                OnPropertyChanged(nameof(ChosenBrand));
                OnPropertyChanged(nameof(ChosenKitchenBrand)); // Notify for BrandName if needed
            }
        }
    }

    // Property for displaying brand name
    //public string ChosenKitchenBrand => _chosenBrand?.BrandName;


    ///*
    private string _chosenKitchenBrand;
    public string ChosenKitchenBrand
    {
        get => _chosenKitchenBrand;
        set
        {
            if (_chosenKitchenBrand != value)
            {
                _chosenKitchenBrand = value;
                OnPropertyChanged(nameof(ChosenKitchenBrand));

            }
        }
    }
    //*/

    // Get the KitchenBrand from the Project Parameter, or set default to `Aristokraft`
    private void GetKitchenBrandFromProjectParam()
    {
        string defaultBrandName = "Aristokraft";
        var eagleKitchenBrands = RevitBrandData.BrandCatalogues;
        //string[] allowedBrands = { "Aristokraft", "Eclipse", "Yorktowne Historic", "Yorktowne Classic" };


        if (doc == null)
        {
            ChosenKitchenBrand = defaultBrandName;
        }

        string parameterName = "KitchenBrand";

        // Get the ProjectInfo element
        ProjectInfo projectInfo = doc.ProjectInformation;

        if (projectInfo == null)
        {
            //TaskDialog.Show("Error", "Project Information not found.");
            ChosenKitchenBrand = defaultBrandName;
            return;
        }

        // Attempt to retrieve the parameter by name
        Parameter param = projectInfo.LookupParameter(parameterName);

        if (param == null)
        {
            //TaskDialog.Show("Error", $"Parameter '{parameterName}' not found in Project Information.");
            ChosenKitchenBrand = defaultBrandName;
            return;
        }
        else
        {
            string value = param.AsString();

            // Check if the value is null, empty, or not in the allowedBrands array
            if (string.IsNullOrEmpty(value) ||
                !eagleKitchenBrands.Any(x => x.BrandName.Equals(value, StringComparison.OrdinalIgnoreCase)))
            //!allowedBrands.Any(b => b.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                ChosenKitchenBrand = defaultBrandName;
                return;
            }

            ChosenKitchenBrand = value;
            return;
        }
    }

    // Set the KitchenBrand to the Project Parameter
    private void SetKitchenBrandFromProjectParam()
    {
        if (doc == null) return;

        ProjectInfo projectInfo = doc.ProjectInformation;
        if (projectInfo == null) return;

        string parameterName = "KitchenBrand";
        Parameter param = projectInfo.LookupParameter(parameterName);
        if (param == null) return;

        if (param.IsReadOnly) return;

        string newValue = ChosenBrand?.BrandName;

        // Chosen Brand should be a valid KitchenBrand
        if (newValue == null || newValue == "") return;
        if (string.IsNullOrEmpty(newValue) ||
             !KitchenBrands.Any(x => x.BrandName.Equals(newValue, StringComparison.OrdinalIgnoreCase)))
        { return; }

        // Begin a transaction to modify the document
        using (Transaction trans = new Transaction(doc, "Update Project Parameter 'KitchenBrand'"))
        {
            trans.Start();
            try
            {
                param.Set(newValue);

                // Commit the transaction
                trans.Commit();
                TaskDialog.Show("Success", $"Parameter '{parameterName}' has been set to '{newValue}'.");
            }
            catch (Exception ex)
            {
                // Roll back the transaction in case of error
                trans.RollBack();
                TaskDialog.Show("Error", $"Failed to set parameter: {ex.Message}");
                return;
            }

        }
    }

    private void GetAllCabinets()
    {
        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);


        var cabinets = collector.ToElements();
    }


    public ICommand ChangeKitchenBrandCommand { get; }
    private void OnChangeKitchenBrand()
    {
        SetKitchenBrandFromProjectParam();
    }

    // Constructor
    public ChangeBrandViewModel(Document doc)
    {
        this.doc = doc;
        GetKitchenBrandFromProjectParam();
        ChangeKitchenBrandCommand = new RelayCommand(OnChangeKitchenBrand);
    }

}
