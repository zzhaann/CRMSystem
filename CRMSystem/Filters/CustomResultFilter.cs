using Microsoft.AspNetCore.Mvc.Filters;

namespace CRMSystem.Filters
{
    public class CustomResultFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            //throw new NotImplementedException();
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("X-Dashboard-Filter", "Processed by CRMSystem");
        }
    }
}
