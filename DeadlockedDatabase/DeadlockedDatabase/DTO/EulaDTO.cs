using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.DTO
{
    public class AddEulaDTO
    {
        public string EulaTitle { get; set; }
        public string EulaBody { get; set; }
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }

    }

    public class ChangeEulaDTO
    {
        public int Id { get; set; }
        public string EulaTitle { get; set; }
        public string EulaBody { get; set; }
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }

    }
}
