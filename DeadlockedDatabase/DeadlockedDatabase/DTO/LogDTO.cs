using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.DTO
{
    public class LogDTO
    {
        public DateTime Timestamp { get; set; }
        public int? AccountId { get; set; }
        public string MethodName { get; set; }
        public string LogTitle { get; set; }
        public string LogMsg { get; set; }
        public string LogStacktrace { get; set; }
        public string Payload { get; set; }
    }
}
