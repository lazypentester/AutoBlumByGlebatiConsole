using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Core.BlumBotApi
{
    public static class Session
    {
        private static TimeSpan MIN_ACCESS_TOKEN_REMAINING_LIFE_TIME = TimeSpan.FromMinutes(5);
        private static TimeSpan MIN_REFRESH_TOKEN_REMAINING_LIFE_TIME = TimeSpan.FromMinutes(5);

        public static (bool IsSuccess, bool? IsValid, Exception? Exception) IsAccessTokenValid(Account account, DateTime serverTimeNow)
        {
            bool status = false;
            bool? valid = false;
            Exception? exception = null;

            try
            {
                Logger.Debug("Парсим данные о времени жизни access токена.", account);

                (DateTime ExpirationTime, DateTime IssuedAt) parsedTokenLifeTimes = ParseTokenLifeTime(account.AuthBlumAccessToken);

                // Период жизни токена
                TimeSpan CurrentTokenLifespan = parsedTokenLifeTimes.ExpirationTime - parsedTokenLifeTimes.IssuedAt;

                // Сколько времени токен уже прожил
                TimeSpan TokenHasAlreadyLived = serverTimeNow - parsedTokenLifeTimes.IssuedAt;

                // Сколько осталось жить токену
                TimeSpan TokenRemainingLifeTime = CurrentTokenLifespan - TokenHasAlreadyLived;

                if(TokenRemainingLifeTime > MIN_ACCESS_TOKEN_REMAINING_LIFE_TIME)
                {
                    Logger.Debug("Access токен валиден.", account);

                    valid = true;
                }
                
                status = true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Не удалось проверить access токен на валидность.", account, nameof(Session), nameof(IsAccessTokenValid));

                exception = ex;
            }

            return (status, valid, exception);
        }

        public static (bool IsSuccess, bool? IsValid, Exception? Exception) IsRefreshTokenValid(Account account, DateTime serverTimeNow)
        {
            bool status = false;
            bool? valid = false;
            Exception? exception = null;

            try
            {
                Logger.Debug("Парсим данные о времени жизни refresh токена.", account);

                (DateTime ExpirationTime, DateTime IssuedAt) parsedTokenLifeTimes = ParseTokenLifeTime(account.AuthBlumRefreshToken);

                TimeSpan CurrentTokenLifespan = parsedTokenLifeTimes.ExpirationTime - parsedTokenLifeTimes.IssuedAt;

                TimeSpan TokenHasAlreadyLived = serverTimeNow - parsedTokenLifeTimes.IssuedAt;

                TimeSpan TokenRemainingLifeTime = CurrentTokenLifespan - TokenHasAlreadyLived;

                if (TokenRemainingLifeTime > MIN_REFRESH_TOKEN_REMAINING_LIFE_TIME)
                {
                    Logger.Debug("Refresh токен валиден.", account);

                    valid = true;
                }

                status = true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Не удалось проверить refresh токен на валидность.", account, nameof(Session), nameof(IsAccessTokenValid));

                exception = ex;
            }

            return (status, valid, exception);
        }

        private static (DateTime ExpirationTime, DateTime IssuedAt) ParseTokenLifeTime(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            
            DateTime exp = DateTime.UnixEpoch.AddSeconds(jwt.Payload.Expiration!.Value);
            DateTime iat = jwt.Payload.IssuedAt;

            return (exp, iat);
        }

        public static async Task<(bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception)> CreateSession(Account account, Client accountClient)
        {
            bool status = false;
            string? accessToken = null;
            string? refreshToken = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "query", account.AuthTelegramData }
            };

            string? jsonData = JsonConvert.SerializeObject(config);

            Logger.Debug("Выполняется запрос на создание новой сессии.", account);

            var createSessionData = await accountClient.TryPostAsync(BlumUrl.CreateSession, headers, jsonData);

            if (!createSessionData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = createSessionData.Exception;
                return (status, accessToken, refreshToken, exception);
            }

            if (createSessionData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("createSessionData.ResponseContent == null");
                return (status, accessToken, refreshToken, exception);
            }

            dynamic? createSessionJsonData = null;

            try
            {
                createSessionJsonData = JsonConvert.DeserializeObject<dynamic>(createSessionData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = ex;
                return (status, accessToken, refreshToken, exception);
            }

            if (createSessionJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("createSessionJsonData == null");
                return (status, accessToken, refreshToken, exception);
            }

            try
            {
                accessToken = createSessionJsonData.token.access;
                refreshToken = createSessionJsonData.token.refresh;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = ex;
                return (status, accessToken, refreshToken, exception);
            }

            if (accessToken == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("accessToken == null");
                return (status, accessToken, refreshToken, exception);
            }

            if (refreshToken == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание новой сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("refreshToken == null");
                return (status, accessToken, refreshToken, exception);
            }

            Logger.Debug("Запрос на создание новой сессии выполнен успешно.", account);

            status = true;

            return (status, accessToken, refreshToken, exception);
        }

        public static async Task<(bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception)> RefreshSession(Account account, Client accountClient)
        {
            bool status = false;
            string? accessToken = null;
            string? refreshToken = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "refresh", account.AuthBlumRefreshToken }
            };

            string? jsonData = JsonConvert.SerializeObject(config);

            Logger.Debug("Выполняется запрос на обновление сессии.", account);

            var refreshSessionData = await accountClient.TryPostAsync(BlumUrl.RefreshSession, headers, jsonData);

            if (!refreshSessionData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = refreshSessionData.Exception;
                return (status, accessToken, refreshToken, exception);
            }

            if (refreshSessionData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("createSessionData.ResponseContent == null");
                return (status, accessToken, refreshToken, exception);
            }

            dynamic? refreshSessionJsonData = null;

            try
            {
                refreshSessionJsonData = JsonConvert.DeserializeObject<dynamic>(refreshSessionData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = ex;
                return (status, accessToken, refreshToken, exception);
            }

            if (refreshSessionJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("createSessionJsonData == null");
                return (status, accessToken, refreshToken, exception);
            }

            try
            {
                accessToken = refreshSessionJsonData.access;
                refreshToken = refreshSessionJsonData.refresh;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = ex;
                return (status, accessToken, refreshToken, exception);
            }

            if (accessToken == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("accessToken == null");
                return (status, accessToken, refreshToken, exception);
            }

            if (refreshToken == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на обновление сессии.", account, nameof(Session), nameof(CreateSession));

                exception = new Exception("refreshToken == null");
                return (status, accessToken, refreshToken, exception);
            }

            Logger.Debug("Запрос на обновление сессии выполнен успешно.", account);

            status = true;

            return (status, accessToken, refreshToken, exception);
        }
    }
}
