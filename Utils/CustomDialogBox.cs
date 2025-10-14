using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ek24.Utils
{
    class CustomDialogBox
    {
        public void ShowWideDialog(string title, string text)
        {
            var textBox = new TextBox
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsReturn = true,
                Margin = new Thickness(20),
                BorderThickness = new Thickness(1),
            };

            Window myDialog = new Window
            {
                Title = title,
                Width = 800,
                Height = 400,
                Content = textBox,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };

            // Make Revit the owner so it stays on top and modal
            var revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            new WindowInteropHelper(myDialog).Owner = revitHandle;

            myDialog.ShowDialog();
        }
    }
}

