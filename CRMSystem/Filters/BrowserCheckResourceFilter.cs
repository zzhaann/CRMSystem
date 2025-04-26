using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace CRMSystem.Filters
{
    public class BrowserCheckResourceFilter : IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            //throw new NotImplementedException();
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
            if (Regex.IsMatch(userAgent, "MSIE|Trident"))
            {
                context.Result = new ContentResult
                {
                    Content = "Ваш браузер устарел",
                    ContentType = "text/plain"
                };
            }

        }
    }
}
