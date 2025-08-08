using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static AutoBlumByGlebati.Core.BlumBotApi.Game;
using static AutoBlumByGlebati.Models.GameBugConfig;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoBlumByGlebati.Core.BlumBotApi
{
    public static class Game
    {
        private static Client _httpClientForPayloadLocalServer = new Client("My software useragent");

        private static readonly int MAX_COUNT_TRIES_START_GAME = 5;
        private static readonly int MAX_COUNT_TRIES_CREATE_PAYLOAD = 8;
        private static readonly int MAX_COUNT_TRIES_END_GAME = 10;
        private enum PAYLOAD_SERVER
        {
            _1 = 1,
            _2
        }

        public static async Task<(bool IsSuccess, Exception? Exception)> PlayGame(Account account, Client accountClient, GameType gameType, bool eligibilityDogs)
        {
            bool status = false;
            Exception? exception = null;

            if (gameType == GameType.Normal)
            {
                Logger.Debug("Запускаем нормальную игру.", account);

                #region START GAME

                (bool IsSuccess, string? GameId, Exception? Exception) startGame = (false, null, null);
                int maxCountTriesStartGame = MAX_COUNT_TRIES_START_GAME;

                while (maxCountTriesStartGame > 0 && !startGame.IsSuccess)
                {
                    startGame = await StartGame(account, accountClient);

                    if (!startGame.IsSuccess)
                    {
                        maxCountTriesStartGame--;

                        Logger.Warning("Не удалось запустить игру..", account);

                        exception = startGame.Exception;

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 11));

                        Logger.Debug("Повторяем попытку запустить игру..", account);
                    }
                }

                if (!startGame.IsSuccess)
                {
                    Logger.Error("Не удалось запустить игру - !startGame.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = startGame.Exception;
                    return (status, exception);
                } 

                #endregion

                await Task.Delay(RandomTool.GetRandomTimeSec(30, 50));

                int blumPoints = RandomTool.GetRandomNumInt(130, 240);
                int dogsPoints = 0;

                if (eligibilityDogs)
                {
                    blumPoints = RandomTool.GetRandomNumInt(90, 110);
                    dogsPoints = (RandomTool.GetRandomNumInt(10, 20) * 5);
                }

                Logger.Debug("Создаем зашифрованную полезную нагрузку для отправки результата игры.", account);

                #region CREATE PAYLOAD ALGORITHM

                //PAYLOAD_SERVER current_payload_server = (PAYLOAD_SERVER)RandomTool.GetRandomNumInt(1, 3);
                int countTriesCreatePayload = MAX_COUNT_TRIES_CREATE_PAYLOAD;

                (bool IsSuccess, string? Hash, Exception? Exception) createPayload = (false, null, null);

                while (countTriesCreatePayload > 0 && !createPayload.IsSuccess)
                {
                    //switch (current_payload_server)
                    //{
                    //    case PAYLOAD_SERVER._1:
                    //        // SERVER #2
                    //        createPayload = await CreateResultPayload2(account, accountClient, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayload2, gameType, TimeSpan.FromSeconds(5));
                    //        if (!createPayload.IsSuccess)
                    //        {
                    //            countTriesCreatePayload--;

                    //            Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #2)", account, nameof(Game), nameof(PlayGame));
                    //        }
                    //        break;
                    //    case PAYLOAD_SERVER._2:
                    //        // SERVER #3
                    //        createPayload = await CreateResultPayload3(account, accountClient, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayload3, gameType, TimeSpan.FromSeconds(5));

                    //        if (!createPayload.IsSuccess)
                    //        {
                    //            countTriesCreatePayload--;

                    //            Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #3)", account, nameof(Game), nameof(PlayGame));
                    //        }
                    //        break;
                    //}

                    //if (current_payload_server == PAYLOAD_SERVER._1)
                    //{
                    //    current_payload_server = PAYLOAD_SERVER._2;
                    //}
                    //else if (current_payload_server == PAYLOAD_SERVER._2)
                    //{
                    //    current_payload_server = PAYLOAD_SERVER._1;
                    //}

                    createPayload = await CreateResultPayloadFromLocalServer(account, _httpClientForPayloadLocalServer, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayloadFromLocalServer, gameType, TimeSpan.FromSeconds(5));
                    if (!createPayload.IsSuccess)
                    {
                        countTriesCreatePayload--;

                        Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #LOCAL)", account, nameof(Game), nameof(PlayGame));
                    }

                }

                if (!createPayload.IsSuccess)
                {
                    Logger.Error("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. !createPayload.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = createPayload.Exception;
                    return (status, exception);
                }

                #endregion

                Logger.Debug("Завершаем нормальную игру.", account);

                #region END GAME

                (bool IsSuccess, string? Message, Exception? Exception) endGame = (false, null, null);
                int maxCountTriesEndGame = MAX_COUNT_TRIES_END_GAME;

                while (maxCountTriesEndGame > 0 && (!endGame.IsSuccess || !endGame.Message!.ToLower().Contains("ok")))
                {
                    endGame = await ClaimNormalGame(account, accountClient, createPayload.Hash!);

                    if (!endGame.IsSuccess || !endGame.Message!.ToLower().Contains("ok"))
                    {
                        maxCountTriesEndGame--;

                        Logger.Warning("Не удалось завершить нормальную игру...", account);

                        if (endGame.IsSuccess)
                        {
                            Logger.Warning($"Запрос на завершение игры с багом был выполнен успешно, но не содержал ключевой фразы, MESSAGE ==> {endGame.Message}" + endGame.Message!, account);
                        }

                        exception = endGame.Exception;

                        Logger.Debug("Повторяем попытку завершить игру..", account);
                    }
                }

                if (!endGame.IsSuccess)
                {
                    Logger.Error("Не удалось завершить нормальную игру. !endGame.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = endGame.Exception;
                    return (status, exception);
                }

                if (!endGame.Message!.ToLower().Contains("ok"))
                {
                    Logger.Error("Не удалось завершить нормальную игру. !endGame.Message!.ToLower().Contains(\"ok\")", account, nameof(Game), nameof(PlayGame));

                    exception = endGame.Exception;
                    return (status, exception);
                } 

                #endregion
            }
            else if(gameType == GameType.WithBug)
            {
                Logger.Debug("Запускаем игру с багом.", account);

                #region START GAME

                (bool IsSuccess, string? GameId, Exception? Exception) startGame = (false, null, null);
                int maxCountTriesStartGame = MAX_COUNT_TRIES_START_GAME;

                while (maxCountTriesStartGame > 0 && !startGame.IsSuccess)
                {
                    startGame = await StartGame(account, accountClient);

                    if (!startGame.IsSuccess)
                    {
                        maxCountTriesStartGame--;

                        Logger.Warning("Не удалось запустить игру..", account);

                        exception = startGame.Exception;

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 11));

                        Logger.Debug("Повторяем попытку запустить игру..", account);
                    }
                }

                if (!startGame.IsSuccess)
                {
                    Logger.Error("Не удалось запустить игру - !startGame.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = startGame.Exception;
                    return (status, exception);
                } 

                #endregion

                await Task.Delay(RandomTool.GetRandomTimeSec(30, 50));

                int blumPoints = RandomTool.GetRandomNumInt(130, 240);
                int dogsPoints = 0;

                if (eligibilityDogs)
                {
                    blumPoints = RandomTool.GetRandomNumInt(90, 110);
                    dogsPoints = (RandomTool.GetRandomNumInt(10, 20) * 5);
                }

                Logger.Debug("Создаем зашифрованную полезную нагрузку для отправки результата игры.", account);

                #region CREATE PAYLOAD ALGORITHM

                //PAYLOAD_SERVER current_payload_server = (PAYLOAD_SERVER)RandomTool.GetRandomNumInt(1, 3);
                int countTriesCreatePayload = MAX_COUNT_TRIES_CREATE_PAYLOAD;

                (bool IsSuccess, string? Hash, Exception? Exception) createPayload = (false, null, null);

                while(countTriesCreatePayload > 0 && !createPayload.IsSuccess)
                {
                    //switch (current_payload_server)
                    //{
                    //    case PAYLOAD_SERVER._1:
                    //        // SERVER #2
                    //        createPayload = await CreateResultPayload2(account, accountClient, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayload2, gameType, TimeSpan.FromSeconds(5));
                    //        if (!createPayload.IsSuccess)
                    //        {
                    //            countTriesCreatePayload--;

                    //            Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #2)", account, nameof(Game), nameof(PlayGame));
                    //        }
                    //        break;
                    //    case PAYLOAD_SERVER._2:
                    //        // SERVER #3
                    //        createPayload = await CreateResultPayload3(account, accountClient, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayload3, gameType, TimeSpan.FromSeconds(5));

                    //        if (!createPayload.IsSuccess)
                    //        {
                    //            countTriesCreatePayload--;

                    //            Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #3)", account, nameof(Game), nameof(PlayGame));
                    //        }
                    //        break;
                    //}

                    //if(current_payload_server == PAYLOAD_SERVER._1)
                    //{
                    //    current_payload_server = PAYLOAD_SERVER._2;
                    //}
                    //else if (current_payload_server == PAYLOAD_SERVER._2)
                    //{
                    //    current_payload_server = PAYLOAD_SERVER._1;
                    //}

                    createPayload = await CreateResultPayloadFromLocalServer(account, _httpClientForPayloadLocalServer, (startGame.GameId!, blumPoints, dogsPoints), PayloadUrl.CreateEncyptedPayloadFromLocalServer, gameType, TimeSpan.FromSeconds(5));
                    if (!createPayload.IsSuccess)
                    {
                        countTriesCreatePayload--;

                        Logger.Warning("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. (SERVER #LOCAL)", account, nameof(Game), nameof(PlayGame));
                    }
                }

                if (!createPayload.IsSuccess)
                {
                    Logger.Error("Не удалось создать зашифрованную полезную нагрузку для отправки результата игры. !createPayload.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = createPayload.Exception;
                    return (status, exception);
                }

                #endregion

                Logger.Debug("Завершаем игру с багом.", account);

                #region END GAME

                (bool IsSuccess, string? Message, Exception? Exception) endGame = (false, null, null);
                int maxCountTriesEndGame = MAX_COUNT_TRIES_END_GAME;

                //while (maxCountTriesEndGame > 0 && (!endGame.IsSuccess || endGame.Message != "cannot update game result"))
                while (maxCountTriesEndGame > 0 && (!endGame.IsSuccess || endGame.Message != "invalid earned asset code"))
                {
                    endGame = await ClaimBugGame(account, accountClient, createPayload.Hash!);

                    //if (!endGame.IsSuccess || endGame.Message != "cannot update game result")
                    if (!endGame.IsSuccess || endGame.Message != "invalid earned asset code")
                    {
                        maxCountTriesEndGame--;

                        Logger.Warning("Не удалось завершить игру с багом...", account);

                        if (endGame.IsSuccess)
                        {
                            Logger.Warning($"Запрос на завершение игры с багом был выполнен успешно, но не содержал ключевой фразы, MESSAGE ==> {endGame.Message}" + endGame.Message!, account);
                        }

                        exception = endGame.Exception;

                        Logger.Debug("Повторяем попытку завершить игру..", account);
                    }
                }

                if (!endGame.IsSuccess)
                {
                    Logger.Error("Не удалось завершить игру с багом. !endGame.IsSuccess", account, nameof(Game), nameof(PlayGame));

                    exception = endGame.Exception;
                    return (status, exception);
                }

                //if (endGame.Message != "cannot update game result")
                if (endGame.Message != "invalid earned asset code")
                {
                    Logger.Error("Не удалось завершить игру с багом. endGame.Message != \"invalid earned asset code\"", account, nameof(Game), nameof(PlayGame));

                    exception = endGame.Exception;
                    return (status, exception);
                } 

                #endregion
            }

            status = true;

            return (status, exception);
        }

        public static async Task<(bool IsSuccess, bool? Eligibility, Exception? Exception)> GetEligibilityDogs(Account account, Client accountClient)
        {
            bool status = false;
            bool? eligibility = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на возможность фарма Dogs.", account);

            var eligibilityData = await accountClient.TryGetAsync(BlumUrl.EligibilityDogsDrop, headers);

            if (!eligibilityData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = eligibilityData.Exception;
                return (status, eligibility, exception);
            }

            if (eligibilityData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = new Exception("eligibilityData.ResponseContent == null");
                return (status, eligibility, exception);
            }

            dynamic? eligibilityJsonData = null;

            try
            {
                eligibilityJsonData = JsonConvert.DeserializeObject<dynamic>(eligibilityData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = ex;
                return (status, eligibility, exception);
            }

            if (eligibilityJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = new Exception("eligibilityJsonData == null");
                return (status, eligibility, exception);
            }

            try
            {
                eligibility = eligibilityJsonData.eligible;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = ex;
                return (status, eligibility, exception);
            }

            if (eligibility == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на возможность фарма Dogs.", account, nameof(Game), nameof(GetEligibilityDogs));

                exception = new Exception("eligibility == null");
                return (status, eligibility, exception);
            }

            Logger.Debug("Запрос на возможность фарма Dogs выполнен успешно.", account);

            status = true;

            return (status, eligibility, exception);
        }

        private static async Task<(bool IsSuccess, string? GameId, Exception? Exception)> StartGame(Account account, Client accountClient)
        {
            bool status = false;
            string? gameId = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на начало новой игры.", account);

            var startGameData = await accountClient.TryPostAsync(BlumUrl.GameStart, headers);

            if (!startGameData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = startGameData.Exception;
                return (status, gameId, exception);
            }

            if (startGameData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = new Exception("startGameData.ResponseContent == null");
                return (status, gameId, exception);
            }

            dynamic? startGameJsonData = null;

            try
            {
                startGameJsonData = JsonConvert.DeserializeObject<dynamic>(startGameData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = ex;
                return (status, gameId, exception);
            }

            if (startGameJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = new Exception("startGameJsonData == null");
                return (status, gameId, exception);
            }

            try
            {
                gameId = startGameJsonData.gameId;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = ex;
                return (status, gameId, exception);
            }

            if (gameId == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на начало новой игры.", account, nameof(Game), nameof(StartGame));

                exception = new Exception("gameId == null");
                return (status, gameId, exception);
            }

            Logger.Debug("Запрос на начало новой игры выполнен успешно.", account);

            status = true;

            return (status, gameId, exception);
        }

        private static async Task<(bool IsSuccess, string? Hash, Exception? Exception)> CreateResultPayload(Account account, Client accountClient, (string GameId, int BlumPoints, int DogsPoints) data, string requestUrl, GameType gameType, TimeSpan timeout)
        {
            bool status = false;
            string? hash = null;
            Exception? exception = null;

            Logger.Debug("Выполняется запрос на создание payload.", account);

            //Dictionary<string, string> config = new Dictionary<string, string>()
            //{
            //    { "gameId", data.GameId },
            //    { "points", data.BlumPoints.ToString() },
            //    { "dogs", data.DogsPoints.ToString() }
            //};

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "gameId", data.GameId },
                { "points", data.BlumPoints.ToString() }
            };

            if(gameType == GameType.WithBug)
            {
                config.Add("dogs", "0");
            }

            string? jsonData = JsonConvert.SerializeObject(config);

            if(jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var createPayloadData = await accountClient.TryPostAsync(requestUrl, timeout: timeout, jsonData: jsonData);

            if (!createPayloadData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = createPayloadData.Exception;
                return (status, hash, exception);
            }

            if (createPayloadData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = new Exception("createPayloadData.ResponseContent == null");
                return (status, hash, exception);
            }

            dynamic? createPayloadJsonData = null;

            try
            {
                createPayloadJsonData = JsonConvert.DeserializeObject<dynamic>(createPayloadData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = ex;
                return (status, hash, exception);
            }

            if (createPayloadJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = new Exception("createPayloadJsonData == null");
                return (status, hash, exception);
            }

            try
            {
                hash = createPayloadJsonData.payload;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = ex;
                return (status, hash, exception);
            }

            if (hash == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload));

                exception = new Exception("hash == null");
                return (status, hash, exception);
            }

            Logger.Debug("Запрос на создание payload выполнен успешно.", account);

            status = true;

            return (status, hash, exception);
        }

        private static async Task<(bool IsSuccess, string? Hash, Exception? Exception)> CreateResultPayload2(Account account, Client accountClient, (string GameId, int BlumPoints, int DogsPoints) data, string requestUrl, GameType gameType, TimeSpan timeout)
        {
            bool status = false;
            string? hash = null;
            Exception? exception = null;

            Logger.Debug("Выполняется запрос на создание payload.", account);

            //Dictionary<string, object> config = new Dictionary<string, object>()
            //{
            //    { "gameId", data.GameId },
            //    { "earnedAssets",
            //        new Dictionary<string, object>()
            //        {
            //            { "CLOVER",
            //                new Dictionary<string, string>()
            //                {
            //                    { "amount", data.BlumPoints.ToString() }
            //                }
            //            },
            //            { "DOGS",
            //                new Dictionary<string, string>()
            //                {
            //                    { "amount", data.DogsPoints.ToString() }
            //                }
            //            }
            //        }
            //    }
            //};

            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                { "gameId", data.GameId }
            };

            if (gameType == GameType.WithBug)
            {
                config.Add("earnedAssets", new Dictionary<string, object>()
                    {
                        { "CLOVER",
                            new Dictionary<string, string>()
                            {
                                { "amount", data.BlumPoints.ToString() }
                            }
                        },
                        { "DOGS",
                            new Dictionary<string, string>()
                            {
                                { "amount", "0" }
                            }
                        }
                    });
            }
            else if(gameType == GameType.Normal)
            {
                config.Add("earnedAssets", new Dictionary<string, object>()
                    {
                        { "CLOVER",
                            new Dictionary<string, string>()
                            {
                                { "amount", data.BlumPoints.ToString() }
                            }
                        }
                    });
            }

            string? jsonData = JsonConvert.SerializeObject(config);

            if (jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var createPayloadData = await accountClient.TryPostAsync(requestUrl, jsonData: jsonData);

            if (!createPayloadData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = createPayloadData.Exception;
                return (status, hash, exception);
            }

            if (createPayloadData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("createPayloadData.ResponseContent == null");
                return (status, hash, exception);
            }

            dynamic? createPayloadJsonData = null;

            try
            {
                createPayloadJsonData = JsonConvert.DeserializeObject<dynamic>(createPayloadData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = ex;
                return (status, hash, exception);
            }

            if (createPayloadJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("createPayloadJsonData == null");
                return (status, hash, exception);
            }

            try
            {
                hash = createPayloadJsonData.pack.hash;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = ex;
                return (status, hash, exception);
            }

            if (hash == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("hash == null");
                return (status, hash, exception);
            }

            Logger.Debug("Запрос на создание payload выполнен успешно.", account);

            status = true;

            return (status, hash, exception);
        }

        private static async Task<(bool IsSuccess, string? Hash, Exception? Exception)> CreateResultPayload3(Account account, Client accountClient, (string GameId, int BlumPoints, int DogsPoints) data, string requestUrl, GameType gameType, TimeSpan timeout)
        {
            bool status = false;
            string? hash = null;
            Exception? exception = null;

            Logger.Debug("Выполняется запрос на создание payload.", account);

            //Dictionary<string, string> config = new Dictionary<string, string>()
            //{
            //    { "gameId", data.GameId },
            //    { "points", data.BlumPoints.ToString() },
            //    { "dogs", data.DogsPoints.ToString() }
            //};

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "gameId", data.GameId },
                { "points", data.BlumPoints.ToString() }
            };

            if (gameType == GameType.WithBug)
            {
                config.Add("dogs", "0");
            }

            string? jsonData = JsonConvert.SerializeObject(config);

            if (jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var createPayloadData = await accountClient.TryPostAsync(requestUrl, jsonData: jsonData);

            if (!createPayloadData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = createPayloadData.Exception;
                return (status, hash, exception);
            }

            if (createPayloadData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("createPayloadData.ResponseContent == null");
                return (status, hash, exception);
            }

            dynamic? createPayloadJsonData = null;

            try
            {
                createPayloadJsonData = JsonConvert.DeserializeObject<dynamic>(createPayloadData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = ex;
                return (status, hash, exception);
            }

            if (createPayloadJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("createPayloadJsonData == null");
                return (status, hash, exception);
            }

            try
            {
                hash = createPayloadJsonData.payload;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = ex;
                return (status, hash, exception);
            }

            if (hash == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayload2));

                exception = new Exception("hash == null");
                return (status, hash, exception);
            }

            Logger.Debug("Запрос на создание payload выполнен успешно.", account);

            status = true;

            return (status, hash, exception);
        }
        private static async Task<(bool IsSuccess, string? Hash, Exception? Exception)> CreateResultPayloadFromLocalServer(Account account, Client httpClient, (string GameId, int BlumPoints, int DogsPoints) data, string requestUrl, GameType gameType, TimeSpan timeout)
        {
            bool status = false;
            string? hash = null;
            Exception? exception = null;

            Logger.Debug("Выполняется запрос на создание payload.", account);

            int FREEZE = data.BlumPoints / 50 + (int)(Random.Shared.NextDouble() * 2); // count / 50 + 1|0
            int BOMB = data.BlumPoints < 150 ? (int)(Random.Shared.NextDouble() * 2) + (int)(Random.Shared.NextDouble() * 2) : 0; // if points are lower than 150, will add from 0 to 2 bombs clicked

            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                { "gameId", data.GameId }
            };

            if (gameType == GameType.WithBug)
            {
                config.Add("earnedPoints", new Dictionary<string, object>()
                    {
                        { "BP",
                            new Dictionary<string, int>()
                            {
                                { "amount", data.BlumPoints }
                            }
                        },
                        { "bp",
                            new Dictionary<string, int>()
                            {
                                { "amount", 0 }
                            }
                        }
                    });
                config.Add("assetClicks", new Dictionary<string, object>()
                    {
                        { "CLOVER",
                            new Dictionary<string, int>()
                            {
                                { "clicks", data.BlumPoints }
                            }
                        },
                        { "DOGS",
                            new Dictionary<string, int>()
                            {
                                { "clicks", 0 }
                            }
                        },
                        { "FREEZE",
                            new Dictionary<string, int>()
                            {
                                { "clicks", FREEZE }
                            }
                        },
                        { "BOMB",
                            new Dictionary<string, int>()
                            {
                                { "clicks", BOMB }
                            }
                        }
                    });
            }
            else if (gameType == GameType.Normal)
            {
                config.Add("earnedPoints", new Dictionary<string, object>()
                    {
                        { "BP",
                            new Dictionary<string, int>()
                            {
                                { "amount", data.BlumPoints }
                            }
                        }
                    });
                config.Add("assetClicks", new Dictionary<string, object>()
                    {
                        { "CLOVER",
                            new Dictionary<string, int>()
                            {
                                { "clicks", data.BlumPoints }
                            }
                        },
                        { "FREEZE",
                            new Dictionary<string, int>()
                            {
                                { "clicks", FREEZE }
                            }
                        },
                        { "BOMB",
                            new Dictionary<string, int>()
                            {
                                { "clicks", BOMB }
                            }
                        }
                    });
            }

            string? jsonData = JsonConvert.SerializeObject(config);

            if (jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var createPayloadData = await httpClient.TryPostAsync(requestUrl, jsonData: jsonData);

            if (!createPayloadData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = createPayloadData.Exception;
                return (status, hash, exception);
            }

            if (createPayloadData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = new Exception("createPayloadData.ResponseContent == null");
                return (status, hash, exception);
            }

            dynamic? createPayloadJsonData = null;

            try
            {
                createPayloadJsonData = JsonConvert.DeserializeObject<dynamic>(createPayloadData.ResponseContent);
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = ex;
                return (status, hash, exception);
            }

            if (createPayloadJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = new Exception("createPayloadJsonData == null");
                return (status, hash, exception);
            }

            try
            {
                hash = createPayloadJsonData.payload;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = ex;
                return (status, hash, exception);
            }

            if (hash == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на создание payload.", account, nameof(Game), nameof(CreateResultPayloadFromLocalServer));

                exception = new Exception("hash == null");
                return (status, hash, exception);
            }

            Logger.Debug("Запрос на создание payload выполнен успешно.", account);

            status = true;

            return (status, hash, exception);
        }

        private static async Task<(bool IsSuccess, string? Message, Exception? Exception)> ClaimNormalGame(Account account, Client accountClient, string hash)
        {
            bool status = false;
            string? message = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на завершение игры.", account);

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "payload", hash }
            };

            string? jsonData = JsonConvert.SerializeObject(config);

            if (jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на завершение игры.", account, nameof(Game), nameof(ClaimNormalGame));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var endGameData = await accountClient.TryPostAsync(BlumUrl.GameClaim, headers, jsonData);

            if (!endGameData.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на завершение игры.", account, nameof(Game), nameof(ClaimNormalGame));

                exception = endGameData.Exception;
                return (status, message, exception);
            }

            if (endGameData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на завершение игры.", account, nameof(Game), nameof(ClaimNormalGame));

                exception = new Exception("endGameData.ResponseContent == null");
                return (status, message, exception);
            }

            message = endGameData.ResponseContent;

            if (message == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на завершение игры.", account, nameof(Game), nameof(ClaimNormalGame));

                exception = new Exception("message == null");
                return (status, message, exception);
            }

            Logger.Debug("Запрос на завершение игры выполнен успешно.", account);

            status = true;

            return (status, message, exception);
        }

        private static async Task<(bool IsSuccess, string? Message, Exception? Exception)> ClaimBugGame(Account account, Client accountClient, string hash)
        {
            bool status = false;
            string? message = null;
            Exception? exception = null;

            var headers = new Dictionary<string, List<string>>
            {
                { "Authorization", new List<string> { $"Bearer {account.AuthBlumAccessToken}" } },
                { "Origin", new List<string> { "https://telegram.blum.codes" } }
            };

            Logger.Debug("Выполняется запрос на завершение игры.", account);

            Dictionary<string, string> config = new Dictionary<string, string>()
            {
                { "payload", hash }
            };

            string? jsonData = JsonConvert.SerializeObject(config);

            if (jsonData == null)
            {
                Logger.Warning("Неудачное создание jsonData при формировании запроса на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

                exception = new Exception("jsonData == null");
                return (status, hash, exception);
            }

            var endGameData = await accountClient.TryPostAsync(BlumUrl.GameClaim, headers, jsonData);

            //if (!endGameData.IsSuccess)
            //{
            //    Logger.Error("Неудачный запрос на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

            //    exception = endGameData.Exception;
            //    return (status, message, exception);
            //}

            if (endGameData.ResponseContent == null)
            {
                Logger.Warning("Неудачный запрос на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

                exception = new Exception("endGameData.ResponseContent == null");
                return (status, message, exception);
            }

            dynamic? endGameJsonData = null;

            try
            {
                endGameJsonData = JsonConvert.DeserializeObject<dynamic>(endGameData.ResponseContent);
            }
            catch (Exception ex)
            {
                if(endGameData.ResponseContent.Contains("unknown connection issue"))
                {
                    Logger.Warning($"Неудачная обработка ответа запроса на завершение игры. (unknown connection issue between cloudfare and blum)", account, nameof(Game), nameof(ClaimBugGame));
                }
                else
                {
                    Logger.Warning($"Неудачная обработка ответа запроса на завершение игры.\nResponseContent = {endGameData.ResponseContent}", account, nameof(Game), nameof(ClaimBugGame));
                }

                exception = ex;
                return (status, message, exception);
            }

            if (endGameJsonData == null)
            {
                Logger.Warning("Неудачная конвертация ответа запроса на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

                exception = new Exception("endGameJsonData == null");
                return (status, message, exception);
            }

            try
            {
                message = endGameJsonData.message;
            }
            catch (Exception ex)
            {
                Logger.Warning("Неудачная обработка ответа запроса на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

                exception = ex;
                return (status, message, exception);
            }

            if (message == null)
            {
                Logger.Warning("Неудачная обработка ответа запроса на завершение игры.", account, nameof(Game), nameof(ClaimBugGame));

                exception = new Exception("message == null");
                return (status, message, exception);
            }

            Logger.Debug("Запрос на завершение игры выполнен успешно.", account);

            status = true;

            return (status, message, exception);
        }
    }
}
