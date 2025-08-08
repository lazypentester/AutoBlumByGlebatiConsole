using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Models
{
    public class Account
    {
        [JsonPropertyName("ProfileNumber")]
        public string ProfileNumber { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userName")]
        public string Username { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; }

        [JsonPropertyName("authTelegramData")]
        public string AuthTelegramData { get; set; }

        [JsonPropertyName("authBlumAccessToken")]
        public string AuthBlumAccessToken { get; set; }

        [JsonPropertyName("authBlumRefreshToken")]
        public string AuthBlumRefreshToken { get; set; }

        [JsonPropertyName("blumPointsBalance")]
        public double BlumPointsBalance { get; set; }

        [JsonPropertyName("proxy")]
        public string Proxy { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        public Account(string profileNumber, string id, string username, string phoneNumber, string userAgent, string authTelegramData, string authBlumAccessToken, string authBlumRefreshToken, double blumPointsBalance, string proxy, bool isEnabled)
        {
            ProfileNumber = profileNumber;
            Id = id;
            Username = username;
            PhoneNumber = phoneNumber;
            UserAgent = userAgent;
            AuthTelegramData = authTelegramData;
            AuthBlumAccessToken = authBlumAccessToken;
            AuthBlumRefreshToken = authBlumRefreshToken;
            BlumPointsBalance = blumPointsBalance;
            Proxy = proxy;
            IsEnabled = isEnabled;
        }
    }
}
