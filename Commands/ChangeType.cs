using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using ek24.UI.ViewModels.Properties;
using ek24.UI.Services;

namespace ek24.Commands;

public class ChangeType
{
    public static void UpdateCabinetTypeForSelectedInstance(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Assume that you have already selected a FamilyInstance and have the new type name

        // moving this logic into SelectinService
        //FamilyInstance selectedInstance = SelectionService.SelectedFamilyInstance;

        FamilyInstance selectedInstance = CurrentSelectionViewModel.SelectedFamilyInstance;

        string newTypeName = CurrentSelectionViewModel.ChosenCabinetType.Item1;


        // Call the function to change the type
        ChangeFamilyInstanceType(selectedInstance, newTypeName, doc);
    }




    public static void ChangeFamilyInstanceType(FamilyInstance instance, string newTypeName, Document doc)
    {
        if (instance == null || string.IsNullOrEmpty(newTypeName))
        {
            TaskDialog.Show("Error", "Invalid family instance or type name provided.");
            return;
        }

        // Find the FamilySymbol with the given name
        FamilySymbol newSymbol = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(symbol => symbol.Name.Equals(newTypeName, StringComparison.OrdinalIgnoreCase) && symbol.Family.Id == instance.Symbol.Family.Id);

        if (newSymbol == null)
        {
            TaskDialog.Show("Error", $"FamilySymbol with name '{newTypeName}' not found in the same family as the current instance.");
            return;
        }

        // Start a transaction to change the type
        using (Transaction trans = new Transaction(doc, "Change Family Instance Type"))
        {
            trans.Start();

            // Change the type of the FamilyInstance
            instance.Symbol = newSymbol;

            trans.Commit();
        }
    }



}