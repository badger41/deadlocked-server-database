using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class BannedMac
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public DateTime FromDt { get; set; }
        public DateTime? ToDt { get; set; }
    }
}
