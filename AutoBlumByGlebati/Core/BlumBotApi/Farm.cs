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
    public static class Farm
    {
        public static async Task<(bool IsSuccess, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception)> StartFarm(Account account, Client accountClient)
        {
            bool status = false;
            DateTime? timeStartFarming = null;
            DateTime? timeEndFarming = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на старт фарминга Blum.", account);

            var startFarmData = await accountClient.TryPostAsync(BlumUrl.FarmStart, headers);

            if (!startFarmData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = startFarmData.Exception;
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            if (startFarmData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = new Exception("startFarmData.ResponseContent == null");
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            dynamic? startFarmJsonData = null;

            try
            {
                startFarmJsonData = JsonConvert.DeserializeObject<dynamic>(startFarmData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = ex;
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            if (startFarmJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = new Exception("startFarmJsonData == null");
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            try
            {
                double milisecs = startFarmJsonData.startTime;
                timeStartFarming = DateTime.UnixEpoch.AddMilliseconds(milisecs);

                milisecs = startFarmJsonData.endTime;
                timeEndFarming = DateTime.UnixEpoch.AddMilliseconds(milisecs);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = ex;
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            if (timeStartFarming == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = new Exception("timeStartFarming == null");
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            if (timeEndFarming == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на старт фарминга Blum.", account, nameof(Farm), nameof(StartFarm));

                exception = new Exception("timeEndFarming == null");
                return (status, timeStartFarming, timeEndFarming, exception);
            }

            Logger.Debug("Запрос на старт фарминга Blum выполнен успешно.", account);

            status = true;

            return (status, timeStartFarming, timeEndFarming, exception);
        }

        public static async Task<(bool IsSuccess, DateTime? TimeNow, Exception? Exception)> ClaimFarm(Account account, Client accountClient)
        {
            bool status = false;
            DateTime? timeNow = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на клейм фарминга Blum.", account);

            var claimFarmData = await accountClient.TryPostAsync(BlumUrl.FarmClaim, headers);

            if (!claimFarmData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = claimFarmData.Exception;
                return (status, timeNow, exception);
            }

            if (claimFarmData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = new Exception("claimFarmData.ResponseContent == null");
                return (status, timeNow, exception);
            }

            dynamic? claimFarmJsonData = null;

            try
            {
                claimFarmJsonData = JsonConvert.DeserializeObject<dynamic>(claimFarmData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = ex;
                return (status, timeNow, exception);
            }

            if (claimFarmJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = new Exception("claimFarmJsonData == null");
                return (status, timeNow, exception);
            }

            try
            {
                double milisecs = claimFarmJsonData.timestamp;
                timeNow = DateTime.UnixEpoch.AddMilliseconds(milisecs);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = ex;
                return (status, timeNow, exception);
            }

            if (timeNow == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на клейм фарминга Blum.", account, nameof(Farm), nameof(ClaimFarm));

                exception = new Exception("timeNow == null");
                return (status, timeNow, exception);
            }

            Logger.Debug("Запрос на клейм фарминга Blum выполнен успешно.", account);

            status = true;

            return (status, timeNow, exception);
        }
    }
}
