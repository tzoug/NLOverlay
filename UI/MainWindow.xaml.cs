using Common;
using NetLimiter.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;
using Window = System.Windows.Window;

namespace UI
{
    public partial class MainWindow : Window
    {
        private OverlayWindow _overlayWindow;

        public ClientViewModel ClientViewModel { get; set; }
        private readonly ObservableCollection<RuleViewModel> _ruleViewModels;
        private readonly SettingsData _settings;

        //private ViewModels.PropertyGridRuleModel _propertyGridRule;

        public MainWindow()
        {
            InitializeComponent();
            _settings = new SettingsData();

            ClientViewModel = new ClientViewModel();
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            
            RulesListView.ItemsSource = _ruleViewModels;

            DataContext = this;

            UpdateRules();
            _settings.Load();
            Load();
        }

        #region Client and Rules

        private void UpdateRules()
        {
            using (var client = new NLClient())
            {
                client.Connect();

                UpdateClientViewModel(client.State);

                var filters = client.Filters;
                var rules = client.Rules;

                if (!filters.IsNullOrEmpty() && !rules.IsNullOrEmpty())
                {
                    UpdateRuleViewModels(filters, rules);
                }

                _ruleViewModels.Clear();

                foreach (var rule in rules)
                {
                    var ruleFor = filters.FirstOrDefault(f => f.Id == rule.FilterId)?.Name;
                    _ruleViewModels.Add(new RuleViewModel
                    {
                        ShowOnOverlay = _settings.EnabledRuleIds.Contains(rule.Id),
                        RuleFor = ruleFor,
                        Rule = rule
                    });
                }
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
        }

        private void UpdateClientViewModel(NLServiceState state)
        {
            ClientViewModel.ServiceState = state;
        }

        private List<string> GetEnabledRules()
        {
            return _ruleViewModels
                .Where(r => r.ShowOnOverlay)
                .Select(r => r.Id)
                .ToList();
        }

        #endregion

        #region Events

        private void RulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RulesListView.SelectedItem is RuleViewModel selectedRule)
            {
                UpdateRules();
                PropertyGrid.SelectedObject = selectedRule;
            }
        }

        private void Launch_Overlay(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow == null || !_overlayWindow.IsVisible)
            {
                var overlayWindow = new OverlayWindow();
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
                ruleViewModel.ShowOnOverlay = _settings.EnabledRuleIds.Contains(ruleViewModel.Id);
            }
            //opacityInput.Text = _settings.OverlayOpacity;
            //pollingRateInput.Text = _settings.ApiPollingRate;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            _settings.EnabledRuleIds = GetEnabledRules();
            //_settings.OverlayOpacity = opacityInput.Text;
            //_settings.ApiPollingRate = pollingRateInput.Text;

            var validationResult = _settings.Validate();

            if (validationResult.IsValid)
            {
                _settings.Save();
                MessageBox.Show("Saved", "Save", MessageBoxButton.OK);
            }
            else
            {
                var output = validationResult.Errors.ConcatStringsForMessageBox();
                MessageBox.Show(output, "Error Saving", MessageBoxButton.OK);
            }
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

    }
}
