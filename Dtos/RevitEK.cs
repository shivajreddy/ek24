﻿using Autodesk.Revit.DB;
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
    public FamilySymbol RevitFamilySymbol { get; set; }  // Actual Revit FamilySymbol
    public string EKBrand { get; set; } // ["Yorktowne Classic", "Yorktowne Historic", "Aristokraft", "Eclipse"];
    public string EKCategory { get; set; } // Configuration
    public string EKConfiguration { get; set; }
    public string EKSKU { get; set; }

    // Constructor to enforce initialization
    public EKFamilySymbol(
        FamilySymbol revitFamilySymbol,
        string ekBrand,
        string ekCategory,
        string ekConfiguration,
        string sku
        )
    {
        RevitFamilySymbol = revitFamilySymbol;
        EKBrand = ekBrand;
        EKCategory = ekCategory;
        EKConfiguration = ekConfiguration;
        EKSKU = sku;
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

