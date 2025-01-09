using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ek24.UI;
using System;
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

        bool all_instances_have_same_brand = true;
        // Get the Manufacturer parameter value of the first FamilyInstance
        string reference_brand = null;
        if (current_selected_familyInstances.Count > 0)
        {
            var first_instance = current_selected_familyInstances.First();
            var first_param = first_instance.Symbol.LookupParameter("Manufacturer");

            if (first_param != null && !string.IsNullOrEmpty(first_param.AsValueString()))
            {
                reference_brand = first_param.AsValueString();
            }
            else
            {
                TaskDialog.Show("ERROR", "The first instance does not have a valid Manufacturer value.");
                return;
            }

            // Compare the reference value with all other FamilyInstance parameter values
            foreach (var fam_inst in current_selected_familyInstances)
            {
                var ekbrand_param = fam_inst.Symbol.LookupParameter("Manufacturer");
                if (ekbrand_param == null || ekbrand_param.AsValueString() != reference_brand)
                {
                    all_instances_have_same_brand = false;
                    break; // Exit the loop as soon as we find a mismatch
                }
            }
        }
        else
        {
            TaskDialog.Show("ERROR", "No FamilyInstances selected.");
            return;
        }
        // Must ensure that all the family-instances whose 'Vendor-Style' param will be changed should have smae EKBrand
        if (!all_instances_have_same_brand)
        {
            TaskDialog.Show("ERROR", "All instances must belong to same Brand");
            return;
        }
        // By this point, we know all family-instances belong to same brand

        // TESTING, with just 1 item for now
        FamilyInstance first_instance_in_the_selection = current_selected_familyInstances.First();

        // Get the Parameters
        Parameter vendor_style_param = first_instance_in_the_selection.LookupParameter("Vendor_Style");

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

        // This the main working api, that will give the allowed values (of type ElementId) that can be set to the parameter
        // Though our parameter is an instance parameter, we have to still look at Symbol.Family as if it was a type parameter, because
        // the `GetFamilyTypeParameterValues` method is definded under 'Family', and 'Family' can be accessed from a family-instance through
        // it's `Symbol`
        ISet<ElementId> all_possible_familytype_parameter_values_set = first_instance_in_the_selection.Symbol.Family.GetFamilyTypeParameterValues(vendor_style_param.Id);
        if (all_possible_familytype_parameter_values_set.Count == 0)
        {
            TaskDialog.Show("ERROR", "One of the chosen istance doesnt have Vendor-Style param or Vendor-Finish Param");
            return;
        }

        Dictionary<string, ElementId> map_name_eid = new Dictionary<string, ElementId>();

        HashSet<string> names = [];
        foreach (var eid in all_possible_familytype_parameter_values_set)
        {
            Element e = doc.GetElement(eid);
            names.Add(e.Name);
            map_name_eid[e.Name] = eid;
        }
        if (names.Count <= 0)
        {
            TaskDialog.Show("ERROR", "No Vendor-Style nested families found");
            return;
        }

        // The CHOSEN STYLE & it's ID by the USER
        string chosen_vendor_style = EK24Modify_ViewModel.SelectedVendorFinish;
        Debug.WriteLine("now update in trasaction");
        ElementId chosen_vendorstyle_eid = map_name_eid[chosen_vendor_style];

        Debug.WriteLine("now update in trasaction");

        bool updatedParamResult = false;


        using (Transaction trans = new Transaction(doc, "Update Vendor-Style & Vendor-Finish"))
        {
            trans.Start();
            foreach (var familyInstance in current_selected_familyInstances)
            {
                Parameter current_vendor_style_param = familyInstance.LookupParameter("Vendor_Style");
                // Finaly: set with the 'ElementId' since that is the storage type of this param, and setting with string won't work
                updatedParamResult = current_vendor_style_param.Set(chosen_vendorstyle_eid);
                if (updatedParamResult == false)
                {
                    trans.RollBack();
                    TaskDialog.Show("ERROR", "FAILED TO UPDATE THE VENDOR-STYLE PARAM");
                    return;
                }
            }

            trans.Commit();
        }

        TaskDialog.Show("SUCCESS", "UPDATED THE VENDOR-STYLE PARAM");
        return;

        /*
         By this point, we have the names & dictionary, where all the names i know for a fact will be in the following manner

            "Aristokraft - Sinclair"
            "Aristokraft - Benton"
            "Aristokraft - Brellin"
            "Aristokraft - Winstead"
            "Yorktowne Classic - Henning"
            "Yorktowne Classic - Stillwater"
            "Yorktowne Classic - Fillmore"
            "Eclipse - Metropolitan"
            "Yorktowne Historic - Corsica"
            "Yorktowne Historic - Langdon"

        and even though all the family instances will accept any of these element-id's as the Set(element-id) since they are all FamilyType parameter
         but I want only the ones that belong to current brand
        */
        // create a variable of the filtered list
        // Filter the available family-type parameter values by the brand:
        var brand_filtered_map = map_name_eid
            .Where(kvp => kvp.Key.StartsWith(reference_brand + " - ", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (brand_filtered_map.Count == 0)
        {
            // No matching items for the current brand
            TaskDialog.Show("ERROR", $"No Vendor-Style nested families found for brand: {reference_brand}");
            return;
        }

        // Optionally, you can retrieve the filtered names as well:
        var filtered_names = brand_filtered_map.Keys.ToList();

        // Debug / confirmation
        var message = "Filtered Vendor-Styles for brand '" + reference_brand + "':\n"
            + string.Join("\n", filtered_names);
        TaskDialog.Show("INFO", message);

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


        Debug.WriteLine("\n");
    }
}
