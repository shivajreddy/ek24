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


public class CreateBrandTypeViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // This comes from the Revit utility class that deserializes & maps data
    public List<BrandCatalogue> BrandCatalogues { get; set; } = RevitBrandData.BrandCatalogues;

    //public List<FamilyType> _brandFamilyTypes { get; set; } = new List<FamilyType>();
    public List<FamilyType> _brandFamilyTypes { get; set; }
    public List<FamilyType> BrandFamilyTypes
    {
        get => _brandFamilyTypes;
        set
        {
            if (_brandFamilyTypes != value)
            {
                _brandFamilyTypes = value;
                OnPropertyChanged(nameof(BrandFamilyTypes));
            }
        }
    }

    public BrandCatalogue _selectedBrandCatalogue { get; set; }
    public BrandCatalogue SelectedBrandCatalogue
    {
        get => _selectedBrandCatalogue;
        set
        {
            if (_selectedBrandCatalogue != value)
            {
                _selectedBrandCatalogue = value;
                OnPropertyChanged($"{nameof(SelectedBrandCatalogue)}");
                UpdateTypes();
            }
        }
    }

    public FamilyType _selectedBrandFamilyType { get; set; }
    public FamilyType SelectedBrandFamilyType
    {
        get => _selectedBrandFamilyType;
        set
        {
            if (_selectedBrandFamilyType != value)
            {
                _selectedBrandFamilyType = value;
                OnPropertyChanged($"{nameof(SelectedBrandFamilyType)}");
            }
        }
    }
    private void UpdateTypes()
    {
        // Ensure that a brand is selected
        // If no brand is selected or no matching BrandCatalogue is found, FamilyTypes will remain empty
        if (SelectedBrandCatalogue == null) return;

        BrandFamilyTypes.Clear();

        //FamilyTypes = SelectedBrandCatalogue.FamilyTypes;

        // Get the brand name
        string brandName = SelectedBrandCatalogue.BrandName;

        // Find the corresponding BrandCatalogue for the selected brand
        var chosenBrandCatalogue = RevitBrandData.BrandCatalogues
                                .FirstOrDefault(catalogue => catalogue.BrandName.Equals(brandName, StringComparison.OrdinalIgnoreCase));

        // If a BrandCatalogue is found, add the associated FamilyTypes to the FamilyTypes collection
        if (chosenBrandCatalogue != null)
        {
            BrandFamilyTypes.AddRange(chosenBrandCatalogue.FamilyTypes);
        }
    }

    public ICommand CreateNewFamilyCommand { get; }
    private void OnCreateNewFamily()
    {
        //GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.RevitNew_FamilyAndType;
        APP.ExternalEvent?.Raise();
    }

    public CreateBrandTypeViewModel()
    {
        SelectedBrandCatalogue = null;
        SelectedBrandFamilyType = null;
        BrandFamilyTypes = new List<FamilyType>();

        //BrandFamilyTypes = RevitBrandData.BrandCatalogues[0].FamilyTypes;
        //BrandFamilyTypes.AddRange(RevitBrandData.BrandCatalogues[1].FamilyTypes);
        //BrandFamilyTypes.AddRange(RevitBrandData.BrandCatalogues[2].FamilyTypes);
        //BrandFamilyTypes.AddRange(RevitBrandData.BrandCatalogues[3].FamilyTypes);

        CreateNewFamilyCommand = new RelayCommand(OnCreateNewFamily);
    }
}
