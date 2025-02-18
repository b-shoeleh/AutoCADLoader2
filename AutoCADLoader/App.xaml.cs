﻿using AutoCADLoader.Models;
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

            EventLogger.Log("Checking for AutoCAD Loader first run...", EventLogEntryType.Information);
            splashScreenViewModel.LoadingStatus = "Checking for AutoCAD Loader first run...";
            FileSyncManager.HandleFirstRun();

            EventLogger.Log("Validating critical files...", EventLogEntryType.Information);
            splashScreenViewModel.LoadingStatus = "Validating critical files...";
            FileSyncManager.ValidateCriticalFiles();

            EventLogger.Log("Setting up offices...", EventLogEntryType.Information);
            splashScreenViewModel.LoadingStatus = "Setting up offices...";
            SetUpOffices();

            fileUpdater = new(Offices.GetSavedOfficeOrDefault());

            EventLogger.Log("Setting up user info...", EventLogEntryType.Information);
            splashScreenViewModel.LoadingStatus = "Setting up user info...";
            UserInfo.Initialize();

            EventLogger.Log("Detecting Autodesk applications...", EventLogEntryType.Information);
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

            EventLogger.Log("Setting up registry info...", EventLogEntryType.Information);
            splashScreenViewModel.LoadingStatus = "Setting up registry info...";
            RegistryInfo.Initialize();
            // Set saved office from registry data
            if (!string.IsNullOrWhiteSpace(RegistryInfo.SavedOfficeId))
            {
                Office savedOffice = Offices.GetOfficeByIdOrDefault(RegistryInfo.SavedOfficeId);
                Offices.SetSavedOffice(savedOffice);
            }
            // Set saved application from registry data
            if(!string.IsNullOrWhiteSpace(RegistryInfo.Title) && !string.IsNullOrWhiteSpace(RegistryInfo.Version))
            {
                AutodeskApplicationsInstalled.SetSavedApplication(RegistryInfo.Title, RegistryInfo.Version, RegistryInfo.Plugin);
            }

            splashScreenViewModel.LoadingStatus = "Detecting packages...";
            InfoCollector.PopulateBundlesData();

            splashScreenViewModel.LoadingStatus = "Checking system health...";
            SystemHealth systemHealth = new();
            var systemHealthValues = systemHealth.Get();

            splashScreenViewModel.LoadingStatus = "Checking for updates...";
            var lastUpdated = DateTime.Now - RegistryInfo.LastUpdated;
            bool update = true;
            MainWindowViewModel mainWindowViewModel = new(AutodeskApplicationsInstalled.Data, systemHealthValues);
            // TODO: Improve
            if (lastUpdated < TimeSpan.FromDays(1))
            {
                update = false;
                FileSyncManager.Enabled = false;
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Up to date";
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Up to date";
            }
#if DEBUG
            //update = true; //TODO:!
#endif
            MainWindow mainWindow = new(mainWindowViewModel);
            mainWindow.Show();
            splashScreenWindow.Close();

            if (update)
            {
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Checking...";
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Checking...";
                fileUpdater = await Task.Run(() => CheckForUpdates(mainWindowViewModel));
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Updated";
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Updated";
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
                mainWindowViewModel.UpdatesAvailable.Packages.FileStatus = "Updating...";
                mainWindowViewModel.UpdatesAvailable.Settings.FileStatus = "Updating...";
                fileUpdater.Update();
            }

            return fileUpdater;
        }
    }
}
