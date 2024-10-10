using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.Commands.Utils;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;

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
    private static ObservableCollection<Brand> _kitchenBrands = new ObservableCollection<Brand>(
        RevitBrandData.Brands
    );

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

    // Get the KitchenBrand from the Project Parameter, or set default to `Aristokraft`
    private void GetKitchenBrandFromProjectParamAndUpdateUI()
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
            if (
                string.IsNullOrEmpty(value)
                || !eagleKitchenBrands.Any(x =>
                    x.BrandName.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            //!allowedBrands.Any(b => b.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                ChosenKitchenBrand = defaultBrandName;
                return;
            }

            ChosenKitchenBrand = value;
            return;
        }
    }
    private string GetKitchenBrandFromProjectParam()
    {

        string defaultBrandName = "Aristokraft";
        var eagleKitchenBrands = RevitBrandData.BrandCatalogues;


        if (doc == null)
        {
            return defaultBrandName;
        }

        string parameterName = "KitchenBrand";

        // Get the ProjectInfo element
        ProjectInfo projectInfo = doc.ProjectInformation;

        if (projectInfo == null)
        {
            return defaultBrandName;
        }

        // Attempt to retrieve the parameter by name
        Parameter param = projectInfo.LookupParameter(parameterName);

        if (param == null)
        {
            return defaultBrandName;
        }
        else
        {
            string value = param.AsString();

            // Check if the value is null, empty, or not in the allowedBrands array
            if (
                string.IsNullOrEmpty(value)
                || !eagleKitchenBrands.Any(x =>
                    x.BrandName.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return defaultBrandName;
            }

            return value;
        }
    }

    // Set the KitchenBrand to the Project Parameter
    private void SetKitchenBrandProjectParamValue(string newValue)
    {
        if (doc == null)
            return;

        ProjectInfo projectInfo = doc.ProjectInformation;
        if (projectInfo == null)
            return;

        string parameterName = "KitchenBrand";
        Parameter param = projectInfo.LookupParameter(parameterName);
        if (param == null)
            return;

        if (param.IsReadOnly)
            return;

        // Chosen Brand should be a valid KitchenBrand
        if (newValue == null || newValue == "")
            return;
        if (
            string.IsNullOrEmpty(newValue)
            || !KitchenBrands.Any(x =>
                x.BrandName.Equals(newValue, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return;
        }

        // Begin a transaction to modify the document
        using (Transaction trans = new Transaction(doc, "Update Project Parameter 'KitchenBrand'"))
        {
            trans.Start();
            try
            {
                param.Set(newValue);

                // Commit the transaction
                trans.Commit();
                //TaskDialog.Show("Success", $"Parameter '{parameterName}' has been set to '{newValue}'.");
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


    /// TESTING: for now convert the following id: 9560558
    //private void WTF(string currentBrand, string currentSKEW, string targetBrand, string targetSKEW)

    public ICommand ChangeKitchenBrandCommand { get; }

    private void OnChangeKitchenBrand()
    {


        // Get the current Brand from the Project parameter, then use the binded value as the target value
        string currentKitchenBrand = GetKitchenBrandFromProjectParam();
        string targetBrand = ChosenBrand.BrandName;
        //HighlightFamilies();


        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        List<FamilyInstance> cabinets = FilterAllCabinets.FilterProjectForEagleCabinets(doc);

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<FamilyInstanceInfo> familyInstanceInfos = BrandMapper.GetFamilyInstanceInfos(cabinets);

        // Create a StringBuilder to store the information to be displayed
        StringBuilder cabinetInfo = new StringBuilder();

        // Create an instance of BrandMapper
        BrandMapper mapper = new BrandMapper();

        // Iterate over the family instance info and map the SKUs
        foreach (var familyInfo in familyInstanceInfos)
        {
            // Use BrandMapper to map the SKU to the target brand
            string mappedSKU = mapper.MapBrandSkews(familyInfo, targetBrand);

            if (!string.IsNullOrEmpty(mappedSKU))
            {
                // Append the mapping information to the StringBuilder
                cabinetInfo.AppendLine($"{familyInfo.BrandName}::{familyInfo.TypeName} => {targetBrand}::{mappedSKU}");
            }
            else
            {
                // If no mapping found, add a message
                cabinetInfo.AppendLine($"XXX :::: {familyInfo.BrandName}::{familyInfo.TypeName}");
            }
        }

        // Display the information using TaskDialog
        TaskDialog.Show("Cabinet SKU Mapping", cabinetInfo.ToString());

        // Set the project parameter value
        SetKitchenBrandProjectParamValue(targetBrand);

        // Now the window should be closed here so that the trasaction doesn't get discarded
        // because while the wpf window is open i can see the transaction, but when i close it, it's gone.
        return;
    }



    // Set the value of the Graphics Filter to the current chosen brand
    // BUG: for some reason this wouldn't work, no errors but the filters are not getting created





    // Constructor
    public ChangeBrandViewModel(Document doc)
    {
        this.doc = doc;
        GetKitchenBrandFromProjectParamAndUpdateUI();
        ChangeKitchenBrandCommand = new RelayCommand(OnChangeKitchenBrand);
    }
}
