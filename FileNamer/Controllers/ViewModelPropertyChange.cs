using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileNamer.Controllers
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        protected virtual void OnPropertyChanging([CallerMemberName] string propName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propName));
        }

        public void SetProperty<T>(ref T field, T value, [CallerMemberName] string propName = null)
        {
            if (Equals(field, value)) return;
            OnPropertyChanging(propName);
            field = value;
            OnPropertyChanged(propName);
        }
    }
}
