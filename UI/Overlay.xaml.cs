using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Common;
using Common.Services;
using NetLimiter.Service;
using UI.Classes;
using UI.ViewModels;
using Brushes = System.Windows.Media.Brushes;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private readonly SettingsData _settings;
        private ObservableCollection<RuleViewModel> _ruleViewModels;
        private IRuleService _ruleService;
        private readonly Helper _helper;

        public OverlayWindow()
        {
            InitializeComponent();
            _ruleService = new RuleService();
            _helper = new Helper();

            Application.Current.MainWindow.Closed += MainWindow_Closed;

            Topmost = true;
            
            SetWindowPosition();
            
            _settings = new SettingsData();
            _settings.Load();

            DataContext = this;
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            rulesGrid.ItemsSource = _ruleViewModels;

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    UpdateRules();
                    Thread.Sleep(System.TimeSpan.FromMilliseconds(_settings.ApiPollingRate.ConvertStringToInt()));
                }
            });
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }

        private void SetWindowPosition()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = 0;
            Top = (workingArea.Height - Height) / 2;
        }

        #region ViewModel Stuff

        private void UpdateRules()
        {
            using (var client = new NLClient())
            {
                client.Connect();

                var filters = client.Filters;
                var rules = client.Rules;

                var activeAndOverlayRules = rules
                    .Where(r => r.IsActive && _settings.RulesOnOverlay.Contains(r.Id))
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _ruleViewModels.Clear();
                    
                    foreach (var rule in activeAndOverlayRules)
                    {
                        var model = _helper.CreateRuleModel(rule, filters, _settings);
                        _ruleViewModels.Add(model);

                        DisableRuleIfThresholdReached(model);
                    }
                });
            }
        }

        private void DisableRuleIfThresholdReached(RuleViewModel model)
        {
            if (model.IsDisableThresholdReached)
            {
                _ruleService.DisableRuleById(model.Id);
            }
        }

        #endregion
    }
}
