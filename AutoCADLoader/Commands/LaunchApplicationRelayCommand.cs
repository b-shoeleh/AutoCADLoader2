using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Utils;
using AutoCADLoader.Utils;
using AutoCADLoader.ViewModels;

namespace AutoCADLoader.Commands
{
    public class LaunchApplicationRelayCommand(Action<object> execute, Predicate<object> canExecute) : RelayCommandBase(execute, canExecute)
    {
        public bool CanExecute(InstalledAutodeskApplication? selectedApplication, Office? selectedOffice)
        {
            // Must ensure that both an application and office have been selected
            return
                selectedApplication is not null
                && selectedOffice is not null;
        }

        public void Execute(
            InstalledAutodeskApplication selectedApplication,
            IEnumerable<BundleViewModel> bundleViewModels,
            Office selectedOffice,
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
            RegistryInfo.UpdateUserRegistry(
                selectedApplication.AutodeskApplication.Title,
                selectedApplication.AppVersion.Number.ToString(),
                selectedApplication.Plugin?.Title,
                selectedBundles,
                selectedOffice.Id,
                hardwareAcceleration
            );

            AutodeskApplicationLauncher.Launch(selectedApplication, bundles, selectedOffice, resetAllSettings, hardwareAcceleration);

            FileSyncManager.SynchronizeFromAzure(selectedOffice);
            FileSyncManager.SynchronizeFromUserData(selectedOffice);

            // TODO: IBI analytics logging?
            Environment.Exit(0);
        }
    }
}
