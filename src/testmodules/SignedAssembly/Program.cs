using LocalizeHelper;
using System;
using System.Reflection;

namespace SignedAssembly
{
    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            // this assembly was signed via the "Signing" property dialog (which adds "<AssemblyOriginatorKeyFile>Key.snk</AssemblyOriginatorKeyFile>") to the csproj
            var signingKey = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();
            if (signingKey == null || signingKey.Length == 0)
            {
                Environment.Exit(-4);
            }

            Console.WriteLine("Assembly was signed");
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
            Environment.Exit(0);
        }

        #endregion Methods
    }
}
