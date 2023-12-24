using System;
using System.ComponentModel;
using NetLimiter.Service;
using UI.Classes;

namespace UI.ViewModels
{
    public class RuleViewModel : INotifyPropertyChanged
    {
        private bool _showOnOverlay;
        private string _ruleFor;
        private Rule _rule;
        private Threshold _threshold = new Threshold();

        [Category("Options")]
        [ReadOnly(false)]
        [DisplayName("Show On Overlay")]
        public bool ShowOnOverlay
        {
            get => _showOnOverlay;
            set
            {
                if (_showOnOverlay != value)
                {
                    _showOnOverlay = value;
                    OnPropertyChanged(nameof(ShowOnOverlay));
                }
            }
        }

        [Category("Options")]
        [ReadOnly(false)]
        [DisplayName("Threshold Enabled")]
        public bool ThresholdEnabled
        {
            get => _threshold == null ? false : _threshold.IsEnabled;
            set
            {
                if (_threshold.IsEnabled != value)
                {
                    _threshold.IsEnabled = value;
                    OnPropertyChanged(nameof(Threshold));
                }
            }
        }

        [Category("Options")]
        [ReadOnly(false)]
        [DisplayName("Threshold Value")]
        public TimeSpan ThresholdSpan
        {
            get => _threshold == null ? TimeSpan.Zero : _threshold.Value;
            set
            {
                if (_threshold.Value != value)
                {
                    _threshold.Value = value;
                    OnPropertyChanged(nameof(Threshold));
                }
            }
        }

        [Category("General")]
        [ReadOnly(true)]
        [DisplayName("Name")]
        public string RuleFor
        {
            get => _ruleFor; 
            set
            {
                if (_ruleFor != value)
                {
                    _ruleFor = value;
                    OnPropertyChanged(nameof(RuleFor));
                }
            }
        }

        [Browsable(false)]
        public Rule Rule
        {
            get => _rule;
            set
            {
                if (_rule != value)
                {
                    _rule = value;
                    OnPropertyChanged(nameof(Rule));
                }
            }
        }

        [Category("General")]
        [ReadOnly(true)]
        [DisplayName("State")]
        public RuleState State => _rule.State;

        [Category("General")]
        [DisplayName("ID")]
        [ReadOnly(true)]
        public string Id => _rule.Id;

        [Browsable(false)]
        public TimeSpan UpdateInterval => DateTime.Now - _rule.UpdatedTime.ToLocalTime();

        [Browsable(false)]
        public string UpdateIntervalString
        {
            get
            {
                var elapsedTime = DateTime.Now - _rule.UpdatedTime.ToLocalTime();

                if (elapsedTime.TotalMinutes < 1)
                {
                    // Less than a minute, display as seconds and milliseconds
                    return $"{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms";
                }
                else
                {
                    // More than a minute, display as minutes, seconds, and milliseconds
                    return $"{elapsedTime.Minutes}m.{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}