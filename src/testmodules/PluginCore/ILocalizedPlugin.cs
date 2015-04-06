using System.Globalization;

namespace PluginCore
{
	public interface ILocalizedPlugin
	{
		#region Properties

		/// <summary>
		/// Use to get or set the current locale.
		/// This will automatically reflect in all properties of this instance.
		/// </summary>
		CultureInfo CurrentLocale { get; set; }

		string LocalizedDescription { get; }

		string LocalizedHeader { get; }

		#endregion Properties
	}
}