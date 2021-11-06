using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSL.Webserver
{
    public static class ReverseProxyServerMiddleware
    {
        public static IApplicationBuilder BasicReverseProxy(this IApplicationBuilder app, string baseURI) =>
            app.Use(async (HttpContext context, Func<Task> next) =>
            await SendToReverseProxy(context, next, baseURI, context.Request.Path + Helpers.WebUtils.BuildAsQueryString(context.Request.Query)));
        public static async Task SendToReverseProxy(HttpContext context, Func<Task> next, string baseURI, string remainingPath)
        {
            HttpClientHandler handler = new HttpClientHandler() { AllowAutoRedirect = false };
            using (HttpClient _httpClient = new HttpClient(handler))
            {
                Uri targetUri = BuildTargetUri(baseURI, remainingPath);

                if (targetUri != null)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        await WebsocketForward(context, targetUri, context.RequestAborted);
                    }
                    else
                    {
                        HttpRequestMessage targetRequestMessage = CreateTargetMessage(context, targetUri);
                        using (HttpResponseMessage? responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                        {
                            context.Response.StatusCode = (int)responseMessage.StatusCode;
                            CopyFromTargetResponseHeaders(context, responseMessage);
                            await responseMessage.Content.CopyToAsync(context.Response.Body);
                        }
                    }
                    return;
                }
            }
            await next.Invoke();
        }

        private static HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            if (context.Request.ContentLength > 0)
            {
                StreamContent streamContent = new StreamContent(context.Request.Body);
                streamContent.Headers.ContentLength = context.Request.ContentLength;
                if (MediaTypeHeaderValue.TryParse(context.Request.ContentType, out MediaTypeHeaderValue? contentType))
                {
                    streamContent.Headers.ContentType = contentType;
                }
                requestMessage.Content = streamContent;
            }

            foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
            {
                string headcompare = header.Key.ToLower();
                //VERY IMPORTANT! PREVENTS SPOOFING SECURITY WRAPPER HEADERS OR IP ADDRESS INFO!
                if (headcompare.Contains("security-wrapper") || headcompare.Contains("securitywrapper") || headcompare.Contains("forwarded")) { continue; }
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            requestMessage.RequestUri = targetUri;

            requestMessage.Method = GetMethod(context.Request.Method);
            requestMessage.Headers.Add("X-Forwarded-For", context.Connection.RemoteIpAddress?.ToString());
            requestMessage.Headers.Add("X-Forwarded-Proto", context.Request.IsHttps ? "https" : "http");
            requestMessage.Headers.Add("X-Forwarded-Host", context.Request.Host.ToUriComponent());
            requestMessage.Headers.Add("Forwarded", "for=" + context.Connection.RemoteIpAddress?.ToString() + ";host=" + context.Request.Host.ToUriComponent() + ";proto=" + (context.Request.IsHttps ? "https" : "http"));
            if (context.User.Identity?.IsAuthenticated == true)
            {
                requestMessage.Headers.Add("security-wrapper-user", context.User.Identity.Name);
                foreach (Claim claim in context.User.Claims)
                {
                    requestMessage.Headers.Add("security-wrapper-" + Helpers.ByteArray.EncodeToHexString(Encoding.UTF8.GetBytes(claim.Type)).ToLower(), claim.Value);
                }
            }
            requestMessage.Headers.Host = targetUri.Host;

            return requestMessage;
        }

        private static void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
            context.Response.Headers.Remove("etag");
        }

        public static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private static Uri BuildTargetUri(string baseURI, string remainingPath) => new Uri(new Uri(baseURI), remainingPath);
        #region Websockets
        private static async Task WebsocketForward(HttpContext context, Uri targetUri, CancellationToken token)
        {
            targetUri = new Uri(targetUri.AbsoluteUri.Replace("https://", "wss://").Replace("http://", "ws://"));
            byte[] clientReceivedData = new byte[8192];
            byte[] serverReceivedData = new byte[8192];
            ArraySegment<byte> cRD = new ArraySegment<byte>(clientReceivedData);
            ArraySegment<byte> sRD = new ArraySegment<byte>(serverReceivedData);
            using (ClientWebSocket serversocket = ConfigureServerWebsocket(context))
            {
                await serversocket.ConnectAsync(targetUri, token);
                using (WebSocket clientsocket = await context.WebSockets.AcceptWebSocketAsync())
                {

                    Task<WebSocketReceiveResult> ClientReceive = clientsocket.ReceiveAsync(cRD, token);
                    Task<WebSocketReceiveResult> ServerReceive = serversocket.ReceiveAsync(sRD, token);
                    while (!ClientReceive.IsFaulted && !ServerReceive.IsFaulted && !ClientReceive.IsCanceled && !ServerReceive.IsCanceled)
                    {
                        try
                        {
                            await Task.WhenAny(ClientReceive, ServerReceive);
                            if (ClientReceive.IsCompleted)
                            {
                                if (await HandleWebsocketReceive(ClientReceive.Result, cRD, serversocket, token))
                                {
                                    ClientReceive = clientsocket.ReceiveAsync(cRD, token);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (ServerReceive.IsCompleted)
                            {
                                if (await HandleWebsocketReceive(ServerReceive.Result, sRD, clientsocket, token))
                                {
                                    ServerReceive = serversocket.ReceiveAsync(sRD, token);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        }
        private static ClientWebSocket ConfigureServerWebsocket(HttpContext context)
        {
            ClientWebSocket serversocket = new ClientWebSocket();
            foreach (string subprotocol in context.WebSockets.WebSocketRequestedProtocols)
            {
                serversocket.Options.AddSubProtocol(subprotocol);
            }
            foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
            {
                string headcompare = header.Key.ToLower();
                //VERY IMPORTANT! PREVENTS SPOOFING SECURITY WRAPPER HEADERS OR IP ADDRESS INFO!
                if (headcompare.Contains("security-wrapper") || headcompare.Contains("securitywrapper") || headcompare.Contains("forwarded") || headcompare.Contains("websocket")) { continue; }
                serversocket.Options.SetRequestHeader(header.Key, header.Value.ToString());
            }
            serversocket.Options.SetRequestHeader("X-Forwarded-For", context.Connection.RemoteIpAddress?.ToString());
            serversocket.Options.SetRequestHeader("X-Forwarded-Proto", context.Request.IsHttps ? "https" : "http");
            serversocket.Options.SetRequestHeader("X-Forwarded-Host", context.Request.Host.ToUriComponent());
            serversocket.Options.SetRequestHeader("Forwarded", "for=" + context.Connection.RemoteIpAddress?.ToString() + ";host=" + context.Request.Host.ToUriComponent() + ";proto=" + (context.Request.IsHttps ? "https" : "http"));
            if (context.User.Identity?.IsAuthenticated == true)
            {
                serversocket.Options.SetRequestHeader("security-wrapper-user", context.User.Identity.Name);
                foreach (Claim claim in context.User.Claims)
                {
                    serversocket.Options.SetRequestHeader("security-wrapper-" + Helpers.ByteArray.EncodeToHexString(Encoding.UTF8.GetBytes(claim.Type)).ToLower(), claim.Value);
                }
            }
            return serversocket;
        }
        private static async Task<bool> HandleWebsocketReceive(WebSocketReceiveResult result, ArraySegment<byte> data, WebSocket other, CancellationToken token)
        {
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await other.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.Empty, result.CloseStatusDescription, token);
                return false;
            }
            //data.Slice(0, result.Count)
            ArraySegment<byte> newSegment = new ArraySegment<byte>(data.Array, data.Offset, result.Count);
            await other.SendAsync(newSegment, result.MessageType, result.EndOfMessage, token);
            return true;
        }
        #endregion
    }
}
