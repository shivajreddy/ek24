using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.UI.ViewModels.ProjectBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using SelectionChangedEventArgs = Autodesk.Revit.UI.Events.SelectionChangedEventArgs;
using View = Autodesk.Revit.DB.View;

namespace ek24.Utils;

/// <summary>
/// - This utility class is responsible to Read the Document data
/// - It creates Datastructures that represent the available Family Symbols,
///   as a forward declaration for the application's UI
/// - DocumentClosedEvent should clear the memory of these symbols
///
/// How to implement methods for Revit Events:
/// https://www.revitapidocs.com/2024/7d51c9f8-1bea-32ec-0e54-5921242e57c3.htm
/// You can use the `sender` and typecast to 'UIApplication'. and from that you can any of the following
///      UIApplication app = commandData.Application;
///      UIDocument uiDoc = app.ActiveUIDocument;
///      Document doc = app.ActiveUIDocument.Document;
/// OR you can use the methods available on the <particular>EventArgs variable
/// These event implementations execute when that revit event/behaviour happens.
/// </summary>
public class EKEventsUtility
{
    public void HandleDocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
    {
        // Grab the Document
        Document doc = args.Document;

        // Create Project State inside the Global State
        string project_name = doc.Title;
        var ek_project_state = APP.Global_State.CreateProjectState(project_name);

        // Set the document for later use
        ek_project_state.Document = doc;

        // Set the current project to newly opened project
        APP.Global_State.Current_Project_State = ek_project_state;

        //APP.global_state.eK24Modify_ViewModel.LatestProjectName = project_name;

        // TODO: Move the EKCaseworkSymbols to ProjectState
        // Element Collector
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        //FilteredElementCollector familyCollecter = collector.OfClass(typeof(Family));
        FilteredElementCollector familySymbolsCollecter = collector.OfClass(typeof(FamilySymbol));

        // Temporary Dictionary and List that will be assigned to the ProjectState
        //Dictionary<FamilySymbol, EKFamilySymbol> temp_revitFamilySymbolToEKFAMILYSYMBOL = new Dictionary<FamilySymbol, EKFamilySymbol>();
        Dictionary<ElementId, EKFamilySymbol> temp_revitFamilySymbolIdEKFamilySymbol = new Dictionary<ElementId, EKFamilySymbol>();
        //Dictionary<string, EKFamilySymbol> temp_revitFamilySymbolToEKFAMILYSYMBOL = new Dictionary<string, EKFamilySymbol>();
        List<EKFamilySymbol> temp_ekFamilySymbols = [];

        foreach (FamilySymbol familySymbol in familySymbolsCollecter)
        {
            // MAIN FILTER: We use only FamilySymbols that have 'Manufacturer' param
            var brandParam = familySymbol.LookupParameter("Manufacturer");
            if (brandParam == null) continue;
            var brandValue = brandParam.AsValueString();
            if (brandValue == null || brandValue == "") continue;
            // NOTE: Brand value must be one of the allowed values
            //List<string> allowed_brandValues = new List<string> { "Aristokraft", "Yorktowne Classic", "", "Yorktowne Historic", "Eclipse" };
            //if (!allowed_brandValues.Contains(brandValue)) continue;

            var ekTypeParam = familySymbol.LookupParameter("EKType");
            var ekTypeValue = ekTypeParam == null ? "" : ekTypeParam.AsValueString();

            var ekCategoryParam = familySymbol.LookupParameter("EKCategory");
            var ekCategoryValue = ekCategoryParam == null ? "" : ekCategoryParam.AsValueString();

            var vendorSKUParam = familySymbol.LookupParameter("Vendor_SKU");
            // ONCE THE FAMILIES ARE UPDATED USE 'Vendor_SKU' param instead of the literal type name
            //var vendorSKUValue = vendorSKUParam == null ? "" : vendorSKUParam.AsValueString();
            var vendorSKUValue = familySymbol.Name;

            var notesParam = familySymbol.LookupParameter("Vendor_Notes");
            var notesValue = notesParam == null ? "" : notesParam.AsValueString();

            var ek_sku = new EK_SKU(vendorSKUValue, notesValue, familySymbol);

            var ekFamilySymbol = new EKFamilySymbol(
                //revitFamilySymbol: familySymbol,
                ekBrand: brandValue,
                ekType: ekTypeValue,
                ekCategory: ekCategoryValue,
                ek_SKU: ek_sku,
                revitFamilySymbolId: familySymbol.Id
                );
            //temp_revitFamilySymbolToEKFAMILYSYMBOL[familySymbol] = ekFamilySymbol;   // add to temporary dictionary
            //temp_revitFamilySymbolToEKFAMILYSYMBOL[familySymbol.ToString()] = ekFamilySymbol;   // add to temporary dictionary
            temp_revitFamilySymbolIdEKFamilySymbol[familySymbol.Id] = ekFamilySymbol;   // add to temporary dictionary
            temp_ekFamilySymbols.Add(ekFamilySymbol);                                   // add to temporary list
        }

        // Set the Dictionary & EK-Symbols ppty for this project
        APP.Global_State.Current_Project_State.Map_RevitFamilySymbolId_EKFamilySymbol = temp_revitFamilySymbolIdEKFamilySymbol;
        APP.Global_State.Current_Project_State.EKCaseworkSymbols = temp_ekFamilySymbols;
        //Debug.WriteLine(ekFamilySymbols.Count);

        // GET PROJECT RELATED PROPERTIES
        // Get KitchenBrand - Project Param
        string currentProject_projectBrand = "";
        ProjectInfo projectInfo = doc.ProjectInformation;
        Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");
        currentProject_projectBrand = kitchenBrandParam?.AsString();

        Parameter kitchenStyleParam = projectInfo.LookupParameter("KitchenStyle");
        string currentProject_kitchenstyle = kitchenStyleParam != null ? kitchenStyleParam.AsString() : "Yorktowne Classic - Henning";
        Parameter kitchenFinishParam = projectInfo.LookupParameter("KitchenFinish");
        string currentProject_kitchenfinish = kitchenFinishParam != null ? kitchenFinishParam.AsString() : "YTC-Henning-PAINT1";

        set_project_view_filter(doc);
        APP.Global_State.Current_Project_State.EKProjectKitchenBrand = currentProject_projectBrand;

        APP.Global_State.Current_Project_State.EKProjectKitchenStyle = currentProject_kitchenstyle;
        APP.Global_State.Current_Project_State.EKProjectKitchenFinish = currentProject_kitchenfinish;
    }

