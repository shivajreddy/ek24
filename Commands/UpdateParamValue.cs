using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ek24.Commands;

public static class UpdateParamValue
{
    public static void UpdateTypeParam(UIApplication app)
    {
    }
    public static void UpdateInstanceParam(UIApplication app)
    {
        UIDocument uiDoc = app.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // ////////////////////////////// TODO //////////////////////////////

        // Chosen Vendor-Style & Vendor-Finish
        string selected_vendor_style = EK24Modify_ViewModel.SelectedVendorStyle;
        string selected_vendor_finish = EK24Modify_ViewModel.SelectedVendorFinish;

        // CURRENT SELECTION
        Selection current_selection = APP.Global_State.Current_Project_State.EKCurrentProjectSelection;
        ICollection<ElementId> selectedIds = current_selection.GetElementIds();

        // 1. Filter for FamilyInstance elements from the selected IDs
        List<FamilyInstance> current_selected_familyInstances = selectedIds
            .Select(id => doc.GetElement(id))
            .OfType<FamilyInstance>()   // Only FamilyInstances
            .ToList();

        // TESTING, with just 1 item for now
        FamilyInstance inst = current_selected_familyInstances.First();


        // Get the Parameters
        Parameter vendor_style_param = inst.LookupParameter("Vendor_Style");
        Parameter vendor_finish_param = inst.LookupParameter("Vendor_Finish");

        ////// API: go to Autodesk.DB.NestedFamilyTypeReference Class, and there is the following note that expalins that these types can't be filtered like normal filtering. The note says:
        ///// ** These elements are very low-level and thus bypassed by standard element filters. However, 
        /////  it is possible to obtain a set of applicable elements of this class for a FamilyType parameter of a family by calling [!:Autodesk::Revit::DB::Family::GetFamilyTypeParameterValues] **
        /////  Revitapi > Family Class > Family Methods > GetFamilyTypeParameterValues method
        ////// API: go to Revitapi > Family Class > Family Methods > GetFamilyTypeParameterValues method

        // Some error handling fn's just to be sure
        if (!Category.IsBuiltInCategory(vendor_style_param.Definition.GetDataType()))
        {
            TaskDialog.Show("ERROR", "Vendor_Style's data type is not a built in category");
            return;
        }
        if (!Category.IsBuiltInCategory(vendor_finish_param.Definition.GetDataType()))
        {
            TaskDialog.Show("ERROR", "Vendor_Finish's data type is not a built in category");
            return;
        }
        ISet<ElementId> all_possible_familytype_parameter_values_set = inst.Symbol.Family.GetFamilyTypeParameterValues(vendor_style_param.Id);
        if (all_possible_familytype_parameter_values_set.Count == 0)
        {
            TaskDialog.Show("ERROR", "One of the chosen istance doesnt have Vendor-Style param or Vendor-Finish Param");
        }

        Debug.WriteLine("\n");

        var val_str = vendor_style_param.AsValueString();
        //AsValueString: EK_TEMPLATE_Panel_ONE_DOOR : Yorktowne Classic - Henning

        var vendor_style_param_id = vendor_style_param.Id; // Vendor_Style, ID9708343
        var vendor_style_element_id = vendor_style_param.AsElementId(); // 9708137

        //NestedFamilyTypeReference vendor_style_nfr = doc.GetElement(vendor_style_element_id) as NestedFamilyTypeReference;
        //vendor_style_nfr

        /*
        Logic of creating the final value string for each family instance
        - Get the familyinstance.param.asvaluestring for each family instance
        - For this(each) family instance get the 2 ppties: EKBrand, EKType
         */

        // Hardcoded
        ElementId yt_classic_henning_id = new ElementId((long)9708137);
        ElementId yt_classic_fillmore_id = new ElementId((long)9708139);

        GetNestedFamilyTypes(inst);

        /*
        using (Transaction trans = new Transaction(doc, "Update Vendor-Style & Vendor-Finish"))
        {
            trans.Start();

            trans.Commit();
        }
        */

        Debug.WriteLine("\n");
    }



    public static void GetNestedFamilyTypes(FamilyInstance instance)
    {
        // find one FamilyType parameter and all values applicable to it

        Parameter aTypeParam = null;
        ISet<ElementId> values = null;

        Family family = instance.Symbol.Family;

        //foreach (Parameter param in instance.Symbol.Parameters)
        //{

        //    if (param.Definition.ParameterType == ParameterType.FamilyType)
        //    {
        //        aTypeParam = param;

        //        values = family.GetFamilyTypeParameterValues(param.Id);

        //        break;
        //    }
        //}

        foreach (Parameter param in instance.Symbol.Parameters)
        {

            if (Category.IsBuiltInCategory(param.Definition.GetDataType()))
            {

                aTypeParam = param;

                values = family.GetFamilyTypeParameterValues(param.Id);

                break;
            }
        }

        return;
        if (aTypeParam == null)
        {
            TaskDialog.Show("Warning", "The selected family has no FamilyType parameter defined.");
        }
        else if (values == null)
        {
            TaskDialog.Show("Error", "A FamilyType parameter does not have any applicable values!?");
        }
        else
        {
            ElementId newValue = values.Last<ElementId>();

            if (newValue != aTypeParam.AsElementId())
            {

                using (Transaction trans = new Transaction(instance.Document, "Setting parameter value"))
                {

                    trans.Start();
                    aTypeParam.Set(newValue);
                    trans.Commit();
                }
            }
        }
    }


}
