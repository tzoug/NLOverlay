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
        private bool _isThresholdEnabled;
        private int _thresholdSeconds;

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

        [Category("General")]
        [DisplayName("ID")]
        [ReadOnly(true)]
        public string Id => _rule.Id;

        [Category("General")]
        [ReadOnly(true)]
        [DisplayName("State")]
        public RuleState State => _rule.State;

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

        [Browsable(true)]
        [Category("Options")]
        [DisplayName("Threshold Enabled")]
        public bool IsThresholdEnabled 
        {
            get => _isThresholdEnabled;
            set
            {
                if (_isThresholdEnabled != value)
                {
                    _isThresholdEnabled = value;
                    OnPropertyChanged(nameof(IsThresholdEnabled));
                }
            }
        }

        [Category("Options")]
        [Browsable(true)]
        [DisplayName("Threshold (s)")]
        public int ThresholdSeconds
        {
            get => _thresholdSeconds;
            set
            {
                if (_thresholdSeconds != value)
                {
                    _thresholdSeconds = value;
                    OnPropertyChanged(nameof(ThresholdSeconds));
                }
            }
        }

        [Browsable(false)]
        public bool IsPassedThreshold => IsThresholdEnabled && UpdateInterval.TotalSeconds >= ThresholdSeconds;


        [Browsable(false)]
        public TimeSpan UpdateInterval => DateTime.Now - _rule.UpdatedTime.ToLocalTime();

        [Browsable(false)]
        public string UpdateIntervalString
        {
            get
            {
                var elapsedTime = DateTime.Now - _rule.UpdatedTime.ToLocalTime();

                return elapsedTime.TotalMinutes > 1
                    ? $"{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms"
                    : $"{elapsedTime.Minutes}m.{elapsedTime.Seconds}s.{elapsedTime.Milliseconds}ms";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}