using ResourceEmbedder.Core;
using System;
using System.Globalization;
using System.Windows.Input;
using WpfTest.Resources;

namespace WpfTest.ViewModels
{
	public class MainWindowViewModel : NotifyPropertyChangedBase
	{
		#region Fields

		private string _error;
		private string _locale;
		private ICommand _setLanguage;

		#endregion Fields

		#region Constructors

		public MainWindowViewModel()
		{
			var args = Environment.GetCommandLineArgs();
			// first argument is always exe name itself
			if (args.Length > 1)
			{
				// argument switched used by unit test EmbeddFilesTests.cs\TestEmbeddMultipleLocalizationsIntoWpfExe()
				if (args[1] == "/throwOnMissingInlineLocalization" || args[1] == "/testFullyProcessed")
				{
					if (args[1] == "/throwOnMissingInlineLocalization")
					{
						// the unit test calling us with that switch does not do code injection, so hook it manually
						AppDomain.CurrentDomain.AssemblyResolve += InjectedResourceLoader.AssemblyResolve;
					}
					// else - the other unit test does resource embedding + code injection, so do not hook the event again

					// test localizations
					SwitchLocale("en");
					if (Translations.Text != "Hello world!")
					{
						Environment.Exit(-1);
					}
					SwitchLocale("fr");
					if (Translations.Text != "Bonjour le monde!")
					{
						Environment.Exit(-2);
					}
					SwitchLocale("de");
					if (Translations.Text != "Hallo Welt!")
					{
						Environment.Exit(-3);
					}
					Environment.Exit(0);
				}
			}
		}

		#endregion Constructors

		#region Properties

		public string Error
		{
			get { return _error; }
			set
			{
				_error = value;
				OnPropertyChanged(() => Error);
			}
		}

		public string Info
		{
			get { return "Switched to: " + (CultureInfo.DefaultThreadCurrentCulture != null ? CultureInfo.DefaultThreadCurrentCulture.TwoLetterISOLanguageName : "<default>"); }
		}

		public string Locale
		{
			get { return _locale; }
			set
			{
				_locale = value;
				OnPropertyChanged(() => Locale);
			}
		}

		public string LocalizedText
		{
			get { return Translations.Text; }
		}

		public ICommand SetLanguage
		{
			get
			{
				return _setLanguage ?? (_setLanguage = new RelayCommand<object>(o => SwitchLocale(Locale), o => !string.IsNullOrEmpty(Locale)));
			}
		}

		#endregion Properties

		#region Methods

		private void SwitchLocale(string culture)
		{
			Error = null;
			if (string.IsNullOrEmpty(culture))
			{
				CultureInfo.DefaultThreadCurrentCulture = null;
				CultureInfo.DefaultThreadCurrentUICulture = null;
				return;
			}
			CultureInfo ci;
			try
			{
				ci = new CultureInfo(culture);
			}
			catch (CultureNotFoundException)
			{
				ci = null;
				Error = "Culture not valid!";
			}
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;

			OnPropertyChanged(() => LocalizedText);
			OnPropertyChanged(() => Info);
		}

		#endregion Methods
	}
}