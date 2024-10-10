using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ek24.UI.Commands;
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
        public ICommand ExecuteCommand { get; }

        // Method executed when the button is clicked
        private void ExecuteTransaction()
        {
            using (Transaction trans = new Transaction(_doc, "My Transaction"))
            {
                trans.Start();

                // Example: Show the input text in a TaskDialog
                TaskDialog.Show("Revit", $"Input text: {InputText}");

                // TODO: Perform actual Revit operations using InputText

                trans.Commit();
            }
        }

        public TestViewModel(UIApplication uiApp)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;

            // Initialize the command
            ExecuteCommand = new RelayCommand(ExecuteTransaction);
        }
    }
}
