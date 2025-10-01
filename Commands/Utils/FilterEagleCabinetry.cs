using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;


namespace ek24.Commands.Utils;


public class FilterEagleCabinetry
{
    private static string[] ekCabinetFamilyNamePrefixes = {
            "Aristokraft-W-",
            "Aristokraft-B-",
            "Aristokraft-T-",
            "Aristokraft-V-",

            "Eclipse-W-",
            "Eclipse-B-",
            "Eclipse-T-",
            "Eclipse-V-",

            "YTC-W-",
            "YTC-B-",
            "YTC-T-",
            "YTC-V-",

            "YTH-W-",
            "YTH-B-",
            "YTH-T-",
            "YTH-V-"
    };

    /// Filter all the eagle cabinetry instances (casework, also genericmodels for now)
    /// if no 'target_brand' is given, shows all eagle casework instances,
    /// if 'target_brand' is given, further filters out any casework instance that is not a 'target_brand'
    public static List<FamilyInstance> FilterProjectForEagleCasework(Document doc, string target_brand = null)
    {
        // FIX: For now we are considering Generic Models also, but after all the families have been set to 'Casework' with out any
        // any generic families, then we remove this
        FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);
        // Create a list of the categories you want
        ICollection<BuiltInCategory> categories = new List<BuiltInCategory> {
                    BuiltInCategory.OST_Casework,
                    BuiltInCategory.OST_GenericModel
        };

        // Create a multi-category filter
        ElementMulticategoryFilter multiCategoryFilter = new ElementMulticategoryFilter(categories);

        // Apply the filter to the collector
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc)
            .WherePasses(multiCategoryFilter);

        ICollection<Element> caseworkFamilyInstances = caseworkCollector
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .ToElements();

        // Filter criteria : 'Manufacturer' value is one of the four (ytc yth ecl aris), also must have vendor SKU
        List<FamilyInstance> ekCaseworkInstances = new List<FamilyInstance>();

        foreach (Element ele in caseworkFamilyInstances)
        {
            FamilyInstance instance = ele as FamilyInstance;

            // Get the family and type names
            string familyName = instance.Symbol.Family.Name;

            // 'Manufacturer' param must be present and should be only 4 brands
            var manufacturerParam = instance.Symbol?.LookupParameter("Manufacturer");
            string manu_param_val = manufacturerParam?.HasValue == true ? manufacturerParam.AsString() ?? manufacturerParam.AsValueString() ?? string.Empty : string.Empty;
            if (manu_param_val is "" or not ("Aristokraft" or "Yorktowne Classic" or "Yorktowne Historic" or "Eclipse"))
                continue;

            // 'VendorSKU' param must be present
            var vendorskuParam = instance.Symbol?.LookupParameter("Vendor_SKU");
            string vendorsku_param_val = vendorskuParam?.HasValue == true ? vendorskuParam.AsString() ?? vendorskuParam.AsValueString() ?? string.Empty : string.Empty;
            if (vendorsku_param_val is "") continue;

            // Filter out all the casework instances that are not of 'target_brand'
            if (target_brand == null) ekCaseworkInstances.Add(instance);
            else
            {
                if (manu_param_val != target_brand) continue;
                ekCaseworkInstances.Add(instance);
            }
        }
        return ekCaseworkInstances;
    }

    public static List<FamilyInstance> FilterProjectForEagleCabinets(Document doc)
    {
        FilteredElementCollector caseworkCollector = new FilteredElementCollector(doc);
        //FilteredElementCollector designOptionsCollector = new FilteredElementCollector(doc);

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
            if (ekCabinetFamilyNamePrefixes.Any(familyName.StartsWith))
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
        if (ekCabinetFamilyNamePrefixes.Any(familyName.StartsWith)) return true;

        return false;
    }

}
