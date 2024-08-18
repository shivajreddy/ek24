using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ek24.UI.Models.Revit;
using System.Runtime.InteropServices;


namespace ek24.Utils;


/// <summary>
/// - This utility class is responsible to convert data from json and excel
///   into Models.
/// - The 'SetUpRevitData' will be called before creating an instance of the MainView.
/// - The 'Model's after converting are set to the 'RevitBrandData' utility 
///   class in ek24.UI.Models.Revit
/// - The data can then be accessed from Views, ViewModels
/// </summary>
public static class RevitUtils
{
    public static void SetUpRevitData()
    {
        try
        {
            /// JSON DATA
            List<Brand> brands = InitializeVendorFinishes();
            RevitBrandData.Brands = brands;

            /// EXCEL DATA
            List<FamilyGroup> familyGroups = InitializeRevitInventory();
            RevitFamilyGroups.FamilyGroups = familyGroups;
        }
        catch (Exception ex)
        {
            // Handle the exception and throw it within Revit
            string errorMessage = $"An error occurred while deserializing EXCEL & JSON data: {ex.Message}";
            throw new InvalidDataException(errorMessage);
        }
    }


    /// <summary>
    /// Convert JSON data into 'Brand' Model
    /// </summary>
    public static List<Brand> InitializeVendorFinishes()
    {
        string assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string jsonFilePath = Path.Combine(assemblyFolderPath, "Resources", "json", "ek24_brands_styles.json");

        string json = File.ReadAllText(jsonFilePath);

        // Deserialize to the BrandsRoot object
        var root = JsonConvert.DeserializeObject<BrandsRoot>(json);

        return root?.Brands; // Return the Brands list
    }

    /// <summary>
    /// Convert EXCEL data into 'RevitInventory' Model
    /// </summary>
    public static List<FamilyGroup> InitializeRevitInventory()
    {
        // Define the path to the CSV directory
        string assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string csvFolderPath = Path.Combine(assemblyFolderPath, "Resources", "CSV");

        // List to hold all family groups
        List<FamilyGroup> familyGroups = new List<FamilyGroup>();

        // Iterate through each CSV file in the folder
        foreach (var csvFile in Directory.GetFiles(csvFolderPath, "*.csv"))
        {
            FamilyGroup familyGroup = new FamilyGroup
            {
                GroupName = Path.GetFileNameWithoutExtension(csvFile), // Set GroupName based on file name
                Familys = new List<Family>()
            };

            // Read all lines of the current CSV file
            var lines = File.ReadAllLines(csvFile);
            if (lines.Length == 0) continue;

            // First line contains Family names
            var familyNames = lines[0].Split(',');

            // Iterate over the columns (families)
            for (int col = 0; col < familyNames.Length; col++)
            {
                Family family = new Family
                {
                    FamilyName = familyNames[col], // Set FamilyName
                    FamilyTypes = new List<FamilyType>()
                };

                // Iterate over the rows to get FamilyType names
                for (int row = 1; row < lines.Length; row++)
                {
                    var columns = lines[row].Split(',');
                    if (col < columns.Length)
                    {
                        string familyTypeName = columns[col];
                        if (!string.IsNullOrEmpty(familyTypeName))
                        {
                            FamilyType familyType = new FamilyType { TypeName = familyTypeName }; // Set TypeName
                            family.FamilyTypes.Add(familyType);
                        }
                    }
                }

                familyGroup.Familys.Add(family);
            }

            familyGroups.Add(familyGroup);
        }

        return familyGroups;
    }


}


