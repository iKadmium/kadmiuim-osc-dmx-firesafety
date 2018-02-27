using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kadmium_osc_dmx_firesafety
{
    public class SafetyStatus : INotifyPropertyChanged
    {
        private Exception _error;
        private bool _status;

        public bool Status {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                _error = null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusString)));
            }
        }

        public string StatusString
        {
            get
            {
                if (_error != null)
                {
                    return _error.Message;
                }
                else
                {
                    return Status ? "Safe" : "Unsafe";
                }
            }
        }

        public float StatusFloat { get { return Status ? 1f : 0f; } }
        public Exception Error
        {
            get { return _error; }
            set
            {
                _error = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusString)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public SafetyStatus()
        {
            Status = false;
        }
    }
}
