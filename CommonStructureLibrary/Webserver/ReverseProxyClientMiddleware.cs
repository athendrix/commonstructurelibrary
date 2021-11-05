using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CSL.Webserver
{
    public static class ReverseProxyClientMiddleware
    {
        public static IApplicationBuilder UseReverseProxyClient(this IApplicationBuilder app) =>
            app.Use((context, next) =>
            {
                IHeaderDictionary Headers = context.Request.Headers;
                bool didanything = false;
                if (Headers.ContainsKey("X-Forwarded-Proto"))
                {
                    if (Headers["X-Forwarded-Proto"].ToString().ToLowerInvariant() == "https")
                    {
                        context.Request.IsHttps = true;
                    }
                    else
                    {
                        context.Request.IsHttps = false;
                    }
                    Headers.Remove("X-Forwarded-Proto");
                    didanything = true;
                }
                if (Headers.ContainsKey("X-Forwarded-Host"))
                {
                    context.Request.Host = new HostString(Headers["X-Forwarded-Host"].ToString());
                    Headers.Remove("X-Forwarded-Host");
                    didanything = true;
                }
                if (Headers.ContainsKey("X-Forwarded-For") && IPAddress.TryParse(Headers["X-Forwarded-For"][0], out IPAddress? address))
                {
                    context.Connection.RemoteIpAddress = address;
                    Headers.Remove("X-Forwarded-For");
                    didanything = true;
                }
                if (Headers.ContainsKey("Forwarded"))
                {
                    Dictionary<string, StringValues> ForwardedValues = new Dictionary<string, StringValues>();
                    string[]? split = Headers["Forwarded"].FirstOrDefault()?.Split(";,".ToCharArray());
                    if (split != null)
                    {
                        foreach (string directive in split)
                        {
                            string[] kvpair = directive.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (kvpair.Length != 2) { continue; }
                            string key = kvpair[0].Trim().ToLowerInvariant();
                            string value = kvpair[1].Trim();
                            if (ForwardedValues.ContainsKey(key))
                            {
                                string[] oldValues = ForwardedValues[key].ToArray();
                                string[] newValues = new string[oldValues.Length + 1];
                                Array.Copy(oldValues, newValues, oldValues.Length);
                                newValues[oldValues.Length] = value;
                                ForwardedValues[key] = new StringValues(newValues);
                            }
                            else
                            {
                                ForwardedValues.Add(key, new StringValues(value));
                            }
                        }
                    }
                    if (ForwardedValues.ContainsKey("for") && IPAddress.TryParse(ForwardedValues["for"][0], out address))
                    {
                        context.Connection.RemoteIpAddress = address;
                    }
                    if (ForwardedValues.ContainsKey("host"))
                    {
                        context.Request.Host = new HostString(ForwardedValues["host"].ToString());
                    }
                    if (ForwardedValues.ContainsKey("proto"))
                    {
                        if (ForwardedValues["proto"].ToString().ToLowerInvariant() == "https")
                        {
                            context.Request.IsHttps = true;
                        }
                        else
                        {
                            context.Request.IsHttps = false;
                        }
                    }
                    Headers.Remove("Forwarded");
                    didanything = true;
                }
                if (!didanything)
                {
                    Console.Error.WriteLine("Warning: No reverse proxy data in headers.");
                    Console.Error.WriteLine("Make sure you're operating behind a reverse proxy.");
                }
                return next.Invoke();
            });
    }
}
