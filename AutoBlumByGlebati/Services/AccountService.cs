using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Services
{
    public static class AccountService
    {
        private static List<Account> Accounts { get; set; } = new List<Account>();

        public static int AccountsCount
        {
            get
            {
                return Accounts.Count;
            }
        }

        public static List<Account> GetAccounts
        {
            get
            {
                return Accounts;
            }
        }

        private static readonly object DatabaseLock = new object();
        private static readonly string DatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabaseDirectory, "Accounts.json");

        public static bool LoadFromDatabase()
        {
            bool loaded = false;

            Logger.Debug("Загружаем данные из БД.", nameof(AccountService), nameof(LoadFromDatabase));

            try
            {
                lock (DatabaseLock)
                {
                    if (!Directory.Exists(DatabaseDirectory))
                    {
                        Directory.CreateDirectory(DatabaseDirectory);
                    }

                    string jsonData = "";

                    if (!File.Exists(DatabaseFile))
                    {
                        Accounts.Add(new Account("", "", "", "+380593765384", "USERAGENT_FROM_BROWSER", "", "TOKEN_WITHOUT_BEARER", "", 0.0, "username:pass@ip:port_OR_ip:port@username:pass", true));

                        jsonData = JsonConvert.SerializeObject(Accounts);

                        using (StreamWriter writer = new StreamWriter(DatabaseFile, false, Encoding.UTF8))
                        {
                            writer.Write(jsonData);
                        }
                    }

                    Accounts.Clear();

                    using (StreamReader reader = new StreamReader(DatabaseFile, Encoding.UTF8))
                    {
                        jsonData = reader.ReadToEnd();
                    }

                    Accounts.AddRange(JsonConvert.DeserializeObject<List<Account>>(jsonData) ?? new List<Account>());

                    loaded = true;
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Произошла фатальная ошибка при загрузке базы данных.");

                Logger.Error(ex.Message, nameof(AccountService), nameof(LoadFromDatabase));
            }

            Logger.Debug("Успешно загрузили данные из БД.", nameof(AccountService), nameof(LoadFromDatabase));

            return loaded;
        }

        public static bool SaveToDatabase()
        {
            bool saved = false;

            Logger.Debug("Сохраняем данные в БД.", nameof(AccountService), nameof(SaveToDatabase));

            try
            {
                lock (DatabaseLock)
                {
                    if (!Directory.Exists(DatabaseDirectory))
                    {
                        Directory.CreateDirectory(DatabaseDirectory);
                    }

                    string jsonData = JsonConvert.SerializeObject(Accounts);

                    using (StreamWriter writer = new StreamWriter(DatabaseFile, false, Encoding.UTF8))
                    {
                        writer.Write(jsonData);
                    }

                    saved = true;
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Произошла фатальная ошибка при сохранении базы данных.");

                Logger.Error(ex.Message, nameof(AccountService), nameof(SaveToDatabase));
            }

            Logger.Debug("Успешно сохранили данные в БД.", nameof(AccountService), nameof(SaveToDatabase));

            return saved;
        }
    }
}
