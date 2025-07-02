using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CSL.Webserver
{
    public static class DebugInfoMiddleware
    {
        public static IApplicationBuilder UseDebugInfo(this IApplicationBuilder app, Func<HttpContext,Task<bool>> trigger) =>
            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                if (await trigger(context))
                {
                    StringBuilder debuginfo = new StringBuilder();
                    debuginfo.Append("<!DOCTYPE html><html><head><title>Debug Info</title></head><body>");
                    debuginfo.AppendLine("Username: " + context.User.Identity?.Name ?? "null");
                    debuginfo.AppendLine("Authenticated: " + context.User.Identity?.IsAuthenticated.ToString() ?? "null");
                    debuginfo.AppendLine("Host: " + context.Request.Host.ToUriComponent());
                    debuginfo.AppendLine("Protocol: " + context.Request.Protocol);
                    debuginfo.AppendLine("Protocol2: " + (context.Request.IsHttps ? "https" : "http"));
                    debuginfo.AppendLine("IPAddress: " + context.Connection.RemoteIpAddress?.ToString() ?? "null");
                    debuginfo.AppendLine("Port: " + context.Connection.RemotePort.ToString());

                    debuginfo.AppendLine("Path:" + context.Request.Path);
                    debuginfo.AppendLine("PathBase:" + context.Request.PathBase);
                    debuginfo.AppendLine("QueryString:" + context.Request.QueryString);

                    debuginfo.AppendLine("Cookies: ");
                    foreach (KeyValuePair<string, string> Cookie in context.Request.Cookies)
                    {
                        debuginfo.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;Key: " + Cookie.Key + " Value: " + Cookie.Value);
                    }
                    debuginfo.AppendLine("Headers: ");
                    foreach (KeyValuePair<string, StringValues> Header in context.Request.Headers)
                    {
                        debuginfo.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;Key: " + Header.Key + " Value: " + Header.Value);
                    }
                    debuginfo.AppendLine("Query Values:");
                    string testquery = "";
                    foreach (KeyValuePair<string, StringValues> QueryPair in context.Request.Query)
                    {
                        foreach (string? value in QueryPair.Value)
                        {
                            testquery += "&" + HttpUtility.UrlEncode(QueryPair.Key) + "=" + HttpUtility.UrlEncode(value);
                            debuginfo.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;Key: " + QueryPair.Key + " Value: " + value);
                        }
                    }
                    debuginfo.AppendLine("Claims:");
                    foreach (Claim claim in context.User.Claims)
                    {
                        debuginfo.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;Claim: Security-Wrapper-" + claim.Type + " Value: " + claim.Value);
                    }
                    if (testquery.Length > 0)
                    {
                        testquery = "?" + testquery.Substring(1);
                    }
                    debuginfo.AppendLine("RebuiltQuery:" + testquery);

                    debuginfo.Append("</body></html>");
                    debuginfo.Replace("\n", "<br>");
                    debuginfo.Replace("\r", "");
                    context.Response.ContentType = "text/html";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync(debuginfo.ToString());
                }
                else
                {
                    await next.Invoke();
                }
            });
        
    }
}
