using AutoCADLoader.Models;
using System.Reflection;
using System.Windows.Input;

namespace AutoCADLoader.ViewModels
{
    public class SplashScreenWindowViewModel : ViewModelBase
    {
        private string? _version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        public string? Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        private string _loadingStatus = "Loading...";
        public string LoadingStatus
        {
            get
            {
                return _loadingStatus;
            }
            set
            {
                _loadingStatus = value;
                OnPropertyChanged("LoadingStatus");
            }
        }
    }
}
