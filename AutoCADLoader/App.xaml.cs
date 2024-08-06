﻿using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using AutoCADLoader.Windows;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AutoCADLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashScreenWindowViewModel splashScreenViewModel = new();
            SplashScreenWindow splashScreenWindow = new(splashScreenViewModel);
            splashScreenWindow.Show();

            splashScreenViewModel.LoadingStatus = "Setting up offices...";
            SetUpOffices();
            
            splashScreenViewModel.LoadingStatus = "Checking for updates...";
            bool update = true;
#if DEBUG
            update = false; //TODO:!
#endif
            MainWindowViewModel mainWindowViewModel = new();

            FileUpdater fileUpdater;
            if(update)
            {
                fileUpdater = await Task.Run(CheckForUpdates);
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Packages", FileStatus = fileUpdater.FileCount(ResourceType.Package) });
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Settings", FileStatus = fileUpdater.FileCount(ResourceType.Setting) });
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Support files", FileStatus = fileUpdater.FileCount(ResourceType.Support) });
            }
            else
            {
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Packages" });
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Settings" });
                mainWindowViewModel.AvailableUpdates.Add(new() { Title = "Support files"});
            }

            MainWindow mainWindow = new(mainWindowViewModel);
            mainWindow.Show();

            splashScreenWindow.Close();
        }


        private static void SetUpOffices()
        {
            EventLogger.Log("Loading office data...", EventLogEntryType.Information);
            string officesStatus = OfficesCollection.LoadOfficesData();
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

        private FileUpdater CheckForUpdates()
        {
            FileUpdater fileUpdater = new(OfficesCollection.GetRememberedOfficeOrDefault());
            fileUpdater.CompareAll();

            var filesToBeUpdated = fileUpdater.AllFiles.Where(f => f.Status != Status.Existing);

            if (filesToBeUpdated.Any())
            {
                MessageBoxResult result = MessageBox.Show("Updates Available, would you like to update your system?", "Updates Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    fileUpdater.Update();
                }

                OfficesCollection.LoadOfficesData(); //since updated, update the offices again

            }

            return fileUpdater;

        }
    }
}
