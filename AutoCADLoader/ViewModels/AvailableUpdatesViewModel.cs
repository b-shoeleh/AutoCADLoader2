﻿using System.Collections.ObjectModel;

namespace AutoCADLoader.ViewModels
{
    public class AvailableUpdatesViewModel : ViewModelBase
    {
        private AvailableUpdateViewModel _packages = new AvailableUpdateViewModel() { Title = "Packages" };
        public AvailableUpdateViewModel Packages { get { return _packages; }
            set
            {
                _packages = value;
                OnPropertyChanged();
            }
        }

        private AvailableUpdateViewModel _settings = new AvailableUpdateViewModel() { Title = "Settings" };
        public AvailableUpdateViewModel Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        private AvailableUpdateViewModel _supportFiles = new AvailableUpdateViewModel() { Title = "Support Files" };
        public AvailableUpdateViewModel SupportFiles
        {
            get { return _supportFiles; }
            set
            {
                _supportFiles = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AvailableUpdateViewModel> GetAll
        {
            get
            {
                ObservableCollection<AvailableUpdateViewModel> list = [Packages, Settings, SupportFiles];
                return list;
            }
        }
    }
}
