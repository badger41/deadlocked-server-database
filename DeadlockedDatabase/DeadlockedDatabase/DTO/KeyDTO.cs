using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.DTO
{
    public class KeyDTO
    {
    }

    public class ServerFlagsDTO
    {
        public MaintenanceDTO MaintenanceMode { get; set; }
    }

    public class MaintenanceDTO
    {
        public bool IsActive { get; set; }
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }
    }
}
