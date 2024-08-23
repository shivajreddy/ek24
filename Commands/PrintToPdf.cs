using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI.Selection;
using ek24.UI.ViewModels.Manage;


namespace ek24.Commands;


public class PrintToPdf
{

    public static void PrintDocument(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // Get the print manager from the document
        PrintManager printManager = doc.PrintManager;

        printManager.PrintRange = PrintRange.Select;

        // Target print settings
        string PrintSettingName = ManageViewModel.PrintSettingName;
        string ViewSheetSetName = ManageViewModel.ViewSheetSetName;

        // Apply the print setting by name
        PrintSetting printSetting = GetPrintSettingByName(doc, PrintSettingName);
        if (printSetting != null)
        {
            printManager.PrintSetup.CurrentPrintSetting = printSetting;
        }
        else
        {
            TaskDialog.Show("Couldn't Print", $"Print setting with the name '{PrintSettingName}' not found");
            return;
        }

        // Get the ViewSheetSet by name (a set of views/sheets to print)
        ViewSheetSet viewSheetSet = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheetSet))
            .Cast<ViewSheetSet>()
            .FirstOrDefault(vss => vss.Name == ViewSheetSetName);

        if (viewSheetSet == null)
        {
            TaskDialog.Show("Error", "ViewSheetSet named '" + ViewSheetSetName + "' not found.");
            return;
        }

        // Set the viewsheet set to print
        printManager.ViewSheetSetting.CurrentViewSheetSet = viewSheetSet;
        printManager.CombinedFile = true;

        // Execute the print command
        printManager.SubmitPrint();

        // Make sure to save the current print setting to apply changes
        printManager.PrintSetup.Save();

    }

    public static PrintSetting GetPrintSettingByName(Document doc, string targetName)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(PrintSetting));

        foreach (PrintSetting ps in collector.ToElements())
        {
            if (ps.Name == targetName)
            {
                return ps;
            }
        }
        return null; // Return null if no matching print setting is found
    }


}
