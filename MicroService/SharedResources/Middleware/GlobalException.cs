using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedResources.Logs;
using System.Net;
using System.Text.Json;

namespace SharedResources.Middleware
{
    public  class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string message = "ISE occured";
            int statusCode = (int) HttpStatusCode.InternalServerError;
            string title = "Error";


            try
            {
                await next(context);


                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;

                    await ModifyHeader(context,title, message,statusCode);
                }

                //If response is unauthorized

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "Unauthorized";
                    await ModifyHeader(context,title, message, statusCode);
                }

                //If response is Forbidden
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "Forbidden";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }

            }
            catch (Exception ex)
            {

                LogException.LogExceptions(ex);

                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out of Time";
                    message = "Request Timeout";
                    statusCode = StatusCodes.Status408RequestTimeout;

                }

                await ModifyHeader(context, title, message, statusCode);

            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails { 
            
                Detail = message,
                Status = statusCode,
                Title = title
            }),CancellationToken.None);
            return;
        }
    }
}
