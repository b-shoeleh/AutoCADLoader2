using AutoCADLoader.Models.Offices;

namespace AutoCADLoader.ViewModels
{
    public class OfficeViewModel
    {
        public string Id { get; }

        public string DisplayName { get; }

        public bool IsSelected { get; set; } = false;

        public bool IsPlaceholder { get; set; } = false;


        public OfficeViewModel(Office office)
        {
            Id = office.Id;
            DisplayName = $"{office.Region.DirectoryName.ToUpper()} - {office.DisplayName}";
            IsSelected = office.IsSavedOffice;
        }

        public OfficeViewModel(bool isPlaceholder)
        {
            Id = "Placeholder";
            DisplayName = "--- Select office ---";
            IsSelected = true;
            IsPlaceholder = true;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

