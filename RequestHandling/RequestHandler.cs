using Autodesk.Revit.UI;

using ek24.Commands;
using ek24.UI.ViewModels.ProjectBrowser;


namespace ek24.RequestHandling;

/// <summary>
/// RequestHandler:
///     - An instance of this class is created and registered as
///       an external-event for Revit's process. 
///     - It does this by passin the instance object into
///       ExternalEvent.create(instance_object) 
///     - Since we don't have multiple Execute functions, because IExternalEventHandler interface
///       expects one `Execute` method, we define the all types of events as RequestType enumeration.
///         - And this is also a ppty on the `MainRequestHandler` class.
///           our plugin i.e., 'Main' class creates a MainRequestHandler instance, and we update the 
///     - RequestType static ppty is created for the RequestHandler class.
///     instance, before calling the Execute method.
///     - Before execute the event, we set this property, and the matching arm
///       of the RequestType property will Execute fn, and all these fn's will have access to
///       Revit's App scope.
///     Example: 
///        The RequestHandler instance that is set as one of APP's static ppty, can be
///        accessed by any code block in the project. so the UI's view's or view-model's
///        can update this(RequestType) static ppty of the APP, and then raise ExternalEvent
///        1. Set the request type on APP's request handler
///           APP.RequestHandler.RequestType = RequestType.RevitUI_SelectCabinets;
///        2. You can use UI utility classes to store any UI's current state, that might be
///           needed by the  fn's that are executed for a particular RequestType
///           Ex: we might need the name of the ViewSheet to set the CurrentView to.
///           
///        3. Raise the Event
///           APP.ExternalEvent.Raise();
///     
/// </summary>


public enum RequestType
{
    // 'Project Browser' related Requests
    RevitUI_UpdateActiveView,
    RevitUI_SelectCaseWork,

    // 'Properties' related Requests
    Properties_FamilyAndType,
    Properties_UpdateCabinetType,
    Properties_MakeCustomizations,

    // 'Manage' related Requests
    Manage_PrintDrawings,
    Manage_ExportQuantitiesToExcel,

    // Misc Requests
    DevTest,

    //// Common Settings
    //RevitUI_UpdateHasLeftFillerStrip,
    //RevitUI_UpdateHasRightFillerStrip,
    //RevitUI_UpdateLeftFillerStripValue,
    //RevitUI_UpdateRightFillerStripValue,
}


public class RequestHandler : IExternalEventHandler
{

    // Ui should set this RequestType ppty before calling the Execute method
    public RequestType RequestType { get; set; }

    public void Execute(UIApplication app)
    {
        // Execute the fn based on the type of request
        switch (RequestType)
        {
            case RequestType.RevitUI_UpdateActiveView:
                UiUpdates.SetView(app, ProjectBrowserViewModel.GoToViewName);
                break;
            case RequestType.RevitUI_SelectCaseWork:
                SelectionUpdates.SelectCaseWorkElements(app);
                break;
            case RequestType.Properties_FamilyAndType:
                Create.CreateNewInstance(app);
                break;
            case RequestType.Properties_UpdateCabinetType:
                ChangeType.UpdateCabinetTypeForSelectedInstance(app);
                break;

            // 'Manage' related
            case RequestType.Manage_PrintDrawings:
                PrintToPdf.PrintDocument(app);

                PrintToPdf.PrintDocumentAsync(app);

                break;
            case RequestType.Manage_ExportQuantitiesToExcel:
                ExportToExcel.ExportQuantitiesToExcel(app);
                break;

            default:
                break;
        }
    }

    // required method by the interface IExternalEventHandler
    public string GetName()
    {
        return "ek24 Request Handler";
    }
}
