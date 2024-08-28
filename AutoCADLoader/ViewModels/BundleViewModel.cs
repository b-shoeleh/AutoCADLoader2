using AutoCADLoader.Models;
using AutoCADLoader.Models.Packages;
using System.IO;

namespace AutoCADLoader.ViewModels
{
    public class BundleViewModel(Bundle bundle) : ViewModelBase
    {
        public string Name { get; } = bundle.Title;

        public string DirectoryName { get; } = $"{bundle.Title}.bundle";

        public bool CompatibleAutocad { get; set; } = bundle.Autocad;

        public bool CompatibleCivil3d { get; set; } = bundle.Civil3d;

        private bool _active = bundle.Active;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                OnPropertyChanged();
                _active = value;
            }
        }


        public string InstalledDirectoryPath
        {
            get
            {
                return Path.Combine(UserInfo.AppDataFolder(@"Autodesk\ApplicationPlugins"), DirectoryName);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void UpdateModel(Bundle bundle)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                bundle.Title = Name;
            }

            bundle.Autocad = CompatibleAutocad;
            bundle.Civil3d = CompatibleCivil3d;
            bundle.Active = Active;
        }
    }
}
