using AutoCADLoader.Models.Applications;

namespace AutoCADLoader.ViewModels
{
    public class InstalledAutodeskApplication
    {
        public AutodeskApplication AutodeskApplication { get; set; }
        public AppVersion AppVersion { get; set; }
        public Plugin? Plugin { get; set; }

        public override string ToString()
        {
            string result = AutodeskApplication.Title + AppVersion.Number;
            if(Plugin is not null)
            {
                result += Plugin.Title;
            }

            return result;
        }
    }
}
