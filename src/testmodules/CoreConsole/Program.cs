using System;
using System.Globalization;

namespace CoreConsole
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
            if (GetLanguage("en", false) != "English")
            {
                Environment.Exit(-2);
            }
            SwitchLocale("fr");
            if (Translations.Text != "Bonjour le monde!")
            {
                Environment.Exit(-3);
            }
            // fallback to english as it does not exist
            if (GetLanguage("fr", false) != "English")
            {
                Environment.Exit(-4);
            }
            // even when forcing locale change
            if (GetLanguage("fr", true) != "English")
            {
                Environment.Exit(-5);
            }
            SwitchLocale("de");
            if (Translations.Text != "Hallo Welt!")
            {
                Environment.Exit(-6);
            }
            if (GetLanguage("de", false) != "German")
            {
                Environment.Exit(-7);
            }
            Environment.Exit(0);
        }
        private static string GetLanguage(string culture, bool changeCulture)
        {
            if (changeCulture)
                SwitchLocale(culture);

            return Translations.Text;
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
