using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize]
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

        [Authorize]
        [HttpGet, Route("getAnnouncements")]
        public async Task<dynamic> getAnnouncements(int? accouncementId, DateTime? fromDt, DateTime? toDt)
        {
            dynamic announcement = null;
            DateTime now = DateTime.UtcNow;
            if (accouncementId != null)
            {
                announcement = (from a in db.DimAnnouncements
                        where a.Id == accouncementId
                        select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt != null)
            {
                announcement = (from a in db.DimAnnouncements
                                where a.FromDt <= fromDt
                        && (a.ToDt == null || a.ToDt >= toDt)
                        select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt == null)
            {
                announcement = (from a in db.DimAnnouncements
                                where a.FromDt <= fromDt
                        && (a.ToDt == null ||a.ToDt >= now)
                        select a).FirstOrDefault();
            }
            else
            {
                return BadRequest("Please provide either an accountmentId, or a valid fromDt or toDt.");
            }

            return announcement;
        }

        [Authorize]
        [HttpGet, Route("getAnnouncementsList")]
        public async Task<dynamic> getAnnouncementsList(DateTime? Dt, int TakeSize = 10)
        {
            dynamic announcements = null;
            if (Dt == null)
                Dt = DateTime.UtcNow;
            DateTime now = DateTime.UtcNow;
            announcements = (from a in db.DimAnnouncements
                             orderby a.FromDt
                            where a.FromDt <= Dt
                    && (a.ToDt == null || a.ToDt >= Dt)
                            select a).Take(TakeSize).ToList();

            return announcements;
        }
    }
}
