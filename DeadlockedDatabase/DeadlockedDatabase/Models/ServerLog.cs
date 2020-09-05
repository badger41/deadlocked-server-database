using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Models
{
    public partial class ServerLog
    {
        public int Id { get; set; }
        public DateTime LogDt { get; set; }
        public int? AccountId { get; set; }
        public string MethodName { get; set; }
        public string LogTitle { get; set; }
        public string LogMsg { get; set; }
        public string LogStacktrace { get; set; }
        public string Payload { get; set; }
    }
}
