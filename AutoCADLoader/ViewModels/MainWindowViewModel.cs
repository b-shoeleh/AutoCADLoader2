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
        public ObservableCollection<AutodeskApplicationViewModel> ApplicationsInstalled { get; set; } = [];

        private AutodeskApplicationViewModel _applicationInstalledSelected = new(true);
        public AutodeskApplicationViewModel ApplicationInstalledSelected
        {
            get
            {
                return _applicationInstalledSelected;
            }
            set
            {
                if (value != _applicationInstalledSelected)
                {
                    _applicationInstalledSelected.IsSelected = false;
                    if (_applicationInstalledSelected.IsPlaceholder)
                    {
                        ApplicationsInstalled.Remove(_applicationInstalledSelected);
                    }
                    _applicationInstalledSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Tuple<string, int>> SystemHealth { get; set; } = [];

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

        public ObservableCollection<OfficeViewModel> Offices { get; set; } = [];

        private OfficeViewModel _officeSelected = new(true);
        public OfficeViewModel OfficeSelected
        {
            get
            {
                return _officeSelected;
            }
            set
            {
                if (value != _officeSelected)
                {
                    _officeSelected.IsSelected = false;

                    if (_officeSelected.IsPlaceholder)
                    {
                        Offices.Remove(_officeSelected);
                    }

                    _officeSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ResetAllSettingsIsChecked { get; set; } = false;

        public bool HardwareAccelerationIsChecked { get; set; } = false;

        public bool Clear { get; set; }

        public Action? CloseAction { get; set; }


        // Commands
        private readonly ICommand _syncOfficeStandardsCommand = new SyncOfficeStandardsCommand();
        public ICommand SyncOfficeStandardsCommand => _syncOfficeStandardsCommand;

        private readonly LaunchApplicationRelayCommand _launchApplicationCommand;
        public LaunchApplicationRelayCommand LaunchApplicationCommand => _launchApplicationCommand;


        public MainWindowViewModel(
            List<AutodeskApplication> installedApplications,
            IEnumerable<Tuple<string, int>> systemHealth)
        {
            // Set up commands
            _launchApplicationCommand = new(
                param => LaunchApplicationCommand.Execute(ApplicationInstalledSelected!, BundlesAvailable, OfficeSelected, ResetAllSettingsIsChecked, HardwareAccelerationIsChecked, CloseAction),
                param => LaunchApplicationCommand.CanExecute(ApplicationInstalledSelected, OfficeSelected)
            );

            SetUpInstalledApplications(installedApplications);

            // Set up system health
            foreach (var item in systemHealth)
            {
                SystemHealth.Add(item);
            }

            SetUpAvailableBundles();
            SetUpOffices();
        }


        private void SetUpInstalledApplications(List<AutodeskApplication> installedApplications)
        {
            foreach (var installedApplication in installedApplications)
            {
                ApplicationsInstalled.Add(new(installedApplication));
            }

            var firstApplicationSelected = ApplicationsInstalled.FirstOrDefault(a => a.IsSelected);
            if(firstApplicationSelected is null)
            {
                ApplicationsInstalled.Insert(0, ApplicationInstalledSelected);
            }
        }

        private void SetUpAvailableBundles()
        {
            ObservableCollection<BundleViewModel> bundlesAvailable = [];
            foreach (Bundle bundle in InfoCollector.BundleCollection.Packages)
            {
                bundlesAvailable.Add(new(bundle));
            }

            BundlesAvailable = bundlesAvailable;
        }

        private void SetUpOffices()
        {
            foreach(Office office in Models.Offices.Offices.Data)
            {
                Offices.Add(new(office));
            }

            var firstOfficeSelected = Offices.FirstOrDefault(a => a.IsSelected);
            if(firstOfficeSelected is null)
            {
                Offices.Insert(0, OfficeSelected);
            }
        }
    }
}
