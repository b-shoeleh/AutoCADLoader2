using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utility;
using System.Windows;
using System.Windows.Input;

namespace AutoCADLoader.Commands
{
    public class SyncOfficeStandardsCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not Office selectedOffice)
            {
                MessageBox.Show("Selected office is not valid.");
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            FileSyncManager.SynchronizeFromAzure(selectedOffice);
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
