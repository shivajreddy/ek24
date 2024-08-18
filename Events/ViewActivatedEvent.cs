using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using View = Autodesk.Revit.DB.View;

using ek24.UI.ViewModels.ProjectBrowser;


namespace ek24.Events;


/// <summary>
/// How to implement methods for Revit Events:
/// https://www.revitapidocs.com/2024/7d51c9f8-1bea-32ec-0e54-5921242e57c3.htm
/// You can use the `sender` and typecast to 'UIApplication'. and from that you can any of the following
///      UIApplication app = commandData.Application;
///      UIDocument uiDoc = app.ActiveUIDocument;
///      Document doc = app.ActiveUIDocument.Document;
/// OR you can use the methods available on the <particular>EventArgs variable
/// These event implementations execute when that revit event/behaviour happens.
/// </summary>


public class ViewActivatedEvent
{
    public static void HandleViewActivatedEvent(object sender, EventArgs e)
    {
        // Grab the Document
        var app = sender as UIApplication;
        var uiDoc = app.ActiveUIDocument;
        var doc = app.ActiveUIDocument.Document;

        var collector = new FilteredElementCollector(doc);
        var allViews = collector.OfClass(typeof(View)).ToElements();

        // Clear existing list in the ViewModel
        ProjectBrowserViewModel.ChosenViews.Clear();
        ProjectBrowserViewModel.ChosenViewSheets.Clear();

        foreach (var view in allViews)
        {
            // Check if the view has the parameter and if it matches the target value
            Parameter param = view.LookupParameter("EK24_view_sheet");

            // Yes/No parameters are stored as integers
            // Check if the integer value corresponds to the boolean parameterValue (1 for true, 0 for false)
            if (param is not { StorageType: StorageType.Integer } || param.AsInteger() != 1) continue;

            // current view has the "EK24_view_sheet" and value is Yes


            // ViewSheet type is the Sheets in Revit Project Browser
            if (view.GetType() == typeof(ViewSheet))
            {
                // Update the ViewModel
                ProjectBrowserViewModel.ChosenViewSheets.Add(view as ViewSheet);
            }
            else
            {
                // Update the ViewModel
                ProjectBrowserViewModel.ChosenViews.Add(view as View);
            }

        }
    }
}
