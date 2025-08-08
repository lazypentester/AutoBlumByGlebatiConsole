using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Models
{
    public static class BlumUrl
    {
        public static readonly string CreateSession = "https://user-domain.blum.codes/api/v1/auth/provider/PROVIDER_TELEGRAM_MINI_APP";
        public static readonly string RefreshSession = "https://user-domain.blum.codes/api/v1/auth/refresh";
        public static readonly string ServerTimeNow = "https://game-domain.blum.codes/api/v1/time/now";
        public static readonly string Me = "https://user-domain.blum.codes/api/v1/user/me";
        public static readonly string PointsBalance = "https://wallet-domain.blum.codes/api/v1/wallet/my/points/balance";
        public static readonly string GameBalance = "https://game-domain.blum.codes/api/v1/user/balance";
        public static readonly string FriendsBalance = "https://user-domain.blum.codes/api/v1/friends/balance";
        public static readonly string GameStart = "https://game-domain.blum.codes/api/v2/game/play";
        public static readonly string GameClaim = "https://game-domain.blum.codes/api/v2/game/claim";
        public static readonly string EligibilityDogsDrop = "https://game-domain.blum.codes/api/v2/game/eligibility/dogs_drop";
        public static readonly string DailyRewardCheckIn = "https://game-domain.blum.codes/api/v1/daily-reward?offset=";
        public static readonly string FarmStart = "https://game-domain.blum.codes/api/v1/farming/start";
        public static readonly string FarmClaim = "https://game-domain.blum.codes/api/v1/farming/claim";
    }
}
