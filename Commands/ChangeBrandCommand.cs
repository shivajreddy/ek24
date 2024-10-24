using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.ViewModels.ChangeBrand;
using ek24.UI.Views.ChangeBrand;
using System.Windows.Interop;


namespace ek24.Commands;


[Transaction(TransactionMode.Manual)]
public class ChangeBrandCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var viewModel = new ChangeBrandViewModel(commandData.Application);
        var window = new ChangeBrandView(viewModel);

        // Set the owner to Revit's main window
        WindowInteropHelper helper = new WindowInteropHelper(window);
        helper.Owner = commandData.Application.MainWindowHandle;

        window.ShowDialog();

        return Result.Succeeded;
    }
}

