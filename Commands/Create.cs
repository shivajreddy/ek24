using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ek24.UI.ViewModels.Properties;


namespace ek24.Commands;


public static class Create
{
    public static void CreateNewFamilyInstanceUsingPanel1(UIApplication app)
    {
        // chosen Family, Type
        var familyName = CreateFamilyTypeViewModel.SelectedFamily.FamilyName;
        var typeName = CreateFamilyTypeViewModel.SelectedFamilyType.TypeName;

        // Active Document
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Use a filtered element collector to find the family symbol
        FamilySymbol familySymbol = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(symbol =>
                            symbol.Family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase) &&
                            symbol.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

        // Handle FamilySymbol Not Found
        if (familySymbol == null)
        {
            TaskDialog.Show("Error", $"Family symbol for '{familyName}' with type '{typeName}' not found.");
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
    public static void CreateNewFamilyInstanceUsingPanel2(UIApplication app)
    {
        // chosen Type
        var typeName  = CreateBrandTypeViewModel.SelectedBrandFamilyType.TypeName;

        // Active Document
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Use a filtered element collector to find the family symbol
        FamilySymbol familySymbol = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(symbol => symbol.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

        // Handle FamilySymbol Not Found
        if (familySymbol == null)
        {
            TaskDialog.Show("Error", $"Family symbol for with type '{typeName}' not found.");
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

    public static FamilySymbol FindFamilySymbolWithTypeName(string typeName, Document doc)
    {
        var collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Casework)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>();

        // Search for the FamilySymbol with the matching name
        var familySymbol = collector.FirstOrDefault(fs => fs.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

        return familySymbol;
    }

    public static FamilySymbol FindFamilySymbolWithFamilyAndTypeName(string familyName, string typeName, Document doc)
    {

        // Find the FamilySymbol by family name and type name
        FamilySymbol familySymbol = null;


        return familySymbol;
    }

}

//uiDoc.RefreshActiveView();
