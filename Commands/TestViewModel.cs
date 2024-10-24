// TestViewModel.cs
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Commands;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ek24.Commands
{
    public class TestViewModel : INotifyPropertyChanged
    {
        private string _inputText;
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        #region INotifyPropertyChanged implementation
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
        #endregion

        // Two-way bound property
        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged(nameof(InputText));
                }
            }
        }

        // Command bound to the button
        public ICommand ExecuteCommand { get; set; }

        // Method executed when the button is clicked
        private void ExecuteTransaction()
        {
            using (Transaction trans = new Transaction(_doc, "Toggle KitchenBrand"))
            {
                trans.Start();

                try
                {
                    // Get the Project Information element
                    ProjectInfo projectInfo = _doc.ProjectInformation;
                    Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

                    if (kitchenBrandParam == null)
                    {
                        TaskDialog.Show("Error", "Parameter 'KitchenBrand' not found.");
                        trans.RollBack();
                        return;
                    }

                    string currentValue = kitchenBrandParam.AsString();

                    if (currentValue == "Yorktowne Classic")
                    {
                        kitchenBrandParam.Set("Yorktowne Historic");
                    }
                    else if (currentValue == "Yorktowne Historic")
                    {
                        kitchenBrandParam.Set("Yorktowne Classic");
                    }
                    else
                    {
                        TaskDialog.Show("Info", $"Current KitchenBrand value is '{currentValue}'. No change made.");
                        trans.RollBack();
                        return;
                    }

                    // Update InputText to reflect new value
                    InputText = kitchenBrandParam.AsString();

                    TaskDialog.Show("Revit", $"New KitchenBrand: {InputText}");

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
                    trans.RollBack();
                }
            }
        }

        public TestViewModel(UIApplication uiApp)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;

            // Initialize the command
            ExecuteCommand = new RelayCommand(ExecuteTransaction);

            // Initialize InputText by reading the "KitchenBrand" parameter
            ProjectInfo projectInfo = _doc.ProjectInformation;
            Parameter kitchenBrandParam = projectInfo.LookupParameter("KitchenBrand");

            if (kitchenBrandParam != null)
            {
                _inputText = kitchenBrandParam.AsString() ?? string.Empty;
            }
            else
            {
                _inputText = string.Empty;
                TaskDialog.Show("Error", "Parameter 'KitchenBrand' not found.");
            }
        }
    }
}



