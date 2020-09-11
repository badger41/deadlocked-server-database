using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class BannedIp
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public DateTime FromDt { get; set; }
        public DateTime? ToDt { get; set; }
    }
}
