using LocalizeHelper;
using System;

namespace DeEnEsJaPlRupt
{
    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            Localize.SwitchLocale("en");
            if (Translation.Language != "English")
            {
                Environment.Exit(-1);
            }
            Localize.SwitchLocale("de");
            if (Translation.Language != "German")
            {
                Environment.Exit(-2);
            }
            Localize.SwitchLocale("pl");
            if (Translation.Language != "Polish")
            {
                Environment.Exit(-3);
            }
            Localize.SwitchLocale("ja");
            if (Translation.Language != "Japanese")
            {
                Environment.Exit(-4);
            }
            Localize.SwitchLocale("ru");
            if (Translation.Language != "Russian")
            {
                Environment.Exit(-5);
            }
            Localize.SwitchLocale("es");
            if (Translation.Language != "Spanish")
            {
                Environment.Exit(-6);
            }
            Localize.SwitchLocale("pt-BR");
            if (Translation.Language != "Portuguese (Brazil)")
            {
                Environment.Exit(-2);
            }
            Environment.Exit(0);
        }

        #endregion Methods
    }
}