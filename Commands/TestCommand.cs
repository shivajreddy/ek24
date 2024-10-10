using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Interop;

namespace ek24.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var viewModel = new TestViewModel(commandData.Application);
            var window = new TestWindow(viewModel);

            // Set the owner to Revit's main window
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = commandData.Application.MainWindowHandle;

            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}

