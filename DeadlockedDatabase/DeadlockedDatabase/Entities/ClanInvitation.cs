using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class ClanInvitation
    {
        public int Id { get; set; }
        public int ClanId { get; set; }
        public int AccountId { get; set; }
        public int? ResponseId { get; set; }
        public string ResponseMsg { get; set; }
        public DateTime? ResponseDt { get; set; }
        public DateTime CreateDt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDt { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public string InviteMsg { get; set; }

        public virtual Account Account { get; set; }
        public virtual Clan Clan { get; set; }
    }
}
