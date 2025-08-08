using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Core
{
    public class Client : IDisposable
    {
        private HttpClient httpClient;
        private HttpClientHandler httpClientHandler;
        private CookieContainer cookieContainer;
        private WebProxy? proxy = null;
        private string userAgent;

        public string IpAddress { get; set; } = string.Empty;

        public HttpClient HttpClient
        {
            get
            {
                return httpClient;
            }
        }

        public bool IsProxyInstalled
        {
            get
            {
                return proxy is null ? false : true;
            }
        }

        private bool disposedValue;

        public Client(string userAgent)
        {
            cookieContainer = new CookieContainer();

            httpClientHandler = new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.All
            };

            httpClient = new HttpClient(httpClientHandler, true)
            {
                Timeout = TimeSpan.FromSeconds(120)
            };

            this.userAgent = userAgent;

            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            //httpClient.DefaultRequestHeaders.Add("Origin", "https://telegram.blum.codes");
            //httpClient.DefaultRequestHeaders.Add("Referer", "https://telegram.blum.codes/");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            httpClient.DefaultRequestHeaders.Add("User-Agent", this.userAgent);
            httpClient.DefaultRequestHeaders.Add("accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", $"\"Windows\"");
        }

        public Client(Proxy proxy, string userAgent)
        {
            cookieContainer = new CookieContainer();

            this.proxy = new WebProxy()
            {
                Address = new Uri(proxy.FullProxyAddressString),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

            if (proxy.Username != null && proxy.Password != null)
            {
                this.proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
            }

            httpClientHandler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = this.proxy,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.All
            };

            httpClient = new HttpClient(httpClientHandler, true)
            {
                Timeout = TimeSpan.FromSeconds(120)
            };

            this.userAgent = userAgent;

            //httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru,en;fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7,en-GB;q=0.6,zh-TW;q=0.5,zh-CN;q=0.4,zh;q=0.3");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            httpClient.DefaultRequestHeaders.Add("User-Agent", this.userAgent);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            //httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", $"Windows");
        }

        public async Task<(bool IsSuccess, string? ResponseContent, Exception? Exception)> TryGetAsync(string url, Dictionary<string, List<string>>? headers = null)
        {
            bool status = false;
            string? responseContent = null;
            Exception? exception = null;

            try
            {
                using(var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (request.Headers.Contains(header.Key))
                            {
                                request.Headers.Remove(header.Key);
                            }

                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    var responce = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    
                    if (responce != null && responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        status = true;
                    }
                    else if (responce != null && !responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        exception = new Exception($"responce != null && !responce.IsSuccessStatusCode\nresponce status code == {((int)responce.StatusCode).ToString()} || {responce.StatusCode.ToString()}");
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Warning(ex.Message, nameof(Client), nameof(TryGetAsync));
                exception = ex;
            }

            return (status, responseContent, exception);
        }

        public async Task<(bool IsSuccess, string? ResponseContent, Exception? Exception)> TryPostAsync(string url, Dictionary<string, List<string>>? headers = null, string? jsonData = null)
        {
            bool status = false;
            string? responseContent = null;
            Exception? exception = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (request.Headers.Contains(header.Key))
                            {
                                request.Headers.Remove(header.Key);
                            }

                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    if(jsonData != null)
                    {
                        request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    }

                    var responce = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (responce != null && responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        status = true;
                    }
                    else if (responce != null && !responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        exception = new Exception($"responce != null && !responce.IsSuccessStatusCode\nresponce status code == {((int)responce.StatusCode).ToString()} || {responce.StatusCode.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message, nameof(Client), nameof(TryPostAsync));
                exception = ex;
            }

            return (status, responseContent, exception);
        }
        public async Task<(bool IsSuccess, string? ResponseContent, Exception? Exception)> TryPostAsync(string url, TimeSpan timeout, Dictionary<string, List<string>>? headers = null, string? jsonData = null)
        {
            bool status = false;
            string? responseContent = null;
            Exception? exception = null;

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    CancellationTokenSource cancellationToken = new CancellationTokenSource(timeout);

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (request.Headers.Contains(header.Key))
                            {
                                request.Headers.Remove(header.Key);
                            }

                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    if (jsonData != null)
                    {
                        request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    }

                    var responce = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token);

                    if (responce != null && responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        status = true;
                    }
                    else if (responce != null && !responce.IsSuccessStatusCode)
                    {
                        responseContent = await responce.Content.ReadAsStringAsync();
                        exception = new Exception($"responce != null && !responce.IsSuccessStatusCode\nresponce status code == {((int)responce.StatusCode).ToString()} || {responce.StatusCode.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message, nameof(Client), nameof(TryPostAsync));
                exception = ex;
            }

            return (status, responseContent, exception);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    httpClient.Dispose();
                    proxy = null;
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~Client()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
