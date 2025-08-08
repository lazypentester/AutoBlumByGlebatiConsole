using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Tools
{
    public static class IpTool
    {
        public static async Task<(bool IsSuccess, string? Ip, Exception? Exception)> GetMyIp(HttpClient client)
        {
            bool status = false;
            string? ip = null;
            Exception? exception = null;

            try
            {
                ip = await client.GetStringAsync("https://api.ipify.org/");

                if (String.IsNullOrEmpty(ip))
                {
                    throw new Exception("String.IsNullOrEmpty(ip)");
                }

                status = true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Произошла ошибка при получении ip клиента.\n" + ex.Message, nameof(IpTool), nameof(GetMyIp));
                exception = ex;
            }

            return (status, ip, exception);
        }
    }
}
