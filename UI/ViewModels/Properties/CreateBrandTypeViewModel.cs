using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;


namespace ek24.UI.ViewModels.Properties;


public class CreateBrandTypeViewModel : INotifyPropertyChanged
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

    // This comes from the Revit utility class that deserializes & maps data
    public List<BrandCatalogue> BrandCatalogues { get; set; } = RevitBrandData.BrandCatalogues;

    /*
    private ObservableCollection<FamilyType> _brandFamilyTypes = new ObservableCollection<FamilyType>();
    public ObservableCollection<FamilyType> BrandFamilyTypes
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
    */

    public class FamilyTypeWithNotes
    {
        public EKFamilyType familyType { get; set; }
        public string notes { get; set; }
        public string DisplayText => $"{familyType.TypeName}    {notes}";
    }

    private ObservableCollection<FamilyTypeWithNotes> _brandFamilyTypesWithNotes = new ObservableCollection<FamilyTypeWithNotes>();
    public ObservableCollection<FamilyTypeWithNotes> BrandFamilyTypesWithNotes
    {
        get => _brandFamilyTypesWithNotes;
        set
        {
            if (_brandFamilyTypesWithNotes != value)
            {
                _brandFamilyTypesWithNotes = value;
                OnPropertyChanged(nameof(BrandFamilyTypesWithNotes));
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
                OnPropertyChanged(nameof(SelectedBrandCatalogue));
                UpdateTypes();
            }
        }
    }

    /*
    public static EKFamilyType _selectedBrandFamilyType { get; set; }
    public static EKFamilyType SelectedBrandFamilyType
    {
        get => _selectedBrandFamilyType;
        set
        {
            if (_selectedBrandFamilyType != value)
            {
                _selectedBrandFamilyType = value;
                OnStaticPropertyChanged(nameof(SelectedBrandFamilyType));
            }
        }
    }
    */

    public static FamilyTypeWithNotes _selectedBrandFamilyTypeWithNotes { get; set; }
    public static FamilyTypeWithNotes SelectedBrandFamilyTypeWithNotes
    {
        get => _selectedBrandFamilyTypeWithNotes;
        set
        {
            if (_selectedBrandFamilyTypeWithNotes != value)
            {
                _selectedBrandFamilyTypeWithNotes = value;
                OnStaticPropertyChanged(nameof(SelectedBrandFamilyTypeWithNotes));
            }
        }
    }

    private string _searchTerm;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                UpdateTypes(); // Update types whenever the search term changes
            }
        }
    }

    private void UpdateTypes()
    {
        // Ensure that a brand is selected
        // If no brand is selected or no matching BrandCatalogue is found, FamilyTypes will remain empty
        if (SelectedBrandCatalogue == null)
        {
            BrandFamilyTypesWithNotes.Clear();
            return;
        }

        // Clear the existing items efficiently
        BrandFamilyTypesWithNotes.Clear();

        List<EKCabinetType> ekCabinetTypes = new List<EKCabinetType>();

        foreach (EKFamilyType familyType in SelectedBrandCatalogue.FamilyTypes)
        {
            string typeName = familyType.TypeName;

            // Check each cabinet family in ProjectCabinetFamilies
            foreach (EKCabinetFamily cabinetFamily in ProjectCabinetFamilies.CabinetFamilies)
            {
                if (cabinetFamily.TypeNames != null)
                {
                    // Find matching types
                    var matchingTypes = cabinetFamily.TypeNames
                        .Where(type => type.TypeName == typeName);

                    // Add the matching EKCabinetType objects to the list
                    ekCabinetTypes.AddRange(matchingTypes);
                }
            }
        }

        Debug.Print("stop here");

        foreach (EKCabinetType ekCabinetType in ekCabinetTypes)
        {
            FamilyTypeWithNotes familyTypeWithNotes = new FamilyTypeWithNotes();

            EKFamilyType familyType = new EKFamilyType();
            familyType.TypeName = ekCabinetType.TypeName;

            familyTypeWithNotes.familyType = familyType;
            familyTypeWithNotes.notes = $"[ {ekCabinetType.Note} ]";

            // Search Term is empty
            if (string.IsNullOrEmpty(SearchTerm))
            {
                BrandFamilyTypesWithNotes.Add(familyTypeWithNotes);
            }
            // Search-Term Filtering
            else
            {
                string search_term = SearchTerm.ToLower();
                if (familyType.TypeName.ToLower().Contains(search_term) || familyTypeWithNotes.notes.ToLower().Contains(search_term))
                {
                    BrandFamilyTypesWithNotes.Add(familyTypeWithNotes);
                }

            }
        }
    }


    public ICommand CreateNewFamilyCommand { get; }
    private void HandleCreateNewFamilyCommand()
    {
        APP.RequestHandler.RequestType = RequestType.Properties_CreateNewFamilyAndTypeV2;
        APP.ExternalEvent?.Raise();
    }

    public CreateBrandTypeViewModel()
    {
        SelectedBrandCatalogue = null;
        SelectedBrandFamilyTypeWithNotes = null;

        CreateNewFamilyCommand = new RelayCommand(HandleCreateNewFamilyCommand);
    }
}
