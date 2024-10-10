using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace ek24.UI.ViewModels.ChangeBrand
{
    public class FamilyInstanceInfo
    {
        // Read-only properties to maintain immutability
        public ElementId ElementId { get; }
        public FamilyInstance FamilyInstance { get; }
        public string BrandName { get; }
        public string TypeName { get; }

        // Constructor for FamilyInstanceInfo
        public FamilyInstanceInfo(FamilyInstance familyInstance, string brandName, string typeName)
        {
            ElementId = familyInstance.Id;
            FamilyInstance = familyInstance;
            BrandName = brandName;
            TypeName = typeName;
        }

        // Optional: Override ToString() for easy printing/debugging
        public override string ToString()
        {
            return $"Element ID: {ElementId}, Brand Name: {BrandName}, Type Name: {TypeName}";
        }
    }

    public class BrandMapper
    {
        // Dictionary to hold the matrix of SKUs between brands
        private static readonly Dictionary<string, Dictionary<string, string>> brandSkuMatrix = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "Aristokraft", new Dictionary<string, string>()
                {
                    { "W3024B", "W3024B" }, // Corresponds to itself
                    { "W3924", "W3924" },
                    { "W2142", "W2142" },
                    { "W2742B", "W2742B" },
                    { "W3342B", "W3342B" },
                    { "B21", "B21" },
                    { "B27B", "B27B" },
                    { "B30B", "B30B" }
                }
            },
            {
                "Yorktowne Classic", new Dictionary<string, string>()
                {
                    { "W3024B", "W3024B" },
                    { "W3924", "W3924" },
                    { "W2142", "W2142" },
                    { "W2742B", "W2742B" },
                    { "W3342B", "W3342B" },
                    { "B21", "B21" },
                    { "B27B", "B27B" },
                    { "B30B", "B30B" }
                }
            },
            {
                "Yorktowne Historic", new Dictionary<string, string>()
                {
                    { "W3024B", "W3024B" },
                    { "W3924", "W3924" },
                    { "W2142", "W2142" },
                    { "W2742B", "W2742B" },
                    { "W3342B", "W3342B" },
                    { "B21", "B21" },
                    { "B27B", "B27B" },
                    { "B30B", "B30B" }
                }
            },
            {
                "Eclipse", new Dictionary<string, string>()
                {
                    { "W3024", "W3024" },
                    { "W3924", "W3924" },
                    { "W2142", "W2142" },
                    { "W2742", "W2742" },
                    { "W3342", "W3342" },
                    { "B21", "B21" },
                    { "B27", "B27" },
                    { "B30", "B30" }
                }
            }
        };

        // Method to map SKUs from one brand to another
        public string MapBrandSkews(FamilyInstanceInfo familyInfo, string targetBrand)
        {
            if (familyInfo == null || string.IsNullOrEmpty(familyInfo.BrandName) || string.IsNullOrEmpty(familyInfo.TypeName))
                return null;

            // Check if both brands exist in the matrix
            if (brandSkuMatrix.ContainsKey(familyInfo.BrandName) && brandSkuMatrix.ContainsKey(targetBrand))
            {
                var currentBrandSKUs = brandSkuMatrix[familyInfo.BrandName];

                // Check if the SKU exists for the current brand
                if (currentBrandSKUs.ContainsKey(familyInfo.TypeName))
                {
                    // Return the corresponding SKU for the target brand
                    return currentBrandSKUs[familyInfo.TypeName];
                }
            }

            // Return null if no valid mapping is found
            return null;
        }

        // Utility method to extract FamilyInstanceInfo from FamilyInstance objects
        public static List<FamilyInstanceInfo> GetFamilyInstanceInfos(List<FamilyInstance> familyInstances)
        {
            // Dictionary to map received "Vendor_Name" values to the correct target brand names
            Dictionary<string, string> vendorNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "YORKTOWNE-CLASSIC", "Yorktowne Classic" },
                { "YORKTOWNE-HISTORIC", "Yorktowne Historic" },
                { "ECLIPSE", "Eclipse by Shiloh" },
                { "ARISTOKRAFT", "Aristokraft" }
            };

            List<FamilyInstanceInfo> familyInstanceInfos = new List<FamilyInstanceInfo>();

            foreach (FamilyInstance familyInstance in familyInstances)
            {
                // Extract brand name from the "Vendor_Name" parameter
                string vendorName = GetTypeParameterValue(familyInstance, "Vendor_Name");

                // Map the vendor name to the target brand name using the dictionary
                string brandName = vendorNameMapping.ContainsKey(vendorName) ? vendorNameMapping[vendorName] : vendorName;

                // Extract type name (SKU)
                string typeName = familyInstance.Symbol?.Name ?? "Unknown Type";

                // Create a new FamilyInstanceInfo object
                FamilyInstanceInfo familyInstanceInfo = new FamilyInstanceInfo(familyInstance, brandName, typeName);

                // Add it to the list
                familyInstanceInfos.Add(familyInstanceInfo);
            }

            return familyInstanceInfos;
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
}
