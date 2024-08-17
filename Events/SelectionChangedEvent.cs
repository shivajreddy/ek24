using System.Diagnostics;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = app.ActiveUIDocument.Document;

        Selection currentSelection = uiDoc.Selection;
        Debug.WriteLine($"current selection {currentSelection.ToString()}");

        UpdateDynamicUi(currentSelection, doc);
    }

    // Helpler method for `OnSelectionChange` event
    private static void UpdateDynamicUi(Selection currentSelection, Document doc)
    {
        /* TODO: uncomment this function and fix the inside using new UI i.e., Views
            // :: Enable the UI ::
            // Enable the dropdown boxes
            UpdateFamilyTypeDropDownUi(currentSelection, doc);
            UpdateTypeDropDownUi(currentSelection, doc);

            //Enable the Common Settings
            UpdateCommonSettingsUi(currentSelection, doc);

            // TODO: below two functions to be implemented
            // Enable the Single Door Settings
            UpdateSingleDoorSettingsUi(currentSelection, doc);
            // Enable the Upper Cabinet Settings
            UpdateUpperDoorSettingsUi(currentSelection, doc);

            // Enable the Style tab, Finish tab, Handles tab under 'Selections' tab
            UpdateStyleFinishHandlesUi(currentSelection, doc);
        */
    }


}
