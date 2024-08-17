using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
////using EK24_old.RequestHandlingUtils;
//using EK24_old.Utils;
//using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using SelectionChangedEventArgs = Autodesk.Revit.UI.Events.SelectionChangedEventArgs;
using View = Autodesk.Revit.DB.View;


namespace ek24.Commands;

public class UiUpdates
{
    public static void SetView(UIApplication app, string gotoViewName)
    {
        Document doc = app.ActiveUIDocument.Document;

        FilteredElementCollector coll = new FilteredElementCollector(doc);

        var allViews = coll.OfClass(typeof(View));

        View targetView = null;

        foreach (View view in allViews)
        {
            if (view.Name == gotoViewName)
            {
                targetView = view;
                break;
            }
        }

        if (targetView != null)
            app.ActiveUIDocument.ActiveView = targetView;
    }
}
