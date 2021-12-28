using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.DTO
{
    public class AddAnnouncementDTO
    {
        public string AnnouncementTitle { get; set; }
        public string AnnouncementBody { get; set; }
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }
        public int AppId { get; set; }
    }

    public class ChangeAnnouncementDTO
    {
        public int Id { get; set; }
        public string AnnouncementTitle { get; set; }
        public string AnnouncementBody { get; set; }
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }
        public int AppId { get; set; }
    }
}
