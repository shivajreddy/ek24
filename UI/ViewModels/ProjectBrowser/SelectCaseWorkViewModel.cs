using System.Windows.Input;
using System.ComponentModel;

using ek24.UI.Commands;
using ek24.UI.Models.ProjectBrowser;
using ek24.RequestHandling;


namespace ek24.UI.ViewModels.ProjectBrowser;


public class SelectCaseWorkViewModel : INotifyPropertyChanged
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
    private SelectCaseWorkModel _selectCaseWorkModel { get; set; }

    /// <summary>
    ///  Static properties in the ViewModel can hold the information that
    ///  event handlers need
    /// </summary>
    public static CaseWorkGroup CurrentCaseWorkGroup = CaseWorkGroup.NoSelection;

    /// <summary>
    /// List of all Command functions and their implementations
    /// These commands can be then Bounded to Command ppty of Buttons
    /// </summary>
    public ICommand SelectAllLowerCabinetsCommand { get; }
    public ICommand SelectAllUpperCabinetsCommand { get; }
    public ICommand SelectAllCabinetsCommand { get; }


    /// <summary>
    /// Constructor of ViewModel
    /// </summary>
    SelectCaseWorkViewModel()
    {
        _selectCaseWorkModel = new SelectCaseWorkModel();
        SelectAllLowerCabinetsCommand = new RelayCommand(SelectAllLowerCabinets);
        SelectAllUpperCabinetsCommand = new RelayCommand(SelectAllUpperCabinets);
        SelectAllCabinetsCommand = new RelayCommand(SelectAllCabinets);
    }

    private void SelectAllLowerCabinets()
    {
        // 1. Set the request type on APP's request handler
        APP.RequestHandler.RequestType = RequestType.RevitUI_SelectCaseWork;
        // 2. Update static property of ViewModel
        CurrentCaseWorkGroup = CaseWorkGroup.AllLowers;
        // 3. Raise the Event
        APP.ExternalEvent.Raise();
    }
    private void SelectAllUpperCabinets()
    {
        // 1. Set the request type on APP's request handler
        APP.RequestHandler.RequestType = RequestType.RevitUI_SelectCaseWork;
        // 2. Update static property of ViewModel
        CurrentCaseWorkGroup = CaseWorkGroup.AllUppers;
        // 3. Raise the Event
        APP.ExternalEvent.Raise();
    }
    private void SelectAllCabinets()
    {
        // 1. Set the request type on APP's request handler
        APP.RequestHandler.RequestType = RequestType.RevitUI_SelectCaseWork;
        // 2. Update static property of ViewModel
        CurrentCaseWorkGroup = CaseWorkGroup.AllUppers;
        // 3. Raise the Event
        APP.ExternalEvent.Raise();
    }
}
