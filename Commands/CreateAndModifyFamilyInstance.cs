using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.Dtos;
using ek24.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ek24.Commands;

public static class CreateAndModifyFamilyInstance
{
    public static void CreateNewFamilyInstance(UIApplication app)
    {
        // chosen Type
        //var typeName = CreateBrandTypeViewModel.SelectedBrandFamilyTypeWithNotes.familyType.TypeName;

        // CHOSEN FAMIILY SYMBOL
        FamilySymbol chosen_family_symbol = EK24Modify_ViewModel.ChosenRevitFamilySymbol;

        // Active Document
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Use a filtered element collector to find the family symbol
        //FamilySymbol familySymbol = new FilteredElementCollector(doc)
        //                .OfClass(typeof(FamilySymbol))
        //                .Cast<FamilySymbol>()
        //                .FirstOrDefault(symbol => symbol.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

        // Handle FamilySymbol Not Found
        if (chosen_family_symbol == null)
        {
            TaskDialog.Show("Error", $"SKU not selected from dropdown");
            return;
        }
        // Activate the family symbol if it's not already activated (outside of a transaction)
        if (!chosen_family_symbol.IsActive)
        {
            using (Transaction trans = new Transaction(doc, "Activate Family Symbol"))
            {
                trans.Start();
                chosen_family_symbol.Activate();
                doc.Regenerate();
                trans.Commit();
            }
        }

        // Use PromptForFamilyInstancePlacement without a surrounding transaction
        try
        {
            uiDoc.PromptForFamilyInstancePlacement(chosen_family_symbol);
        }
        catch (Exception)
        {
            // This is a dirty of cancelling out of creating event for now
            TaskDialog.Show("Successfully Exited Creating Elements", "");
        }
    }

    public static void UpdateFamilySymbolsTypeForSelectedInstance(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // TARGET FAMILY SYMBOL
        FamilySymbol chosen_family_symbol = EK24Modify_ViewModel.ChosenRevitFamilySymbol;

        // CURRENT SELECTION
        Selection current_selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;
        ICollection<ElementId> selectedIds = current_selection.GetElementIds();


        // NOTE: Ensure all selected elements are family instances
        // 1. Filter for FamilyInstance elements from the selected IDs
        List<FamilyInstance> current_selected_familyInstances = selectedIds
            .Select(id => doc.GetElement(id))
            .OfType<FamilyInstance>()   // Only FamilyInstances
            .ToList();

        // Handle FamilySymbol Not Found
        if (chosen_family_symbol == null)
        {
            TaskDialog.Show("Error", $"SKU not selected from dropdown");
            return;
        }

        // Start a transaction to change the type
        using (Transaction trans = new Transaction(doc, "Change Cabinet SKU"))
        {
            trans.Start();

            foreach (FamilyInstance instance in current_selected_familyInstances)
            {
                if (instance == null)
                {
                    TaskDialog.Show("Error", "Invalid family instance provided.");
                }
                instance.Symbol = chosen_family_symbol;
            }

            trans.Commit();
        }
    }


}
