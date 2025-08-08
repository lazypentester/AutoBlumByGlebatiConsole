using AutoBlumByGlebati.Core;
using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AutoBlumByGlebati.Models.Proxy;

namespace AutoBlumByGlebati.Tools
{
    public static class ProxyTool
    {
        private static readonly string REIpPattern = @"(((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))(\.)){3}((25[0-5])|(2[0-4][0-9])|(1[0-9][0-9])|([1-9][0-9]|[0-9]))";
        private static readonly string REPortPattern = @"((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9][0-9])|(6[0-4][0-9][0-9][0-9])|([1-5][0-9][0-9][0-9][0-9])|([1-9][0-9][0-9][0-9])|([1-9][0-9][0-9])|([1-9][0-9])|([0-9]))";
        private static readonly string REProxyAddressPattern = @$"({REIpPattern}" + @"(\:)" + @$"{REPortPattern})";

        private static readonly int MAX_COUNT_RETRY_CHECK_IS_PROXY_CONNECTED = 10;

        private static IPAddress? myClearIp = null;
        private static HttpClient clearHttpClient = new HttpClient();

        public static string MyClearIp
        {
            get
            {
                return myClearIp is null ? "" : myClearIp.ToString();
            }
        }

        public static bool TryParseHttpProxy(string proxy, out Proxy? result)
        {
            Logger.Debug("Пытаемся спарсить прокси аккаунта.", nameof(ProxyTool), nameof(TryParseHttpProxy));

            result = null;
            bool parse = false;

            Regex regexAddress = new Regex(REIpPattern);
            Regex regexPort = new Regex(REPortPattern);

            string[] parts = [];

            string? credentials = null;
            string? username = null;
            string? password = null;

            string data = "";
            string address = "";
            int port = 0;

            try
            {
                parts = proxy.Split('@');

                credentials = parts[0];
                username = credentials.Split(':')[0];
                password = credentials.Split(':')[1];

                data = parts[1];
                address = data.Split(":")[0];
                port = Int32.Parse(data.Split(":")[1]);

                if (port > 65535)
                    throw new Exception($"Max port number is '65535', but current value was '{port}'");

                MatchCollection matchesAddress = regexAddress.Matches(address);
                if (matchesAddress.Count != 1)
                {
                    throw new Exception("matchesAddress.Count != 1");
                }
                address = matchesAddress[0].Value;

                MatchCollection matchesPort = regexPort.Matches(port.ToString());
                if (matchesPort.Count != 1)
                {
                    throw new Exception($"matchesPort.Count != 1");
                }
                port = Int32.Parse(matchesPort[0].Value);

                parse = true;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message, nameof(ProxyTool), nameof(TryParseHttpProxy));

                parse = false;
            }

            if (!parse)
            {
                try
                {
                    parts = proxy.Split('@');

                    data = parts[0];
                    address = data.Split(":")[0];
                    port = Int32.Parse(data.Split(":")[1]);

                    credentials = parts[1];
                    username = credentials.Split(':')[0];
                    password = credentials.Split(':')[1];

                    if (port > 65535)
                        throw new Exception($"Max port number is '65535', but current value was '{port}'");

                    MatchCollection matchesAddress = regexAddress.Matches(address);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception("matchesAddress.Count != 1");
                    }
                    address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(port.ToString());
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1");
                    }
                    port = Int32.Parse(matchesPort[0].Value);

                    parse = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex.Message, nameof(ProxyTool), nameof(TryParseHttpProxy));

                    parse = false;
                }
            }

            if (!parse)
            {
                username = null;
                password = null;

                try
                {
                    address = proxy.Split(":")[0];
                    port = Int32.Parse(proxy.Split(":")[1]);

                    if (port > 65535)
                        throw new Exception($"Max port number is '65535', but current value was '{port}'");

                    MatchCollection matchesAddress = regexAddress.Matches(address);
                    if (matchesAddress.Count != 1)
                    {
                        throw new Exception("matchesAddress.Count != 1");
                    }
                    address = matchesAddress[0].Value;

                    MatchCollection matchesPort = regexPort.Matches(port.ToString());
                    if (matchesPort.Count != 1)
                    {
                        throw new Exception($"matchesPort.Count != 1");
                    }
                    port = Int32.Parse(matchesPort[0].Value);

                    parse = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex.Message, nameof(ProxyTool), nameof(TryParseHttpProxy));

                    parse = false;
                }
            }

            if (parse)
            {
                try
                {
                    result = new Proxy(address, port.ToString(), ProxyProtocol.http, username, password);

                    Logger.Debug("Успешно спарсили прокси аккаунта.", nameof(ProxyTool), nameof(TryParseHttpProxy));
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex.Message, nameof(ProxyTool), nameof(TryParseHttpProxy));

                    parse = false;
                    result = null;
                }
            }

            return parse;
        }

        public static async Task<bool> GetMyClearIp()
        {
            //https://api.ipify.org?format=json
            bool getIp = false;

            try
            {
                (bool IsSuccess, string? Ip, Exception? Exception) request = await IpTool.GetMyIp(clearHttpClient);

                getIp = IPAddress.TryParse(request.Ip, out myClearIp);

                if (!getIp)
                {
                    throw new Exception("!getIp");
                }
            }
            catch(Exception ex)
            {
                Logger.Error("Произошла фатальная ошибка при получении ip текущего компьютера.\n" + ex.Message, nameof(ProxyTool), nameof(GetMyClearIp));
            }

            return getIp;
        }

        public static async Task<bool> IsProxyConnected(Client accountClient)
        {
            IPAddress? accountClientProxyIP = null;
            int currentCountRetryProxy = 0;
            bool isProxyConnected = false;

            while (!isProxyConnected && currentCountRetryProxy < MAX_COUNT_RETRY_CHECK_IS_PROXY_CONNECTED)
            {
                try
                {
                    (bool IsSuccess, string? Ip, Exception? Exception) request = await IpTool.GetMyIp(accountClient.HttpClient);

                    if (request.IsSuccess)
                    {
                        IPAddress.TryParse(request.Ip, out accountClientProxyIP);

                        if (accountClientProxyIP?.ToString() != myClearIp?.ToString())
                        {
                            isProxyConnected = true;

                            accountClient.IpAddress = request.Ip!;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex.Message, nameof(ProxyTool), nameof(IsProxyConnected));
                }
                finally
                {
                    currentCountRetryProxy++;
                }
            }

            return isProxyConnected;
        }
    }
}