    public void HandleDocumentClosingEvent(object sender, DocumentClosingEventArgs e)
    {
        // Grab the Document
        Document doc = e.Document;

        // Remove Project State inside the Global State
        string project_name = doc.Title;
        APP.Global_State.RemoveProjectState(project_name);
    }

    public void HandleDocumentClosedEvent(object sender, DocumentClosedEventArgs e)
    {
        //EKCaseworkSymbols.Clear();
        /*
        // Clear the UI data property on document close
        if (ProjectCabinetFamilies.CabinetFamilies != null)
        {
            ProjectCabinetFamilies.CabinetFamilies.Clear();
        }
        */
    }

    /// <summary>
    /// How to implement methods for Revit Events:
    /// https://www.revitapidocs.com/2024/7d51c9f8-1bea-32ec-0e54-5921242e57c3.htm
    /// You can use the `sender` and typecast to 'UIApplication'. and from that you can any of the following
    ///      UIApplication app = commandData.Application;
    ///      UIDocument uiDoc = app.ActiveUIDocument;
    ///      Document doc = app.ActiveUIDocument.Document;
    /// OR you can use the methods available on the <particular>EventArgs variable
    /// These event implementations execute when that revit event/behaviour happens.
    /// </summary>
    public void HandleViewActivatedEvent(object sender, EventArgs e)
    {
        // Grab the Document
        var app = sender as UIApplication;
        var uiDoc = app.ActiveUIDocument;
        var doc = app.ActiveUIDocument.Document;

        // Create Project State inside the Global State
        string project_name = doc.Title;
        // Update current project
        var project_state = APP.Global_State.GetProjectState(project_name);
        APP.Global_State.Current_Project_State = project_state;

        // TODO: Move these to Project_State
        var collector = new FilteredElementCollector(doc);
        var allViews = collector.OfClass(typeof(View)).ToElements();

        // Clear existing list in the ViewModel
        ProjectBrowserViewModel.ChosenViews.Clear();
        ProjectBrowserViewModel.ChosenViewSheets.Clear();

        foreach (var view in allViews)
        {
            // Check if the view has the parameter and if it matches the target value
            Parameter param = view.LookupParameter("EK24_view_sheet");

            // Yes/No parameters are stored as integers
            // Check if the integer value corresponds to the boolean parameterValue (1 for true, 0 for false)
            if (param is not { StorageType: StorageType.Integer } || param.AsInteger() != 1) continue;

            // current view has the "EK24_view_sheet" and value is Yes


            // ViewSheet type is the Sheets in Revit Project Browser
            if (view.GetType() == typeof(ViewSheet))
            {
                // Update the ViewModel
                ProjectBrowserViewModel.ChosenViewSheets.Add(view as ViewSheet);
            }
            else
            {
                // Update the ViewModel
                ProjectBrowserViewModel.ChosenViews.Add(view as View);
            }

        }
    }

    /// <summary>
    /// This event is added as revit api's 'SelectionChanged' event, and performs
    /// logic when ever 'SelectionChanged' event on Revit triggers
    /// </summary>
    public void HandleSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
    {
        UIApplication app = sender as UIApplication;
        UIDocument uiDoc = app?.ActiveUIDocument;
        Document doc = uiDoc?.Document;
        if (doc == null) return;

        string project_name = doc?.Title;

        Selection currentSelection = uiDoc.Selection;

        APP.Global_State.Current_Project_State.EKCurrentProjectSelection = currentSelection;
        APP.Global_State.Current_Project_State.EKSelectionCount = currentSelection.GetElementIds().Count;

        // Update the ViewModel with the new selection

        // moving this to SelectionService
        //SelectionService.SyncSelectionWithRevit(currentSelection, doc);

        // :: Update ViewModels ::
        //TypeParamsViewModel.SyncCurrentSelectionWithTypeParamsViewModel(currentSelection, doc);
        //InstanceParamsViewModel.SyncCurrentSelectionWithInstanceParamsViewModel(currentSelection, doc);
    }

