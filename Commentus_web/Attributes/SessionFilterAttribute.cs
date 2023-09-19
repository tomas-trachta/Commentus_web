using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Commentus_web.Attributes
{
    public class SessionFilterAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var result = context.HttpContext.Session.GetInt32("IsLoggedIn");

            if(result == 0 || result == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
