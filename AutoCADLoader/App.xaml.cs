using AutoCADLoader.Models;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using AutoCADLoader.Windows;
using System.Data;
using System.Diagnostics;
using System.Windows;

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

            splashScreenViewModel.LoadingStatus = "Setting up user info...";
            UserInfo userInfo = new();

            splashScreenViewModel.LoadingStatus = "Setting up registry info...";
            RegistryInfo UserRegistryInfo = new(userInfo.Office.OfficeCode);
            var usersOffice = OfficesCollection.GetOfficeByMachineCodeOrDefault(UserRegistryInfo.ActiveOffice);
            OfficesCollection.SetUserOffice(usersOffice, userInfo);

            splashScreenViewModel.LoadingStatus = "Checking for updates...";
            bool update = true;
#if DEBUG
            update = true; //TODO:!
#endif
            MainWindowViewModel mainWindowViewModel = new();
            MainWindow mainWindow = new(mainWindowViewModel);
            mainWindow.Show();

            splashScreenWindow.Close();

            FileUpdater fileUpdater;
            if(update)
            {
                mainWindowViewModel.AvailableUpdates.Packages.FileStatus = "Loading...";
                mainWindowViewModel.AvailableUpdates.Settings.FileStatus = "Loading...";
                mainWindowViewModel.AvailableUpdates.SupportFiles.FileStatus = "Loading...";
                fileUpdater = await Task.Run(CheckForUpdates);
                mainWindowViewModel.AvailableUpdates.Packages.FileStatus = fileUpdater.FileCount(ResourceType.Package);
                mainWindowViewModel.AvailableUpdates.Settings.FileStatus = fileUpdater.FileCount(ResourceType.Setting);
                mainWindowViewModel.AvailableUpdates.SupportFiles.FileStatus = fileUpdater.FileCount(ResourceType.Support);
          }
            else
            {
                //
            }

            MainWindow mainWindow = new(mainWindowViewModel, UserRegistryInfo);
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

        private FileUpdaterLoader CheckForUpdates()
        {
            FileUpdaterLoader fileUpdater = new(OfficesCollection.GetRememberedOfficeOrDefault());
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
