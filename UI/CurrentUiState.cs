using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ek24.UI;


/// <summary>
/// This utility class will be used to get and set the current UI state
/// of the application. When UI is updated by the user, the latest state
/// is set here, and EventHandler actions can get this recent state from
/// this utility class.
/// Example: for going to a sheet view, when a 'View' button is clicked,
/// it sets the go-to-view-name in this utility class. And raises the 
/// appropriate event (in this case UpdateCurrentView event), and the 
/// action of that event will get the view name it has to go to from here.
/// </summary>

// TODO: may be all of this can be handled at ViewModel level,
// respectively for individual views 
static class CurrentUiState
{
    public static string GoToViewName { get; set; }

}
