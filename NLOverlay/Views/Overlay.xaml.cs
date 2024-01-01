using NetLimiter.Service;
using NLOverlay.Helpers;
using NLOverlay.Models;
using NLOverlay.Services;
using NLOverlay.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private Settings _settings;
        private ObservableCollection<RuleViewModel> _ruleViewModels;
        private IRuleService _ruleService;
        private readonly Helper _helper;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public OverlayWindow()
        {
            InitializeComponent();
            _ruleService = new RuleService();
            _helper = new Helper();

            var settings = new Settings();
            _settings = new Settings();
            _settings.Load();

            Topmost = true;

            SetWindowPosition();

            DataContext = this;
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            rulesGrid.ItemsSource = _ruleViewModels;
            rulesGrid.PreviewMouseDown += RulesGrid_PreviewMouseDown;

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

        private void SetWindowPosition()
        {
            var workingArea = SystemParameters.WorkArea;

            switch (_settings.OverlayPlacement)
            {
                case Enums.OverlayPlacement.TopLeft:
                    Top = 0;
                    Left = 0;
                    break;
                case Enums.OverlayPlacement.TopCenter:
                    Top = 0;
                    Left = (workingArea.Width - Width) / 2;
                    break;
                case Enums.OverlayPlacement.TopRight:
                    Top = 0;
                    Left = workingArea.Width - Width;
                    break;
                case Enums.OverlayPlacement.LeftCenter:
                    Top = (workingArea.Height - Height) / 2;
                    Left = 0;
                    break;
                case Enums.OverlayPlacement.RightCenter:
                    Top = (workingArea.Height - Height) / 2;
                    Left = workingArea.Width - Width;
                    break;
                default:
                    // Defaults to LeftCenter
                    Top = (workingArea.Height - Height) / 2;
                    Left = 0;
                    break;
            }
        }

        public void LoadSettings()
        {
            _settings.Load();
            _ruleViewModels.Clear();

            SetWindowPosition();
        }

        private void RulesGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;

            if (originalSource != null)
            {
                Console.WriteLine($"Clicked element type: {originalSource.GetType().Name}");
            }
            e.Handled = true;
        }

        private void OverlayWindow_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;

            if (originalSource != null)
            {
                Console.WriteLine($"Clicked element type: {originalSource.GetType().Name}");
            }
            e.Handled = true;
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
