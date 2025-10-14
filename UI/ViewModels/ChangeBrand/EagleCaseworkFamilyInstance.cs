using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace ek24.UI.ViewModels.ChangeBrand;

public class EagleCaseworkFamilyInstance
{
    // Read-only properties to maintain immutability
    public ElementId ElementId { get; }
    public FamilyInstance FamilyInstance { get; }
    public string BrandName { get; }
    public string TypeName { get; }
    public string FamilyName { get; }
    public string EKCategory { get; }
    public string EKType { get; }
    public string VendorSKU { get; }

    // Constructor for FamilyInstanceInfo
    public EagleCaseworkFamilyInstance(FamilyInstance familyInstance, string brandName, string typeName,
        string familyName, string ekCategory, string ekType, string vendorSKU)
    {
        ElementId = familyInstance.Id;
        FamilyInstance = familyInstance;
        BrandName = brandName;
        TypeName = typeName;
        FamilyName = familyName;
        EKCategory = ekCategory;
        EKType = ekType;
        VendorSKU = vendorSKU;
    }

    // Optional: Override ToString() for easy printing/debugging
    public override string ToString()
    {
        return $"Element ID: {ElementId}, Brand Name: {BrandName}, Type Name: {TypeName}";
    }
}

/// This is the utility class that represents the file '\ek24\Resources\Excel\Cabinet Manufacturer Matrix' in
/// the form of C# data structures
public class BrandMapper
{
    // Define brand names and their corresponding columns in the matrix
    static Dictionary<string, int> brandIndex = new Dictionary<string, int>
    {
        { "Aristokraft", 0 },
        { "Yorktowne Classic", 1 },
        { "Yorktowne Historic", 2 },
        { "Eclipse", 3 }
    };

