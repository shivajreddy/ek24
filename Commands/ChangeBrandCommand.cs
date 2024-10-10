using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Views.ChangeBrand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ek24.Commands;


[Transaction(TransactionMode.Manual)]
public class ChangeBrandCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // App and Doc
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        // Create the WPF window
        var window = new ChangeBrandView(doc);

        var result = window.ShowDialog();

        window.Close();

        return result == true ? Result.Succeeded : Result.Cancelled;
    }
}
