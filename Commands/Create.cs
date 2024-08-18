using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ek24.UI.ViewModels.Properties;


namespace ek24.Commands;


public static class Create
{
    public static void CreateNewInstance(UIApplication app)
    {
        // chosen Family, Type
        var family_name = CreateInstanceViewModel.SelectedFamily.FamilyName;
        var type_name = CreateInstanceViewModel.SelectedFamilyType.TypeName;

        // Active Document
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Find the FamilySymbol by family name and type name
        FamilySymbol familySymbol = null;


        // Use a filtered element collector to find the family symbol
        familySymbol = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(symbol =>
                            symbol.Family.Name.Equals(family_name, StringComparison.OrdinalIgnoreCase) &&
                            symbol.Name.Equals(type_name, StringComparison.OrdinalIgnoreCase));

        // Handle FamilySymbol Not Found
        if (familySymbol == null)
        {
            TaskDialog.Show("Error", $"Family symbol for '{family_name}' with type '{type_name}' not found.");
            return;
        }
        // Activate the family symbol if it's not already activated (outside of a transaction)
        if (!familySymbol.IsActive)
        {
            using (Transaction trans = new Transaction(doc, "Activate Family Symbol"))
            {
                trans.Start();
                familySymbol.Activate();
                doc.Regenerate();
                trans.Commit();
            }
        }

        // Use PromptForFamilyInstancePlacement without a surrounding transaction
        try
        {
            uiDoc.PromptForFamilyInstancePlacement(familySymbol);
        }
        catch (Exception)
        {
            // This is a dirty of cancelling out of creating event for now
            TaskDialog.Show("Finished Exited", "");
        }

    }

}

//uiDoc.RefreshActiveView();
