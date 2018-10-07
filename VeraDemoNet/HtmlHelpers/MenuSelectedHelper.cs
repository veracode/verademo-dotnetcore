using System.Web.Mvc;

namespace VeraDemoNet.HtmlHelpers
{
    public static class MenuSelectedHelper
    {
        public static string ActivePage(this HtmlHelper helper, string controller, string action)
        {
            var classValue = "";
 
            var currentController = helper.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString();
            var currentAction = helper.ViewContext.Controller.ValueProvider.GetValue("action").RawValue.ToString();
 
            if (currentController == controller && currentAction == action)
            {
                classValue = "selected";
            }
 
            return classValue;
        }
    }
}