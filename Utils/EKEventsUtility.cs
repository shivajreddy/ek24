using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.UI.ViewModels.ProjectBrowser;
using ek24.UI.ViewModels.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    #region Properties
    public static List<EKFamilySymbol> EKCaseworkSymbols { get; set; }
    #endregion


    public void HandleDocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
    {
        // Grab the Document
        Document doc = args.Document;

        // Create Project State inside the Global State
        string project_name = doc.Title;
        var ek_project_state = APP.global_state.CreateProjectState(project_name);

        // Set the current project to newly opened project
        APP.global_state.Current_Project_State = ek_project_state;

        //APP.global_state.eK24Modify_ViewModel.LatestProjectName = project_name;

        // TODO: Move the EKCaseworkSymbols to ProjectState
        // Element Collector
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        //FilteredElementCollector familyCollecter = collector.OfClass(typeof(Family));
        FilteredElementCollector familySymbolsCollecter = collector.OfClass(typeof(FamilySymbol));

        List<EKFamilySymbol> ekFamilySymbols = [];

        foreach (FamilySymbol familySymbol in familySymbolsCollecter)
        {
            // MAIN FILTER: We use only FamilySymbols that have 'Manufacturer' param
            var brandParam = familySymbol.LookupParameter("Manufacturer");
            if (brandParam == null) continue;
            var brandValue = brandParam.AsValueString();
            if (brandValue == null || brandValue == "") continue;

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
                ek_SKU: ek_sku
                );
            ekFamilySymbols.Add(ekFamilySymbol);
        }
        EKCaseworkSymbols = ekFamilySymbols;

        //Debug.WriteLine(ekFamilySymbols.Count);
    }


    // Clear the Project's State When document is closing
    public void HandleDocumentClosingEvent(object sender, DocumentClosingEventArgs e)
    {
        // Grab the Document
        Document doc = e.Document;

        // Remove Project State inside the Global State
        string project_name = doc.Title;
        APP.global_state.RemoveProjectState(project_name);
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

        //EK_Project_State project_state = APP.global_state.current_project_state;
        //project_state.EKProjectsCurrentSelection = currentSelection;
        //project_state.EKSelectionCount = currentSelection.GetElementIds().Count;

        APP.global_state.EKSelectionCount = currentSelection.GetElementIds().Count;
        APP.global_state.Current_Project_State.EKProjectsCurrentSelection = currentSelection;

        Debug.WriteLine("hi");

        // Update the ViewModel with the new selection

        // moving this to SelectionService
        //SelectionService.SyncSelectionWithRevit(currentSelection, doc);

        // :: Update ViewModels ::
        TypeParamsViewModel.SyncCurrentSelectionWithTypeParamsViewModel(currentSelection, doc);
        InstanceParamsViewModel.SyncCurrentSelectionWithInstanceParamsViewModel(currentSelection, doc);
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
        var project_state = APP.global_state.GetProjectState(project_name);
        APP.global_state.Current_Project_State = project_state;

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


