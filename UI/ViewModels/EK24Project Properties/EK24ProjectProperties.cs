namespace ek24.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.Commands.Utils;
using ek24.Dtos;
using ek24.RequestHandling;
using ek24.UI.Commands;
using ek24.UI.Models.Revit;
using ek24.UI.ViewModels.ChangeBrand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;


public class EK24ProjectProperties_ViewModel : INotifyPropertyChanged
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
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
    public List<string> BrandItems { get; } = EKBrands.all_brand_names;
    public bool canUpdateKitchenBrandButton => EKProjectKitchenBrand != "" && SelectedBrand != "" && SelectedBrand != EKProjectKitchenBrand;

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
                OnPropertyChanged(nameof(EKProjectKitchenBrand)); // Notify for BrandName if needed
            }
        }
    }

    public string EKProjectKitchenBrand
    {
        get
        {
            if (APP.Global_State.Current_Project_State == null || APP.Global_State.Current_Project_State.EKProjectKitchenBrand == null) return "";
            return APP.Global_State.Current_Project_State.EKProjectKitchenBrand;
        }
        set
        {
            APP.Global_State.Current_Project_State.EKProjectKitchenBrand = value;
        }
    }
    #endregion

    #region Selected Item for ComboBoxes
    private string _selectedBrand { get; set; }
    public string SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (_selectedBrand == value) return;
            _selectedBrand = value;
            OnPropertyChanged(nameof(SelectedBrand));
            OnPropertyChanged(nameof(canUpdateKitchenBrandButton));
        }
    }
    #endregion

    #region GLOBAL STATE PROPERTY CHANGE HANDLERS
    private EK_Project_State _currentProjectState;
    private void GlobalState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Global_State.Current_Project_State))
        {
            // Unsubscribe from the old project state
            if (_currentProjectState != null)
            {
                _currentProjectState.PropertyChanged -= CurrentProjectState_PropertyChanged;
            }

            // Subscribe to the new project state
            _currentProjectState = APP.Global_State.Current_Project_State;
            if (_currentProjectState != null)
            {
                _currentProjectState.PropertyChanged += CurrentProjectState_PropertyChanged;
            }

            // Force the UI to reevaluate these properties, 
            // in case the new project state is entirely different
            OnPropertyChanged(nameof(EKProjectKitchenBrand));
        }
    }
    private void CurrentProjectState_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EK_Project_State.EKProjectKitchenBrand))
        {
            OnPropertyChanged(nameof(EKProjectKitchenBrand));
        }
    }
    #endregion

    #region Relay Commands
    public ICommand Command_UpdateKitchenBrand { get; }
    public void Handle_Command_UpdateKitchenBrand()
    {
        Update_ProjectKitchenBrand.target_project_kitchenBrand = SelectedBrand;

        Debug.WriteLine("UPDATE Instance Param Value");
        APP.RequestHandler.RequestType = RequestType.ProjectProperties_UpdaateKitchenBrand;
        APP.ExternalEvent?.Raise();

    }
    #endregion

    #region Constructor
    public EK24ProjectProperties_ViewModel()
    {
        // THESE 2 THINGS ARE ACTUALLY TRIGGERING THE ONPROPERTY CHANGE CALLS
        // 1) Listen to changes on the Global State itself (especially Current_Project_State)
        APP.Global_State.PropertyChanged += GlobalState_PropertyChanged;
        // 2) Subscribe to the *current* project state's changes
        _currentProjectState = APP.Global_State.Current_Project_State;
        if (_currentProjectState != null)
        {
            _currentProjectState.PropertyChanged += CurrentProjectState_PropertyChanged;
        }


        Command_UpdateKitchenBrand = new RelayCommand(Handle_Command_UpdateKitchenBrand);
    }
    #endregion
}


public static class Update_ProjectKitchenBrand
{
    public static string target_project_kitchenBrand { get; set; }

