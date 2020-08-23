using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.DTO
{
    public class StatDTO
    {
    }

    public class LeaderboardDTO
    {
        public int StartIndex { get; set; }
        public int Index { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int StatValue { get; set; }
        public string MediusStats { get; set; }
        public int TotalRankedAccounts { get; set; }
    }

    public class StatPostDTO
    {
        public int AccountId { get; set; }
        public List<int> stats { get; set; }
    }
}
