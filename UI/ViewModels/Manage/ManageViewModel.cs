using ek24.RequestHandling;
using ek24.UI.Commands;
using System.ComponentModel;
using System.Windows.Input;


namespace ek24.UI.ViewModels.Manage;


public class ManageViewModel : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    private static void OnStaticPropertyChanged(string propertyName)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }


    public static string _printSettingName { get; set; } = "CABINETRY DRAWINGS";
    public static string PrintSettingName
    {
        get => _printSettingName;
        set
        {
            if (_printSettingName != value)
            {
                _printSettingName = value;
                OnStaticPropertyChanged(nameof(PrintSettingName));
            }
        }
    }

    public static string _viewSheetSetName { get; set; } = "EAGLE CABINETRY - LOT SPEC";
    public static string ViewSheetSetName
    {
        get => _viewSheetSetName;
        set
        {
            if (_viewSheetSetName != value)
            {
                _viewSheetSetName = value;
                OnStaticPropertyChanged(nameof(ViewSheetSetName));
            }
        }

    }


    public ICommand PrintToPdfCommand { get; }
    private void RaisePrintEvent()
    {
        APP.RequestHandler.RequestType = RequestType.Manage_PrintDrawings;
        APP.ExternalEvent?.Raise();
    }

    //public ICommand ExportToExcelCommand { get; }
    //private void RaiseExportToExcelEvent()
    //{
    //    APP.RequestHandler.RequestType = RequestType.Manage_ExportQuantitiesToExcel;
    //    APP.ExternalEvent?.Raise();
    //}


    public ManageViewModel()
    {
        PrintToPdfCommand = new RelayCommand(RaisePrintEvent);

        // Asynchronous command to handle Print PDF operation
        //PrintToPdfCommand = new AsyncRelayCommand(RaisePrintEvent);


        //ExportToExcelCommand = new RelayCommand(RaiseExportToExcelEvent);
    }

}
