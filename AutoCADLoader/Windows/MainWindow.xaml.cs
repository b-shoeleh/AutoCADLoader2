using AutoCADLoader.Models;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AutoCADLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// TODO: Lots of hangovers from the WinForms implementation, logic should be moved out of the window
    /// </summary>
    public partial class MainWindow : Window
    {
        private RegistryInfo _userRegistryInfo;

        public MainWindow(MainWindowViewModel viewModel, RegistryInfo userRegistryInfo)
        {
            _userRegistryInfo = userRegistryInfo;
            DataContext = viewModel;
            InitializeComponent();
            _userRegistryInfo = userRegistryInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.IsEnabled = false;

            // Deal with selected application
            InstalledAutodeskApplication? selectedApplication = (InstalledAutodeskApplication)cbApplications.SelectedItem;
            if (selectedApplication is null)
            {
                MessageBox.Show("No selected application");
                button.IsEnabled = true;
                return;
            }

            _userRegistryInfo.Title = selectedApplication.AutodeskApplication.Title;
            _userRegistryInfo.Version = selectedApplication.AppVersion.Number.ToString();

            Office? selectedOffice = (Office)cbOffices.SelectedItem;
            if (selectedOffice is null)
            {
                MessageBox.Show("No selected office");
                button.IsEnabled = true;
                return;
            }

            var firstRun = UpdateProfilePlotters(selectedApplication, selectedOffice);

            _userRegistryInfo.ActiveOffice = selectedOffice.OfficeCode;
            _userRegistryInfo.Plugin = selectedApplication.Plugin?.Title ?? string.Empty;

            // TODO: Get selected packages

            if (File.Exists(selectedApplication.AppVersion.RunPath))
            {
                Process autoCADProcess = new Process();
                autoCADProcess.StartInfo.FileName = selectedApplication.AppVersion.RunPath;
                autoCADProcess.StartInfo.WorkingDirectory = selectedApplication.AppVersion.AcadPath;
                string appArguments = selectedApplication.AutodeskApplication.GetArguments();

                if (selectedApplication.Plugin is not null)
                {
                    string pluginArguments = selectedApplication.Plugin.GetArguments();
                    appArguments += pluginArguments;
                    _userRegistryInfo.Plugin = selectedApplication.Plugin.Title;
                }
                else
                {
                    _userRegistryInfo.Plugin = string.Empty;
                }

                // TODO: Hardware acceleration
                //if (!cbHardware.Checked)
                //{
                //    appArguments += " /nohardware";
                //    UserRegistryInfo.Hardware = "false";
                //}
                //else
                //{
                //    UserRegistryInfo.Hardware = "true";
                //}

                //if (cbReset.Checked)
                //    appArguments += " /reset";

                _userRegistryInfo.SaveUserRegistry();

                if (!string.IsNullOrWhiteSpace(firstRun) && selectedApplication.AutodeskApplication.Title == "Civil3d")
                {
                    appArguments += " /p " + firstRun;
                }

                autoCADProcess.StartInfo.Arguments = appArguments;
                autoCADProcess.Start();

                // TODO: IBI logging?
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Update the plotter file for registry import
        /// </summary>
        /// <returns>Civil 3D unit selection (if found)</returns>
        private string UpdateProfilePlotters(InstalledAutodeskApplication selectedApplication, Office selectedOffice)
        {
            string units = FileUpdaterLoader.UpdateRegistryFile(selectedOffice, selectedApplication.AppVersion, selectedApplication.AutodeskApplication.Title);
            return units;
        }
    }
}