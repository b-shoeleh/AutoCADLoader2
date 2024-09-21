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
            viewModel.CloseAction ??= new(Close);
            DataContext = viewModel;
            InitializeComponent();
        }

        // TODO: Improve/move these methods out

        private void lstPackages_Initialized(object sender, EventArgs e)
        {
            FilterPackagesView(cbApplications);
        }

        private void cbApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterPackagesView(sender);
        }

        private void FilterPackagesView(object sender)
        {
            ComboBox? cb = sender as ComboBox;
            AutodeskApplicationViewModel? selectedApplication = cb?.SelectedItem as AutodeskApplicationViewModel;

            if (lstPackages is null)
            {
                return;
            }
            var collectionView = CollectionViewSource.GetDefaultView(lstPackages.ItemsSource);

            // TODO: Improve - not ideal to filter on display names
            if (selectedApplication is null)
            {
                collectionView.Filter = p => { return false; };
            }
            else if (selectedApplication.DisplayName.Contains("AutoCAD", StringComparison.InvariantCultureIgnoreCase))
            {
                collectionView.Filter = p =>
                {
                    var Package = p as BundleViewModel;
                    return Package?.CompatibleAutocad == true;
                };
            }
            else if (selectedApplication.DisplayName.Contains("Civil3d", StringComparison.InvariantCultureIgnoreCase))
            {
                collectionView.Filter = p =>
                {
                    var Package = p as BundleViewModel;
                    return Package?.CompatibleCivil3d == true;
                };
            }
            else
            {
                collectionView.Filter = p => { return false; };
            }

            collectionView.Refresh();
        }
    }
}