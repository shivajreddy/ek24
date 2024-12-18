using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using ek24.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ek24.Utils;

/// <summary>
/// - This utility class is responsible to Read the Document data
/// - It creates Datastructures that represent the available Family Symbols,
///   as a forward declaration for the application's UI
/// - DocumentClosedEvent should clear the memory of these symbols
/// </summary>
public class EKUtils
{
    public static List<EKCaseworkFamily> EKCaseworkFamilies { get; set; }


    List<EKCaseworkFamily> createDataStructures(FilteredElementCollector familyCollecter)
    {
        List<EKCaseworkFamily> ekCaseworkFamily;



        return ekCaseworkFamily;
    }


    public static void HandleDocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
    {
        // Grab the Document
        Document doc = args.Document;
        string documentPath = doc.PathName;

        FilteredElementCollector collector = new FilteredElementCollector(doc);
        FilteredElementCollector familyCollecter = collector.OfClass(typeof(Family));

        // Define cabinet family prefixes used for filtering
        string[] ekCabinetFamilyNamePrefixes = {
            "Aristokraft-W-",
            "Aristokraft-B-",
            "Aristokraft-T-",
            "Eclipse-W-",
            "Eclipse-B-",
            "Eclipse-T-",
            "Eclipse",
            "YTC-W-",
            "YTC-B-",
            "YTC-T-",
            "YTH-W-",
            "YTH-B-",
            "YTH-T-"
        };

        ICollection<Element> familyElements = familyCollecter.ToElements();
        int collectorCount = familyElements.Count();

        // Filter families matching the prefixes
        var cabinetFamilies = familyElements
            .Cast<Family>()
            .Where(family => ekCabinetFamilyNamePrefixes.Any(prefix => family.Name.StartsWith(prefix)))
            .Select(family => new EKCabinetFamily
            {
                FamilyName = family.Name,
                TypeNames = family.GetFamilySymbolIds()
                    .Select(id => doc.GetElement(id) as FamilySymbol)
                    .Where(symbol => symbol != null)
                    .Select(symbol => new EKCabinetType
                    {
                        TypeName = symbol.Name,
                        Note = GetCabinetNoteFromSymbol(symbol)
                    })
                    .GroupBy(type => type.TypeName) // Ensure unique types by grouping
                    .Select(groupedType => groupedType.First())
                    .ToList()
            })
            .ToList();


        // Set the UI data property
        ProjectCabinetFamilies.CabinetFamilies = cabinetFamilies;
    }


    public static void HandleDocumentClosedEvent(object sender, EventArgs e)
    {
        // Clear the UI data property on document close
        if (ProjectCabinetFamilies.CabinetFamilies != null)
        {
            ProjectCabinetFamilies.CabinetFamilies.Clear();
        }
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

