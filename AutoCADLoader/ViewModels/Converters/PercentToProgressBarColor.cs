using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCADLoader.ViewModels.Converters
{
    public class PercentToProgressBarColor : IValueConverter
    {
        // TODO: Review
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var progress = (double)value;
            if (progress < 50)
            {
                return Brushes.Green;
            }

            if (progress < 80)
            {
                return Brushes.Orange;
            }

            return Brushes.OrangeRed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}