using AutoCADLoader.Commands;
using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AutoCADLoader.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<InstalledAutodeskApplication> ApplicationsInstalled { get; set; } = [];

        private InstalledAutodeskApplication? _applicationInstalledSelected;
        public InstalledAutodeskApplication? ApplicationInstalledSelected
        {
            get
            {
                return _applicationInstalledSelected;
            }
            set
            {
                if (value != _applicationInstalledSelected)
                {
                    _applicationInstalledSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Tuple<string, int>> SystemHealth { get; set; } = [
            new Tuple<string, int>("Windows", 0),
            new Tuple<string, int>("HDD Space", 0),
            new Tuple<string, int>("User Temp", 0)
        ];

        private ObservableCollection<BundleViewModel> _bundlesAvailable = [];
        public ObservableCollection<BundleViewModel> BundlesAvailable
        {
            get
            {
                return _bundlesAvailable;
            }
            set
            {
                _bundlesAvailable = value;
            }
        }

        private AvailableUpdatesViewModel _updatesAvailable = new();
        public AvailableUpdatesViewModel UpdatesAvailable
        {
            get
            {
                return _updatesAvailable;
            }
            set
            {
                _updatesAvailable = value;
                OnPropertyChanged();
            }
        }

        public string UserName { get; } = UserInfo.UserName();

        public ObservableCollection<Office> Offices { get; set; } = new ObservableCollection<Office>(Models.Offices.Offices.Data);

        private Office? _officeSelected;
        public Office? OfficeSelected
        {
            get
            {
                return _officeSelected;
            }
            set
            {
                if (value != _officeSelected)
                {
                    _officeSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ResetAllSettingsIsChecked { get; set; } = false;

        public bool HardwareAccelerationIsChecked { get; set; } = false;

        public bool Clear { get; set; }

        // Commands
        private readonly ICommand _syncOfficeStandardsCommand = new SyncOfficeStandardsCommand();
        public ICommand SyncOfficeStandardsCommand => _syncOfficeStandardsCommand;

        private readonly LaunchApplicationRelayCommand _lauchApplicationCommand;
        public LaunchApplicationRelayCommand LaunchApplicationCommand => _lauchApplicationCommand;


        public MainWindowViewModel()
        {
            _lauchApplicationCommand = new(
                param => LaunchApplicationCommand.Execute(ApplicationInstalledSelected!, BundlesAvailable, OfficeSelected!, ResetAllSettingsIsChecked, HardwareAccelerationIsChecked),
                param => LaunchApplicationCommand.CanExecute(ApplicationInstalledSelected, OfficeSelected)
            );

            // TODO: Improve
            SetUpInstalledApplications();
            SetUpAvailableBundles();
            var rememberedOffice = Models.Offices.Offices.GetSavedOfficeOrDefault();
        }


        private void SetUpInstalledApplications()
        {
            var allApplications = InfoCollector.AutodeskApplicationsCollection.Apps;
            foreach (var app in allApplications)
            {
                foreach (var appVersion in app.AppVersions)
                {
                    if (appVersion.Installed)
                    {
                        ApplicationsInstalled.Add(new()
                        {
                            AutodeskApplication = app,
                            AppVersion = appVersion
                        });

                        if (appVersion.Plugins != null && appVersion.Plugins.Count() > 0)
                        {
                            foreach (var plugin in appVersion.Plugins)
                            {
                                if (plugin.Installed == true)
                                {
                                    ApplicationsInstalled.Add(new()
                                    {
                                        AutodeskApplication = app,
                                        AppVersion = appVersion,
                                        Plugin = plugin
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetUpAvailableBundles()
        {
            ObservableCollection<BundleViewModel> bundlesAvailable = [];
            foreach(Bundle bundle in InfoCollector.BundleCollection.Packages)
            {
                bundlesAvailable.Add(new(bundle));
            }

            BundlesAvailable = bundlesAvailable;
        }
    }
}
