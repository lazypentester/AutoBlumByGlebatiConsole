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
    public static class DailyReward
    {
        private static readonly int MAX_COUNT_TRIES_CREATE_ACCOUNT_OFFSET = 5;
        private static readonly int MAX_COUNT_TRIES_CHECK_IN_DATA = 5;
        private static readonly int MAX_COUNT_TRIES_CHECK_IN = 5;

        public static async Task<(bool IsSuccess, bool? CheckIn, Exception? Exception)> CheckIn(Account account, Client accountClient)
        {
            bool status = false;
            bool? checkIn = null;
            Exception? exception = null;

            Logger.Debug("Выполняется запрос на получение данных о дневной награде..", account);

            #region CREATE OFFSET

            (bool IsSuccess, string? Offset, Exception? Exception) offsetRequest = (false, null, null);
            int maxCountTriesCreateAccountOffset = MAX_COUNT_TRIES_CREATE_ACCOUNT_OFFSET;

            while (maxCountTriesCreateAccountOffset > 0 && !offsetRequest.IsSuccess)
            {
                offsetRequest = await GetAccountOffset(account, accountClient);

                if (!offsetRequest.IsSuccess)
                {
                    maxCountTriesCreateAccountOffset--;

                    Logger.Warning("Не удалось создать оффсет для аккаунта..", account);

                    exception = offsetRequest.Exception;

                    await Task.Delay(RandomTool.GetRandomTimeSec(1, 11));

                    Logger.Debug("Повторяем попытку создать оффсет для аккаунта..", account);
                }
            }

            if (!offsetRequest.IsSuccess)
            {
                Logger.Error("Не удалось создать оффсет для аккаунта.", account, nameof(DailyReward), nameof(CheckIn));

                exception = offsetRequest.Exception;
                return (status, checkIn, exception);
            } 

            #endregion

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            #region CHECKIN DATA

            (bool IsSuccess, string? ResponceContent, Exception? Exception) checkInDataRequest = (false, null, null);
            int maxCountTriesCheckInData = MAX_COUNT_TRIES_CHECK_IN_DATA;

            string exeptionMessage = String.Empty;

            while (maxCountTriesCheckInData > 0 && !checkInDataRequest.IsSuccess && !exeptionMessage.Contains("404") && !exeptionMessage.Contains("Not Found"))
            {
                checkInDataRequest = await accountClient.TryGetAsync(BlumUrl.DailyRewardCheckIn + offsetRequest.Offset, headers);

                exeptionMessage = checkInDataRequest.Exception?.Message ?? "";

                if (!checkInDataRequest.IsSuccess && !exeptionMessage.Contains("404") && !exeptionMessage.Contains("NotFound"))
                {
                    maxCountTriesCheckInData--;

                    Logger.Warning("Не удалось отправить запрос на получение данных о дневной награде..", account);

                    exception = checkInDataRequest.Exception;

                    await Task.Delay(RandomTool.GetRandomTimeSec(1, 11));

                    Logger.Debug("Повторяем попытку отправить запрос на получение данных о дневной награде..", account);
                }
            }

            if (!checkInDataRequest.IsSuccess && (exeptionMessage.Contains("404") || exeptionMessage.Contains("Not Found")))
            {
                Logger.Warning("Дневная награда уже была получена ранее.", account);

                status = true;
                checkIn = true;

                return (status, checkIn, exception);
            }
            else if (!checkInDataRequest.IsSuccess)
            {
                Logger.Error("Неудачный запрос на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                return (status, checkIn, exception);
            }

            if (checkInDataRequest.ResponceContent == null)
            {
                Logger.Error("Неудачный запрос на получение данных о дневной награде.checkInDataRequest.ResponceContent == null", account, nameof(DailyReward), nameof(CheckIn));

                exception = checkInDataRequest.Exception;
                return (status, checkIn, exception);
            }

            dynamic? checkInDataJsonData = null;

            try
            {
                checkInDataJsonData = JsonConvert.DeserializeObject<dynamic?>(checkInDataRequest.ResponceContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = ex;
                return (status, checkIn, exception);
            }

            if (checkInDataJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("checkInDataJsonData == null");
                return (status, checkIn, exception);
            }

            string? day = null;
            string? passes = null;
            string? points = null;

            try
            {
                day = (string)checkInDataJsonData.days[1].ordinal;
                passes = (string)checkInDataJsonData.days[1].reward.passes;
                points = (string)checkInDataJsonData.days[1].reward.points;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = ex;
                return (status, checkIn, exception);
            }

            if (String.IsNullOrEmpty(day))
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("String.IsNullOrEmpty(day)");
                return (status, checkIn, exception);
            }

            if (String.IsNullOrEmpty(passes))
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("String.IsNullOrEmpty(passes)");
                return (status, checkIn, exception);
            }

            if (String.IsNullOrEmpty(points))
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("String.IsNullOrEmpty(points)");
                return (status, checkIn, exception);
            }

            #endregion

            Logger.Debug("Выполняется запрос на получение дневной награды..", account);

            #region CHECKIN

            (bool IsSuccess, string? ResponceContent, Exception? Exception) checkInRequest = (false, null, null);
            int maxCountTriesCheckIn = MAX_COUNT_TRIES_CHECK_IN;

            while (maxCountTriesCheckIn > 0 && !checkInRequest.IsSuccess)
            {
                checkInRequest = await accountClient.TryPostAsync(BlumUrl.DailyRewardCheckIn + offsetRequest.Offset, headers);

                if (!checkInRequest.IsSuccess)
                {
                    maxCountTriesCheckIn--;

                    Logger.Warning("Не удалось отправить запрос на получение дневной награды..", account);

                    exception = checkInRequest.Exception;

                    await Task.Delay(RandomTool.GetRandomTimeSec(1, 11));

                    Logger.Debug("Повторяем попытку отправить запрос на получение дневной награды..", account);
                }
            }

            if (!checkInRequest.IsSuccess)
            {
                Logger.Warning("Не удалось отправить запрос на получение дневной награды.", account);

                exception = new Exception("!checkInRequest.IsSuccess");

                return (status, checkIn, exception);
            }

            if (checkInRequest.ResponceContent == null)
            {
                Logger.Error("Неудачный запрос на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("checkInRequest.ResponceContent == null");

                return (status, checkIn, exception);
            }

            if (!checkInRequest.ResponceContent.Contains("OK"))
            {
                Logger.Error("Неудачный запрос на получение данных о дневной награде.", account, nameof(DailyReward), nameof(CheckIn));

                exception = new Exception("!checkInRequest.ResponceContent.Contains(\"OK\")");

                return (status, checkIn, exception);
            } 

            #endregion

            Logger.Debug("Запрос на получение дневной награды выполнен успешно.", account);
            Logger.Success($"Дневная награда успешно получена: [День - {day}, Билеты - {passes}, Очки - {points}]", account);

            status = true;
            checkIn = true;

            return (status, checkIn, exception);
        }

        private static async Task<(bool IsSuccess, string? Offset, Exception? Exception)> GetAccountOffset(Account account, Client accountClient)
        {
            bool status = false;
            string? offset = null;
            Exception? exception = null;

            Logger.Debug("Формирование Offset для аккаунта.", account);
            Logger.Debug("Выполняется запрос на получение времени в текущей временной зоне по ip.", account);

            var timeApiData = await accountClient.TryGetAsync($"https://timeapi.io/api/time/current/ip?ipAddress={accountClient.IpAddress}");

            if (!timeApiData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = timeApiData.Exception;
                return (status, offset, exception);
            }

            if (timeApiData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = new Exception("eligibilityData.ResponseContent == null");
                return (status, offset, exception);
            }

            dynamic? timeApiJsonData = null;

            try
            {
                timeApiJsonData = JsonConvert.DeserializeObject<dynamic>(timeApiData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = ex;
                return (status, offset, exception);
            }

            if (timeApiJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = new Exception("eligibilityJsonData == null");
                return (status, offset, exception);
            }

            try
            {
                DateTime timeOfCurrentTimeZoneByIp = (DateTime)timeApiJsonData.dateTime;
                char offsetSymbol = '-';

                if (timeOfCurrentTimeZoneByIp < DateTime.UtcNow)
                {
                    offsetSymbol = '+';

                    if (DateTime.UtcNow.Day > timeOfCurrentTimeZoneByIp.Day)
                    {
                        offset = ((DateTime.UtcNow.Hour + 24 - timeOfCurrentTimeZoneByIp.Hour) * 60).ToString();
                    }
                    else
                    {
                        offset = ((DateTime.UtcNow.Hour - timeOfCurrentTimeZoneByIp.Hour) * 60).ToString();
                    }
                }
                else
                {
                    if (timeOfCurrentTimeZoneByIp.Day > DateTime.UtcNow.Day)
                    {
                        offset = ((timeOfCurrentTimeZoneByIp.Hour + 24 - DateTime.UtcNow.Hour) * 60).ToString();
                    }
                    else
                    {
                        offset = ((timeOfCurrentTimeZoneByIp.Hour - DateTime.UtcNow.Hour) * 60).ToString();
                    }
                }

                offset = offsetSymbol + offset;

            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = ex;
                return (status, offset, exception);
            }

            if (String.IsNullOrEmpty(offset))
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение времени в текущей временной зоне по ip.", account, nameof(DailyReward), nameof(GetAccountOffset));

                exception = new Exception("String.IsNullOrEmpty(offset)");
                return (status, offset, exception);
            }

            Logger.Debug("Запрос на получение времени в текущей временной зоне по ip выполнен успешно.", account);
            Logger.Debug($"Формирование Offset для аккаунта выполнено успешно. [Offset {offset}]", account);

            status = true;

            return (status, offset, exception);
        }
    }
}
