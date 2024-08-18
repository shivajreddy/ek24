using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
    public List<Family> Families { get; set; }

}

public class Family
{
    public string FamilyName { get; set; }
    public List<FamilyType> FamilyTypes;

}

public class FamilyType
{
    public string TypeName;
}

