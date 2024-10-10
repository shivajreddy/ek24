using Autodesk.Revit.UI;
using System;
using System.Reflection;

using ek24.UI.Views;


namespace ek24.Utils;


public static class PluginUtils
{

    public static string PanelId = "409EC347-50BC-48D2-BBEB-48B40E799927";
    private static readonly DockablePaneId DockId = new DockablePaneId(new Guid(PanelId));
    private const string DockName = "Eagle Kitchen";

    public static MainView MainView;

    /// <summary>
    /// Register the DockablePane with a Static instance of the MainView
    /// </summary>
    /// <param name="uiControlledApplication"></param>
    public static void RegisterDockablePanel(UIControlledApplication uiControlledApplication)
    {
        MainView = new MainView();

        uiControlledApplication.RegisterDockablePane(DockId, DockName, MainView);
    }

    /// <summary>
    /// Create the push button with icon and set the class name path
    /// </summary>
    /// <param name="ribbonPanel"></param>
    public static void CreatePushButtonAndAddToPanel(RibbonPanel ribbonPanel)
    {
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
        if (declaringType == null) return;
        var pushButtonName = declaringType?.Name;
        const string pushButtonTextName = "Show EagleKitchen";
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        const string iconName = "ek24_icon.png";
        const string toolTipInfo = "Show the EagleKitchen Dock";

        /// Class that handles showing the UI
        const string fullClassName = "ek24.Commands.ShowDockCommand";

        var pushButtonData = new PushButtonData(
            name: pushButtonName,
            text: pushButtonTextName,
            assemblyName: assemblyLocation,
            className: fullClassName
        )
        {
            ToolTip = toolTipInfo,
            Image = ImageUtilities.LoadImage(assembly, iconName),
            LargeImage = ImageUtilities.LoadImage(assembly, iconName),
            ToolTipImage = ImageUtilities.LoadImage(assembly, iconName)
        };
        ribbonPanel.AddItem(pushButtonData);
    }

    public static void CreateButton2AndAddToPanel(RibbonPanel ribbonPanel)
    {
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
        if (declaringType == null) return;
        var pushButtonName = declaringType?.Name;
        const string pushButtonTextName = "Change Brand";
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        const string iconName = "change_brand.png";
        const string toolTipInfo = "Change the Brand of Kitchen";

        /// Class that handles showing the UI
        const string fullClassName = "ek24.Commands.ChangeBrandCommand";

        var pushButtonData = new PushButtonData(
            //name: pushButtonName,
            name: "ChangeBrand",
            text: pushButtonTextName,
            assemblyName: assemblyLocation,
            className: fullClassName
        )
        {
            ToolTip = toolTipInfo,
            Image = ImageUtilities.LoadImage(assembly, iconName),
            LargeImage = ImageUtilities.LoadImage(assembly, iconName),
            ToolTipImage = ImageUtilities.LoadImage(assembly, iconName)
        };
        ribbonPanel.AddItem(pushButtonData);
    }

    public static void CreateButton3AndAddToPanel(RibbonPanel ribbonPanel)
    {
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
        if (declaringType == null) return;
        var pushButtonName = declaringType?.Name;
        const string pushButtonTextName = "Update View Filter";
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        const string iconName = "view_filter.png";
        const string toolTipInfo = "Update the View Filter to match Project Param Value";

        /// Class that handles showing the UI
        const string fullClassName = "ek24.Commands.UpdateViewFilterCommand";

        var pushButtonData = new PushButtonData(
            //name: pushButtonName,
            name: "Update View Filter",
            text: pushButtonTextName,
            assemblyName: assemblyLocation,
            className: fullClassName
        )
        {
            ToolTip = toolTipInfo,
            Image = ImageUtilities.LoadImage(assembly, iconName),
            LargeImage = ImageUtilities.LoadImage(assembly, iconName),
            ToolTipImage = ImageUtilities.LoadImage(assembly, iconName)
        };
        ribbonPanel.AddItem(pushButtonData);
    }

    public static void CreateButton4AndAddToPanel(RibbonPanel ribbonPanel)
    {
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
        if (declaringType == null) return;
        var pushButtonName = declaringType?.Name;
        const string pushButtonTextName = "TEST";
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        const string iconName = "view_filter.png";
        const string toolTipInfo = "Update the View Filter to match Project Param Value";

        /// Class that handles showing the UI
        const string fullClassName = "ek24.Commands.TestCommand";

        var pushButtonData = new PushButtonData(
            //name: pushButtonName,
            name: "TEST",
            text: pushButtonTextName,
            assemblyName: assemblyLocation,
            className: fullClassName
        )
        {
            ToolTip = toolTipInfo,
            Image = ImageUtilities.LoadImage(assembly, iconName),
            LargeImage = ImageUtilities.LoadImage(assembly, iconName),
            ToolTipImage = ImageUtilities.LoadImage(assembly, iconName)
        };
        ribbonPanel.AddItem(pushButtonData);
    }



    /// <summary>
    /// Show the Dockable pane UI, only show if not current shown
    /// </summary>
    public static void ShowDockablePanel(UIApplication app)
    {
        DockablePane dock = app.GetDockablePane(DockId);
        if (dock == null || dock.IsShown()) return;
        dock.Show();
    }
}
