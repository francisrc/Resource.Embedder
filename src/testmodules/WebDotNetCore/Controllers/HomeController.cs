using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading;
using WebDotNetCore.Resources;

namespace WebDotNetCore.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public string Index([FromQuery] string language)
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
