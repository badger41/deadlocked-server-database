using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class ClanMessage
    {
        public int Id { get; set; }
        public int ClanId { get; set; }
        public string Message { get; set; }
        public DateTime CreateDt { get; set; }
        public int CreatedBy { get; set; }
        public bool? IsActive { get; set; }

        public virtual Clan Clan { get; set; }
    }
}
