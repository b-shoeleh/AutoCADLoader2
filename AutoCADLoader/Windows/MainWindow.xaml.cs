using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AutoCADLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RegistryInfo UserRegistryInfo { get; set; }


        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            //other userInfo
            //UserInfo UserInfo = new();

            //user registry info
            //UserRegistryInfo = new RegistryInfo(UserInfo.Office.OfficeCode);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.IsEnabled = false;

            // Deal with selected application
            AutodeskApplication? selectedApplication = (AutodeskApplication)cbApplications.SelectedItem;
            if(selectedApplication is null)
            {
                button.IsEnabled = true;
                return;
            }

            UserRegistryInfo.Title = selectedApplication.Title;
            UserRegistryInfo.Version = selectedApplication.AppVersions.ToString();
        }
    }
}