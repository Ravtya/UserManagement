using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UserManagement.Filters;

public class RedirectAuthenticatedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity!.IsAuthenticated)
        {
            context.Result = new RedirectResult("/");
        }
    }
}