﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using ek24.UI.Models.Revit;
using System;
using System.Linq;

namespace ek24.Events;

public static class DocumentOpenedEvent
{

}


public class DocumentOpenedEventOld
{
    public static void HandleDocumentOpenedEvent(object sender, DocumentOpenedEventArgs args)
    {
        // Grab the Document
        var doc = args.Document;

        // Define cabinet family prefixes
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

        // Collect all families of the Casework category
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Casework)
            .OfClass(typeof(Family));

        /*
        // Filter families matching the prefixes
        var cabinetFamilies = caseworkCollector
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
        */
    }

    public static void HandleDocumentClosedEvent(object sender, EventArgs e)
    {
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
        var noteParam = symbol.LookupParameter("Note"); // Example
        return noteParam?.AsString() ?? string.Empty;
    }

    private static string GetCabinetNoteFromInstance(FamilyInstance cabinet)
    {
        // Implement logic to extract a note from the cabinet instance if available
        // This might involve reading parameters or other properties
        var noteParam = cabinet.LookupParameter("Note"); // Example
        return noteParam?.AsString() ?? string.Empty;
    }
}

