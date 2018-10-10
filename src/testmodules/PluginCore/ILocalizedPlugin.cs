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

        /// <summary>
        /// Ignores the value of <see cref="CurrentLocale"/> and instead localized to whichever
        /// locale the current thread has.
        /// </summary>
        string HeaderLocalizedByThread { get; }

        string LocalizedDescription { get; }

        string LocalizedHeader { get; }

        #endregion Properties
    }
}