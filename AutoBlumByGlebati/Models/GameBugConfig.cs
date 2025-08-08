using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Models
{
    public static class GameBugConfig
    {
        public enum GameType
        {
            Normal,
            WithBug
        }

        public static GameType CurrentGameType { get; set; } = GameType.Normal;
        public static int PlayPassesForGameWithBug { get; set; } = 0;
    }
}
