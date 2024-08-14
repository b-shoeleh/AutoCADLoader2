using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using System.Collections.ObjectModel;

namespace AutoCADLoader.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<AutodeskApplication> Applications { get; set; } = new ObservableCollection<AutodeskApplication>(InfoCollector.AppCollector().Apps);

        public ObservableCollection<InstalledAutodeskApplication> InstalledApplications { get; set; } = [];

        public ObservableCollection<Tuple<string, int>> SystemHealth { get; set; } = [
            new Tuple<string, int>("Windows", 0),
            new Tuple<string, int>("HDD Space", 0),
            new Tuple<string, int>("User Temp", 0)
        ];

        public ObservableCollection<string> AvailablePackages { get; set; } = ["Package 1", "Package 2", "Package 3"];

        private AvailableUpdatesViewModel _availableUpdates = new();
        public AvailableUpdatesViewModel AvailableUpdates
        {
            get
            {
                return _availableUpdates;
            }
            set
            {
                _availableUpdates = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Office> Offices { get; set; } = new ObservableCollection<Office>(OfficesCollection.Data);
        //public ObservableCollection<string> Notices { get; set; } = ["Notice 1", "Notice 2", "Notice 3"];

        public string UserName { get; } = UserInfo.UserName();


        public MainWindowViewModel()
        {
            SetUpInstalledApplications();

            var rememberedOffice = OfficesCollection.GetRememberedOfficeOrDefault();
        }

        private void SetUpInstalledApplications()
        {
            var allApplications = InfoCollector.AppCollector().Apps;
            foreach (var app in allApplications)
            {
                foreach (var appVersion in app.AppVersions)
                {
                    if (appVersion.Installed)
                    {
                        InstalledApplications.Add(new()
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
                                    InstalledApplications.Add(new()
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
    }
}
