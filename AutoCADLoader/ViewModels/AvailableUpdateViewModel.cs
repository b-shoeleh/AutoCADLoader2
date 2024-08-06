namespace AutoCADLoader.ViewModels
{
    public class AvailableUpdateViewModel : ViewModelBase
    {
        private string _title = "Unknown";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _fileStatus = "Unknown";
        public string FileStatus
        {
            get
            {
                return _fileStatus;
            }
            set
            {
                _fileStatus = value;
                OnPropertyChanged();
            }
        }

        private int _notInstalled = 0;
        public int NotInstalled
        {
            get { return _notInstalled; }
            set
            {
                _notInstalled = value;
                OnPropertyChanged();
            }
        }

        private int _total = 0;
        public int Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }
    }
}
