using NetLimiter.Service;
using System;
using System.ComponentModel;
using NLInfoTools;

namespace UI.ViewModels
{
    public class RuleViewModel : INotifyPropertyChanged
    {
        private bool _showOnOverlay;
        private string _ruleFor;
        private Rule _rule;

        // Thresholds - to refactor with List<Threshold> -> and output to property grid
        private bool _flashThresholdEnabled;
        private int _flashThreshold;
        private bool _disableThresholdEnabled;
        private int _disableThreshold;

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

        [Category("Readonly")]
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

        [Category("Readonly")]
        [DisplayName("ID")]
        [ReadOnly(true)]
        public string Id => _rule.Id;

        [Category("Readonly")]
        [ReadOnly(true)]
        [DisplayName("State")]
        public RuleState State => _rule.State;

        [Category("Options")]
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

        [Category("Rule Flash Threshold")]
        [DisplayName("State")]
        public bool FlashThresholdEnabled
        {
            get => _flashThresholdEnabled;
            set
            {
                if (_flashThresholdEnabled != value)
                {
                    _flashThresholdEnabled = value;
                    OnPropertyChanged(nameof(FlashThresholdEnabled));
                }
            }
        }

        [Category("Rule Flash Threshold")]
        [DisplayName("Value (s)")]
        public int FlashThreshold
        {
            get => _flashThreshold;
            set
            {
                if (_flashThreshold != value)
                {
                    _flashThreshold = value;
                    OnPropertyChanged(nameof(FlashThreshold));
                }
            }
        }

        [Category("Rule Disable Threshold")]
        [DisplayName("State")]
        public bool DisableThresholdEnabled
        {
            get => _disableThresholdEnabled;
            set
            {
                if (_disableThresholdEnabled != value)
                {
                    _disableThresholdEnabled = value;
                    OnPropertyChanged(nameof(DisableThresholdEnabled));
                }
            }
        }

        [Category("Rule Disable Threshold")]
        [DisplayName("Value (s)")]
        public int DisableThreshold
        {
            get => _disableThreshold;
            set
            {
                if (_disableThreshold != value)
                {
                    _disableThreshold = value;
                    OnPropertyChanged(nameof(DisableThreshold));
                }
            }
        }

        [Browsable(false)]
        public bool IsFlashThresholdReached => FlashThresholdEnabled && SecondsSinceLastUpdate() >= FlashThreshold;

        [Browsable(false)]
        public bool IsDisableThresholdReached => DisableThresholdEnabled && SecondsSinceLastUpdate() >= DisableThreshold;

        [Browsable(false)]
        private TimeSpan UpdateInterval => DateTime.Now - _rule.UpdatedTime.ToLocalTime();

        [Browsable(false)]
        private int SecondsSinceLastUpdate()
        {
            return (int)UpdateInterval.TotalSeconds;
        }

        [Browsable(false)]
        public string UpdateIntervalString
        {
            get
            {
                var elapsedTime = DateTime.Now - _rule.UpdatedTime.ToLocalTime();

                return elapsedTime.TotalMinutes > 1
                    ? $"{elapsedTime.Minutes}m.{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms"
                    : $"{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}