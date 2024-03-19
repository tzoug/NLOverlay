using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NLOverlay.Views
{
    public partial class MainWindow : FluentWindow
    {
        private OverlayWindow _overlayWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            ApplicationThemeManager.Apply(this);

            Loaded += OnMainWindowLoaded;
            Closed += MainWindow_Closed;

            DataContext = this;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(RulesView));
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
                _overlayWindow.Setup();
            }
            else
            {
                _overlayWindow.Activate();
                _overlayWindow.Setup();
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
    }
}
