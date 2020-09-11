using System;
using System.Collections.Generic;

namespace DeadlockedDatabase.Entities
{
    public partial class ServerFlags
    {
        public int Id { get; set; }
        public string ServerFlag { get; set; }
        public string Value { get; set; }
    }
}
