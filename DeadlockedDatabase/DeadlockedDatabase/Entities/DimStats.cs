using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class DimStats
    {
        public DimStats()
        {
            AccountStat = new HashSet<AccountStat>();
        }

        public int StatId { get; set; }
        public string StatName { get; set; }
        public int DefaultValue { get; set; }

        public virtual ICollection<AccountStat> AccountStat { get; set; }
    }
}
