using System.Web.Mvc;

namespace VeraDemoNet.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AccessDenied()  
        {  
            return View();  
        }  
    }
}