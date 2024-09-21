using AutoCADLoader.Models.Applications;

namespace AutoCADLoader.ViewModels
{
    public class AutodeskApplicationViewModel
    {
        public string AutodeskApplicationId { get; set; }
        public string DisplayName { get; }

        public VersionYear? AppVersion { get; set; }
        public Plugin? Plugin { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsPlaceholder { get; set; } = false;


        public AutodeskApplicationViewModel(AutodeskApplication application)
        {
            AutodeskApplicationId = application.Id;
            DisplayName = $"{application.Title} {application.Version.Number}";
            if (application.Plugin is not null)
            {
                DisplayName += $" {application.Plugin.Title}";
            }
            IsSelected = application.IsSaved;
        }

        public AutodeskApplicationViewModel(bool isPlaceholder)
        {
            AutodeskApplicationId = "Placeholder";
            DisplayName = "--- Select application ---";
            IsSelected = true;
            IsPlaceholder = true;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