    public static void change_ekKitchenBrand(UIApplication app)
    {
        if (target_project_kitchenBrand == APP.Global_State.Current_Project_State.EKProjectKitchenBrand) return;

        var current_project_state = APP.Global_State.Current_Project_State;
        var current_project_kitchenBrand = APP.Global_State.Current_Project_State.EKProjectKitchenBrand;

        Document doc = app.ActiveUIDocument.Document;

        // Call the function that changes the project parameter

        // Get the Project Information element
        ProjectInfo projectInfo = doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

        using (Transaction t = new Transaction(doc, "Change Kitchen Brand"))
        {
            try
            {
                t.Start();
                // 1. Update ProjectInformation parameter
                kitchenBrandParam.Set(target_project_kitchenBrand);

                // 2. Update the Cabinet Families
                update_cabinets(doc, current_project_kitchenBrand, target_project_kitchenBrand);

                // 3. Updat view filter
                update_value_of_view_kitchenbrand_viewfilter(doc, target_project_kitchenBrand);

                t.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ERROR", $"Failed to change Kitchen Brand {ex.Message}");
                t.RollBack();
                return;
            }
        }

        // 4. Update the global state
        APP.Global_State.Current_Project_State.EKProjectKitchenBrand = target_project_kitchenBrand;
    }

    public static void update_value_of_view_kitchenbrand_viewfilter(Document doc, string brandName) // Happens with in a transaction
    {
        // Define the filter name
        string filterName = "Vendor Name Filter";

        // Get Active View
        Autodesk.Revit.DB.View view = doc.ActiveView;

        // Create a list of categories that will be used for the filter
        IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Casework) };

        // Get the ElementId for the "Manufacturer" built-in parameter
        ElementId manufacturerParamId = new ElementId(BuiltInParameter.ALL_MODEL_MANUFACTURER);

        // Create the filter based on the 
        // Create a filter rule where "Manufacturer" equals "MY VALUE" (case-insensitive)
        FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateNotEqualsRule(manufacturerParamId, brandName);

        // Create an ElementParameterFilter with the rule
        ElementParameterFilter parameterFilter = new ElementParameterFilter(manufacturerRule);

        // Attempt to find an existing filter with the same name
        ParameterFilterElement paramFilter = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

        if (paramFilter == null)
        {
            TaskDialog.Show("ERROR", "There is no existing View Filter to modify");
            return;
        }

        // Update the existing filter's element filter
        paramFilter.SetElementFilter(parameterFilter);
    }

    private static void update_cabinets(Document doc, string current_kitchen_brand_name, string new_Kitchten_brand_name)
    {
        // Get all the cabinets in the project
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(FamilyInstance));
        collector.OfCategory(BuiltInCategory.OST_Casework);

        List<FamilyInstance> cabinets = FilterAllCabinets.FilterProjectForEagleCabinets(doc);

        var x = APP.Global_State.Current_Project_State.EKCaseworkSymbols;

        // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
        List<CabinetFamilyInstance> cabinetFaimlyInstances = BrandMapper.ConvertFamilyInstaceIntoCabinetFamilyInstance(cabinets);

        // Create a StringBuilder to store the information to be displayed
        StringBuilder resultMessage = new StringBuilder();
        int match_found_count = 0, match_not_found_count = 0;
        resultMessage.Append($"Total Cabinet Instances in Project: {cabinets.Count()}\n");
        //resultMessage.Append($"Match Found:{match_found_count}   Match NOT-Found: {match_not_found_count}");

        StringBuilder result_Success_Message = new StringBuilder();
        StringBuilder result_Failed_Message = new StringBuilder();


        // Create an instance of BrandMapper
        BrandMapper mapper = new BrandMapper();

        // Iterate over the cabinet family instances and map the SKUs
        foreach (var cabinetFamilyInstance in cabinetFaimlyInstances)
        {
            // Use BrandMapper to map the SKU to the target brand
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, new_Kitchten_brand_name);

            if (!string.IsNullOrEmpty(mappedSKU))
            {
                match_found_count += 1;
                // Append the mapping information to the StringBuilder
                //resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => {new_Kitchten_brand_name}-{mappedSKU}");
                result_Success_Message.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => {new_Kitchten_brand_name}-{mappedSKU}");
            }
            else
            {
                match_not_found_count += 1;
                // If no mapping found, add a message
                //resultMessage.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => XXX ");
                result_Failed_Message.AppendLine($"{cabinetFamilyInstance.BrandName}-{cabinetFamilyInstance.TypeName} => XXX ");
            }
        }
        // Insert the totals at the top of the StringBuilder
        //resultMessage.Insert(1, $"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append($"Total Matches Found: {match_found_count}\nTotal Matches NOT-Found: {match_not_found_count}\n");
        resultMessage.Append(result_Success_Message);
        resultMessage.Append(result_Failed_Message);

        // Display the information using TaskDialog
        TaskDialog.Show("SKU MAPPING RESULTS", resultMessage.ToString());

        // TODO 1:
        // Map all current family-types to valid new valid-family types
        var cabinetInstancesWithTargetFamilyType = new List<Tuple<CabinetFamilyInstance, string, string>>();

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
            string mappedSKU = BrandMapper.FindTargetBrandType(cabinetFamilyInstance.BrandName, cabinetFamilyInstance.TypeName, new_Kitchten_brand_name);

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
                if (brandCodeMapping.TryGetValue(new_Kitchten_brand_name, out string targetBrandCode))
                {
                    // Construct the new target family name
                    targetFamilyName = $"{targetBrandCode}-{familyNameParts[1]}";
                }
                string targetTypeName = mappedSKU;


                // Add the pair (CabinetFamilyInstance, new family name, new type name) to the list
                cabinetInstancesWithTargetFamilyType.Add(new Tuple<CabinetFamilyInstance, string, string>(
                    cabinetFamilyInstance,
                    targetFamilyName,   // New brand name
                    mappedSKU               // Mapped SKU/type name for the new brand
                ));
            }
        }


        // TODO 2:
        // Convert all valid FamilyTypes
        ChangeFamilyTypes(doc, cabinetInstancesWithTargetFamilyType);
    }

    private static void ChangeFamilyTypes(Document doc, List<Tuple<CabinetFamilyInstance, string, string>> cabinetInstancesWithTargetFamilyType)
    {
        // StringBuilder to collect error messages
        StringBuilder errorMessages = new StringBuilder();

        // Loop through the list of cabinet instances and target family/type pairs
        foreach (var item in cabinetInstancesWithTargetFamilyType)
        {
            CabinetFamilyInstance currentInstance = item.Item1;
            string targetFamilyName = item.Item2;
            string targetTypeName = item.Item3;

            // Get the family instance to modify
            FamilyInstance familyInstance = currentInstance.FamilyInstance;

            // Find the target family symbol (family type) using family and type names
            FamilySymbol targetFamilySymbol = FindFamilySymbol(doc, targetFamilyName, targetTypeName);
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
    private static FamilySymbol FindFamilySymbol(Document doc, string familyName, string typeName)
    {
        // Iterate through all the families in the document
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(Autodesk.Revit.DB.Family));

        foreach (Autodesk.Revit.DB.Family family in collector)
        {
            if (family.Name == familyName)
            {
                foreach (ElementId symbolId in family.GetFamilySymbolIds())
                {
                    FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;
                    if (symbol.Name == typeName)
                    {
                        return symbol;
                    }
                }
            }
        }
        return null; // Return null if no matching family symbol is found
    }

}

