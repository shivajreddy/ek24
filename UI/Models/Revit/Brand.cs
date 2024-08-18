using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ek24.UI.Models.Revit;



/// <summary>
/// RevitUtils will parse the JSON data, and set the converted
/// models into this class
/// </summary>
public static class RevitBrandData
{
    public static List<Brand> Brands { get; set; }
}

/// <summary>
/// Exact shape of the JSON file
/// </summary>
public class BrandsRoot
{
    public List<Brand> Brands { get; set; }
}

public class Brand
{
    public string BrandName { get; set; }
    public List<Style> Styles { get; set; }
}

public class Style
{
    public string StyleName { get; set; }
    public List<string> Properties { get; set; }
    public List<SpeciesVariant> SpeciesVariants { get; set; }
}

public class SpeciesVariant
{
    public string SpeciesName { get; set; }
    public List<string> Finishes { get; set; }
}