    // Define the matrix of item types and brand-specific types
    // TODO: make the following variable to be instantiated during Revit opening(ek24 loading for first time), by
    // using a CSV file and converting the data in csv file into this matrix.
    static string[,] matrix = new string[,]
    {
        { "W3024B", "W3024B", "W3024B", "W3024" },
        { "W3924", "W3924", "W3924", "W3924" },
        { "W2142", "W2142", "W2142", "W2142" },
        { "W2742B", "W2742B", "W2742B", "W2742" },
        { "W3342B", "W3342B", "W3342B", "W3342" },
        { "B21", "B21", "B21", "B21" },
        { "B27B", "B27B", "B27B", "B27" },
        { "B30B", "B30B", "B30B", "B30" },
        { "B33B", "B33B", "B33B", "B33" },
        { "SB36B", "SB36B", "SB36B", "SB36" },
        { "F331", "BF330", "BF330", "F330" },
        { "F342", "WF342", "WF342", "F342" },
        { "PEPR335", "DEP3WL/R", "DEP3WL/R", "BEP3-L/R" },
        { "PP9635", "PNL149634", "PNL129634", "BB1/4" },
        { "TOEKICK8", "TK96", "TK96", "TK" },
        { "MQR8", "SM8", "SM8", "3/4QR" },
        { "MOCW8", "CMD8", "CMD8", "3/4OSC" },
        { "MSW8", "MLD8", "MLD8", "NA" },
        { "BWB15BMG", "B15WB", "B15WBS", "BWDMWSD15" },
        { "BWB18BMG", "B18DWB", "B18DWBS", "BWDMWSD18" },
        { "MCROWN8", "CCM8", "CCM8", "NA" },
        { "MSMCOVECR8", "WCVM8", "WCVM8", "NA" },
        { "MBS8", "RBHM8", "RBHM8", "NA" },
        { "MSINBEAD8", "PSSB396", "PSSB396", "3SRM11/2F" },
        { "DWEP42", "Modification", "Modification", "NA" },
        { "DBEPFH", "Modification", "Modification", "NA" },
        { "DUEP96", "Modification", "Modification", "NA" },
        { "RW3624B", "24W3624B", "24W3624B", "RW3624" },
        { "PREPRP1.596", "REP2496W", "REP2496W", "REP1 1/2-FTK-24-L/R" },
        { "BPP09", "BPS09", "BPS09", "BPOS-9" },
        { "NA", "BPS15", "BPS15", "BPOS-15" },
        { "WWG(W)(H)", "Modification", "Modification", "Modification" },
        { "NA", "2DB3034", "2DB3034", "B2HD30" },
        { "NA", "2DB3334", "2DB3334", "B2HD33" },
        { "NA", "2DB3634", "2DB3634", "B2HD36" },
        { "DB12", "3DB12", "3DB12", "B3D12" },
        { "DB15", "3DB15", "3DB15", "B3D15" },
        { "DB18", "3DB18", "3DB18", "B3D18" },
        { "DB21", "3DB21", "3DB21", "B3D21" },
        { "DB24", "3DB24", "3DB24", "B3D24" },
        { "DB27", "3DB27", "3DB27", "B3D27" },
        { "DB30", "3DB30", "3DB30", "B3D30" },
        { "DB33", "3DB33", "3DB33", "B3D33" },
        { "DB36", "3DB36", "3DB36", "B3D36" },
        { "DB12-4", "4DB12", "4DB12", "B4D12" },
        { "DB15-4", "4DB15", "4DB15", "B4D15" },
        { "DB18-4", "4DB18", "4DB18", "B4D18" },
        { "DB21-4", "4DB21", "4DB21", "B4D21" },
        { "DB24-4", "4DB24", "4DB24", "B4D24" },
        { "DW2442", "DW2442", "DW2442", "A2142" },
        { "SCER36", "SCB36", "SCB36", "BL36-PH" },
        { "BWRER36", "SSCB36WT", "SSCB36WT", "BL36-SS-PH" },
        { "NA", "BTK12-1", "NA", "DROT5/8" },
        { "RT15", "BTK15-1", "BTK15-1", "" },
        { "RT18", "BTK18-1", "BTK18-1", "" },
        { "RT21", "BTK21-1", "BTK21-1", "" },
        { "RT24", "BTK24-1", "BTK24-1", "" },
        { "RT27", "BTK27-1", "BTK27-1", "" },
        { "RT30", "BTK30-1", "BTK30-1", "" },
        { "RT33", "BTK33-1", "BTK33-1", "" },
        { "RT36", "BTK36-1", "BTK36-1", "" },
        { "TBD", "KNIFE15/18/21/24", "KNIFE15/18/21/24", "KB1/KB2/KB3" },
        { "WCDT", "CUT12/15/18/21/24/27/30", "CUT12/15/18/21/24/27/30", "WCD1/WCD2/WCD3" },
        { "TBD", "24PEGKIT30/33/36", "24PEGKIT30/33/36", "DPS-27/DPS-33/DPS-42" },
        { "BMW2435DD", "BW24LD", "BW24LD", "BO27" },
        { "NA", "BFHKC15", "BFHKC15", "BKI-15" },
        { "B09FH", "B09", "B09", "B9-FHD" },
        { "B09TDFH", "TB09", "TB09", "TB9" },
        { "CNTYSB36B", "SBA36", "SBA36", "SBA36" },
        { "FS24", "FLTS24", "FLTS24", "FLS - custom size" },
        { "NA", "FLTS27", "FLTS27", "" },
        { "FS30", "FLTS30", "FLTS30", "" },
        { "NA", "FLTS33", "FLTS33", "" },
        { "FS36", "FLTS36", "FLTS36", "" },
        { "NA", "FLTS39", "FLTS39", "" },
        { "FS42", "FLTS42", "FLTS42", "" },
        { "NA", "FLTS45", "FLTS45", "" },
        { "NA", "FLTS48", "FLTS48", "" },
        { "NA", "FLTS54", "FLTS54", "" },
        { "NA", "FLTS60", "FLTS60", "" },
        { "NA", "FLTS66", "FLTS66", "" },
        { "NA", "FLTS72", "FLTS72", "" }
    };

    // Takes in family instances and Id's of target FamilySymbols they should be changed to
    // Returns a list of (ElementId, string) with status messages
    private static List<Tuple<ElementId, string>> ChangeFamilyInstanceTypes(
        Document doc,
        List<Tuple<EagleCaseworkFamilyInstance, ElementId>> pairs)
    {
        var results = new List<Tuple<ElementId, string>>();

        foreach (var pair in pairs)
        {
            var familyInstance = pair.Item1;
            var familySymbolId = pair.Item2;

            try
            {
                FamilySymbol targetFamilySymbol = doc.GetElement(familySymbolId) as FamilySymbol;

                if (targetFamilySymbol == null)
                {
                    results.Add(Tuple.Create(familyInstance.FamilyInstance.Id,
                        $"Error: Target FamilySymbol with Id {familySymbolId.IntegerValue} not found."));
                    continue;
                }

                // Ensure that the target family symbol is active
                if (!targetFamilySymbol.IsActive)
                {
                    targetFamilySymbol.Activate();
                    doc.Regenerate(); // Make sure activation takes effect
                }

                // Change the family type for the family instance
                familyInstance.FamilyInstance.Symbol = targetFamilySymbol;

                // Success message
                results.Add(Tuple.Create(familyInstance.FamilyInstance.Id,
                    $"Changed FamilyInstance {familyInstance.FamilyInstance.Id.IntegerValue} to FamilySymbol {targetFamilySymbol.Name}"));
            }
            catch (Exception ex)
            {
                results.Add(Tuple.Create(familyInstance.FamilyInstance.Id,
                    $"Error changing FamilyInstance {familyInstance.FamilyInstance.Id.IntegerValue}: {ex.Message}"));
            }
        }

        return results;
    }


