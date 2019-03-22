using System.Windows;
using WpfCosturaAndRE.ViewModels;

namespace WpfCosturaAndRE.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        #endregion Constructors
    }
}