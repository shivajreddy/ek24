using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;


namespace ek24.Commands.Utils;


public class FilterAllCabinets
{
    private static string[] ekCaseworkFamilyNamePrefixes = {
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

    public static List<FamilyInstance> FilterProjectForEagleCabinets(Document doc)
    {
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);
        FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);

        // Filter all the cabinet instances
        FilteredElementCollector caseWorkCollector = caseworkCollector.OfCategory(BuiltInCategory.OST_Casework);

        // Filter for FamilyInstance elements of 'Casework' category
        ICollection<Element> caseworkFamilyInstances = caseWorkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        // Filter for cabinet instances with the prefixes
        List<FamilyInstance> ekCabinetInstances = new List<FamilyInstance>();


        foreach (Element element in caseworkFamilyInstances)
        {
            FamilyInstance instance = element as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            // Check if the family name starts with any of the prefixes
            if (ekCaseworkFamilyNamePrefixes.Any(familyName.StartsWith))
            {
                // Add the matching instance to the list
                ekCabinetInstances.Add(instance);
            }
        }

        return ekCabinetInstances;
    }

    public static bool IsInstanceAEagleCasework(FamilyInstance familyInstance)
    {
        if (familyInstance == null) return false;
        // Get the family and type names
        string familyName = familyInstance.Symbol.Family.Name;

        if (string.IsNullOrEmpty(familyName)) return false;

        // Has family name as one of the Predefined(hardcoded) value
        if (ekCaseworkFamilyNamePrefixes.Any(familyName.StartsWith)) return true;

        return false;
    }

}
