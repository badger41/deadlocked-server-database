using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Models
{
    public partial class AccountIgnored
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int IgnoredAccountId { get; set; }
        public DateTime CreateDt { get; set; }

        public virtual Account Account { get; set; }
    }
}
