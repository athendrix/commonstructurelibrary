using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CSL.Webserver
{
    public static class ZipHostingMiddleware
    {
        public static void ZipHosting(this IApplicationBuilder app, CachedZipHost czh)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/"))
                {
                    string path = context.Request.Path.Value.Substring(1);//remove first slash to make web paths align with zip paths

                    //Tries to load content using the base path, and if unsuccessful, tries with index.html on the end, and then with /index.html on the end.
                    //If it can't load anything, then we give up and pass onto the next Middleware.
                    if (LoadContent(czh,context, path) || LoadContent(czh,context,path + "index.html") || LoadContent(czh,context,path + "/index.html"))
                    {
                        return;
                    }
                }
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
        }
        private static bool LoadContent(CachedZipHost czh, HttpContext context, string path)
        {
            if(czh.ContainsContent(path))
            {
                context.Response.StatusCode = 200;
                using (MemoryStream ms = czh.GetContent(path))
                {
                    ms.CopyTo(context.Response.Body);
                    return true;
                }
            }
            return false;
        }
    }
}
