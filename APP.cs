﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ek24.RequestHandling;
using ek24.Utils;
using ek24.Events;


namespace ek24;

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
    // static Request-handler variables
    public static RequestHandler RequestHandler { get; set; }
    public static ExternalEvent ExternalEvent { get; set; }

    private const string PluginTabName = "EK24";
    private const string PluginPanelName = "EagleKitchen";
    private static RibbonPanel _ribbonPanel;

    public Result OnStartup(UIControlledApplication application)
    {
        // Create Tab, Panel on RevitUI
        application.CreateRibbonTab(PluginTabName);
        _ribbonPanel = application.CreateRibbonPanel(PluginTabName, PluginPanelName);

        // Set up the EventHandler
        RequestHandler = new RequestHandler();
        ExternalEvent = ExternalEvent.Create(RequestHandler);

        // Register Docks, Add PushButtons to Panels
        PluginUtils.RegisterDockablePanel(application);
        PluginUtils.CreatePushButtonAndAddToPanel(_ribbonPanel);

        /// Subscribe to Events
        /// - List of Revit Events you can subscribe to: https://www.revitapidocs.com/2024/418cd49d-9c2f-700f-3db2-fcbe8929c5e5.htm
        /// - For each event, look at that particular EventArgs that revit will pass as 2nd arg to the given function.
        /// - the first argument is the sender: Here sender is the 'UIApplication' not 'UIControlledApplication' nor 'Application',
        /// - but however we can type cast to 'Application' type, if we need that type.
        // Event: 1
        application.SelectionChanged += SelectionChangedEvent.HandleSelectionChangedEvent;
        // Event: 2
        application.ViewActivated += ViewActivatedEvent.HandleViewActivatedEvent;

        return Result.Succeeded;
    }
    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

}