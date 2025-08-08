using AutoBlumByGlebati.Models;
using AutoBlumByGlebati.Services;
using AutoBlumByGlebati.Tools;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoBlumByGlebati
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Logger.DebugModeIsEnable = true;

            await MainMenuService.OpenMenu();

            Console.ReadKey();

            Environment.Exit(0);
        }
    }
}
