using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class Banned
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime FromDt { get; set; }
        public DateTime? ToDt { get; set; }
    }
}
