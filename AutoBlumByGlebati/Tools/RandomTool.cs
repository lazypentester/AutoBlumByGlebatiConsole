using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBlumByGlebati.Tools
{
    public static class RandomTool
    {
        public static int GetRandomNumInt(int from, int to)
        {
            return Random.Shared.Next(from, to);
        }

        public static TimeSpan GetRandomTimeSec(int from, int to)
        {
            return TimeSpan.FromSeconds(Random.Shared.Next(from, to));
        }
    }
}
