using System;
using System.Globalization;

namespace DotNetCoreCli
{
    class Program
    {
        static void Main(string[] args)
        {
            // test localizations
            SwitchLocale("en");
            if (Translations.Text != "Hello world!")
            {
                Environment.Exit(-1);
            }
            // fallback to english
            SwitchLocale("pl");
            if (Translations.Text != "Hello world!")
            {
                Environment.Exit(-2);
            }
            SwitchLocale("fr");
            if (Translations.Text != "Bonjour le monde!")
            {
                Environment.Exit(-3);
            }
            SwitchLocale("de");
            if (Translations.Text != "Hallo Welt!")
            {
                Environment.Exit(-4);
            }
            Environment.Exit(0);
        }

        private static void SwitchLocale(string culture)
        {
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
            }
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
        }
    }
}
