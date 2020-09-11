using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class Account
    {
        public Account()
        {
            AccountFriend = new HashSet<AccountFriend>();
            AccountIgnored = new HashSet<AccountIgnored>();
            AccountStat = new HashSet<AccountStat>();
        }

        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime? ModifiedDt { get; set; }
        public DateTime? LastSignInDt { get; set; }
        public string MachineId { get; set; }
        public bool? IsActive { get; set; }
        public int? AppId { get; set; }
        public string MediusStats { get; set; }
        public string LastSignInIp { get; set; }

        public virtual ICollection<AccountFriend> AccountFriend { get; set; }
        public virtual ICollection<AccountIgnored> AccountIgnored { get; set; }
        public virtual ICollection<AccountStat> AccountStat { get; set; }
    }
}
