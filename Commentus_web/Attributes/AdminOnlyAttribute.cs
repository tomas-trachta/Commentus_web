using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Commentus_web.Attributes
{
    public class AdminOnlyAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var result = context.HttpContext.Session.GetInt32("IsAdmin");

            if (result == 0 || result == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
