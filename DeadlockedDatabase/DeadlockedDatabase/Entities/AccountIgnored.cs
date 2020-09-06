using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
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
