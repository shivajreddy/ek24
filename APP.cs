using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.RequestHandling;
using ek24.UI;
using ek24.UI.ViewModels;
using ek24.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ek24;

/// <summary>
/// This Object will hold the state of the plugin, in the current revit instance
/// Inside is all the project's state of each opened project in this instance of Revit
/// </summary>
public class EK_Global_State
{
    // This will hold all projects state, mapped to the unique project name
    // Dictionary to hold all projects' states, mapped by unique project name
    private Dictionary<string, EK_Project_State> _projectStates = new Dictionary<string, EK_Project_State>();

    // Add a new project state
    public void AddProjectState(string projectName)
    {
        if (!_projectStates.ContainsKey(projectName))
        {
            _projectStates[projectName] = new EK_Project_State(projectName);
        }
    }

    // Get a project state by name
    public EK_Project_State GetProjectState(string projectName)
    {
        return _projectStates.TryGetValue(projectName, out var projectState) ? projectState : null;
    }

    // Remove a project state
    public void RemoveProjectState(string projectName)
    {
        if (_projectStates.ContainsKey(projectName))
        {
            _projectStates.Remove(projectName);
        }
    }

    // Get all project states
    public IEnumerable<EK_Project_State> GetAllProjectStates()
    {
        return _projectStates.Values;
    }

    // Create ViewModels
    public MainViewModel mainViewModel { get; set; }
    public EK24Modify_ViewModel eK24Modify_ViewModel { get; set; }

    // Constructor
    public EK_Global_State()
    {
        eK24Modify_ViewModel = new EK24Modify_ViewModel();
        mainViewModel = new MainViewModel();
    }
}

/// <summary>
/// This is the state of the Project that is opened inside the current Revit Instance
/// </summary>
public class EK_Project_State
{
    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    private static void OnStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
    public string ProjectName { get; set; }
    public Selection EKProjectsCurrentSelection { get; set; }

    private static ObservableCollection<Selection> _project_current_selection = new ObservableCollection<Selection>();
    public static ObservableCollection<Selection> Project_current_selection
    {
        get => _project_current_selection;
        set
        {
            if (value == null || value == _project_current_selection) return;
            _project_current_selection = value;
        }
    }

    // Other Project properties
    public int total_items { get; set; }

    public EK_Project_State(string project_name)
    {
        ProjectName = project_name;
    }
}

/// <summary>
/// Entry Point of the application
/// When Revit.exe opens it loads the plugins, in the addin folder
/// It looks for the class that implements `IExternalApplication`
/// 
/// This entry point will do the following
///     1. Create ek24 Ribbon Panel, create Tab in the ek24 Panel
///     2. Registers the DockablePane(UI of the application, that
///     implements IDockablePaveProvider)
///     3. Registers the ek24's RequestHandler into the Revit process
///        and will execute event handler functions that are ek24
///        subscribes to. This Requesthandler class should implement
///        Revit's IExternalEventHandler
///     4. Subscribe to Revit Events, with ek24 EventHandler functions.
///     5. Define code that will run when Revit application is closing.
/// 
/// </summary>
public class APP : IExternalApplication
{
    // CREATE EK24 PLUGIN'S GLOBAL STATE
    public static EK_Global_State global_state = new EK_Global_State();

    // static Request-handler variables
    public static RequestHandler RequestHandler { get; set; }
    public static ExternalEvent ExternalEvent { get; set; }

    private const string PluginTabName = "EK24";
    private const string PluginPanelName = "EagleKitchen";
    private static RibbonPanel _ribbonPanel;

    private EKEventsUtility ekEventsUtility { get; set; }

    public Result OnStartup(UIControlledApplication application)
    {
        // Create Tab, Panel on RevitUI
        application.CreateRibbonTab(PluginTabName);
        _ribbonPanel = application.CreateRibbonPanel(PluginTabName, PluginPanelName);

        // Set up the EventHandler
        RequestHandler = new RequestHandler();
        ExternalEvent = ExternalEvent.Create(RequestHandler);

        // Set up Events Untility
        ekEventsUtility = new EKEventsUtility();

        // TODO: Now going to be handled by document opened event
        // Setup the Revit Data
        //RevitUtils.SetUpRevitData();

        // Register Docks, Add PushButtons to Panels
        PluginUtils.RegisterDockablePanel(application);
        PluginUtils.CreatePushButtonAndAddToPanel(_ribbonPanel);

        PluginUtils.CreateButton2AndAddToPanel(_ribbonPanel);

        //PluginUtils.CreateButton3AndAddToPanel(_ribbonPanel);
        //PluginUtils.CreateButton4AndAddToPanel(_ribbonPanel);

        /// HOW TO : Subscribe to Events
        /// - List of Revit Events you can subscribe to: https://www.revitapidocs.com/2024/418cd49d-9c2f-700f-3db2-fcbe8929c5e5.htm
        /// - For each event, look at that particular EventArgs that revit will pass as 2nd arg to the given function.
        /// - the first argument is the sender: Here sender is the 'UIApplication' not 'UIControlledApplication' nor 'Application',
        /// - but however we can type cast to 'Application' type, if we need that type.
        // Event: 1
        application.ControlledApplication.DocumentOpened += ekEventsUtility.HandleDocumentOpenedEvent;
        // Event: 2
        application.ControlledApplication.DocumentClosing += ekEventsUtility.HandleDocumentClosingEvent;
        // Event: 3
        application.ControlledApplication.DocumentClosed += ekEventsUtility.HandleDocumentClosedEvent;

        // Event: 4
        application.SelectionChanged += ekEventsUtility.HandleSelectionChangedEvent;
        // Event: 5
        application.ViewActivated += ekEventsUtility.HandleViewActivatedEvent;

        return Result.Succeeded;
    }
    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

}
