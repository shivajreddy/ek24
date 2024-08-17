using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using ek24.Utils;


namespace ek24.Commands;


[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
[Journaling(JournalingMode.NoCommandData)]
public class TestCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        TaskDialog.Show("Test", "Command");
        return Result.Succeeded;
    }

    public static void CreatePushButtonAndAddToPanel(RibbonPanel ribbonPanel)
    {
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
        if (declaringType == null) return;
        var pushButtonName = declaringType?.Name;
        const string pushButtonTextName = "Test Command";
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyLocation = assembly.Location;
        const string iconName = "ek24_icon.png";
        const string fullClassName = "ek24.TestCommand";
        const string toolTipInfo = "Sample tool tip of this command";

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

