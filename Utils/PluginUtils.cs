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


    public static void RegisterDockablePanel(UIControlledApplication uiControlledApplication)
    {
        MainView = new MainView();

        uiControlledApplication.RegisterDockablePane(DockId, DockName, MainView);
    }

    public static void ShowDockablePanel(UIApplication app)
    {
        DockablePane dock = app.GetDockablePane(DockId);
        // Don't show if already is showing
        if (dock == null || dock.IsShown()) return;
        dock.Show();
    }


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
}
