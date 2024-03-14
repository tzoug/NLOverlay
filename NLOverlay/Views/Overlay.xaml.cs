using NetLimiter.Service;
using NLOverlay.Helpers;
using NLOverlay.Models;
using NLOverlay.Services;
using NLOverlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        #region Window

        // Import the SetWindowLong function
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Import the GetWindowLong function
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        // Import the GetForegroundWindow function
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Constants for window styles
        private const int GWL_HWNDPARENT = -8;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

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

            Topmost = true;
            SourceInitialized += OverlayWindow_SourceInitialized;

            DataContext = this;
            _ruleViewModels = new ObservableCollection<RuleViewModel>();
            rulesGrid.ItemsSource = _ruleViewModels;

            Setup();

            Task.Run(RenderAsync);
        }

        private async Task RenderAsync()
        {
            try
            {
                using (var client = new NLClient())
                {
                    client.Connect();
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await UpdateRulesAsync(client, _cancellationTokenSource.Token);
                        await Task.Delay(TimeSpan.FromMilliseconds(_settings.ApiPollingRate), _cancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, handle as needed
            }
        }

        private void SetCellBackgroundColor()
        {
            var backgroundColor = _settings.OverlayTextBackgroundColor;
            var opacity = _settings.OverlayTextBackgroundOpacity;

            var background = Utils.ConvertToHexWithOpacity(backgroundColor, opacity);
            rulesGrid.RowBackground = Utils.ConvertHexStringToBrush(background);
        }

        private void SetCellTextColor()
        {
            var style = new Style(typeof(TextBlock));

            // Setters
            style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.UltraBold));
            style.Setters.Add(new Setter(FocusableProperty, false));
            style.Setters.Add(new Setter(IsHitTestVisibleProperty, false));

            // Triggers
            var highlightThresholdReachedTrigger = new DataTrigger
            {
                Binding = new Binding("IsHighlightThresholdReached"),
                Value = true
            };
            var thresholdReachedColor = Utils.ConvertHexStringToBrush(_settings.OverlayTextThresholdReachColor);
            highlightThresholdReachedTrigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, thresholdReachedColor));

            var notHighlightThresholdReachedTrigger = new DataTrigger
            {
                Binding = new Binding("IsHighlightThresholdReached"),
                Value = false
            };
            
            var textColor = Utils.ConvertHexStringToBrush(_settings.OverlayTextColor);
            notHighlightThresholdReachedTrigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, textColor));

            style.Triggers.Add(highlightThresholdReachedTrigger);
            style.Triggers.Add(notHighlightThresholdReachedTrigger);

            RuleForGridColumn.ElementStyle = style;
            IntervalStringGridColumn.ElementStyle = style;
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

        public void Setup()
        {
            _settings.Load();
            _ruleViewModels.Clear();

            SetWindowPosition();
            SetCellBackgroundColor();
            SetCellTextColor();
        }

        #region Window Events

        private void OverlayWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Get the HWND of the game window
            IntPtr gameWindowHandle = GetGameWindowHandle();

            // Set the owner of the overlay window to be the game window
            SetWindowLong(new WindowInteropHelper(this).Handle, GWL_HWNDPARENT, new IntPtr(gameWindowHandle.ToInt32()));

            // Modify the window style to be a tool window (optional)
            SetWindowLong(new WindowInteropHelper(this).Handle, GWL_EXSTYLE, new IntPtr(GetWindowLong(new WindowInteropHelper(this).Handle, GWL_EXSTYLE).ToInt32() | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE));
        }

        private IntPtr GetGameWindowHandle()
        {
            return IntPtr.Zero;
        }

        #endregion

        #region ViewModel Stuff

        private async Task UpdateRulesAsync(NLClient client, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                var filters = client.Filters;
                var rules = client.Rules;

                var activeAndOverlayRules = rules
                    .Where(r => r.IsActive && _settings.RulesOnOverlay.Contains(r.Id))
                    .ToList();

                var modelsToAdd = new List<RuleViewModel>();
                foreach (var rule in activeAndOverlayRules.ToList())
                {
                    var model = _helper.CreateRuleModel(rule, filters, _settings);
                    modelsToAdd.Add(model);
                }

                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        _ruleViewModels.Clear();

                        foreach (var model in modelsToAdd)
                        {
                            _ruleViewModels.Add(model);
                            DisableRuleIfThresholdReached(model);
                        }
                    });
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
