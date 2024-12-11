using System.Collections.Generic;


namespace ek24.UI.Models.Revit;


/// <summary>
/// RevitUtils will parse the EXCEL data, and set the converted
/// models into this class
/// </summary>
public static class RevitFamilyGroups
{
    public static List<FamilyGroup> FamilyGroups;

}


public class FamilyGroup
{
    public string GroupName { get; set; }
    public List<EKFamily> Familys { get; set; }

}

public class EKFamily
{
    public string FamilyName { get; set; }
    public List<EKFamilyType> FamilyTypes;

}

public class EKFamilyType
{
    public string TypeName { get; set; }
}

// Represent the family instance with properties that we care about

public class EKCabinetType
{
    public string TypeName { get; set; }
    public string Note { get; set; }
}
public class EKCabinetFamily
{
    public string FamilyName { get; set; }  // name of this family
    public List<EKCabinetType> TypeNames { get; set; }    // all type names of this fmaily
}

// This is an UI data
public static class ProjectCabinetFamilies
{
    public static List<EKCabinetFamily> CabinetFamilies { get; set; }
}

