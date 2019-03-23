using System.Globalization;
using System.Threading;
using System.Web.ModelBinding;
using System.Web.Mvc;
using WebFullFramework.Resources;

namespace WebFullFramework.Controllers
{
    public class HomeController : Controller
    {
        public string Index([QueryString] string language)
        {
            if (language != null)
            {
                var ci = new CultureInfo(language);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            return $"Current culture: {Translation.Language}. switch using ?language=<iso>";
        }
    }
}
