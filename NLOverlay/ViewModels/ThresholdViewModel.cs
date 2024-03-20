using System.ComponentModel;
using NLOverlay.Enums;

namespace NLOverlay.ViewModels
{
    public class ThresholdViewModel : INotifyPropertyChanged
    {
        private ThresholdTypes _type;
        private bool _state;
        private int _limit;

        public ThresholdTypes Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public bool State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        public int Limit
        {
            get => _limit;
            set
            {
                if (_limit != value)
                {
                    _limit = value;
                    OnPropertyChanged(nameof(Limit));
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
