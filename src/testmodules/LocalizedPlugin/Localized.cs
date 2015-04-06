using LocalizedPlugin.Resources;
using PluginCore;
using System.Globalization;

namespace LocalizedPlugin
{
	public class Localized : ILocalizedPlugin
	{
		#region Constructors

		public Localized()
		{
			CurrentLocale = CultureInfo.DefaultThreadCurrentUICulture;
		}

		#endregion Constructors

		#region Properties

		public CultureInfo CurrentLocale { get; set; }

		public string HeaderLocalizedByThread
		{
			get { return Translations.Header; }
		}

		public string LocalizedDescription
		{
			get { return Translations.ResourceManager.GetString("Description", CurrentLocale); }
		}

		public string LocalizedHeader
		{
			get { return Translations.ResourceManager.GetString("Header", CurrentLocale); }
		}

		#endregion Properties

		#region Methods

		public override string ToString()
		{
			return string.Format("Culture: {0}, Header: {1}, Description: {2}", CurrentLocale != null ? CurrentLocale.ToString() : "<default>", LocalizedHeader, LocalizedDescription);
		}

		#endregion Methods
	}
}