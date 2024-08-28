using System.Windows;

namespace AutoCADLoader.Windows
{
    /// <summary>
    /// Interaction logic for UnitSelectionWindow.xaml
    /// </summary>
    public partial class UnitSelectionWindow : Window
    {
        public UnitSelectionWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Improve this
            DialogResult = rbUnitsImperial.IsChecked;
        }
    }
}
