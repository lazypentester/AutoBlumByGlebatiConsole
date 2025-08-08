using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Services;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Core.BlumBotApi
{
    public static class Me
    {
        public static async Task<(bool IsSuccess, string? Id, string? Username, Exception? Exception)> GetInfoAboutMe(Account account, Client accountClient)
        {
            bool status = false;
            string? id = null;
            string? username = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на получение информации о себе.", account);

            var meData = await accountClient.TryGetAsync(BlumUrl.Me, headers);

            if (!meData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = meData.Exception;
                return (status, id, username, exception);
            }

            if(meData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = new Exception("meData.ResponseContent == null");
                return (status, id, username, exception);
            }

            dynamic? meJsonData = null;

            try
            {
                meJsonData = JsonConvert.DeserializeObject<dynamic>(meData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = ex;
                return (status, id, username, exception);
            }

            if (meJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = new Exception("meJsonData == null");
                return (status, id, username, exception);
            }

            try
            {
                id = meJsonData.id.id;
                username = meJsonData.username;
            }
            catch(Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = ex;
                return (status, id, username, exception);
            }

            if (id == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = new Exception("id == null");
                return (status, id, username, exception);
            }

            if (username == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на получение информации о себе.", account, nameof(Me), nameof(GetInfoAboutMe));

                exception = new Exception("username == null");
                return (status, id, username, exception);
            }

            Logger.Debug("Запрос на получение информации о себе выполнен успешно.", account);

            status = true;

            return (status, id, username, meData.Exception);
        }
    }
}
