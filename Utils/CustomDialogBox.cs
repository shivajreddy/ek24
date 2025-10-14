using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

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

        // New fancy card-style dialog
        public void ShowInfoCard()
        {
            // Main panel
            var panel = new StackPanel
            {
                Margin = new Thickness(30),
                Background = Brushes.White
            };

            // Title
            var titleBlock = new TextBlock
            {
                Text = "Eagle Kitchen (EK24)",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(titleBlock);

            // Version info
            var versionBlock = new TextBlock
            {
                Text = $"Version: {EK_Global_State.VERSION_NUMBER} 🚀",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(versionBlock);

            // Author info with clickable link
            var authorBlock = new TextBlock
            {
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            };
            authorBlock.Inlines.Add(new Run("Created with ❤️ by: "));

            // Clickable author name
            var authorLink = new Hyperlink(new Run("Shiva"))
            {
                NavigateUri = new Uri("https://github.com/shivajreddy") // replace with your preferred link
            };
            authorLink.RequestNavigate += (sender, e) =>
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });

            authorBlock.Inlines.Add(authorLink);

            panel.Children.Add(authorBlock);

            // Special thanks
            var specialThanksBlock = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };
            specialThanksBlock.Inlines.Add(new Run("Special Credit for building & maintaining projects and families to: "));
            specialThanksBlock.Inlines.Add(new Run("Rodolfo Arias") { FontWeight = FontWeights.SemiBold });
            panel.Children.Add(specialThanksBlock);

            // Design team credits
            var designTeamBlock = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            designTeamBlock.Inlines.Add(new Run("Special Thanks to the design team members: "));
            designTeamBlock.Inlines.Add(new Run("Emily Lowery, Kaitie Chance") { FontWeight = FontWeights.SemiBold });
            panel.Children.Add(designTeamBlock);

            // Description
            var descBlock = new TextBlock
            {
                Text = "EK24 is a comprehensive tool for Eagle of VA that streamlines kitchen project management by automating design, pricing, and estimation tasks with a user-friendly interface and customizable options.\n" +
                        "\nStats for nerds: \n" +
                        "Number of lines of Code so far   : 10,571\n" +
                        "Number of Core Models so far   : 35\n" +
                        "Number of Families Created so far: 270\n" +
                        "Number of Design Options Created : 150+\n" +
                        "",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(descBlock);

            // Hyperlink
            var linkBlock = new TextBlock { FontSize = 14 };
            var hyperlink = new Hyperlink(new Run("Visit Project Repository 🌐"))
            {
                NavigateUri = new System.Uri("https://github.com/shivajreddy/ek24")
            };
            hyperlink.RequestNavigate += (sender, e) => Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            linkBlock.Inlines.Add(hyperlink);
            panel.Children.Add(linkBlock);

            // Window
            var window = new Window
            {
                Title = "About EK24",
                Width = 600,
                Height = 500,
                Content = panel,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var revitHandle = Process.GetCurrentProcess().MainWindowHandle;
            new WindowInteropHelper(window).Owner = revitHandle;

            window.ShowDialog();
        }
    }
}

