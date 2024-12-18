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

