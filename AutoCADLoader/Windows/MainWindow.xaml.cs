using AutoCADLoader.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AutoCADLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        // TODO: Improve/move these methods out

        private void lstPackages_Initialized(object sender, EventArgs e)
        {
            FilterPackagesView(sender);
        }

        private void cbApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterPackagesView(sender);
        }

        private void FilterPackagesView(object sender)
        {
            ComboBox? cb = sender as ComboBox;
            InstalledAutodeskApplication? selectedApplication = cb?.SelectedItem as InstalledAutodeskApplication;
            var collectionView = CollectionViewSource.GetDefaultView(lstPackages.ItemsSource);

            if (selectedApplication is null)
            {
                collectionView.Filter = p => { return false; };
            }
            else
            {
                if (selectedApplication.AutodeskApplication.Title.Contains("AutoCAD", StringComparison.InvariantCultureIgnoreCase))
                {
                    collectionView.Filter = p =>
                    {
                        var Package = p as BundleViewModel;
                        return Package?.CompatibleAutocad == true;
                    };
                }
                else
                {
                    collectionView.Filter = p =>
                    {
                        var Package = p as BundleViewModel;
                        return Package?.CompatibleCivil3d == true;
                    };
                }
            }

            collectionView.Refresh();
        }
    }
}