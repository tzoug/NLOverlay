using Common;
using System;
using System.Windows;

namespace UI
{
    /// <summary>
    /// Interaction logic for AdvancedSetings.xaml
    /// </summary>
    public partial class AdvancedSetings : Window
    {
        private SettingsData _settings;
        public AdvancedSetings()
        {
            InitializeComponent();

            _settings = new SettingsData();
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
