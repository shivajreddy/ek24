using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using ek24.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ek24.Utils;

/// <summary>
/// - This utility class is responsible to Read the Document data
/// - It creates Datastructures that represent the available Family Symbols,
///   as a forward declaration for the application's UI
/// - DocumentClosedEvent should clear the memory of these symbols
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

            var categoryParam = familySymbol.LookupParameter("EKType");
            var categoryValue = categoryParam == null ? "" : categoryParam.AsValueString();

            var configurationParam = familySymbol.LookupParameter("EKCategory");
            var configurationValue = configurationParam == null ? "" : configurationParam.AsValueString();

            var vendorSKUParam = familySymbol.LookupParameter("Vendor_SKU");
            var vendorSKUValue = vendorSKUParam == null ? "" : vendorSKUParam.AsValueString();

            var notesParam = familySymbol.LookupParameter("Vendor_Notes");
            var notesValue = notesParam == null ? "" : notesParam.AsValueString();

            var ek_sku = new EK_SKU(vendorSKUValue, notesValue);

            var ekFamilySymbol = new EKFamilySymbol(
                revitFamilySymbol: familySymbol,
                ekBrand: brandValue,
                ekCategory: categoryValue,
                ekConfiguration: configurationValue,
                ekSKU: ek_sku
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

    private static string GetCabinetNoteFromSymbol(FamilySymbol symbol)
    {
        // Implement logic to extract a note from the family symbol if available
        // This might involve reading parameters or other properties
        const string VENDOR_NOTES_PARAM_NAME = "Vendor_Notes";
        var noteParam = symbol.LookupParameter(VENDOR_NOTES_PARAM_NAME);
        return noteParam?.AsString() ?? string.Empty;
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
