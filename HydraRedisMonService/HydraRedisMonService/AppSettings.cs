using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydraRedisMonService
{
    public class AppSettings
    {
        public int SweepIntervalInSeconds { get; set; } = 0;
        public int RedisHydraDb { get; set; } = 0;
    }
}
