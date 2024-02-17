using System;
using NetLimiter.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NLOverlay.ViewModels;
using Window = System.Windows.Window;
using NLOverlay.Helpers;
using NLOverlay.Models;
using HandyControl.Controls;
using MessageBox = System.Windows.MessageBox;
using System.ComponentModel;
using System.Windows.Input;
using System.Drawing;
using Windows.UI.Xaml.Controls.Maps;
using System.IO;

namespace NLOverlay.Views
{
    public partial class MainWindow : Window
    {
        private OverlayWindow _overlayWindow;
        private readonly ObservableCollection<RuleViewModel> _ruleViewModels;
        private readonly Settings _settings;
        private readonly Helper _helper;

        private NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            Closed += MainWindow_Closed;

            _settings = new Settings();
            _helper = new Helper();
            _overlayWindow = new OverlayWindow();
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

                foreach (var rule in rules.ToList())
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Hide the main window instead of closing
            e.Cancel = true;
            Hide();
        }

        private void InitializeNotifyIcon()
        {
            var appIcon = System.Windows.Application.Current.MainWindow.Icon;

            notifyIcon = new NotifyIcon
            {
                Icon = appIcon,
                Text = "NLOverlay",
                Visibility = Visibility.Visible
            };

            //notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Handle double-click event (e.g., restore the window)
            Show();
            WindowState = WindowState.Normal;
        }

        private void DisposeNotifyIcon()
        {
            if (notifyIcon != null)
            {
                //notifyIcon.MouseDoubleClick -= NotifyIcon_MouseDoubleClick;
                //notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
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
                _overlayWindow = new OverlayWindow
                {
                    Owner = null
                };
                _overlayWindow.Show();
            }
            else
            {
                _overlayWindow.Activate();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (_overlayWindow != null && _overlayWindow.IsVisible)
            {
                _overlayWindow.Close();
            }
            Environment.Exit(0);
        }

        private void Refresh_Rules(object sender, RoutedEventArgs e)
        {
            UpdateRules();
        }

        private void Load()
        {
            foreach (var ruleViewModel in _ruleViewModels.ToList())
            {
                _helper.UpdateRuleModel(ruleViewModel, _settings);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            _settings.RulesOnOverlay = GetEnabledRules();
            _settings.HighlightThresholds = _ruleViewModels
                .Where(r => r.HighlightThresholdEnabled)
                .ToDictionary(r => r.Id, r => r.HighlightThreshold);
            _settings.DisableThresholds = _ruleViewModels
                .Where(r => r.DisableThresholdEnabled)
                .ToDictionary(r => r.Id, r => r.DisableThreshold);

            var validationResult = _settings.Validate();

            if (validationResult.IsValid)
            {
                _settings.Save();
                MessageBox.Show("Saved", "Save");
                
                // Refresh the overlay settings
                _overlayWindow?.LoadSettings();
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
            Application.Current.Shutdown();
        }

        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            var advancedSettingsWindow = new AdvancedSetings
            {
                Owner = this
            };
            advancedSettingsWindow.Show();
        }

        private void UpdateOverlay_Click(object sender, RoutedEventArgs e)
        {
            _overlayWindow.LoadSettings();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion
    }
}
