using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Common;
using NetLimiter.Service;
using UI.ViewModels;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private readonly SettingsData _settings;
        private ObservableCollection<RuleViewModel> _ruleViewModels;

        public OverlayWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow.Closed += MainWindow_Closed;

            Topmost = true;
            
            SetWindowPosition();
            
            _settings = new SettingsData();
            _settings.Load();

            Set();

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

        private void Set()
        {
            Opacity = _settings.OverlayOpacity.ConvertStringToInt() / 10;
        }

        private void SetWindowPosition()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = 0;
            Top = (workingArea.Height - Height) / 2;
        }

        #region ViewModel Stuff

        // TODO Refactor

        private void UpdateRules()
        {
            using (var client = new NLClient())
            {
                client.Connect();

                var filters = client.Filters;
                var rules = client.Rules;

                // Clear existing items and add new items on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _ruleViewModels.Clear();

                    foreach (var rule in rules)
                    {
                        if (_settings.EnabledRuleIds.Contains(rule.Id) && rule.State == RuleState.Active)
                        {
                            var ruleFor = filters.FirstOrDefault(f => f.Id == rule.FilterId)?.Name;
                            _ruleViewModels.Add(new RuleViewModel
                            {
                                ShowOnOverlay = _settings.EnabledRuleIds.Contains(rule.Id),
                                RuleFor = ruleFor,
                                Rule = rule
                            });

                            //if (_ruleViewModels.First().UpdateInterval > )
                            //{

                            //}
                        }
                    }
                });
            }
        }

        private void UpdateRuleViewModels(IEnumerable<Filter> filters, IEnumerable<Rule> rules)
        {
            // Update the ObservableCollection based on the changes from the API
            var updatedRuleViewModels = new ObservableCollection<RuleViewModel>();

            foreach (var rule in rules)
            {
                var ruleFor = filters.FirstOrDefault(f => f.Id == rule.FilterId)?.Name;

                updatedRuleViewModels.Add(new RuleViewModel
                {
                    RuleFor = ruleFor,
                    Rule = rule
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update the existing collection to reflect changes
                foreach (var updatedRuleViewModel in updatedRuleViewModels)
                {
                    var existingViewModel = _ruleViewModels.FirstOrDefault(vm => vm.Rule.Id == updatedRuleViewModel.Rule.Id);
                    if (existingViewModel != null)
                    {
                        // Update existing item
                        existingViewModel.RuleFor = updatedRuleViewModel.RuleFor;
                    }
                    else
                    {
                        // Add new item
                        _ruleViewModels.Add(updatedRuleViewModel);
                    }
                }

                // Remove items that are no longer present in the updated collection
                var itemsToRemove = _ruleViewModels.Where(vm => !updatedRuleViewModels.Any(u => u.Rule.Id == vm.Rule.Id)).ToList();
                foreach (var itemToRemove in itemsToRemove)
                {
                    _ruleViewModels.Remove(itemToRemove);
                }
            });
        }

        #endregion
    }
}
