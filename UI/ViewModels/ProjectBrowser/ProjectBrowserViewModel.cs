using Autodesk.Revit.DB;

using ek24.RequestHandling;
using ek24.UI.Commands;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace ek24.UI.ViewModels.ProjectBrowser;


public class ProjectBrowserViewModel : INotifyPropertyChanged
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

    public static ObservableCollection<View> ChosenViews
    {
        get => _chosenViews;
        set
        {
            if (_chosenViews != value)
            {
                _chosenViews = value;
                OnStaticPropertyChanged(nameof(ChosenViews));
            }
        }
    }

    public static ObservableCollection<ViewSheet> ChosenViewSheets
    {
        get => _chosenViewSheets;
        set
        {
            if (_chosenViewSheets != value)
            {
                _chosenViewSheets = value;
                OnStaticPropertyChanged(nameof(ChosenViewSheets));
            }
        }
    }

    public ICommand ViewSelectedCommand { get; }
    public ICommand SheetSelectedCommand { get; }

    public static string GoToViewName { get; set; }

    public ProjectBrowserViewModel()
    {
        if (ChosenViews == null)
            ChosenViews = new ObservableCollection<View>();
        if (ChosenViewSheets == null)
            ChosenViewSheets = new ObservableCollection<ViewSheet>();

        ViewSelectedCommand = new RelayCommand<View>(OnViewSelected);
        SheetSelectedCommand = new RelayCommand<ViewSheet>(OnSheetSelected);
    }

    private void OnViewSelected(View view)
    {
        GoToViewName = view.Name;
        APP.RequestHandler.RequestType = RequestType.RevitUI_UpdateActiveView;
        APP.ExternalEvent?.Raise();
    }

    private void OnSheetSelected(ViewSheet sheet)
    {
        GoToViewName = sheet.Name;
        APP.RequestHandler.RequestType = RequestType.RevitUI_UpdateActiveView;
        APP.ExternalEvent?.Raise();
    }

}

