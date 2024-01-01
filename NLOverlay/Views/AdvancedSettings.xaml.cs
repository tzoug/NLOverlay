using NLOverlay.Helpers;
using NLOverlay.Models;
using System;
using System.Windows;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for AdvancedSetings.xaml
    /// </summary>
    public partial class AdvancedSetings : Window
    {
        private Settings _settings;
        public AdvancedSetings()
        {
            InitializeComponent();

            _settings = new Settings();
            _settings.Load();

            SettingsPropertyGrid.SelectedObject = _settings;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            var validationResult = _settings.Validate();

            if (validationResult.IsValid)
            {
                _settings.Save();
                MessageBox.Show("Saved", "Save");
            }
            else
            {
                var output = validationResult.Errors.ConcatStringsForMessageBox();
                MessageBox.Show(output, "Error Saving");
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
