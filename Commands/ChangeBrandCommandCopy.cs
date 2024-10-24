using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Models.Revit;
using ek24.UI.Views.ChangeBrand;
using System;
using System.Collections.ObjectModel;
using System.Linq;


namespace ek24.Commands;


[Transaction(TransactionMode.Manual)]
public class ChangeBrandCommandCopy : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // App and Doc
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;


        /*
        // Create the WPF window
        var window = new ChangeBrandView();

        var result = window.ShowDialog();

        window.Close();

        return result == true ? Result.Succeeded : Result.Cancelled;
        //*/

        return Result.Succeeded;
    }

    private void SetKitchenBrandProjectParamValue(string newValue, Document doc)
    {
        if (doc == null)
            return;

        ProjectInfo projectInfo = doc.ProjectInformation;
        if (projectInfo == null)
            return;

        string parameterName = "KitchenBrand";
        Parameter param = projectInfo.LookupParameter(parameterName);
        if (param == null)
            return;

        if (param.IsReadOnly)
            return;

        // Chosen Brand should be a valid KitchenBrand
        if (newValue == null || newValue == "")
            return;

        var kitchenBrands = RevitBrandData.Brands;
        if (
            string.IsNullOrEmpty(newValue)
            || !kitchenBrands.Any(x =>
                x.BrandName.Equals(newValue, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return;
        }

        // Begin a transaction to modify the document
        using (Transaction trans = new Transaction(doc, "Update Project Parameter 'KitchenBrand'"))
        {
            trans.Start();
            try
            {
                param.Set(newValue);

                // Commit the transaction
                trans.Commit();
                //TaskDialog.Show("Success", $"Parameter '{parameterName}' has been set to '{newValue}'.");
            }
            catch (Exception ex)
            {
                // Roll back the transaction in case of error
                trans.RollBack();
                TaskDialog.Show("Error", $"Failed to set parameter: {ex.Message}");
                return;
            }
        }
    }
}
