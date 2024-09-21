using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Utils;
using AutoCADLoader.ViewModels;
using System.Windows;

namespace AutoCADLoader.Commands
{
    public class LaunchApplicationRelayCommand(Action<object> execute, Predicate<object> canExecute) : RelayCommandBase(execute, canExecute)
    {
        public bool CanExecute(AutodeskApplicationViewModel? selectedApplication, OfficeViewModel selectedOffice)
        {
            // Must ensure that both an application and office have been selected
            return
                !selectedOffice.IsPlaceholder
                && !selectedOffice.IsPlaceholder;
        }

        public void Execute(
            AutodeskApplicationViewModel selectedApplicationViewModel,
            IEnumerable<BundleViewModel> bundleViewModels,
            OfficeViewModel selectedOfficeViewModel,
            bool resetAllSettings, bool hardwareAcceleration,
            Action? closeAction = null)
        {
            if (closeAction is not null)
            {
                closeAction();
            }

            // Update bundles from viewmodels
            IEnumerable<Bundle> bundles = InfoCollector.BundleCollection.Packages;
            foreach (BundleViewModel bundleViewModel in bundleViewModels)
            {
                foreach (BundleViewModel viewModel in bundleViewModels)
                {
                    Bundle? bundle = bundles.Where(b => string.Equals(b.Title, viewModel.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (bundle is not null)
                    {
                        viewModel.UpdateModel(bundle);
                    }
                }
            }
            IEnumerable<Bundle>? selectedBundles = bundles.Where(b => b.Active);

            AutodeskApplication? selectedApplication = AutodeskApplicationsInstalled.GetByIdOrDefault(selectedApplicationViewModel.AutodeskApplicationId);
            if (selectedApplication is null)
            {
                EventLogger.Log($"Selected application ID cannot be found in supported applications: {selectedApplicationViewModel.AutodeskApplicationId}", System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show(
                    "The selected application is not currently supported by the Loader.",
                    "Unsupported application",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Office selectedOffice = Offices.GetOfficeByIdOrDefault(selectedOfficeViewModel.Id);

            RegistryInfo.SaveSettings(
                selectedApplication.Title,
                selectedApplication.Version.Number.ToString(),
                selectedApplication.Plugin?.Title,
                selectedBundles,
                selectedOffice.Id,
                hardwareAcceleration
            );

            AutodeskApplicationLauncher.Launch(selectedApplication, bundles, selectedOffice, resetAllSettings, hardwareAcceleration);

            // TODO: Improve
            if (FileSyncManager.Enabled)
            {
                FileSyncManager.SynchronizeFromAzure(selectedOffice);
                FileSyncManager.SynchronizeFromUserData(selectedOffice);
                RegistryInfo.SaveLastUpdated(DateTime.Now);
            }

            // TODO: IBI analytics logging?
            Environment.Exit(0);
        }
    }
}