    public static string FindTargetFamilySymbolBrandType(
    Document doc,
    string currentBrandName,
    string sku_val,
    string targetBrandName)
    {
        // LOGIC: - the column no. to look for is the current brand
        //        - target column no. is the target brand
        //        - find an entry in this column that is equal to current sku, and the target sku from target column

        // Get column indices for current and target brands
        int currentBrandColumn = brandIndex[currentBrandName];
        int targetBrandColumn = brandIndex[targetBrandName];

        // Search each row for a match in current brand type
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, currentBrandColumn] == sku_val)
            {
                string targetSku = matrix[row, targetBrandColumn];

                // Handle "NA" or "Modification" placeholders
                if (string.IsNullOrWhiteSpace(targetSku) ||
                    targetSku == "NA" ||
                    targetSku == "Modification")
                {
                    return (string)null;
                }
                return targetSku;

                //// Now lookup the FamilySymbol with this targetSku
                //FilteredElementCollector collector = new FilteredElementCollector(doc)
                //    .OfClass(typeof(FamilySymbol));

                //FamilySymbol symbol = collector
                //    .Cast<FamilySymbol>()
                //    .FirstOrDefault(s => s.Name.Equals(targetSku, StringComparison.OrdinalIgnoreCase));

                // Return both the ElementId and the target SKU string
                //return Tuple.Create(symbol?.Id ?? (ElementId)null, targetSku);
            }
        }

        // If no match was found
        //return Tuple.Create((ElementId)null, (string)null);
        return (string)null;
    }


    public static string FindTargetBrandType(string currentBrandName, string currentBrandType, string targetBrandName)
    {

        if (currentBrandName == targetBrandName)
        {
            return null;
        }
        if (!brandIndex.ContainsKey(currentBrandName))
        {
            return null;
        }
        if (!brandIndex.ContainsKey(targetBrandName))
        {
            return null;
        }

        // Get column indices for current and target brands
        int currentBrandColumn = brandIndex[currentBrandName];
        int targetBrandColumn = brandIndex[targetBrandName];

        // Search the row for the current brand type
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, currentBrandColumn] == currentBrandType)
            {
                // Return the corresponding type for the target brand
                return matrix[row, targetBrandColumn];
            }
        }

        return null; // Return null if the item type is not found
    }

    // Utility method to extract FamilyInstanceInfo from FamilyInstance objects
    public static List<EagleCaseworkFamilyInstance> ConvertFamilyInstaceToEagleCaseworkFamilyInstance(List<FamilyInstance> familyInstances)
    {
        var eagleCaseWorkInstances = new List<EagleCaseworkFamilyInstance>();

        foreach (FamilyInstance familyInstance in familyInstances)
        {
            // Extract brand name from the "Manufacturer" parameter
            string brandName = familyInstance.Symbol.LookupParameter("Manufacturer").AsValueString();

            // Extract type name (SKU)
            string typeName = familyInstance.Symbol?.Name ?? "Unknown Type";

            string familyName = familyInstance.Symbol?.FamilyName;
            string ekCategory = familyInstance.Symbol.LookupParameter("EKCategory").AsValueString();
            string ekType = familyInstance.Symbol.LookupParameter("EKType").AsValueString();
            string vendorSKU = familyInstance.Symbol.LookupParameter("Vendor_SKU").AsValueString();

            // Create the EagleCaseworkFamilyInstance object
            var eagleCaseworkFamilyInstance = new EagleCaseworkFamilyInstance(familyInstance, brandName, typeName,
                familyName, ekCategory, ekType, vendorSKU);

            // Add it to the list
            eagleCaseWorkInstances.Add(eagleCaseworkFamilyInstance);
        }

        return eagleCaseWorkInstances;
    }

    // Helper method to get the value of a type parameter from FamilyInstance
    private static string GetTypeParameterValue(FamilyInstance instance, string typeParamName)
    {
        var type = instance.Symbol;
        var typeParam = type?.LookupParameter(typeParamName);

        // Return the parameter value if it exists and has a value
        return typeParam?.HasValue == true ? typeParam.AsString() ?? typeParam.AsValueString() ?? string.Empty : string.Empty;
    }
}
