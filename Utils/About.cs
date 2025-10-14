using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace ek24.Utils
{
    class About
    {
        private string FilePath { get; set; }

        public About(string filePath)
        {
            FilePath = filePath;
        }

        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                throw new InvalidOperationException("Unable to release the object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    class ShowAbout : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var box = new CustomDialogBox();
            box.ShowInfoCard();

            //TaskDialog.Show("About", "Version Number");
            return Result.Succeeded;
        }

    }

}
