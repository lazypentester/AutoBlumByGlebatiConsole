using AutoBlumByGlebati.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Tools
{
    public static class Logger
    {
        public static bool DebugModeIsEnable = false;

        public enum LogMessageType
        {
            Info,
            Debug,
            Success,
            Warning,
            Error
        }

        public static readonly Dictionary<LogMessageType, ConsoleColor> logMessageColors = new()
        {
            { LogMessageType.Info,    ConsoleColor.White },
            { LogMessageType.Debug, ConsoleColor.Cyan },
            { LogMessageType.Success, ConsoleColor.Green },
            { LogMessageType.Warning, ConsoleColor.DarkYellow },
            { LogMessageType.Error,   ConsoleColor.DarkRed }
        };

        public static readonly Dictionary<LogMessageType, string> logMessageTypeName = new()
        {
            { LogMessageType.Info,    "INFO" },
            { LogMessageType.Debug,   "DEBUG" },
            { LogMessageType.Success, "SUCCESS" },
            { LogMessageType.Warning, "WARNING" },
            { LogMessageType.Error,   "ERROR" }
        };

        private static readonly object logLock = new object();

        private static readonly StringBuilder stringBuilder = new StringBuilder();

        private static readonly string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), nameof(Logger));

        #region INFO
        public static void Info(string message)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Info);
            }
        }

        public static void Info(string message, Account account)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Info, account);
            }
        }

        public static void Info(string message, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Info, className, methodName);
            }
        }

        public static void Info(string message, Account account, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Info, account, className, methodName);
            }
        }
        #endregion

        #region DEBUG
        public static void Debug(string message)
        {
            if (DebugModeIsEnable)
            {
                lock (logLock)
                {
                    WriteToConsole(ref message, LogMessageType.Debug);
                } 
            }
        }

        public static void Debug(string message, Account account)
        {
            if (DebugModeIsEnable)
            {
                lock (logLock)
                {
                    WriteToConsole(ref message, LogMessageType.Debug, account);
                } 
            }
        }

        public static void Debug(string message, string className, string methodName)
        {
            if (DebugModeIsEnable)
            {
                lock (logLock)
                {
                    WriteToConsole(ref message, LogMessageType.Debug, className, methodName);
                } 
            }
        }

        public static void Debug(string message, Account account, string className, string methodName)
        {
            if (DebugModeIsEnable)
            {
                lock (logLock)
                {
                    WriteToConsole(ref message, LogMessageType.Debug, account, className, methodName);
                } 
            }
        }
        #endregion

        #region SUCCESS
        public static void Success(string message)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Success);
            }
        }

        public static void Success(string message, Account account)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Success, account);
            }
        }

        public static void Success(string message, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Success, className, methodName);
            }
        }

        public static void Success(string message, Account account, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Success, account, className, methodName);
            }
        }
        #endregion

        #region WARNING
        public static void Warning(string message)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Warning);
                WriteToFile(message, LogMessageType.Warning);
            }
        }

        public static void Warning(string message, Account account)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Warning, account);
                WriteToFile(message, LogMessageType.Warning, account);
            }
        }

        public static void Warning(string message, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Warning, className, methodName);
                WriteToFile(message, LogMessageType.Warning, className, methodName);
            }
        }

        public static void Warning(string message, Account account, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Warning, account, className, methodName);
                WriteToFile(message, LogMessageType.Warning, account, className, methodName);
            }
        }
        #endregion

        #region ERROR
        public static void Error(string message)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Error);
                WriteToFile(message, LogMessageType.Error);
            }
        }

        public static void Error(string message, Account account)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Error, account);
                WriteToFile(message, LogMessageType.Error, account);
            }
        }

        public static void Error(string message, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Error, className, methodName);
                WriteToFile(message, LogMessageType.Error, className, methodName);
            }
        }

        public static void Error(string message, Account account, string className, string methodName)
        {
            lock (logLock)
            {
                WriteToConsole(ref message, LogMessageType.Error, account, className, methodName);
                WriteToFile(message, LogMessageType.Error, account, className, methodName);
            }
        }
        #endregion

        #region WRITE TO CONSOLE
        private static void WriteToConsole(ref readonly string message, LogMessageType messageType)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            Console.ForegroundColor = logMessageColors[messageType];

            switch (messageType)
            {
                case LogMessageType.Info:
                    Console.Write($"[{logMessageTypeName[messageType]}]    ---> ");
                    break;
                case LogMessageType.Debug:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
                case LogMessageType.Success:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Warning:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Error:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
            }

            Console.Write(message);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void WriteToConsole(ref readonly string message, LogMessageType messageType, Account account)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            Console.ForegroundColor = logMessageColors[messageType];

            switch (messageType)
            {
                case LogMessageType.Info:
                    Console.Write($"[{logMessageTypeName[messageType]}]    ---> ");
                    break;
                case LogMessageType.Debug:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
                case LogMessageType.Success:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Warning:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Error:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
            }

            //Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[Account {account.PhoneNumber}] ");
            Console.Write(message);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void WriteToConsole(ref readonly string message, LogMessageType messageType, string className, string methodName)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            Console.ForegroundColor = logMessageColors[messageType];

            switch (messageType)
            {
                case LogMessageType.Info:
                    Console.Write($"[{logMessageTypeName[messageType]}]    ---> ");
                    break;
                case LogMessageType.Debug:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
                case LogMessageType.Success:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Warning:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Error:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
            }

            //Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[Class {className}] ");
            Console.Write($"[Method {methodName}] ");
            Console.Write(message);
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void WriteToConsole(ref readonly string message, LogMessageType messageType, Account account, string className, string methodName)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            Console.ForegroundColor = logMessageColors[messageType];

            switch (messageType)
            {
                case LogMessageType.Info:
                    Console.Write($"[{logMessageTypeName[messageType]}]    ---> ");
                    break;
                case LogMessageType.Debug:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
                case LogMessageType.Success:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Warning:
                    Console.Write($"[{logMessageTypeName[messageType]}] ---> ");
                    break;
                case LogMessageType.Error:
                    Console.Write($"[{logMessageTypeName[messageType]}]   ---> ");
                    break;
            }

            //Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[Account {account.PhoneNumber}] ");
            Console.Write($"[Class {className}] ");
            Console.Write($"[Method {methodName}] ");
            Console.Write(message);
            Console.WriteLine();
            Console.ResetColor();
        }
        #endregion

        #region WRITE TO FILE
        private static void WriteToFile(string message, LogMessageType messageType)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string file = Path.Combine(logDirectory, $"{logMessageTypeName[messageType]}.txt");

            //if (!File.Exists(file))
            //{
            //    File.Create(file);
            //}

            stringBuilder.Clear();
            stringBuilder.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            stringBuilder.Append($"[{logMessageTypeName[messageType]}]\n");
            stringBuilder.Append(message);

            using (StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            {
                writer.WriteLine(stringBuilder.ToString());
            }

            //using(StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            //{
            //    await writer.WriteLineAsync(stringBuilder.ToString());
            //}

            //using (var stream = File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            //    {
            //        await writer.WriteLineAsync(stringBuilder.ToString());
            //    }
            //}
        }

        private static void WriteToFile(string message, LogMessageType messageType, Account account)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string file = Path.Combine(logDirectory, $"{logMessageTypeName[messageType]}.txt");

            //if (!File.Exists(file))
            //{
            //    File.Create(file);
            //}

            stringBuilder.Clear();
            stringBuilder.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            stringBuilder.Append($"[{logMessageTypeName[messageType]}] ");
            stringBuilder.Append($"[Account {account.PhoneNumber}] ");
            stringBuilder.Append(message);

            using (StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            {
                writer.WriteLine(stringBuilder.ToString());
            }

            //using(StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            //{
            //    await writer.WriteLineAsync(stringBuilder.ToString());
            //}

            //using (var stream = File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            //    {
            //        await writer.WriteLineAsync(stringBuilder.ToString());
            //    }
            //}
        }

        private static void WriteToFile(string message, LogMessageType messageType, string className, string methodName)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string file = Path.Combine(logDirectory, $"{logMessageTypeName[messageType]}.txt");

            //if (!File.Exists(file))
            //{
            //    File.Create(file);
            //}

            stringBuilder.Clear();
            stringBuilder.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            stringBuilder.Append($"[{logMessageTypeName[messageType]}] ");
            stringBuilder.Append($"[Class {className}] ");
            stringBuilder.Append($"[Method {methodName}]\n");
            stringBuilder.Append(message);

            using (StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            {
                writer.WriteLine(stringBuilder.ToString());
            }

            //using(StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            //{
            //    await writer.WriteLineAsync(stringBuilder.ToString());
            //}

            //using (var stream = File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            //    {
            //        await writer.WriteLineAsync(stringBuilder.ToString());
            //    }
            //}
        }

        private static void WriteToFile(string message, LogMessageType messageType, Account account, string className, string methodName)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string file = Path.Combine(logDirectory, $"{logMessageTypeName[messageType]}.txt");

            //if (!File.Exists(file))
            //{
            //    File.Create(file);
            //}

            stringBuilder.Clear();
            stringBuilder.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ");
            stringBuilder.Append($"[{logMessageTypeName[messageType]}] ");
            stringBuilder.Append($"[Account {account.PhoneNumber}] ");
            stringBuilder.Append($"[Class {className}] ");
            stringBuilder.Append($"[Method {methodName}]\n");
            stringBuilder.Append(message);

            using (StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            {
                writer.WriteLine(stringBuilder.ToString());
            }

            //using(StreamWriter writer = new StreamWriter(file, true, Encoding.UTF8))
            //{
            //    await writer.WriteLineAsync(stringBuilder.ToString());
            //}

            //using (var stream = File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            //    {
            //        await writer.WriteLineAsync(stringBuilder.ToString());
            //    }
            //}
        } 
        #endregion
    }
}
