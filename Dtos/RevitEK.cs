using Autodesk.Revit.DB;
using System.Collections.Generic;

/// Summary
/// Dtos (Data Transfer Objects)
/// DTOs are widely used to represent structured data in a way that simplifies communication between 
/// external APIs and your application. They often serve as clean models for transferring data.
namespace ek24.Dtos;


public static class EKBrands
{
    public static List<string> all_brand_names = ["Yorktowne Classic", "Yorktowne Historic", "Aristokraft", "Eclipse"];
}


public class EKFamilySymbol
{
    public string EKBrand { get; set; } // ["Yorktowne Classic", "Yorktowne Historic", "Aristokraft", "Eclipse"];
    public string EKType { get; set; } // Configuration
    public string EKCategory { get; set; }
    public EK_SKU EKSKU { get; set; } // <Actual_Revit_FamilySymbol, Type_name, VendorNote_param_value>
    public ElementId RevitFamilySymbolId { get; set; }

    // Constructor to enforce initialization
    public EKFamilySymbol(
        //FamilySymbol revitFamilySymbol,
        string ekBrand,
        string ekType,
        string ekCategory,
        EK_SKU ek_SKU,
        ElementId revitFamilySymbolId
        )
    {
        //RevitFamilySymbol = revitFamilySymbol;
        EKBrand = ekBrand;
        EKType = ekType;
        EKCategory = ekCategory;
        EKSKU = ek_SKU;
        RevitFamilySymbolId = revitFamilySymbolId;
    }
}

public class EK_SKU
{
    public FamilySymbol RevitFamilySymbol { get; set; }  // Actual Revit FamilySymbol
    public string TypeName { get; set; }    // Revit's Type of a family
    public string VendorNotes { get; set; }    // "Vendor_Notes" param's value

    public override string ToString()
    {
        if (VendorNotes == "")
        {
            return TypeName;
        }
        return $"{TypeName} - {VendorNotes}";
    }

    public EK_SKU(string typeName, string vendorNotes, FamilySymbol revitFamilySymbol)
    {
        TypeName = typeName;
        VendorNotes = vendorNotes;
        RevitFamilySymbol = revitFamilySymbol;
    }
}



// Specifically Cabinet
public class EKCabinetFamily
{
    public string FamilyName { get; set; }  // name of this family
    public List<EKCabinetType> TypeNames { get; set; }    // all type names of this fmaily
}

// Represent the family instance with properties that we care about
public class EKCabinetType
{
    public string TypeName { get; set; }
    public string Note { get; set; }     // Parameter-value of "Vendor-Notes"
}



