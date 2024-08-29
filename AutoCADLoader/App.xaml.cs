using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utils;
using AutoCADLoader.ViewModels;
using AutoCADLoader.Windows;
using System.Diagnostics;
using System.Windows;

namespace AutoCADLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        FileUpdaterLoader? fileUpdater;


        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool logInfo = e.Args.Contains("log");
            EventLogger.Initialize(logInfo);

            SplashScreenWindowViewModel splashScreenViewModel = new();
            SplashScreenWindow splashScreenWindow = new(splashScreenViewModel);
            splashScreenWindow.Show();

            splashScreenViewModel.LoadingStatus = "Checking for AutoCAD Loader first run...";
            FileSyncManager.HandleFirstRun();

            splashScreenViewModel.LoadingStatus = "Validating critical files...";
            FileSyncManager.ValidateCriticalFiles();

            splashScreenViewModel.LoadingStatus = "Setting up offices...";
            SetUpOffices();

            fileUpdater = new(Offices.GetSavedOfficeOrDefault());

            splashScreenViewModel.LoadingStatus = "Setting up user info...";
            UserInfo.Initialize();

            splashScreenViewModel.LoadingStatus = "Setting up registry info...";
            RegistryInfo.Initialize();
            Office usersOffice = Offices.GetOfficeByIdOrDefault(RegistryInfo.ActiveOffice);
            Offices.SetRememberedOffice(usersOffice);

            splashScreenViewModel.LoadingStatus = "Detecting Autodesk applications...";
            bool applicationsPopulated = InfoCollector.PopulateApplicationData();
            if (!applicationsPopulated)
            {
                // Defined applications are critical for the loader to work, cannot proceed without them
                var result = MessageBox.Show(
                    "An unrecoverable error has occurred. This application will now close.",
                    "Error detecting Autodesk application definitions",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(1);
            }

            splashScreenViewModel.LoadingStatus = "Detecting packages...";
            InfoCollector.PopulateBundlesData();

            splashScreenViewModel.LoadingStatus = "Checking system health...";
            SystemHealth systemHealth = new();
            var systemHealthValues = systemHealth.Get();

            splashScreenViewModel.LoadingStatus = "Checking for updates...";
            bool update = true;
#if DEBUG
            update = true; //TODO:!
#endif
            MainWindowViewModel mainWindowViewModel = new(systemHealthValues);
            MainWindow mainWindow = new(mainWindowViewModel);
            mainWindow.Show();
            splashScreenWindow.Close();

            if (update)
            {
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Loading...";
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Loading...";
                fileUpdater = await Task.Run(() => CheckForUpdates(mainWindowViewModel));
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = fileUpdater.FileCount(ResourceType.Package);
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = fileUpdater.FileCount(ResourceType.Setting);
            }
        }


        private static void SetUpOffices()
        {
            EventLogger.Log("Loading office data...", EventLogEntryType.Information);
            string? officesStatus = Offices.LoadOfficesData();
            if (officesStatus is null)
            {
                MessageBox.Show(
                    "Office information could not be found." +
                    "\n\nThis application cannot continue and will now close.",
                    "Unrecoverable error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(0);
            }
            EventLogger.Log(officesStatus, EventLogEntryType.Information);
        }

        private FileUpdaterLoader CheckForUpdates(MainWindowViewModel mainWindowViewModel)
        {
            fileUpdater.CompareAll();
            if (fileUpdater.UpdatesAvailable)
            {
                MessageBoxResult result = MessageBox.Show("Updates Available, would you like to update your system?", "Updates Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Updating...";
                    mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Updating...";
                    fileUpdater.Update();
                }
            }

            return fileUpdater;

        }
    }
}
