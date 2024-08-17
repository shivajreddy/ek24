using System;
using System.Diagnostics;


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
        Debug.WriteLine("new view activated");
        /* TODO uncomment and update all variables to point to the new View 
        // Reset-UI
        EagleKitchenDockUtils.EagleKitchenMainUi.ListOfEK24Views.Children.Clear();
        EagleKitchenDockUtils.EagleKitchenMainUi.ListOfEK24Sheets.Children.Clear();

        // Grab the Document
        var app = sender as UIApplication;
        var uiDoc = app.ActiveUIDocument;
        var doc = app.ActiveUIDocument.Document;

        var collector = new FilteredElementCollector(doc);
        var allViews = collector.OfClass(typeof(View)).ToElements();


        foreach (var view in allViews)
        {
            // Check if the view has the parameter and if it matches the target value
            Parameter param = view.LookupParameter("EK24_view_sheet");

            // Yes/No parameters are stored as integers
            // Check if the integer value corresponds to the boolean parameterValue (1 for true, 0 for false)
            if (param is not { StorageType: StorageType.Integer } || param.AsInteger() != 1) continue;

            var button = new Button
            {
                Content = "",
                Tag = view.Name,
                Height = 30,
                IsEnabled = true,
                Style = EagleKitchenDockUtils.EagleKitchenMainUi.FindResource("GoToViewButtonStyle") as System.Windows.Style

            };
            button.Click += Update_Current_View;

            // Update-UI :: add buttons
            // This is a view sheet -> so a sheet in the project browser
            if (view.GetType() == typeof(ViewSheet))
            {
                var viewSheet = view as ViewSheet;
                button.Content = $"{viewSheet.SheetNumber} : {viewSheet.Name}";
                EagleKitchenDockUtils.EagleKitchenMainUi.ListOfEK24Sheets.Children.Add(button);
            }
            else
            {
                button.Content = $"{view.Name}";
                EagleKitchenDockUtils.EagleKitchenMainUi.ListOfEK24Views.Children.Add(button);
            }
        }
        */
    }
}
