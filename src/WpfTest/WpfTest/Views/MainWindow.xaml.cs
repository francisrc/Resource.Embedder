using System.Windows;
using WpfTest.ViewModels;

namespace WpfTest.Views
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