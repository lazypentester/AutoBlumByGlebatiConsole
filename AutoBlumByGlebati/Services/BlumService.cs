using AutoBlumByGlebati.Core;
using AutoBlumByGlebati.Core.BlumBotApi;
using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutoBlumByGlebati.Services
{
    public static class BlumService
    {
        private static List<Task> accountTasks = new List<Task>();

        private static readonly object accountCountNowtAtWorkLock = new object();
        private static int accountCountNowtAtWork = 0;
        private static int accountAllCount = 0;
        private static bool allAccountsIsDone = false;

        private static readonly string CURRENT_WORK_DIRECTORY = Directory.GetCurrentDirectory();
        private static bool forcedCompletionOfTasks = false;

        private static readonly int MAX_COUNT_TRIES_GET_ME_INFO = 5;
        private static readonly int MAX_COUNT_TRIES_GET_GAME_BALANCE = 5;
        private static readonly int MAX_COUNT_TRIES_START_FARM = 5;
        private static readonly int MAX_COUNT_TRIES_CLAIM_FARM = 5;

        public static async Task Launch()
        {
            if (!AccountService.LoadFromDatabase())
            {
                return;
            }

            Logger.Info("Запускаем AutoBlum для всех аккаунтов.");

            foreach (var account in AccountService.GetAccounts.Where(acc => acc.IsEnabled))
            {
                accountTasks.Add(Task.Run(async () => 
                {
                    try
                    {
                        bool ConnectionViaProxy = String.IsNullOrEmpty(account.Proxy) is true ? false : true;

                        if (ConnectionViaProxy)
                        {
                            Proxy? accountProxy = null;
                            if (!ProxyTool.TryParseHttpProxy(account.Proxy, out accountProxy))
                            {
                                throw new Exception("Не удалось спарсить прокси. !ProxyTool.TryParseHttpProxy(account.Proxy, out accountProxy)");
                            }

                            if (accountProxy == null)
                            {
                                throw new Exception("Не удалось спарсить прокси. accountProxy == null");
                            }

                            Client accountClient = new Client(accountProxy, account.UserAgent);

                            if (!await ProxyTool.IsProxyConnected(accountClient))
                            {
                                throw new Exception("Не удалось убедиться что прокси действительно подключены. !await ProxyTool.IsProxyConnected(accountClient)");
                            }

                            Logger.Success($"Подтверждено подключение к прокси: {accountProxy.FullProxyAddressString}", account);

                            await Authenticate(account, accountClient);
                            await Task.Delay(RandomTool.GetRandomTimeSec(10, 15));

                            await GetInfoAboutMe(account, accountClient, false, 3);
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 7));

                            var checkInResult = await DailyReward.CheckIn(account, accountClient);
                            if (!checkInResult.IsSuccess)
                            {
                                throw new Exception("!checkInResult.IsSuccess\n" + checkInResult.Exception?.Message);
                            }
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 7));

                            await GetCurrentWalletPointsBalance(account, accountClient, true);
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 15));

                            await PlayTheSnowflakeGame(account, accountClient);
                        }
                        else
                        {
                            Client accountClient = new Client(account.UserAgent);
                            accountClient.IpAddress = ProxyTool.MyClearIp;

                            await Authenticate(account, accountClient);
                            await Task.Delay(RandomTool.GetRandomTimeSec(10, 15));

                            await GetInfoAboutMe(account, accountClient, false, 3);
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 7));

                            var checkInResult = await DailyReward.CheckIn(account, accountClient);
                            if (!checkInResult.IsSuccess)
                            {
                                throw new Exception("!checkInResult.IsSuccess\n" + checkInResult.Exception?.Message);
                            }
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 7));

                            await GetCurrentWalletPointsBalance(account, accountClient, true);
                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 15));

                            await PlayTheSnowflakeGame(account, accountClient);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message, account, nameof(BlumService), nameof(Launch));
                    }
                    finally
                    {
                        lock (accountCountNowtAtWorkLock)
                        {
                            accountCountNowtAtWork--;
                        }
                    }
                }));

                accountAllCount++;

                lock (accountCountNowtAtWorkLock)
                {
                    accountCountNowtAtWork++;
                }

                await Task.Delay(RandomTool.GetRandomTimeSec(0, 31));
            }

            #region ADDITIONAL TASKS

            accountTasks.Add(Task.Run(async () =>
                {
                    while (!allAccountsIsDone)
                    {
                        lock (accountCountNowtAtWorkLock)
                        {
                            Logger.Warning($"[# Важная информация #] КОЛИЧЕСТВО АККАУНТОВ В РАБОТЕ ===> [{accountCountNowtAtWork} из {accountAllCount}]");
                        }

                        if (accountCountNowtAtWork <= 0)
                        {
                            allAccountsIsDone = true;
                            break;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }));

            accountTasks.Add(Task.Run(async () =>
            {
                while (!allAccountsIsDone)
                {
                    try
                    {
                        forcedCompletionOfTasks = Directory.GetFiles(CURRENT_WORK_DIRECTORY).Select(file => Path.GetFileNameWithoutExtension(file)).Where(file_name => file_name == "stop").Any();
                    }
                    catch { }

                    if (forcedCompletionOfTasks)
                    {
                        Logger.Warning($"[# Важная информация #] ВЫПОЛНЕНИЕ ЗАДАНИЙ БЫЛО ПРИНУДИТЕЛЬНО ОСТАНОВЛЕНО ==> ОЖИДАЙТЕ ЗАВЕРШЕНИЯ РАБОТЫ.");
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }));

            #endregion

            await Task.WhenAll(accountTasks);

            Logger.Success("AutoBlum отработал все аккаунты и успешно завершил работу.");

            if (!AccountService.SaveToDatabase())
            {
                return;
            }
        }

        private static async Task<bool> Authenticate(Account account, Client accountClient)
        {
            Logger.Info($"Авторизация....", account);

            bool status = false;

            if (String.IsNullOrEmpty(account.AuthBlumAccessToken) || String.IsNullOrEmpty(account.AuthBlumRefreshToken))
            {
                (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) createSession = await Session.CreateSession(account, accountClient);

                if (!createSession.IsSuccess)
                {
                    throw new Exception("Cannot create session\n" + createSession.Exception?.Message);
                }

                if (String.IsNullOrEmpty(createSession.AccessToken))
                {
                    Logger.Error("String.IsNullOrEmpty(createSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                    throw new Exception("String.IsNullOrEmpty(createSession.AccessToken)");
                }
                if (String.IsNullOrEmpty(createSession.RefreshToken))
                {
                    Logger.Error("String.IsNullOrEmpty(createSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                    throw new Exception("String.IsNullOrEmpty(createSession.RefreshToken)");
                }

                account.AuthBlumAccessToken = createSession.AccessToken ?? "";
                account.AuthBlumRefreshToken = createSession.RefreshToken ?? "";

                Logger.Success($"Авторизация успешна.", account);

                status = true;
            }
            else
            {
                (bool IsSuccess, string? Id, string? Username, Exception? Exception) me = await GetInfoAboutMe(account, accountClient, false, 3);

                if (me.IsSuccess)
                {
                    (bool IsSuccess, bool? NeedToRefresh, Exception? Exception) needToRefreshSession = await IsNeedToRefreshSession(account, accountClient);

                    if (!needToRefreshSession.IsSuccess)
                    {
                        throw new Exception("!needToRefreshSession.IsSuccess");
                    }

                    if (needToRefreshSession.NeedToRefresh!.Value)
                    {
                        Logger.Info($"Обновление сессии....", account);

                        (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) refreshSession = await Session.RefreshSession(account, accountClient);

                        if (!refreshSession.IsSuccess)
                        {
                            throw new Exception("Cannot refresh session\n" + refreshSession.Exception?.Message);
                        }

                        if (String.IsNullOrEmpty(refreshSession.AccessToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.AccessToken)");
                        }
                        if (String.IsNullOrEmpty(refreshSession.RefreshToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.RefreshToken)");
                        }

                        account.AuthBlumAccessToken = refreshSession.AccessToken ?? "";
                        account.AuthBlumRefreshToken = refreshSession.RefreshToken ?? "";

                        Logger.Info($"Обновление сессии успешно.", account);
                    }

                    Logger.Success($"Авторизация успешна.", account);

                    status = true;
                }
                else
                {
                    Logger.Info($"Обновление сессии....", account);

                    (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) refreshSession = await Session.RefreshSession(account, accountClient);

                    if (refreshSession.IsSuccess)
                    {
                        if (String.IsNullOrEmpty(refreshSession.AccessToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.AccessToken)");
                        }
                        if (String.IsNullOrEmpty(refreshSession.RefreshToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.RefreshToken)");
                        }

                        account.AuthBlumAccessToken = refreshSession.AccessToken ?? "";
                        account.AuthBlumRefreshToken = refreshSession.RefreshToken ?? "";

                        Logger.Success($"Обновление сессии успешно.", account);

                        status = true;
                    }
                    else
                    {
                        Logger.Info($"Создание сессии....", account);

                        (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) createSession = await Session.CreateSession(account, accountClient);

                        if (!createSession.IsSuccess)
                        {
                            throw new Exception("Cannot create session\n" + createSession.Exception?.Message);
                        }

                        if (String.IsNullOrEmpty(createSession.AccessToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(createSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(createSession.AccessToken)");
                        }
                        if (String.IsNullOrEmpty(createSession.RefreshToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(createSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(createSession.RefreshToken)");
                        }

                        account.AuthBlumAccessToken = createSession.AccessToken ?? "";
                        account.AuthBlumRefreshToken = createSession.RefreshToken ?? "";

                        Logger.Success($"Создание сессии успешно.", account);

                        status = true;
                    }
                }
            }


            return status;
        }

        private static async Task<(bool IsSuccess, bool? NeedToRefresh, Exception? Exception)> IsNeedToRefreshSession(Account account, Client accountClient)
        {
            bool status = false;
            bool? needToRefresh = false;
            Exception? exception = null;

            Logger.Debug("Проверка сессии на актуальность.", account);

            (bool IsSuccess, DateTime? TimeNow, Exception? Exception) serverCurrentTime = await ServerTime.Now(account, accountClient);

            if (!serverCurrentTime.IsSuccess)
            {
                Logger.Error("!serverCurrentTime.IsSuccess", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("!serverCurrentTime.IsSuccess");
            }

            if(serverCurrentTime.TimeNow == null)
            {
                Logger.Error("serverCurrentTime.TimeNow == null", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("serverCurrentTime.TimeNow == null");
            }

            (bool IsSuccess, bool? IsValid, Exception? Exception) checkAccessTokenValid = Session.IsAccessTokenValid(account, serverCurrentTime.TimeNow.Value);

            if (!checkAccessTokenValid.IsSuccess)
            {
                Logger.Error("!checkAccessTokenValid.IsSuccess", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("!checkAccessTokenValid.IsSuccess");
            }

            if(checkAccessTokenValid.IsValid == null)
            {
                Logger.Error("checkAccessTokenValid.IsValid == null", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("checkAccessTokenValid.IsValid == null");
            }

            (bool IsSuccess, bool? IsValid, Exception? Exception) checkRefreshTokenValid = Session.IsRefreshTokenValid(account, serverCurrentTime.TimeNow.Value);

            if (!checkRefreshTokenValid.IsSuccess)
            {
                Logger.Error("!checkRefreshTokenValid.IsSuccess", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("!checkRefreshTokenValid.IsSuccess");
            }

            if (checkRefreshTokenValid.IsValid == null)
            {
                Logger.Error("checkRefreshTokenValid.IsValid == null", account, nameof(BlumService), nameof(IsNeedToRefreshSession));

                throw new Exception("checkRefreshTokenValid.IsValid == null");
            }

            if(!checkAccessTokenValid.IsValid.Value || !checkRefreshTokenValid.IsValid.Value)
            {
                needToRefresh = true;
            }

            Logger.Debug("Сессия успешно проверена на актуальность.", account);

            status = true;

            return (status, needToRefresh, exception);
        }

        private static async Task<(bool IsSuccess, string? Id, string? Username, Exception? Exception)> GetInfoAboutMe(Account account, Client accountClient, bool rewriteIfFilled, int? repeatRequestTriesCount = null)
        {
            bool status = false;
            Exception? exception = null;

            Logger.Info("Запрос на получение информации о себе.", account);

            (bool IsSuccess, string? Id, string? Username, Exception? Exception) meResponce = (false, null, null, null);
            int maxCountTriesGetMeInfo = repeatRequestTriesCount ?? MAX_COUNT_TRIES_GET_ME_INFO;

            while(maxCountTriesGetMeInfo > 0 && !meResponce.IsSuccess)
            {
                meResponce = await Me.GetInfoAboutMe(account, accountClient);

                if (!meResponce.IsSuccess)
                {
                    maxCountTriesGetMeInfo--;

                    Logger.Warning("Неудачный запрос на получение информации о себе...", account);

                    exception = meResponce.Exception ?? new Exception("!meResponce.IsSuccess");

                    await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                    Logger.Debug("Повторяем запрос на получение информации о себе...", account);
                }
            }

            if (!meResponce.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение информации о себе.", account, nameof(BlumService), nameof(GetInfoAboutMe));

                exception = meResponce.Exception ?? new Exception("!meResponce.IsSuccess");
            }

            Logger.Success("Данные о себе успешно получены.", account);

            if (String.IsNullOrEmpty(account.Id))
            {
                account.Id = meResponce.Id ?? "";
            }
            else if (!String.IsNullOrEmpty(account.Id) && rewriteIfFilled)
            {
                account.Id = meResponce.Id ?? "";
            }

            if (String.IsNullOrEmpty(account.Username))
            {
                account.Username = meResponce.Username ?? "";
            }
            else if (!String.IsNullOrEmpty(account.Username) && rewriteIfFilled)
            {
                account.Username = meResponce.Username ?? "";
            }

            if (String.IsNullOrEmpty(meResponce.Id))
            {
                Logger.Warning("String.IsNullOrEmpty(meResponce.Id)", account, nameof(BlumService), nameof(GetInfoAboutMe));
            }
            if (String.IsNullOrEmpty(meResponce.Username))
            {
                Logger.Warning("String.IsNullOrEmpty(meResponce.Username)", account, nameof(BlumService), nameof(GetInfoAboutMe));
            }

            status = true;

            return (status, meResponce.Id, meResponce.Username, exception);
        }

        private static async Task<(bool IsSuccess, double? BlumPoints, double? DogsPoints)> GetCurrentWalletPointsBalance(Account account, Client accountClient, bool rewriteIfFilled)
        {
            Logger.Info("Запрос на получение текущего баланса кошелька.", account);

            (bool IsSuccess, double? BlumPoints, double? DogsPoints, Exception? Exception) walletBalanceResponce = await Balance.GetCurrentPointsBalance(account, accountClient);

            if (!walletBalanceResponce.IsSuccess)
            {
                Logger.Warning("Неудачный запрос на получение текущего баланса кошелька.", account, nameof(BlumService), nameof(GetCurrentWalletPointsBalance));

                throw walletBalanceResponce.Exception ?? new Exception("!walletBalanceResponce.IsSuccess");
            }

            Logger.Success("Данные о текущем балансе кошелька успешно получены.", account);

            if (account.BlumPointsBalance == 0)
            {
                account.BlumPointsBalance = walletBalanceResponce.BlumPoints ?? 0.0;
            }
            else if (account.BlumPointsBalance == 0 && rewriteIfFilled)
            {
                account.BlumPointsBalance = walletBalanceResponce.BlumPoints ?? 0.0;
            }
            else if (rewriteIfFilled)
            {
                account.BlumPointsBalance = walletBalanceResponce.BlumPoints ?? 0.0;
            }

            if (account.BlumPointsBalance == 0)
            {
                Logger.Warning("account.BlumPointsBalance == 0)", account, nameof(BlumService), nameof(GetCurrentWalletPointsBalance));
            }

            return (walletBalanceResponce.IsSuccess, walletBalanceResponce.BlumPoints, walletBalanceResponce.DogsPoints);
        }

        private static async Task PlayTheSnowflakeGame(Account account, Client accountClient)
        {
            double curentWalletBalance = account.BlumPointsBalance;

            Logger.Info($"Текущий баланс: {curentWalletBalance} Blum Points.", account);

            Logger.Info("Запрос на получение возможности фарма Dogs.", account);

            (bool IsSuccess, bool? Eligibility, Exception? Exception) eligibilityDogs = await Game.GetEligibilityDogs(account, accountClient);

            if (!eligibilityDogs.IsSuccess)
            {
                Logger.Error("Неудачный запрос на получение возможности фарма Dogs.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                throw eligibilityDogs.Exception ?? new Exception("!eligibilityDogs.IsSuccess");
            }

            Logger.Success("Запрос на получение возможности фарма Dogs выполнен успешно.", account);

            await Task.Delay(RandomTool.GetRandomTimeSec(3, 10));

            if(GameBugConfig.CurrentGameType == GameBugConfig.GameType.Normal)
            {
                int playTickets = 0;

                do
                {
                    #region GET GAME BALANCE

                    Logger.Info("Запрос на получение текущего баланса игры.", account);

                    (bool IsSuccess, int? PlayPasses, DateTime? TimeNow, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception) gameBalance = (false, null, null, null, null, null);
                    int maxCountTriesGetGameBalance = MAX_COUNT_TRIES_GET_GAME_BALANCE;

                    while (maxCountTriesGetGameBalance > 0 && !gameBalance.IsSuccess)
                    {
                        gameBalance = await Balance.GetCurrentGameBalance(account, accountClient);

                        if (!gameBalance.IsSuccess)
                        {
                            maxCountTriesGetGameBalance--;

                            Logger.Warning("Неудачный запрос на получение текущего баланса игры...", account);

                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                            Logger.Debug("Повторяем запрос на получение текущего баланса игры...", account);
                        }
                    }

                    if (!gameBalance.IsSuccess)
                    {
                        Logger.Error("Неудачный запрос на получение текущего баланса игры.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                        throw gameBalance.Exception ?? new Exception("!gameBalance.IsSuccess");
                    }

                    playTickets = gameBalance.PlayPasses!.Value;

                    Logger.Success($"Текущий баланс игры: {playTickets} Passes.", account);

                    if(playTickets == 0)
                    {
                        break;
                    }

                    await Task.Delay(RandomTool.GetRandomTimeSec(1, 5));

                    #endregion

                    #region CHECK FARMING

                    if (gameBalance.TimeNow!.Value > gameBalance.TimeEndFarming!.Value)
                    {
                        #region CLAIM FARM

                        Logger.Info("Запрос на клейм фарминга..", account);

                        (bool IsSuccess, DateTime? TimeNow, Exception? Exception) claimFarm = (false, null, null);
                        int maxCountTriesClaimFarm = MAX_COUNT_TRIES_CLAIM_FARM;

                        while (maxCountTriesClaimFarm > 0 && !claimFarm.IsSuccess)
                        {
                            claimFarm = await Farm.ClaimFarm(account, accountClient);

                            if (!claimFarm.IsSuccess)
                            {
                                maxCountTriesClaimFarm--;

                                Logger.Warning("Неудачный запрос на клейм фарминга...", account);

                                await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                                Logger.Debug("Повторяем запрос на клейм фарминга...", account);
                            }
                        }

                        if (!claimFarm.IsSuccess)
                        {
                            Logger.Error("Неудачный запрос на клейм фарминга.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                            throw claimFarm.Exception ?? new Exception("!claimFarm.IsSuccess");
                        }

                        Logger.Success("Запрос на клейм фарминга выполнен успешно.", account);

                        #endregion

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 15));

                        #region START FARM

                        Logger.Info("Запрос на старт фарминга..", account);

                        (bool IsSuccess, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception) startFarm = (false, null, null, null);
                        int maxCountTriesStartFarm = MAX_COUNT_TRIES_START_FARM;

                        while (maxCountTriesStartFarm > 0 && !startFarm.IsSuccess)
                        {
                            startFarm = await Farm.StartFarm(account, accountClient);

                            if (!startFarm.IsSuccess)
                            {
                                maxCountTriesStartFarm--;

                                Logger.Warning("Неудачный запрос на старт фарминга...", account);

                                await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                                Logger.Debug("Повторяем запрос на старт фарминга...", account);
                            }
                        }

                        if (!startFarm.IsSuccess)
                        {
                            Logger.Error("Неудачный запрос на старт фарминга.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                            throw startFarm.Exception ?? new Exception("!startFarm.IsSuccess");
                        }

                        Logger.Success("Запрос на старт фарминга выполнен успешно.", account);

                        #endregion

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 15));
                    }

                    #endregion

                    #region PLAY ONE GAME

                    Logger.Info("Начинаем игру.", account);

                    (bool IsSuccess, Exception? Exception) game = await Game.PlayGame(account, accountClient, GameBugConfig.CurrentGameType, eligibilityDogs.Eligibility!.Value);

                    if (!game.IsSuccess)
                    {
                        Logger.Error("Не удалось сыграть в игру со снежинками. !game.IsSuccess", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                        throw game.Exception ?? new Exception("!game.IsSuccess");
                    }

                    playTickets--;

                    Logger.Success($"Одна игра успешно завершена. Осталось попыток {playTickets}", account); 

                    #endregion

                    #region CHECK NEED TO REFRESH SESSION

                    (bool IsSuccess, bool? NeedToRefresh, Exception? Exception) needToRefreshSession = await IsNeedToRefreshSession(account, accountClient);

                    if (needToRefreshSession.IsSuccess && needToRefreshSession.NeedToRefresh!.Value)
                    {
                        (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) refreshSession = await Session.RefreshSession(account, accountClient);

                        if (!refreshSession.IsSuccess)
                        {
                            throw new Exception("Cannot refresh session\n" + refreshSession.Exception?.Message);
                        }

                        if (String.IsNullOrEmpty(refreshSession.AccessToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.AccessToken)");
                        }
                        if (String.IsNullOrEmpty(refreshSession.RefreshToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.RefreshToken)");
                        }

                        account.AuthBlumAccessToken = refreshSession.AccessToken ?? "";
                        account.AuthBlumRefreshToken = refreshSession.RefreshToken ?? "";
                    }

                    #endregion

                    await Task.Delay(RandomTool.GetRandomTimeSec(5, 15));
                }
                while (playTickets > 0 && !forcedCompletionOfTasks);
            }
            else if (GameBugConfig.CurrentGameType == GameBugConfig.GameType.WithBug)
            {
                int playTickets = GameBugConfig.PlayPassesForGameWithBug;

                Logger.Success($"Текущий баланс игры: {playTickets} Passes.", account);

                while (playTickets > 0 && !forcedCompletionOfTasks)
                {
                    #region GET GAME BALANCE

                    Logger.Info("Запрос на получение текущего баланса игры.", account);

                    (bool IsSuccess, int? PlayPasses, DateTime? TimeNow, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception) gameBalance = (false, null, null, null, null, null);
                    int maxCountTriesGetGameBalance = MAX_COUNT_TRIES_GET_GAME_BALANCE;

                    while (maxCountTriesGetGameBalance > 0 && !gameBalance.IsSuccess)
                    {
                        gameBalance = await Balance.GetCurrentGameBalance(account, accountClient);

                        if (!gameBalance.IsSuccess)
                        {
                            maxCountTriesGetGameBalance--;

                            Logger.Warning("Неудачный запрос на получение текущего баланса игры...", account);

                            await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                            Logger.Debug("Повторяем запрос на получение текущего баланса игры...", account);
                        }
                    }

                    if (!gameBalance.IsSuccess)
                    {
                        Logger.Error("Неудачный запрос на получение текущего баланса игры.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                        throw gameBalance.Exception ?? new Exception("!gameBalance.IsSuccess");
                    }

                    Logger.Success($"Текущий баланс игры: {playTickets} Passes.", account);

                    if (playTickets == 0)
                    {
                        break;
                    }

                    await Task.Delay(RandomTool.GetRandomTimeSec(1, 5));

                    #endregion

                    #region CHECK FARMING

                    if (gameBalance.TimeNow!.Value > gameBalance.TimeEndFarming!.Value)
                    {
                        #region CLAIM FARM

                        Logger.Info("Запрос на клейм фарминга..", account);

                        (bool IsSuccess, DateTime? TimeNow, Exception? Exception) claimFarm = (false, null, null);
                        int maxCountTriesClaimFarm = MAX_COUNT_TRIES_CLAIM_FARM;

                        while (maxCountTriesClaimFarm > 0 && !claimFarm.IsSuccess)
                        {
                            claimFarm = await Farm.ClaimFarm(account, accountClient);

                            if (!claimFarm.IsSuccess)
                            {
                                maxCountTriesClaimFarm--;

                                Logger.Warning("Неудачный запрос на клейм фарминга...", account);

                                await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                                Logger.Debug("Повторяем запрос на клейм фарминга...", account);
                            }
                        }

                        if (!claimFarm.IsSuccess)
                        {
                            Logger.Error("Неудачный запрос на клейм фарминга.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                            throw claimFarm.Exception ?? new Exception("!claimFarm.IsSuccess");
                        }

                        Logger.Success("Запрос на клейм фарминга выполнен успешно.", account);

                        #endregion

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 15));

                        #region START FARM

                        Logger.Info("Запрос на старт фарминга..", account);

                        (bool IsSuccess, DateTime? TimeStartFarming, DateTime? TimeEndFarming, Exception? Exception) startFarm = (false, null, null, null);
                        int maxCountTriesStartFarm = MAX_COUNT_TRIES_START_FARM;

                        while (maxCountTriesStartFarm > 0 && !startFarm.IsSuccess)
                        {
                            startFarm = await Farm.StartFarm(account, accountClient);

                            if (!startFarm.IsSuccess)
                            {
                                maxCountTriesStartFarm--;

                                Logger.Warning("Неудачный запрос на старт фарминга...", account);

                                await Task.Delay(RandomTool.GetRandomTimeSec(3, 11));

                                Logger.Debug("Повторяем запрос на старт фарминга...", account);
                            }
                        }

                        if (!startFarm.IsSuccess)
                        {
                            Logger.Error("Неудачный запрос на старт фарминга.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                            throw startFarm.Exception ?? new Exception("!startFarm.IsSuccess");
                        }

                        Logger.Success("Запрос на старт фарминга выполнен успешно.", account);

                        #endregion

                        await Task.Delay(RandomTool.GetRandomTimeSec(1, 15));
                    }

                    #endregion

                    #region PLAY ONE GAME

                    Logger.Info("Начинаем игру (с багом).", account);

                    (bool IsSuccess, Exception? Exception) game = await Game.PlayGame(account, accountClient, GameBugConfig.CurrentGameType, eligibilityDogs.Eligibility!.Value);

                    if (!game.IsSuccess)
                    {
                        Logger.Error("Не удалось сыграть в игру со снежинками (с багом). !game.IsSuccess", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                        throw game.Exception ?? new Exception("!game.IsSuccess");
                    }

                    playTickets--;

                    Logger.Success($"Одна игра (с багом) успешно завершена. Осталось попыток {playTickets}", account); 

                    #endregion

                    #region CHECK NEED TO REFRESH SESSION

                    (bool IsSuccess, bool? NeedToRefresh, Exception? Exception) needToRefreshSession = await IsNeedToRefreshSession(account, accountClient);

                    if (needToRefreshSession.IsSuccess && needToRefreshSession.NeedToRefresh!.Value)
                    {
                        (bool IsSuccess, string? AccessToken, string? RefreshToken, Exception? Exception) refreshSession = await Session.RefreshSession(account, accountClient);

                        if (!refreshSession.IsSuccess)
                        {
                            throw new Exception("Cannot refresh session\n" + refreshSession.Exception?.Message);
                        }

                        if (String.IsNullOrEmpty(refreshSession.AccessToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.AccessToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.AccessToken)");
                        }
                        if (String.IsNullOrEmpty(refreshSession.RefreshToken))
                        {
                            Logger.Error("String.IsNullOrEmpty(refreshSession.RefreshToken)", account, nameof(BlumService), nameof(Authenticate));

                            throw new Exception("String.IsNullOrEmpty(refreshSession.RefreshToken)");
                        }

                        account.AuthBlumAccessToken = refreshSession.AccessToken ?? "";
                        account.AuthBlumRefreshToken = refreshSession.RefreshToken ?? "";
                    }

                    #endregion

                    await Task.Delay(RandomTool.GetRandomTimeSec(5, 15));
                }
            }

            (bool IsSuccess, double? BlumPoints, double? DogsPoints) balanceData = await GetCurrentWalletPointsBalance(account, accountClient, true);

            if (!balanceData.IsSuccess)
            {
                Logger.Error("Неудачный запрос на получение текущего баланса Blum.", account, nameof(BlumService), nameof(PlayTheSnowflakeGame));

                throw new Exception("!balanceData.IsSuccess");
            }

            Logger.Success($"ЗАРАБОТАНО: +{balanceData.BlumPoints!.Value - curentWalletBalance} Blum Points.", account);
        }
    }
}
