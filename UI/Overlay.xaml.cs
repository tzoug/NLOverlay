using Common;
using Common.Services;
using NetLimiter.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UI.Classes;
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
        private IRuleService _ruleService;
        private readonly Helper _helper;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public OverlayWindow()
        {
            InitializeComponent();
            _ruleService = new RuleService();
            _helper = new Helper();

            Topmost = true;
            
            SetWindowPosition();
            
            var settings = new SettingsData();
            _settings = new SettingsData();
            _settings.Load();

            DataContext = this;
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            rulesGrid.ItemsSource = _ruleViewModels;

            Task.Run(RenderAsync);
        }

        private async Task RenderAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await UpdateRulesAsync(_cancellationTokenSource.Token);
                    await Task.Delay(TimeSpan.FromMilliseconds(_settings.ApiPollingRate.ConvertStringToInt()), _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, handle as needed
            }
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

        public void LoadSettings()
        {
            _settings.Load();
            _ruleViewModels.Clear();
        }

        #region ViewModel Stuff

        private async Task UpdateRulesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                using (var client = new NLClient())
                {
                    client.Connect();

                    var filters = client.Filters;
                    var rules = client.Rules;

                    var activeAndOverlayRules = rules
                        .Where(r => r.IsActive && _settings.RulesOnOverlay.Contains(r.Id))
                        .ToList();

                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

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
            }, cancellationToken);
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
