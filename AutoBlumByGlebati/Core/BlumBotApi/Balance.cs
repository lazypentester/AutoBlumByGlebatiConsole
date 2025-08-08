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
    public static class Balance
    {
        public static async Task<(bool IsSuccess, double? BlumPoints, double? DogsPoints, Exception? Exception)> GetCurrentPointsBalance(Account account, Client accountClient)
        {
            bool status = false;
            double? blumPoints = null;
            double? dogsPoints = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на получение текущего баланса кошелька.", account);

            var balanceData = await accountClient.TryGetAsync(BlumUrl.PointsBalance, headers);

            if (!balanceData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение текущего баланса кошелька.", account, nameof(Balance), nameof(GetCurrentPointsBalance));

                exception = balanceData.Exception;
                return (status, blumPoints, dogsPoints, exception);
            }

            if (balanceData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на получение текущего баланса кошелька.", account, nameof(Balance), nameof(GetCurrentPointsBalance));

                exception = new Exception("balanceData.ResponseContent == null");
                return (status, blumPoints, dogsPoints, exception);
            }

            dynamic? balanceJsonData = JsonConvert.DeserializeObject<dynamic>(balanceData.ResponseContent);

            if (balanceJsonData == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса кошелька.", account, nameof(Balance), nameof(GetCurrentPointsBalance));

                exception = new Exception("balanceJsonData == null");
                return (status, blumPoints, dogsPoints, exception);
            }

            try
            {
                blumPoints = balanceJsonData.points[0].balance;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса кошелька.", account, nameof(Balance), nameof(GetCurrentPointsBalance));

                exception = ex;
                return (status, blumPoints, dogsPoints, exception);
            }

            if (blumPoints == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса кошелька.", account, nameof(Balance), nameof(GetCurrentPointsBalance));

                exception = new Exception("blumPoints == null");
                return (status, blumPoints, dogsPoints, exception);
            }

            Logger.Debug("Запрос на получение текущего баланса кошелька выполнен успешно.", account);

            status = true;

            return (status, blumPoints, dogsPoints, balanceData.Exception);
        }

        public static async Task<(bool IsSuccess, int? PlayPasses, DateTime? TimeNow, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception)> GetCurrentGameBalance(Account account, Client accountClient)
        {
            bool status = false;
            int? playPasses = null;
            DateTime? timeNow = null;
            DateTime? timeStartFarming = null;
            DateTime? timeEndFarming = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на получение текущего баланса игры.", account);

            var balanceData = await accountClient.TryGetAsync(BlumUrl.GameBalance, headers);

            if (!balanceData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = balanceData.Exception;
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            if (balanceData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("balanceData.ResponseContent == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            dynamic? balanceJsonData = JsonConvert.DeserializeObject<dynamic>(balanceData.ResponseContent);

            if (balanceJsonData == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("balanceJsonData == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            try
            {
                playPasses = balanceJsonData.playPasses;

                double milisecs = balanceJsonData.timestamp;
                timeNow = DateTime.UnixEpoch.AddMilliseconds(milisecs);

                milisecs = balanceJsonData.farming.startTime;
                timeStartFarming = DateTime.UnixEpoch.AddMilliseconds(milisecs);

                milisecs = balanceJsonData.farming.endTime;
                timeEndFarming = DateTime.UnixEpoch.AddMilliseconds(milisecs);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = ex;
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            if (playPasses == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("playPasses == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            if (timeNow == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("timeNow == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            if (timeStartFarming == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("timeStartFarming == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            if (timeEndFarming == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение текущего баланса игры.", account, nameof(Balance), nameof(GetCurrentGameBalance));

                exception = new Exception("timeEndFarming == null");
                return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
            }

            Logger.Debug("Запрос на получение текущего баланса игры выполнен успешно.", account);

            status = true;

            return (status, playPasses, timeNow, timeStartFarming, timeEndFarming, exception);
        }
    }
}
