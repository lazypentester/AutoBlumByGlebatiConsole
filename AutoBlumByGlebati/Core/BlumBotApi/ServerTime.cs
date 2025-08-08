using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Core.BlumBotApi
{
    public static class ServerTime
    {
        public static async Task<(bool IsSuccess, DateTime? TimeNow, Exception? Exception)> Now(Account account, Client accountClient)
        {
            bool status = false;
            DateTime? timeNow = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на получение информации о текущем времени на сервере.", account);

            var timeNowData = await accountClient.TryGetAsync(BlumUrl.ServerTimeNow, headers);

            if (!timeNowData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = timeNowData.Exception;
                return (status, timeNow, exception);
            }

            if (timeNowData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = new Exception("meData.ResponseContent == null");
                return (status, timeNow, exception);
            }

            dynamic? timeNowJsonData = null;

            try
            {
                timeNowJsonData = JsonConvert.DeserializeObject<dynamic>(timeNowData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = ex;
                return (status, timeNow, exception);
            }

            if (timeNowJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = new Exception("timeNowJsonData == null");
                return (status, timeNow, exception);
            }

            try
            {
                double milisecs = timeNowJsonData.now;

                timeNow = DateTime.UnixEpoch.AddMilliseconds(milisecs);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = ex;
                return (status, timeNow, exception);
            }

            if (timeNow == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о текущем времени на сервере.", account, nameof(ServerTime), nameof(Now));

                exception = new Exception("timeNow == null");
                return (status, timeNow, exception);
            }

            Logger.Debug("Запрос на получение информации о текущем времени на сервере.", account);

            status = true;

            return (status, timeNow, exception);
        }
    }
}