/*
    public class ChangeBrandViewModel_Old : INotifyPropertyChanged
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

            List<FamilyInstance> cabinets = FilterAllCabinets.FilterProjectForEagleCabinets(_doc);

            // Convert the FamilyInstance list into a list of FamilyInstanceInfo objects using the utility
            List<CabinetFamilyInstance> cabinetFaimlyInstances = BrandMapper.ConvertFamilyInstaceIntoCabinetFamilyInstance(cabinets);

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
            var cabinetInstancesWithTargetFamilyType = new List<Tuple<CabinetFamilyInstance, string, string>>();

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
                    cabinetInstancesWithTargetFamilyType.Add(new Tuple<CabinetFamilyInstance, string, string>(
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
        private void ChangeFamilyTypes(List<Tuple<CabinetFamilyInstance, string, string>> cabinetInstancesWithTargetFamilyType)
        {
            // StringBuilder to collect error messages
            StringBuilder errorMessages = new StringBuilder();

            // Loop through the list of cabinet instances and target family/type pairs
            foreach (var item in cabinetInstancesWithTargetFamilyType)
            {
                CabinetFamilyInstance currentInstance = item.Item1;
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

            string DEFAULT_STANDARD_KITCHEN_BRAND = "Yorktowne Classic";

            string currentKitchenBrandParamValue = kitchenBrandParam == null ? DEFAULT_STANDARD_KITCHEN_BRAND : kitchenBrandParam.AsString();
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

        public ChangeBrandViewModel_Old()
        {
            Debug.WriteLine("");
        }
        // Constructor
        //public ChangeBrandViewModel(Document doc)
        public ChangeBrandViewModel_Old(UIApplication uiApp)
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
*/

