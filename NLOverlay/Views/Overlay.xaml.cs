﻿using NetLimiter.Service;
using NLOverlay.Helpers;
using NLOverlay.Models;
using NLOverlay.Services;
using NLOverlay.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            _settings.Load();

            Topmost = true;
            SourceInitialized += OverlayWindow_SourceInitialized;

            SetWindowPosition();

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
