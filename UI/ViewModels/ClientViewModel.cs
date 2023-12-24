using System.ComponentModel;
using System.Runtime.CompilerServices;
using NetLimiter.Service;

namespace UI.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private NLServiceState _serviceState;

        public NLServiceState ServiceState
        {
            get { return _serviceState; }
            set
            {
                if (_serviceState != value)
                {
                    _serviceState = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
