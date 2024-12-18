using Autodesk.Revit.DB;
using System.Collections.Generic;

/// Summary
/// Dtos (Data Transfer Objects)
/// DTOs are widely used to represent structured data in a way that simplifies communication between 
/// external APIs and your application. They often serve as clean models for transferring data.
namespace ek24.Dtos;

public enum EKBrand
{
    YORKTOWNE_CLASSIC,
    YORKTOWNE_HISTORIC,
    ARISTOKRAFT,
    ECLIPSE,
}

public class EKCaseworkFamily
{
    public Family revitFamily;  // Actual Revit Family instance, used for symbol

    public EKBrand EKBrand { get; set; } // Brand
    public string Category1 { get; set; } // Configuration
    public string Category2 { get; set; }
    public List<EKCaseworkFamilyType> FamilyTypes; // Actual type of the family Type

}

public class EKCaseworkFamilyType
{
    public string TypeName { get; set; }
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



class RevitEK
{
}
