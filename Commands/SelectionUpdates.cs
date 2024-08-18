using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ek24.UI.Models.ProjectBrowser;
using ek24.UI.ViewModels.ProjectBrowser;


namespace ek24.Commands;


public class SelectionUpdates
{

    public static void SelectCaseWorkElements(UIApplication app)
    {
        Cursor.Current = Cursors.WaitCursor;

        var uiDoc = app.ActiveUIDocument;
        var doc = uiDoc.Document;

        // Create an ElementOwnerView filter with id of active view
        ElementOwnerViewFilter elementOwnerViewFilter = new ElementOwnerViewFilter(doc.ActiveView.Id);

        // Filter for all the instances of 'ChosenCabinetConfiguration'
        FilteredElementCollector coll = new FilteredElementCollector(doc, doc.ActiveView.Id);
        IList<Element> familyInstances = coll.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements();

        ICollection<ElementId> filteredElementIds;

        switch (SelectCaseWorkViewModel.CurrentCaseWorkGroup)
        {
            case CaseWorkGroup.AllLowers:
                filteredElementIds = familyInstances
                    .Where(x => (x as FamilyInstance)?.Symbol.Family.Name.Contains("-B-") ?? false)
                    .Select(x => x.Id)
                    .ToList();
                uiDoc.Selection.SetElementIds(filteredElementIds);
                break;
            case CaseWorkGroup.AllUppers:
                filteredElementIds = familyInstances
                    .Where(x => (x as FamilyInstance)?.Symbol.Family.Name.Contains("-W-") ?? false)
                    .Select(x => x.Id)
                    .ToList();
                uiDoc.Selection.SetElementIds(filteredElementIds);
                break;
            case CaseWorkGroup.AllCabinets:
                // Select all casework instances
                filteredElementIds = familyInstances
                    .Where(x => (x as FamilyInstance)?.Symbol.Family.FamilyCategory.Name == "Casework")
                    .Select(x => x.Id)
                    .ToList();
                uiDoc.Selection.SetElementIds(filteredElementIds);
                break;
        }

        Cursor.Current = Cursors.Default;
    }

}
