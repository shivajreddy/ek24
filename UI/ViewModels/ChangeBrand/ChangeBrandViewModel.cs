using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.Commands.Utils;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ek24.UI.ViewModels.ChangeBrand;

public class ChangeBrandViewModel : INotifyPropertyChanged
{
    private readonly UIDocument _uiDoc;
    private readonly Document _doc;

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

    #region Properties to bind to UI
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

    private Brand _chosenBrandFromDropdown;
    public Brand ChosenBrandFromDropdown
    {
        get => _chosenBrandFromDropdown;
        set
        {
            if (_chosenBrandFromDropdown != value)
            {
                _chosenBrandFromDropdown = value;
                OnPropertyChanged(nameof(ChosenBrandFromDropdown));
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
    #endregion

    private void UpdateProjectParam(Transaction trans)
    {
        // Get the Project Information element
        ProjectInfo projectInfo = _doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        if (kitchenBrandParam == null)
        {
            TaskDialog.Show("Error", "Parameter 'KitchenBrand' not found.");
            trans.RollBack();
            return;
        }

        string currentKitchenBrandParamValue = kitchenBrandParam.AsString();

        string newKitchenBrandValue = _chosenBrandFromDropdown.BrandName;

        if (currentKitchenBrandParamValue == newKitchenBrandValue)
        {
            TaskDialog.Show("Info", $"Current KitchenBrand value is '{currentKitchenBrandParamValue}'. No change made.");
            trans.RollBack();
            return;
        }

        kitchenBrandParam.Set(newKitchenBrandValue);

    }

    // TODO: Convert all cabinets to the new brand
    private void ConvertCabinets()
    {
        ProjectInfo projectInfo = _doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        // Get the current Brand from the Project parameter, then use the binded value as the target value
        string currentKitchenBrandParamValue = kitchenBrandParam.AsString();
        string newKitchenBrandValue = _chosenBrandFromDropdown.BrandName;

        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(_doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        List<FamilyInstance> cabinets = FilterEagleCabinetry.FilterProjectForEagleCabinets(_doc);

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<EagleCaseworkFamilyInstance> cabinetFaimlyInstances = BrandMapper.ConvertFamilyInstaceToEagleCaseworkFamilyInstance(cabinets);

        // Create a StringBuilder to store the information to be displayed
        StringBuilder resultMessage = new StringBuilder();
        int match_found_count = 0, match_not_found_count = 0;
        resultMessage.Append($"Match Found:{match_found_count}   Match NOT-Found: {match_not_found_count}");

        // Create an instance of BrandMapper
        BrandMapper mapper = new BrandMapper();


        // Iterate over the cabinet family instances and map the SKUs
        foreach (var cabinetFamilyInstance in cabinetFaimlyInstances)
        {
            // Use BrandMapper to map the SKU to the target brand
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, newKitchenBrandValue);

            if (!string.IsNullOrEmpty(mappedSKU))
            {
                match_found_count += 1;
                // Append the mapping information to the StringBuilder
                resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => {newKitchenBrandValue}-{mappedSKU}");
            }
            else
            {
                match_not_found_count += 1;
                // If no mapping found, add a message
                resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => XXX ");
            }
        }
        // Insert the totals at the top of the StringBuilder
        resultMessage.Insert(0, $"Match Found: {match_found_count}   Match NOT-Found: {match_not_found_count}\n");

        // Display the information using TaskDialog
        TaskDialog.Show("Cabinet SKU Mapping", resultMessage.ToString());

        // TODO 1:
        // Map all current family-types to valid new valid-family types
        var cabinetInstancesWithTargetFamilyType = new List<Tuple<EagleCaseworkFamilyInstance, string, string>>();

        // Mapping of brand names to their respective brand codes
        Dictionary<string, string> brandCodeMapping = new Dictionary<string, string>
        {
            { "Aristokraft", "Aristokraft" },
            { "Yorktowne Classic", "YTC" },
            { "Yorktowne Historic", "YTH" },
            { "Eclipse", "Eclipse" }
        };

        foreach (var cabinetFamilyInstance in cabinetFaimlyInstances)
        {
            // Use BrandMapper to find the target SKU in the new brand
            // the value we get is the target family's type name
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, newKitchenBrandValue);

            // If a valid SKU is found in the new brand, add the mapping to the list
            if (!string.IsNullOrEmpty(mappedSKU))
            {
                // Create the target family name
                // All the family names are in this order : "<Brand Code>-<something>-<something>"
                // And here are the brand codes: 
                //{ "Aristokraft", "Aristokraft" },
                //{ "Yorktowne Classic", "YTC" },
                //{ "Yorktowne Historic", "YTH" },
                //{ "Eclipse", "Eclipse" }
                // so if a family name is: "YTH-B-One Door One Drawer" and the newKitchenBrandValue = Yorktowne Classic
                // then the targetFamilyName should be "YTC-B-One Door One Drawer"
                // where as if the newKitchenBrandValue = Aristokraft then the targetFamilyName should be "Aristokraft-B-One Door One Drawer"
                // Split the current family name by the first hyphen to get the components

                string targetFamilyName = "";
                string[] familyNameParts = cabinetFamilyInstance.FamilyInstance.Symbol.Family.Name.Split(new[] { '-' }, 2);
                // Get the target brand code based on the new brand name
                if (brandCodeMapping.TryGetValue(newKitchenBrandValue, out string targetBrandCode))
                {
                    // Construct the new target family name
                    targetFamilyName = $"{targetBrandCode}-{familyNameParts[1]}";
                }
                string targetTypeName = mappedSKU;


                // Add the pair (CabinetFamilyInstance, new family name, new type name) to the list
                cabinetInstancesWithTargetFamilyType.Add(new Tuple<EagleCaseworkFamilyInstance, string, string>(
                    cabinetFamilyInstance,
                    targetFamilyName,   // New brand name
                    mappedSKU               // Mapped SKU/type name for the new brand
                ));
            }
        }


        // TODO 2:
        // Convert all valid FamilyTypes
        ChangeFamilyTypes(cabinetInstancesWithTargetFamilyType);
    }

    // This is given a list of pairs. 1st item in pair is a cabinet family instances that have the revit element id,
    // and the 2nd item in the pair is the target family&type to change to
    private void ChangeFamilyTypes(List<Tuple<EagleCaseworkFamilyInstance, string, string>> cabinetInstancesWithTargetFamilyType)
    {
        // StringBuilder to collect error messages
        StringBuilder errorMessages = new StringBuilder();

        // Loop through the list of cabinet instances and target family/type pairs
        foreach (var item in cabinetInstancesWithTargetFamilyType)
        {
            EagleCaseworkFamilyInstance currentInstance = item.Item1;
            string targetFamilyName = item.Item2;
            string targetTypeName = item.Item3;

            // Get the family instance to modify
            FamilyInstance familyInstance = currentInstance.FamilyInstance;

            // Find the target family symbol (family type) using family and type names
            FamilySymbol targetFamilySymbol = FindFamilySymbol(targetFamilyName, targetTypeName);
            if (targetFamilySymbol == null)
            {
                // Append error message to the StringBuilder
                errorMessages.AppendLine($"Family or type not found for {targetFamilyName} - {targetTypeName}");
                continue;
            }

            // Ensure that the target family symbol is loaded and active in the project
            if (!targetFamilySymbol.IsActive)
            {
                targetFamilySymbol.Activate();
                // Document.Regenerate might be needed after activation to ensure the change takes effect
                currentInstance.FamilyInstance.Document.Regenerate();
            }

            // Change the family type for the family instance
            currentInstance.FamilyInstance.Symbol = targetFamilySymbol;
        }

        // Show the task dialog with all error messages if there are any
        if (errorMessages.Length > 0)
        {
            TaskDialog.Show("Errors", errorMessages.ToString());
        }
    }

    // Helper method to find the family symbol (family type) by family name and type name
    private FamilySymbol FindFamilySymbol(string familyName, string typeName)
    {
        // Iterate through all the families in the document
        FilteredElementCollector collector = new FilteredElementCollector(_doc);
        collector.OfClass(typeof(Autodesk.Revit.DB.Family));

        foreach (Autodesk.Revit.DB.Family family in collector)
        {
            if (family.Name == familyName)
            {
                foreach (ElementId symbolId in family.GetFamilySymbolIds())
                {
                    FamilySymbol symbol = _doc.GetElement(symbolId) as FamilySymbol;
                    if (symbol.Name == typeName)
                    {
                        return symbol;
                    }
                }
            }
        }
        return null; // Return null if no matching family symbol is found
    }

    private void CreateViewFilter()
    {
        // Define the filter name
        string filterName = "Vendor Name Filter";

        // Get Active View
        Autodesk.Revit.DB.View view = _doc.ActiveView;

        // Create a list of categories that will be used for the filter
        IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Casework) };

        // Get the ElementId for the "Manufacturer" built-in parameter
        ElementId manufacturerParamId = new ElementId(BuiltInParameter.ALL_MODEL_MANUFACTURER);

        // Create a filter rule where "Manufacturer" equals "MY VALUE" (case-insensitive)

        // Get the Project Information element
        ProjectInfo projectInfo = _doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");
        string currentKitchenBrandParamValue = kitchenBrandParam.AsString();

        //FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateEqualsRule(manufacturerParamId, currentKitchenBrandParamValue);
        FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateNotEqualsRule(manufacturerParamId, currentKitchenBrandParamValue);

        // Create an ElementParameterFilter with the rule
        ElementParameterFilter parameterFilter = new ElementParameterFilter(manufacturerRule);

        // Attempt to find an existing filter with the same name
        ParameterFilterElement paramFilter = new FilteredElementCollector(_doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

        if (paramFilter == null)
        {
            // Create the ParameterFilterElement since it doesn't exist
            paramFilter = ParameterFilterElement.Create(_doc, filterName, categories, parameterFilter);
        }
        else
        {
            // Update the existing filter's element filter
            paramFilter.SetElementFilter(parameterFilter);
        }

        // Add the filter to the active view if it's not already added
        if (!view.GetFilters().Contains(paramFilter.Id))
        {
            view.AddFilter(paramFilter.Id);
        }

        // Set the filter's visibility to true in the view
        view.SetFilterVisibility(paramFilter.Id, true);

        // Create graphic overrides
        OverrideGraphicSettings overrides = new OverrideGraphicSettings()
            .SetProjectionLineColor(new Autodesk.Revit.DB.Color(255, 0, 0))
            .SetProjectionLineWeight(5);

        // Apply the graphic overrides to the filter in the view
        view.SetFilterOverrides(paramFilter.Id, overrides);
    }

    public ICommand ChangeKitchenBrandCommand { get; set; }

    private void ExecuteChangeKitchenBrandCommand()
    {

        // Get the Project Information element
        ProjectInfo projectInfo = _doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        string currentKitchenBrandParamValue = kitchenBrandParam == null ? "Aristokraft" : kitchenBrandParam.AsString();
        string newKitchenBrandValue = _chosenBrandFromDropdown.BrandName;

        // Don't do anything if current and target are same
        if (currentKitchenBrandParamValue == newKitchenBrandValue) return;

        using (Transaction trans = new Transaction(_doc, "Change KitchenBrand"))
        {
            trans.Start();

            try
            {
                // Run all things
                ConvertCabinets();
                UpdateProjectParam(trans);
                CreateViewFilter();

                TaskDialog.Show("Revit", $"New KitchenBrand: {_chosenBrandFromDropdown.BrandName}");
                trans.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
                trans.RollBack();
            }
        }
    }

    // Set the value of the Graphics Filter to the current chosen brand
    // BUG: for some reason this wouldn't work, no errors but the filters are not getting created

    public ChangeBrandViewModel()
    {
        Debug.WriteLine("");
    }
    // Constructor
    //public ChangeBrandViewModel(Document doc)
    public ChangeBrandViewModel(UIApplication uiApp)
    {
        _uiDoc = uiApp.ActiveUIDocument;
        _doc = _uiDoc.Document;

        // Initialize the command
        ChangeKitchenBrandCommand = new RelayCommand(ExecuteChangeKitchenBrandCommand);

        /// Initialize the chosen brand
        ProjectInfo projectInfo = _doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        if (kitchenBrandParam != null)
        {
            _chosenKitchenBrand = kitchenBrandParam.AsString() ?? string.Empty;
        }
        else
        {
            _chosenKitchenBrand = "Aristokraft"; // Default value
        }
    }
}
