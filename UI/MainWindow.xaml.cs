using System;
using Common;
using NetLimiter.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UI.Classes;
using UI.ViewModels;
using Window = System.Windows.Window;

namespace UI
{
    public partial class MainWindow : Window
    {
        private OverlayWindow _overlayWindow;

        public ClientViewModel ClientViewModel { get; set; }
        private readonly ObservableCollection<RuleViewModel> _ruleViewModels;
        private readonly SettingsData _settings;
        private readonly Helper _helper;

        public MainWindow()
        {
            InitializeComponent();
            _settings = new SettingsData();
            _helper = new Helper();

            ClientViewModel = new ClientViewModel();
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            
            RulesListView.ItemsSource = _ruleViewModels;

            DataContext = this;

            UpdateRules();
            _settings.Load();
            Load();
        }

        private void UpdateRules()
        {
            using (var client = new NLClient())
            {
                client.Connect();

                var filters = client.Filters;
                var rules = client.Rules;

                _ruleViewModels.Clear();

                foreach (var rule in rules)
                {
                    _ruleViewModels.Add(_helper.CreateRuleModel(rule, filters, _settings));
                }
            }
        }

        private List<string> GetEnabledRules()
        {
            return _ruleViewModels
                .Where(r => r.ShowOnOverlay)
                .Select(r => r.Id)
                .ToList();
        }
        
        #region Events
        
        private void RulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RulesListView.SelectedItem is RuleViewModel selectedRule)
            {
                //UpdateRules();
                PropertyGrid.SelectedObject = selectedRule;
            }
        }

        private void Launch_Overlay(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible)
            {
                var overlayWindow = new OverlayWindow
                {
                    Owner = this
                };
                overlayWindow.Show();
            }
            else
            {
                _overlayWindow.Activate();
            }
        }

        private void Refresh_Rules(object sender, RoutedEventArgs e)
        {
            UpdateRules();
        }

        private void Load()
        {
            foreach (var ruleViewModel in _ruleViewModels)
            {
                _helper.UpdateRuleModel(ruleViewModel, _settings);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            _settings.RulesOnOverlay = GetEnabledRules();
            _settings.FlashThresholds = _ruleViewModels
                .Where(r => r.FlashThresholdEnabled)
                .ToDictionary(r => r.Id, r => r.FlashThreshold);
            _settings.DisableThresholds = _ruleViewModels
                .Where(r => r.DisableThresholdEnabled)
                .ToDictionary(r => r.Id, r => r.DisableThreshold);

            var validationResult = _settings.Validate();

            if (validationResult.IsValid)
            {
                _settings.Save();
                MessageBox.Show("Saved", "Save");
            }
            else
            {
                var output = validationResult.Errors.ConcatStringsForMessageBox();
                MessageBox.Show(output, "Error Saving") ;
            }
        }

        private void OpenConfigFile_Click(object sender, RoutedEventArgs e)
        {
            var path = _settings.GetFilePath();
            
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error");
            }            
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            var advancedSettingsWindow = new AdvancedSetings
            {
                Owner = this
            };
            advancedSettingsWindow.Show();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion
    }
}
