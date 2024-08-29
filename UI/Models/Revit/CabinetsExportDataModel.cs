using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;


namespace ek24.UI.Models.Revit;


public class CabinetsExportDataModel
{

    public static List<CabinetDataModel> ConvertInstancesToDataModels(List<FamilyInstance> familyInstances)
    {
        // HashMap for all unique rows
        var cabinetDataModels = new Dictionary<string, CabinetDataModel>();


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
                cabinetDataModels[key] = new CabinetDataModel
                {
                    DesignOption = designOption,
                    Brand = brand,
                    Shape = shape,
                    EagleSkew = eagleSkew,
                    BrandSkew = vendorSkew,
                    Notes = notes,
                    Style = vendorStyle,
                    Species = vendorSpecies,
                    Finish = finish,
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
public class CabinetDataModel
{
    /*
    // Look at headers in ExcelExporter.ExportCabinetDataToExcel() and both places should have all same headers

    List of all the parameters we need on the Excel Sheet
    Brand           Shape           Eagle-SKEW      Brand-SKEW      Notes           Style           Finish           Count
    Vendor_Name     Family-Name     Eagle-SKEW      Brand-SKEW      Vendor_Notes    Vendor_Style    Material

    Other Properties Needed
    DesignOption

    */

    public string DesignOption { get; set; }
    public string Brand { get; set; }
    public string Shape { get; set; }
    public string EagleSkew { get; set; }
    public string BrandSkew { get; set; }
    public string Notes { get; set; }
    public string Style { get; set; }
    public string Species { get; set; }
    public string Finish { get; set; }
    public int Count { get; set; }

    public CabinetDataModel()
    {
    }

    /*
    public CabinetDataModel(string designOption, string brand, string shape, string eagleSkew, string brandSkew, string notes, string style, string finish, int count)
    {
        DesignOption = designOption;
        Brand = brand;
        Shape = shape;
        EagleSkew = eagleSkew;
        BrandSkew = brandSkew;
        Notes = notes;
        Style = style;
        Finish = finish;
        Count = count;
    }
    */


}
