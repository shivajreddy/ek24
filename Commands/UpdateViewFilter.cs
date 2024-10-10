using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
using ek24.UI.Views.ChangeBrand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ek24.Commands;


[Transaction(TransactionMode.Manual)]
public class UpdateViewFilterCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // App and Doc
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        HighlightFamilies(doc);


        return Result.Succeeded;
    }


    public void HighlightFamilies(Document doc)
    {
        try
        {
            // Start a transaction to modify the Revit document
            using (Transaction t = new Transaction(doc, "Create Q Filter"))
            {
                t.Start();

                // Define the filter name
                string filterName = "Vendor Name Filter";

                // Get Active View
                Autodesk.Revit.DB.View view = doc.ActiveView;

                // Create a list of categories that will be used for the filter
                IList<ElementId> categories = new List<ElementId> { new ElementId(BuiltInCategory.OST_Casework) };

                // Get the ElementId for the "Manufacturer" built-in parameter
                ElementId manufacturerParamId = new ElementId(BuiltInParameter.ALL_MODEL_MANUFACTURER);

                // Create a filter rule where "Manufacturer" equals "MY VALUE" (case-insensitive)
                string brandValueInProjectParam = GetKitchenBrandFromProjectParam(doc);
                FilterRule manufacturerRule = ParameterFilterRuleFactory.CreateEqualsRule(manufacturerParamId, brandValueInProjectParam);

                // Create an ElementParameterFilter with the rule
                ElementParameterFilter parameterFilter = new ElementParameterFilter(manufacturerRule);

                // Attempt to find an existing filter with the same name
                ParameterFilterElement paramFilter = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterFilterElement))
                    .Cast<ParameterFilterElement>()
                    .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

                if (paramFilter == null)
                {
                    // Create the ParameterFilterElement since it doesn't exist
                    paramFilter = ParameterFilterElement.Create(doc, filterName, categories, parameterFilter);
                }
                else
                {
                    // Update the existing filter's element filter
                    paramFilter.SetElementFilter(parameterFilter);
                }

                // Add the filter to the active view if it's not already added
                if (!view.GetFilters().Contains(paramFilter.Id))
                {
                    view.AddFilter(paramFilter.Id);
                }

                // Set the filter's visibility to true in the view
                view.SetFilterVisibility(paramFilter.Id, true);

                // Create graphic overrides
                OverrideGraphicSettings overrides = new OverrideGraphicSettings()
                    .SetProjectionLineColor(new Autodesk.Revit.DB.Color(255, 0, 255))
                    .SetProjectionLineWeight(5);

                // Apply the graphic overrides to the filter in the view
                view.SetFilterOverrides(paramFilter.Id, overrides);

                // Commit the transaction
                t.Commit();

                // Optionally, inform the user that the filter was successfully applied
                //TaskDialog.Show("Success", $"Filter '{filterName}' has been created/updated and applied to the view.");
            }
        }
        catch (Exception ex)
        {
            // Display the exception message to understand what went wrong
            TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
        }
    }

    private string GetKitchenBrandFromProjectParam(Document doc)
    {

        string defaultBrandName = "Aristokraft";
        var eagleKitchenBrands = RevitBrandData.BrandCatalogues;
        //string[] allowedBrands = { "Aristokraft", "Eclipse", "Yorktowne Historic", "Yorktowne Classic" };


        if (doc == null)
        {
            return defaultBrandName;
        }

        string parameterName = "KitchenBrand";

        // Get the ProjectInfo element
        ProjectInfo projectInfo = doc.ProjectInformation;

        if (projectInfo == null)
        {
            return defaultBrandName;
        }

        // Attempt to retrieve the parameter by name
        Parameter param = projectInfo.LookupParameter(parameterName);

        if (param == null)
        {
            return defaultBrandName;
        }
        else
        {
            string value = param.AsString();

            // Check if the value is null, empty, or not in the allowedBrands array
            if (
                string.IsNullOrEmpty(value)
                || !eagleKitchenBrands.Any(x =>
                    x.BrandName.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return defaultBrandName;
            }

            return value;
        }
    }


}

