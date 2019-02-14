using LocalizeHelper;

namespace LibraryWithTranslations
{
    public class LibraryTranslationHelper
    {
        public string GetLanguage(string culture, bool changeCulture)
        {
            if (changeCulture)
                Localize.SwitchLocale(culture);

            return Translation.Language;
        }
    }
}
