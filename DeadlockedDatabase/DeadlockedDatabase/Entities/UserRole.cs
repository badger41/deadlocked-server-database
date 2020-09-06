using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class UserRole
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int RoleId { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime FromDt { get; set; }
        public DateTime? ToDt { get; set; }
    }
}
