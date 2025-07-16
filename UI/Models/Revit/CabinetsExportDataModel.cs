using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;


namespace ek24.UI.Models.Revit;


public class CabinetsExportDataModel
{
    public static List<EKCrownMoldingTableDataModel> ConvertEKCrownMoldingInstancesToEKCrownMoldingDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var crownMoldingDataModelsDictionary = new Dictionary<string, EKCrownMoldingTableDataModel>();


        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Vendor_Name");
            var vendorSkew = GetTypeParameterValue(instance, "Vendor_SKEW");

            // Create a unique key by concatenating relevant fields
            string key = $"{designOption}|{brand}|{vendorSkew}";

            // Same row item already exists, just increase the Count property 
            if (crownMoldingDataModelsDictionary.ContainsKey(key))
            {
                crownMoldingDataModelsDictionary[key].Count++;

            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                crownMoldingDataModelsDictionary[key] = new EKCrownMoldingTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    SKU = vendorSkew,
                    Count = 1
                };

            }

        }


        return crownMoldingDataModelsDictionary.Values.ToList();
    }

    public static List<EKSidePanelTableDataModel> ConvertEKSidePanelInstancesToEKCrownMoldingDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var sidePanelDataModelsDictionary = new Dictionary<string, EKSidePanelTableDataModel>();

        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Vendor_Name");
            var vendorSkew = GetTypeParameterValue(instance, "Vendor_SKEW");

            // Create a unique key by concatenating relevant fields
            string key = $"{designOption}|{brand}|{vendorSkew}";

            // Same row item already exists, just increase the Count property 
            if (sidePanelDataModelsDictionary.ContainsKey(key))
            {
                sidePanelDataModelsDictionary[key].Count++;

            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                sidePanelDataModelsDictionary[key] = new EKSidePanelTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    BrandSKU = vendorSkew,
                    Count = 1
                };

            }

        }

        return sidePanelDataModelsDictionary.Values.ToList();
    }

    public static List<EKFillerStripTableDataModel> ConvertEKFillerStripInstancesToEKCrownMoldingDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var fillerStripDataModelsDictionary = new Dictionary<string, EKFillerStripTableDataModel>();

        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Vendor_Name");
            var vendorSkew = GetTypeParameterValue(instance, "Vendor_SKEW");

            // Create a unique key by concatenating relevant fields
            string key = $"{designOption}|{brand}|{vendorSkew}";

            // Same row item already exists, just increase the Count property 
            if (fillerStripDataModelsDictionary.ContainsKey(key))
            {
                fillerStripDataModelsDictionary[key].Count++;

            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                fillerStripDataModelsDictionary[key] = new EKFillerStripTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    BrandSKU = vendorSkew,
                    Count = 1
                };

            }

        }

        return fillerStripDataModelsDictionary.Values.ToList();
    }

    public static List<EKCabinetTableDataModel> ConvertEKCabinetInstancesToEKCabinetDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var cabinetDataModels = new Dictionary<string, EKCabinetTableDataModel>();


        foreach (var instance in familyInstances)
        {
            // Only Main Model instances have 'null' as DesignOption
            var designOption = instance.DesignOption != null ? instance.DesignOption.Name : "Main Model";

            var brand = GetTypeParameterValue(instance, "Vendor_Name");
            var shape = GetTypeParameterValue(instance, "Family Name");
            var eagleSkew = GetTypeParameterValue(instance, "Eagle_SKEW");
            var vendorSkew = GetTypeParameterValue(instance, "Vendor_SKEW");
            var notes = GetTypeParameterValue(instance, "Vendor_Notes");
            var vendorStyle = GetInstanceParameterValue(instance, "Vendor_Style");
            var vendorSpecies = GetInstanceParameterValue(instance, "Vendor_Species");

            var finish = GetInstanceParameterValue(instance, "Vendor_Finish");
            // empty value for material is '<By Category>' so ignore that
            finish = finish != "<By Category>" ? finish : string.Empty;

            // Create a unique key by concatenating relevant fields
            string key = $"{designOption}|{brand}|{shape}|{eagleSkew}|{vendorSkew}|{notes}|{vendorStyle}|{vendorSpecies}|{finish}";

            // Same row item already exists, just increase the Count property 
            if (cabinetDataModels.ContainsKey(key))
            {
                cabinetDataModels[key].Count++;

            }
            // New row item, add a new CabinetDataModel with count set to 1
            else
            {
                cabinetDataModels[key] = new EKCabinetTableDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    Configuration = shape,
                    //EagleSkew = eagleSkew,
                    BrandSKU = vendorSkew,
                    Notes = notes,
                    VendorStyle = vendorStyle,
                    //Species = vendorSpecies,
                    VendorFinish = finish,
                    Count = 1
                };

            }

        }

        return cabinetDataModels.Values.ToList();
    }
    private static string GetTypeParameterValue(FamilyInstance instance, string typeParamName)
    {
        var type = instance.Symbol;
        var typeParam = type.LookupParameter(typeParamName);

        // Param is not found or has no value
        if (typeParam == null || !typeParam.HasValue)
        {
            return string.Empty;
        }
        if (typeParam.AsString() != null)
        {
            return typeParam.AsString();
        }
        if (typeParam.AsValueString() != null)
        {
            return typeParam.AsValueString();
        }
        return string.Empty;
    }

    private static string GetInstanceParameterValue(FamilyInstance familyInstance, string instanceParamName)
    {
        var instanceParam = familyInstance.LookupParameter(instanceParamName);

        // Param is not found or has no value
        if (instanceParam == null || !instanceParam.HasValue)
        {
            return string.Empty;
        }
        if (instanceParam.AsString() != null)
        {
            return instanceParam.AsString();
        }
        if (instanceParam.AsValueString() != null)
        {
            return instanceParam.AsValueString();
        }
        return string.Empty;
    }
}

// usually you should place this in its own class file, but for convenience to look at the shape, i put it herre
public class EKCabinetTableDataModel
{
    public string DesignOption { get; set; }

    public string Brand { get; set; }
    public string BrandSKU { get; set; }
    public string Configuration { get; set; }
    public string Notes { get; set; }
    public string VendorStyle { get; set; }
    public string VendorSpecies { get; set; }   //new
    public string VendorFinish { get; set; }
    public string Features { get; set; }  // new
    public string HingleLocation { get; set; }  // new
    public double BaseCost { get; set; }
    public double CostWithFeatures { get; set; }
    public int Count { get; set; }
    //public string EagleSkew { get; set; }
    //public string Species { get; set; }

    public EKCabinetTableDataModel()
    {
    }

}

public class EKCrownMoldingTableDataModel
{
    public string DesignOption { get; set; }
    public string Brand { get; set; }
    public string SKU { get; set; }
    public string Notes { get; set; }
    public string Length { get; set; }
    public int Count { get; set; }

    public EKCrownMoldingTableDataModel() { }
}


public class EKSidePanelTableDataModel
{
    public string DesignOption { get; set; }
    public string Brand { get; set; }
    public string BrandSKU { get; set; }
    public int Count { get; set; }

    public EKSidePanelTableDataModel() { }
}

public class EKFillerStripTableDataModel
{
    public string DesignOption { get; set; }
    public string Brand { get; set; }
    public string BrandSKU { get; set; }
    public int Count { get; set; }
    public EKFillerStripTableDataModel() { }
}

