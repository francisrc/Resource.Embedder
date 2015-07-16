using LocalizeHelper;
using System;

namespace EnglishOnly
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
			// we only localized to english so it should never change
			Localize.SwitchLocale("fr");
			if (Translation.Language != "English")
			{
				Environment.Exit(-2);
			}
			Environment.Exit(0);
		}

		#endregion Methods
	}
}