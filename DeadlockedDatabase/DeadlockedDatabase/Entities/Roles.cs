using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class Roles
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreateDt { get; set; }
        public bool? IsActive { get; set; }
    }
}