    // Helper Function: Set View Filter
    void set_project_view_filter(Document doc)
    {
        using (Transaction t = new Transaction(doc, "Add view filter"))
        {
            try
            {
                t.Start();

                // Define the filter name
                string filterName = "Vendor Name Filter";

                // Get Active View
                View active_view = doc.ActiveView;

                // Create a list of categories that will be used for the filter
                IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Casework) };

                // Get the ElementId for the "Manufacturer" built-in parameter
                ElementId manufacturerParamId = new ElementId(BuiltInParameter.ALL_MODEL_MANUFACTURER);

                // Create a filter rule where "Manufacturer" equals "MY VALUE" (case-insensitive)

                // Get the Project Information element
                ProjectInfo projectInfo = doc.ProjectInformation;
                Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");
                string currentKitchenBrandParamValue = kitchenBrandParam.AsString();

                //FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateEqualsRule(manufacturerParamId, currentKitchenBrandParamValue);
                FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateNotEqualsRule(manufacturerParamId, currentKitchenBrandParamValue);

                // Create an ElementParameterFilter with the rule
                ElementParameterFilter parameterFilter = new ElementParameterFilter(manufacturerRule);

                // Attempt to find an existing filter with the same name
                ParameterFilterElement paramFilter = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterFilterElement))
                    .Cast<ParameterFilterElement>()
                    .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

                if (paramFilter == null)
                {
                    // Create the ParameterFilterElement since it doesn't exist
                    paramFilter = ParameterFilterElement.Create(doc, filterName, categories, parameterFilter);
                }
                else
                {
                    // Update the existing filter's element filter
                    paramFilter.SetElementFilter(parameterFilter);
                }

                // Add the filter to the active view if it's not already added
                if (!active_view.GetFilters().Contains(paramFilter.Id))
                {
                    active_view.AddFilter(paramFilter.Id);
                }

                // Set the filter's visibility to true in the view
                active_view.SetFilterVisibility(paramFilter.Id, true);

                // Create graphic overrides
                OverrideGraphicSettings overrides = new OverrideGraphicSettings()
                    .SetProjectionLineColor(new Autodesk.Revit.DB.Color(255, 0, 0))
                    .SetProjectionLineWeight(5);

                // Apply the graphic overrides to the filter in the view
                active_view.SetFilterOverrides(paramFilter.Id, overrides);

                t.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ERROR", $"Failed to GET/SET Kitchen Brand {ex.Message}");
                t.RollBack();
                return;
            }
        }

    }

}

/*
// Convert each revit-family into ek-family
foreach (Family family in familyCollecter)
{
    //bc.Add(family.FamilyCategory.BuiltInCategory);
    //continue;

    // Only Families that a non empty "Manufacturer" parameter 
    if (family == null) continue;
    if (family.FamilyCategory.BuiltInCategory != BuiltInCategory.OST_Casework) continue;

    // Check if the Family has any symbols
    var familySymbolIds = family.GetFamilySymbolIds();
    if (!familySymbolIds.Any()) continue;

    // Get the first FamilySymbol
    var firstSymbol = doc.GetElement(familySymbolIds.First()) as FamilySymbol;
    if (firstSymbol == null) continue;

    //if (family.GetFamilySymbolIds().Count == 0) continue;

    var brandParam = firstSymbol.LookupParameter("Manufacturer");
    if (brandParam == null) continue;

    Debug.WriteLine("has atleast 1 type & it has Manufacturer Parameter");

    var category1Param = family.LookupParameter("Category1");
    var category2Param = family.LookupParameter("Category2");

    string brandValue = brandParam.AsValueString();
    string category1Value = category1Param == null ? "" : category1Param.AsValueString();
    string category2Value = category2Param == null ? "" : category2Param.AsValueString();

    // Collect all types of this family
    List<EKCaseworkFamilyType> allEKFamilyTypes = [];

    // Get all family symbols (types) for this family
    foreach (FamilySymbol symbol in family.GetFamilySymbolIds()
        .Select(id => doc.GetElement(id))
        .Cast<FamilySymbol>())
    {
        var ekFamilyType = new EKCaseworkFamilyType(
            symbol.Name,
            symbol
        );
        allEKFamilyTypes.Add(ekFamilyType);
    }

    //names.Add(family.Name);
    //symbol_names.Add(firstSymbol.Name);
    //vals.Add(new Tuple<string, string, string>(brandValue, category1Value, category2Value));
    //continue;

    var ekFamily = new EKFamilySymbol(
        revitFamily: family,
        ekBrand: brandValue,
        category1: category1Value,
        category2: category2Value,
        ekCaseworkFamilyTypes: allEKFamilyTypes
    );
    ekCaseworkFamilies.Add(ekFamily);
}
EKCaseworkFamilies = ekCaseworkFamilies;
*/


