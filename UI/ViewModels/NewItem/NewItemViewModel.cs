using Autodesk.Revit.DB;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ek24.UI.ViewModels.NewItem;
public class NewItemViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Implement InotifyPropertyChanged, to enable binding & updating
    /// UI when values are properties change values
    /// </summary>
    // Implement INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    private static void OnStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }

    private static ObservableCollection<View> _chosenViews;
    private static ObservableCollection<ViewSheet> _chosenViewSheets;
}

