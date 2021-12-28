using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Models;
using DeadlockedDatabase.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeadlockedDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeysController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public KeysController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [Authorize("database")]
        [HttpGet, Route("getEULA")]
        public async Task<dynamic> getEULA(int? eulaId, DateTime? fromDt, DateTime? toDt)
        {
            dynamic eula = null;
            DateTime now = DateTime.UtcNow;
            if (eulaId != null)
            {
                eula = (from e in db.DimEula
                        where e.Id == eulaId
                        select e).FirstOrDefault();
            } else if(fromDt != null && toDt != null)
            {
                eula = (from e in db.DimEula
                        where e.FromDt <= fromDt
                        && (e.ToDt == null || e.ToDt >= toDt)
                        select e).FirstOrDefault();
            } else if(fromDt != null && toDt == null)
            {
                eula = (from e in db.DimEula
                        where e.FromDt <= fromDt
                        && (e.ToDt == null || e.ToDt >= now)
                        select e).FirstOrDefault();
            } else
            {
                return BadRequest("Please provide either a eulaId, or a valid fromDt or toDt.");
            }

            return eula;
        }

        [Authorize("database")]
        [HttpGet, Route("deleteEULA")]
        public async Task<dynamic> deleteEULA(int id)
        {
            var eula = db.DimEula.FirstOrDefault(x => x.Id == id);
            if (eula == null)
            {
                return StatusCode(403, "Cannot delete an eula entry that doesn't exist.");
            }

            db.DimEula.Remove(eula);
            db.SaveChanges();

            return Ok("EULA Deleted");
        }

        [Authorize("database")]
        [HttpPost, Route("updateEULA")]
        public async Task<dynamic> updateEULA([FromBody] ChangeEulaDTO request)
        {
            var eula = db.DimEula.FirstOrDefault(x => x.Id == request.Id);
            if (eula == null)
            {
                return StatusCode(403, "Cannot change an eula entry that doesn't exist.");
            }

            db.DimEula.Attach(eula);
            db.Entry(eula).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            eula.EulaTitle = request.EulaTitle ?? eula.EulaTitle;
            eula.EulaBody = request.EulaBody ?? eula.EulaBody;
            eula.ModifiedDt = DateTime.UtcNow;
            eula.FromDt = request.FromDt ?? eula.FromDt;
            eula.ToDt = request.ToDt ?? eula.ToDt;

            db.SaveChanges();

            return Ok("EULA Changed");
        }

        [Authorize("database")]
        [HttpPost, Route("postEULA")]
        public async Task<dynamic> postEULA([FromBody] AddEulaDTO request)
        {
            var eula = new DimEula()
            {
                EulaTitle = request.EulaTitle,
                EulaBody = request.EulaBody,
                FromDt = request.FromDt ?? DateTime.UtcNow,
                ToDt = request.ToDt,
                CreateDt = DateTime.UtcNow,
            };

            db.DimEula.Add(eula);
            db.SaveChanges();

            return Ok("EULA Added");
        }

        [Authorize("database")]
        [HttpGet, Route("getAnnouncements")]
        public async Task<dynamic> getAnnouncements(int? accouncementId, int? appId, DateTime? fromDt, DateTime? toDt, int AppId)
        {
            dynamic announcement = null;
            DateTime now = DateTime.UtcNow;
            if (accouncementId != null)
            {
                announcement = (from a in db.DimAnnouncements
                        where a.Id == accouncementId
                        select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt != null && appId != null)
            {
                announcement = (from a in db.DimAnnouncements
                                where a.AppId == AppId && a.FromDt <= fromDt
                        && (a.ToDt == null || a.ToDt >= toDt)
                        select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt == null)
            {
                announcement = (from a in db.DimAnnouncements
                                where a.AppId == AppId && a.FromDt <= fromDt
                        && (a.ToDt == null ||a.ToDt >= now)
                        select a).FirstOrDefault();
            }
            else
            {
                return BadRequest("Please provide either an accountmentId, or a valid fromDt or toDt.");
            }

            return announcement;
        }

        [Authorize("database")]
        [HttpGet, Route("getAnnouncementsList")]
        public async Task<dynamic> getAnnouncementsList(DateTime? Dt, int TakeSize = 10, int AppId)
        {
            dynamic announcements = null;
            if (Dt == null)
                Dt = DateTime.UtcNow;
            DateTime now = DateTime.UtcNow;
            announcements = (from a in db.DimAnnouncements
                             orderby a.FromDt
                            where a.AppId == AppId && a.FromDt <= Dt
                    && (a.ToDt == null || a.ToDt >= Dt)
                            select a).Take(TakeSize).ToList();

            return announcements;
        }

        [Authorize("database")]
        [HttpGet, Route("deleteAnnouncement")]
        public async Task<dynamic> deleteAnnouncement(int id)
        {
            var announcement = db.DimAnnouncements.FirstOrDefault(x => x.Id == id);
            if (announcement == null)
            {
                return StatusCode(403, "Cannot delete an announcement that doesn't exist.");
            }

            db.DimAnnouncements.Remove(announcement);
            db.SaveChanges();

            return Ok("Announcement Deleted");
        }

        [Authorize("database")]
        [HttpPost, Route("updateAnnouncement")]
        public async Task<dynamic> updateAnnouncement([FromBody] ChangeAnnouncementDTO request)
        {
            var announcement = db.DimAnnouncements.FirstOrDefault(x => x.Id == request.Id);
            if (announcement == null)
            {
                return StatusCode(403, "Cannot change an announcement that doesn't exist.");
            }

            db.DimAnnouncements.Attach(announcement);
            db.Entry(announcement).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            announcement.AnnouncementTitle = request.AnnouncementTitle ?? announcement.AnnouncementTitle;
            announcement.AnnouncementBody = request.AnnouncementBody ?? announcement.AnnouncementBody;
            announcement.ModifiedDt = DateTime.UtcNow;
            announcement.FromDt = request.FromDt ?? announcement.FromDt;
            announcement.ToDt = request.ToDt ?? announcement.ToDt;
            announcement.AppId = request.AppId;

            db.SaveChanges();

            return Ok("Announcement Changed");
        }

        [Authorize("database")]
        [HttpPost, Route("postAnnouncement")]
        public async Task<dynamic> postAnnouncement([FromBody] AddAnnouncementDTO request)
        {
            var announcement = new DimAnnouncements()
            {
                AnnouncementTitle = request.AnnouncementTitle,
                AnnouncementBody = request.AnnouncementBody,
                FromDt = request.FromDt ?? DateTime.UtcNow,
                ToDt = request.ToDt,
                CreateDt = DateTime.UtcNow,
                AppId = request.AppId,
            };

            db.DimAnnouncements.Add(announcement);
            db.SaveChanges();

            return Ok("Announcement Added");
        }

        [Authorize("database")]
        [HttpPost, Route("postMaintenanceFlag")]
        public async Task<dynamic> postMaintenanceFlag([FromBody] MaintenanceDTO request)
        {
            var existingData = db.ServerFlags.Where(acs => acs.ServerFlag == "maintenance_mode").FirstOrDefault();
            if (existingData != null)
            {
                existingData.Value = request.IsActive.ToString();
                existingData.FromDt = request.FromDt;
                existingData.ToDt = request.ToDt;
                db.ServerFlags.Attach(existingData);
                db.Entry(existingData).State = EntityState.Modified;
            }
            else
            {
                var flag = new ServerFlags()
                {
                    ServerFlag = "maintenance_mode",
                    FromDt = request.FromDt,
                    ToDt = request.ToDt,
                    Value = request.IsActive.ToString()
                };
                db.ServerFlags.Add(flag);
            }
            db.SaveChanges();

            return Ok("Maintenance Flag Added");
        }

        [Authorize("database")]
        [HttpGet, Route("getServerFlags")]
        public async Task<dynamic> getServerFlags()
        {
            var flags = (from sg in db.ServerFlags
                         select sg).ToList();

            return new ServerFlagsDTO()
            {
                MaintenanceMode = flags.Where(f => f.ServerFlag == "maintenance_mode").Select(f => new MaintenanceDTO()
                {
                    IsActive = bool.Parse(f.Value),
                    FromDt = f.FromDt,
                    ToDt = f.ToDt
                }).FirstOrDefault(),
            };
        }
    }
}
