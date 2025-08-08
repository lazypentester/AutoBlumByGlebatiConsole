using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Services
{
    public static class MainMenuService
    {
        private static readonly List<string> helloPics = new List<string>()
        {
            "\r\n    ___         __        ____  __                   __             ________     __          __  _ \r\n   /   | __  __/ /_____  / __ )/ /_  ______ ___     / /_  __  __   / ____/ /__  / /_  ____ _/ /_(_)\r\n  / /| |/ / / / __/ __ \\/ __  / / / / / __ `__ \\   / __ \\/ / / /  / / __/ / _ \\/ __ \\/ __ `/ __/ / \r\n / ___ / /_/ / /_/ /_/ / /_/ / / /_/ / / / / / /  / /_/ / /_/ /  / /_/ / /  __/ /_/ / /_/ / /_/ /  \r\n/_/  |_\\__,_/\\__/\\____/_____/_/\\__,_/_/ /_/ /_/  /_.___/\\__, /   \\____/_/\\___/_.___/\\__,_/\\__/_/   \r\n                                                       /____/                                      \r\n",
            "\r\n  ______               __                _______   __                                __                         ______   __            __                   __      __ \r\n /      \\             |  \\              |       \\ |  \\                              |  \\                       /      \\ |  \\          |  \\                 |  \\    |  \\\r\n|  $$$$$$\\ __    __  _| $$_     ______  | $$$$$$$\\| $$ __    __  ______ ____        | $$____   __    __       |  $$$$$$\\| $$  ______  | $$____    ______  _| $$_    \\$$\r\n| $$__| $$|  \\  |  \\|   $$ \\   /      \\ | $$__/ $$| $$|  \\  |  \\|      \\    \\       | $$    \\ |  \\  |  \\      | $$ __\\$$| $$ /      \\ | $$    \\  |      \\|   $$ \\  |  \\\r\n| $$    $$| $$  | $$ \\$$$$$$  |  $$$$$$\\| $$    $$| $$| $$  | $$| $$$$$$\\$$$$\\      | $$$$$$$\\| $$  | $$      | $$|    \\| $$|  $$$$$$\\| $$$$$$$\\  \\$$$$$$\\\\$$$$$$  | $$\r\n| $$$$$$$$| $$  | $$  | $$ __ | $$  | $$| $$$$$$$\\| $$| $$  | $$| $$ | $$ | $$      | $$  | $$| $$  | $$      | $$ \\$$$$| $$| $$    $$| $$  | $$ /      $$ | $$ __ | $$\r\n| $$  | $$| $$__/ $$  | $$|  \\| $$__/ $$| $$__/ $$| $$| $$__/ $$| $$ | $$ | $$      | $$__/ $$| $$__/ $$      | $$__| $$| $$| $$$$$$$$| $$__/ $$|  $$$$$$$ | $$|  \\| $$\r\n| $$  | $$ \\$$    $$   \\$$  $$ \\$$    $$| $$    $$| $$ \\$$    $$| $$ | $$ | $$      | $$    $$ \\$$    $$       \\$$    $$| $$ \\$$     \\| $$    $$ \\$$    $$  \\$$  $$| $$\r\n \\$$   \\$$  \\$$$$$$     \\$$$$   \\$$$$$$  \\$$$$$$$  \\$$  \\$$$$$$  \\$$  \\$$  \\$$       \\$$$$$$$  _\\$$$$$$$        \\$$$$$$  \\$$  \\$$$$$$$ \\$$$$$$$   \\$$$$$$$   \\$$$$  \\$$\r\n                                                                                              |  \\__| $$                                                               \r\n                                                                                               \\$$    $$                                                               \r\n                                                                                                \\$$$$$$                                                                \r\n"
        };
        
        private static readonly List<string> helloAnimation = new List<string>()
        {
            "\r\n  ______               __                _______   __                                __                         ______   __            __                    __      __ \r\n /      \\             /  |              /       \\ /  |                              /  |                       /      \\ /  |          /  |                  /  |    /  |\r\n/$$$$$$  | __    __  _$$ |_     ______  $$$$$$$  |$$ | __    __  _____  ____        $$ |____   __    __       /$$$$$$  |$$ |  ______  $$ |____    ______   _$$ |_   $$/ \r\n$$ |__$$ |/  |  /  |/ $$   |   /      \\ $$ |__$$ |$$ |/  |  /  |/     \\/    \\       $$      \\ /  |  /  |      $$ | _$$/ $$ | /      \\ $$      \\  /      \\ / $$   |  /  |\r\n$$    $$ |$$ |  $$ |$$$$$$/   /$$$$$$  |$$    $$< $$ |$$ |  $$ |$$$$$$ $$$$  |      $$$$$$$  |$$ |  $$ |      $$ |/    |$$ |/$$$$$$  |$$$$$$$  | $$$$$$  |$$$$$$/   $$ |\r\n$$$$$$$$ |$$ |  $$ |  $$ | __ $$ |  $$ |$$$$$$$  |$$ |$$ |  $$ |$$ | $$ | $$ |      $$ |  $$ |$$ |  $$ |      $$ |$$$$ |$$ |$$    $$ |$$ |  $$ | /    $$ |  $$ | __ $$ |\r\n$$ |  $$ |$$ \\__$$ |  $$ |/  |$$ \\__$$ |$$ |__$$ |$$ |$$ \\__$$ |$$ | $$ | $$ |      $$ |__$$ |$$ \\__$$ |      $$ \\__$$ |$$ |$$$$$$$$/ $$ |__$$ |/$$$$$$$ |  $$ |/  |$$ |\r\n$$ |  $$ |$$    $$/   $$  $$/ $$    $$/ $$    $$/ $$ |$$    $$/ $$ | $$ | $$ |      $$    $$/ $$    $$ |      $$    $$/ $$ |$$       |$$    $$/ $$    $$ |  $$  $$/ $$ |\r\n$$/   $$/  $$$$$$/     $$$$/   $$$$$$/  $$$$$$$/  $$/  $$$$$$/  $$/  $$/  $$/       $$$$$$$/   $$$$$$$ |       $$$$$$/  $$/  $$$$$$$/ $$$$$$$/   $$$$$$$/    $$$$/  $$/ \r\n                                                                                              /  \\__$$ |                                                                \r\n                                                                                              $$    $$/                                                                 \r\n                                                                                               $$$$$$/                                                                  \r\n",
            "\r\n $$$$$$\\              $$\\               $$$$$$$\\  $$\\                               $$\\                        $$$$$$\\  $$\\           $$\\                  $$\\     $$\\ \r\n$$  __$$\\             $$ |              $$  __$$\\ $$ |                              $$ |                      $$  __$$\\ $$ |          $$ |                 $$ |    \\__|\r\n$$ /  $$ |$$\\   $$\\ $$$$$$\\    $$$$$$\\  $$ |  $$ |$$ |$$\\   $$\\ $$$$$$\\$$$$\\        $$$$$$$\\  $$\\   $$\\       $$ /  \\__|$$ | $$$$$$\\  $$$$$$$\\   $$$$$$\\ $$$$$$\\   $$\\ \r\n$$$$$$$$ |$$ |  $$ |\\_$$  _|  $$  __$$\\ $$$$$$$\\ |$$ |$$ |  $$ |$$  _$$  _$$\\       $$  __$$\\ $$ |  $$ |      $$ |$$$$\\ $$ |$$  __$$\\ $$  __$$\\  \\____$$\\\\_$$  _|  $$ |\r\n$$  __$$ |$$ |  $$ |  $$ |    $$ /  $$ |$$  __$$\\ $$ |$$ |  $$ |$$ / $$ / $$ |      $$ |  $$ |$$ |  $$ |      $$ |\\_$$ |$$ |$$$$$$$$ |$$ |  $$ | $$$$$$$ | $$ |    $$ |\r\n$$ |  $$ |$$ |  $$ |  $$ |$$\\ $$ |  $$ |$$ |  $$ |$$ |$$ |  $$ |$$ | $$ | $$ |      $$ |  $$ |$$ |  $$ |      $$ |  $$ |$$ |$$   ____|$$ |  $$ |$$  __$$ | $$ |$$\\ $$ |\r\n$$ |  $$ |\\$$$$$$  |  \\$$$$  |\\$$$$$$  |$$$$$$$  |$$ |\\$$$$$$  |$$ | $$ | $$ |      $$$$$$$  |\\$$$$$$$ |      \\$$$$$$  |$$ |\\$$$$$$$\\ $$$$$$$  |\\$$$$$$$ | \\$$$$  |$$ |\r\n\\__|  \\__| \\______/    \\____/  \\______/ \\_______/ \\__| \\______/ \\__| \\__| \\__|      \\_______/  \\____$$ |       \\______/ \\__| \\_______|\\_______/  \\_______|  \\____/ \\__|\r\n                                                                                              $$\\   $$ |                                                               \r\n                                                                                              \\$$$$$$  |                                                               \r\n                                                                                               \\______/                                                                \r\n",
            "\r\n  /$$$$$$              /$$               /$$$$$$$  /$$                               /$$                        /$$$$$$  /$$           /$$                   /$$     /$$\r\n /$$__  $$            | $$              | $$__  $$| $$                              | $$                       /$$__  $$| $$          | $$                  | $$    |__/\r\n| $$  \\ $$ /$$   /$$ /$$$$$$    /$$$$$$ | $$  \\ $$| $$ /$$   /$$ /$$$$$$/$$$$       | $$$$$$$  /$$   /$$      | $$  \\__/| $$  /$$$$$$ | $$$$$$$   /$$$$$$  /$$$$$$   /$$\r\n| $$$$$$$$| $$  | $$|_  $$_/   /$$__  $$| $$$$$$$ | $$| $$  | $$| $$_  $$_  $$      | $$__  $$| $$  | $$      | $$ /$$$$| $$ /$$__  $$| $$__  $$ |____  $$|_  $$_/  | $$\r\n| $$__  $$| $$  | $$  | $$    | $$  \\ $$| $$__  $$| $$| $$  | $$| $$ \\ $$ \\ $$      | $$  \\ $$| $$  | $$      | $$|_  $$| $$| $$$$$$$$| $$  \\ $$  /$$$$$$$  | $$    | $$\r\n| $$  | $$| $$  | $$  | $$ /$$| $$  | $$| $$  \\ $$| $$| $$  | $$| $$ | $$ | $$      | $$  | $$| $$  | $$      | $$  \\ $$| $$| $$_____/| $$  | $$ /$$__  $$  | $$ /$$| $$\r\n| $$  | $$|  $$$$$$/  |  $$$$/|  $$$$$$/| $$$$$$$/| $$|  $$$$$$/| $$ | $$ | $$      | $$$$$$$/|  $$$$$$$      |  $$$$$$/| $$|  $$$$$$$| $$$$$$$/|  $$$$$$$  |  $$$$/| $$\r\n|__/  |__/ \\______/    \\___/   \\______/ |_______/ |__/ \\______/ |__/ |__/ |__/      |_______/  \\____  $$       \\______/ |__/ \\_______/|_______/  \\_______/   \\___/  |__/\r\n                                                                                               /$$  | $$                                                                \r\n                                                                                              |  $$$$$$/                                                                \r\n                                                                                               \\______/                                                                 \r\n",
            "\r\n  ______               __                _______   __                                __                         ______   __            __                   __      __ \r\n /      \\             |  \\              |       \\ |  \\                              |  \\                       /      \\ |  \\          |  \\                 |  \\    |  \\\r\n|  $$$$$$\\ __    __  _| $$_     ______  | $$$$$$$\\| $$ __    __  ______ ____        | $$____   __    __       |  $$$$$$\\| $$  ______  | $$____    ______  _| $$_    \\$$\r\n| $$__| $$|  \\  |  \\|   $$ \\   /      \\ | $$__/ $$| $$|  \\  |  \\|      \\    \\       | $$    \\ |  \\  |  \\      | $$ __\\$$| $$ /      \\ | $$    \\  |      \\|   $$ \\  |  \\\r\n| $$    $$| $$  | $$ \\$$$$$$  |  $$$$$$\\| $$    $$| $$| $$  | $$| $$$$$$\\$$$$\\      | $$$$$$$\\| $$  | $$      | $$|    \\| $$|  $$$$$$\\| $$$$$$$\\  \\$$$$$$\\\\$$$$$$  | $$\r\n| $$$$$$$$| $$  | $$  | $$ __ | $$  | $$| $$$$$$$\\| $$| $$  | $$| $$ | $$ | $$      | $$  | $$| $$  | $$      | $$ \\$$$$| $$| $$    $$| $$  | $$ /      $$ | $$ __ | $$\r\n| $$  | $$| $$__/ $$  | $$|  \\| $$__/ $$| $$__/ $$| $$| $$__/ $$| $$ | $$ | $$      | $$__/ $$| $$__/ $$      | $$__| $$| $$| $$$$$$$$| $$__/ $$|  $$$$$$$ | $$|  \\| $$\r\n| $$  | $$ \\$$    $$   \\$$  $$ \\$$    $$| $$    $$| $$ \\$$    $$| $$ | $$ | $$      | $$    $$ \\$$    $$       \\$$    $$| $$ \\$$     \\| $$    $$ \\$$    $$  \\$$  $$| $$\r\n \\$$   \\$$  \\$$$$$$     \\$$$$   \\$$$$$$  \\$$$$$$$  \\$$  \\$$$$$$  \\$$  \\$$  \\$$       \\$$$$$$$  _\\$$$$$$$        \\$$$$$$  \\$$  \\$$$$$$$ \\$$$$$$$   \\$$$$$$$   \\$$$$  \\$$\r\n                                                                                              |  \\__| $$                                                               \r\n                                                                                               \\$$    $$                                                               \r\n                                                                                                \\$$$$$$                                                                \r\n"
        };
        
        public static async Task OpenMenu()
        {
            if (!SetConsoleSize())
            {
                return;
            }
            await HelloAnimation();

            bool exit = false;
            bool autoBlumStart = false;

            while (!exit && !autoBlumStart)
            {
                await Hello();

                Console.WriteLine(" [1] - Запуск AutoBlum");
                Console.WriteLine(" [2] - Аккаунты");
                Console.WriteLine(" [3] - Выход");

                var MainMenuKey = Console.ReadKey();

                switch (MainMenuKey.Key)
                {
                    case ConsoleKey.D1:
                        {
                            bool BackToMainMenuFromLaunchMenu = false;

                            while (!BackToMainMenuFromLaunchMenu)
                            {
                                await Hello();

                                Console.WriteLine(" [1] - Игра с багом");
                                Console.WriteLine(" [2] - Обычная игра");
                                Console.WriteLine(" [3] - Назад");

                                var LaunchMenuKey = Console.ReadKey();

                                switch (LaunchMenuKey.Key)
                                {
                                    case ConsoleKey.D1:
                                        {
                                            bool passed = false;
                                            int playPassesCount = 0;

                                            while (!passed)
                                            {
                                                await Hello();

                                                Console.WriteLine();
                                                Console.WriteLine();
                                                Console.ForegroundColor = ConsoleColor.Cyan;
                                                Console.Write(" Введите кол-во билетов игры (применяется для всех аккаунтов): ");
                                                Console.ForegroundColor = ConsoleColor.Yellow;

                                                if (Int32.TryParse(Console.ReadLine(), out playPassesCount))
                                                {
                                                    passed = true;
                                                }

                                                Console.ResetColor();
                                            }

                                            GameBugConfig.CurrentGameType = GameBugConfig.GameType.WithBug;
                                            GameBugConfig.PlayPassesForGameWithBug = playPassesCount;
                                            autoBlumStart = true;
                                            BackToMainMenuFromLaunchMenu = true;
                                        }
                                        break;
                                    case ConsoleKey.D2:
                                        {
                                            GameBugConfig.CurrentGameType = GameBugConfig.GameType.Normal;
                                            autoBlumStart = true;
                                            BackToMainMenuFromLaunchMenu = true;
                                        }
                                        break;
                                    case ConsoleKey.D3:
                                        {
                                            BackToMainMenuFromLaunchMenu = true;
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case ConsoleKey.D2:
                        {
                            bool BackToMainMenuFromAccountsMenu = false;

                            while (!BackToMainMenuFromAccountsMenu)
                            {
                                await Hello();

                                if (!AccountService.LoadFromDatabase())
                                {
                                    return;
                                }

                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write(" Количество аккаунтов в базе: ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{AccountService.AccountsCount}");
                                Console.WriteLine();
                                Console.ResetColor();
                                Console.WriteLine(" [1] - Назад");

                                var AccountsMenuKey = Console.ReadKey();

                                switch (AccountsMenuKey.Key)
                                {
                                    case ConsoleKey.D1:
                                        {
                                            BackToMainMenuFromAccountsMenu = true;
                                        }
                                        break;
                                } 
                            }
                        }
                        break;
                    case ConsoleKey.D3:
                        exit = true;
                        break;
                }
            }

            if (autoBlumStart)
            {
                if (!await ProxyTool.GetMyClearIp())
                {
                    return;
                }

                Console.Clear();

                await BlumService.Launch();
            }
        }

        private static bool SetConsoleSize()
        {
            bool consoleSized = false;

            try
            {
                int Width = (Console.LargestWindowWidth - Console.WindowWidth) + (Console.WindowWidth / 2);
                int Height = (Console.LargestWindowHeight - Console.WindowHeight) + (Console.WindowHeight / 2);

                Console.SetWindowSize(Width, Height);

                consoleSized = true;
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Произошла фатальная ошибка при изменении размера окна консоли.");

                Logger.Error(ex.Message, nameof(MainMenuService), nameof(SetConsoleSize));
            }

            return consoleSized;
        }

        private static async Task Hello()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();
            Console.WriteLine(helloPics[RandomTool.GetRandomNumInt(0, helloPics.Count)]);
            await Task.Delay(100);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static async Task HelloAnimation()
        {
            foreach (var slide in helloAnimation)
            {
                Console.Clear();
                Console.WriteLine(slide);
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
            Console.ResetColor();
        }
    }
}
