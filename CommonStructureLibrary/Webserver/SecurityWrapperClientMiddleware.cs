using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

using static CSL.Helpers.ByteArray;

namespace CSL.Webserver
{
    public static class SecurityWrapperClientMiddleware
    {
        const string SecurityWrapper = "security-wrapper-";
        public static IApplicationBuilder UseSecurityWrapperClient(this IApplicationBuilder app) => app.Use(async (context, next) =>
        {
            try
            {
                string? username = null;
                List<Claim> claims = new();
                List<string> roles = new();
                foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
                {
                    if (header.Key.StartsWith(SecurityWrapper))
                    {
                        foreach (string? value in header.Value)
                        {
                            if (header.Key == SecurityWrapper + "user")
                            {
                                username = value;
                            }
                            else
                            {
                                try
                                {
                                    string key = Encoding.UTF8.GetString(header.Key.Substring(SecurityWrapper.Length).DecodeFromHexString());
                                    switch (key)
                                    {
                                        case ClaimTypes.Name:
                                            username = value;
                                            break;
                                        case ClaimTypes.Role:
                                            foreach (string role in value?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                                            {
                                                roles.Add(role.Trim().ToLower());
                                            }
                                            break;
                                        default:
                                            claims.Add(new Claim(key, value));
                                            break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                if (username != null)
                {
                    context.User = new GenericPrincipal(new ClaimsIdentity(new GenericIdentity(username, "SecurityWrapper"), claims), roles.ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            await next.Invoke();
        });
        
    }
}
