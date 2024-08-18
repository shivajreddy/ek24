using System.ComponentModel;
using System.Windows.Input;

using ek24.UI.Models.ProjectBrowser;
using ek24.UI.Commands;
using ek24.RequestHandling;


namespace ek24.UI.ViewModels.ProjectBrowser;


public class ProjectBrowserViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Implement InotifyPropertyChanged, to enable binding & updating
    /// UI when values are properties change values
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///  ViewModel of this View
    /// </summary>
    private ProjectBrowserModel _projectBrowserModel;
    public ProjectBrowserModel ProjectBrowserModel
    {
        get => _projectBrowserModel;
        set
        {
            _projectBrowserModel = value;
        }
    }

    /// <summary>
    /// List of all Command functions and their implementations
    /// These commands can be then Bounded to Command ppty of Buttons
    /// </summary>

    /// <summary>
    /// Constructor of ViewModel
    /// </summary>
    public ProjectBrowserViewModel()
    {
        _projectBrowserModel = new ProjectBrowserModel();

    }



}

