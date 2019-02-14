using LibraryWithTranslations;
using ResourceEmbedder.Core.GeneratedCode;
using System;
using System.Globalization;
using WinFormsTest.Resources;

namespace WinFormsTest
{
    /// <summary>
    /// WinForms JITs the forms code behind on startup-
    /// Costura isn't hooked up yet, so it crashes with a reference not found exception.
    /// By putting the code here, the JIT only sees the method call on startup and offloads
    /// the actual reference resolving to the actual runtime (at which point costura can find the reference).
    /// </summary>
    public class JitDelayHelper
    {
        public void Run(Action<string> callback)
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
                    SwitchLocale("en", callback);
                    if (Translations.Text != "Hello world!")
                    {
                        Environment.Exit(-1);
                    }
                    var lib = new LibraryTranslationHelper();
                    if (lib.GetLanguage("en", false) != "English")
                    {
                        Environment.Exit(-2);
                    }
                    SwitchLocale("fr", callback);
                    if (Translations.Text != "Bonjour le monde!")
                    {
                        Environment.Exit(-3);
                    }
                    // fallback to english as it does not exist
                    if (lib.GetLanguage("fr", false) != "English")
                    {
                        Environment.Exit(-4);
                    }
                    // even when forcing locale change
                    if (lib.GetLanguage("fr", true) != "English")
                    {
                        Environment.Exit(-5);
                    }
                    SwitchLocale("de", callback);
                    if (Translations.Text != "Hallo Welt!")
                    {
                        Environment.Exit(-6);
                    }
                    if (lib.GetLanguage("de", false) != "German")
                    {
                        Environment.Exit(-7);
                    }
                    Environment.Exit(0);
                }
            }
        }

        private void SwitchLocale(string culture, Action<string> callback)
        {
            callback("");
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
                callback("Culture not valid!");
                return;
            }
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;

            callback($"Translation: {Translations.Text}. Switched to: " +
                (CultureInfo.DefaultThreadCurrentCulture != null ? CultureInfo.DefaultThreadCurrentCulture.TwoLetterISOLanguageName : "<default>"));
        }
    }
}
