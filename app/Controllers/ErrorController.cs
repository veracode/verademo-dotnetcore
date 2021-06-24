using Microsoft.AspNetCore.Mvc;

namespace Verademo.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AccessDenied()
        {  
            return View();
        }
    }
}