using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.UI.ViewModels.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;

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
public class EKUtils
{
    public static List<EKFamilySymbol> EKCaseworkSymbols { get; set; }

    public void HandleDocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
    {
        // Grab the Document
        Document doc = args.Document;

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

        Debug.WriteLine(ekFamilySymbols.Count);
    }

    public void HandleDocumentClosedEvent(object sender, EventArgs e)
    {
        EKCaseworkSymbols.Clear();
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
    public static void HandleSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
    {
        UIApplication app = sender as UIApplication;
        UIDocument uiDoc = app?.ActiveUIDocument;
        Document doc = uiDoc?.Document;
        if (doc == null) return;

        Selection currentSelection = uiDoc.Selection;

        // Update the ViewModel with the new selection


        // moving this to SelectionService
        //SelectionService.SyncSelectionWithRevit(currentSelection, doc);

        // :: Update ViewModels ::
        TypeParamsViewModel.SyncCurrentSelectionWithTypeParamsViewModel(currentSelection, doc);
        InstanceParamsViewModel.SyncCurrentSelectionWithInstanceParamsViewModel(currentSelection, doc);
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
