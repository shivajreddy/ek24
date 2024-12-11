using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.UI.ViewModels.Properties;
using SelectionChangedEventArgs = Autodesk.Revit.UI.Events.SelectionChangedEventArgs;


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


public class SelectionChangedEvent
{
    /// <summary>
    /// This event is added as revit api's 'SelectionChanged' event, and performs
    /// logic when ever 'SelectionChanged' event on Revit triggers
    /// </summary>

    public static void HandleSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
    {
        UIApplication app = sender as UIApplication;
        UIDocument uiDoc = app?.ActiveUIDocument;
        Document doc = uiDoc?.Document;
        if (doc == null) return;

        Selection currentSelection = uiDoc.Selection;

        // Update the ViewModel with the new selection


        // moving this to SelectionService
        //SelectionService.SyncSelectionWithRevit(currentSelection, doc);

        // :: Update ViewModels ::
        TypeParamsViewModel.SyncCurrentSelectionWithTypeParamsViewModel(currentSelection, doc);
        InstanceParamsViewModel.SyncCurrentSelectionWithInstanceParamsViewModel(currentSelection, doc);
    }


}
