using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MentalTracker_API.Helpers
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var errorResponse = new
            {
                Message = context.Exception.Message,
                Detail = "inner exception: " + context.Exception.InnerException
            };

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = 400 //BadRequest
            };

            context.ExceptionHandled = true;
        }
    }
}
