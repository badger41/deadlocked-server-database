using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeadlockedDatabase.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BuddyController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public BuddyController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [Authorize("database")]
        [HttpPost, Route("addBuddy")]
        public async Task<dynamic> addBuddy([FromBody] BuddyDTO buddyReq)
        {
            AccountFriend existingFriend = db.AccountFriend.Where(af => af.AccountId == buddyReq.AccountId && af.FriendAccountId == buddyReq.BuddyAccountId).FirstOrDefault();
            AccountIgnored existingIgnored = db.AccountIgnored.Where(af => af.AccountId == buddyReq.AccountId && af.IgnoredAccountId == buddyReq.BuddyAccountId).FirstOrDefault();

            if (existingFriend != null)
                return StatusCode(403, "Buddy already exists.");

            if (existingIgnored != null)
            {
                db.AccountIgnored.Attach(existingIgnored);
                db.Entry(existingIgnored).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            AccountFriend newFriend = new AccountFriend()
            {
                AccountId = buddyReq.AccountId,
                FriendAccountId = buddyReq.BuddyAccountId,
                CreateDt = DateTime.UtcNow
            };
            db.AccountFriend.Add(newFriend);
            db.SaveChanges();

            return Ok("Buddy Added");
        }

        [Authorize("database")]
        [HttpPost, Route("removeBuddy")]
        public async Task<dynamic> removeBuddy([FromBody] BuddyDTO buddyReq)
        {
            AccountFriend existingFriend = db.AccountFriend.Where(af => af.AccountId == buddyReq.AccountId && af.FriendAccountId == buddyReq.BuddyAccountId).FirstOrDefault();

            if (existingFriend == null)
                return StatusCode(403, "Cannot remove a buddy that isn't a buddy.");

            db.AccountFriend.Attach(existingFriend);
            db.Entry(existingFriend).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            db.SaveChanges();

            return Ok("Buddy Removed");
        }

        [Authorize("database")]
        [HttpPost, Route("addIgnored")]
        public async Task<dynamic> addIgnored([FromBody] IgnoredDTO ignoreReq)
        {
            AccountIgnored existingIgnored = db.AccountIgnored.Where(af => af.AccountId == ignoreReq.AccountId && af.IgnoredAccountId == ignoreReq.IgnoredAccountId).FirstOrDefault();
            AccountFriend existingFriend = db.AccountFriend.Where(af => af.AccountId == ignoreReq.AccountId && af.FriendAccountId == ignoreReq.IgnoredAccountId).FirstOrDefault();

            if (existingIgnored != null)
                return StatusCode(403, "This player is already ignored.");

            if(existingFriend != null)
            {
                db.AccountFriend.Attach(existingFriend);
                db.Entry(existingFriend).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }

            AccountIgnored newIgnore = new AccountIgnored()
            {
                AccountId = ignoreReq.AccountId,
                IgnoredAccountId = ignoreReq.IgnoredAccountId,
                CreateDt = DateTime.UtcNow
            };
            db.AccountIgnored.Add(newIgnore);
            db.SaveChanges();

            return Ok("Player Ignored");
        }

        [Authorize("database")]
        [HttpPost, Route("removeIgnored")]
        public async Task<dynamic> removeIgnored([FromBody] IgnoredDTO ignoreReq)
        {
            AccountIgnored existingIgnored = db.AccountIgnored.Where(af => af.AccountId == ignoreReq.AccountId && af.IgnoredAccountId == ignoreReq.IgnoredAccountId).FirstOrDefault();

            if (existingIgnored == null)
                return StatusCode(403, "Cannot unignore a player that isn't ignored.");

            db.AccountIgnored.Attach(existingIgnored);
            db.Entry(existingIgnored).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            db.SaveChanges();

            return Ok("Player Unignored");
        }
    }
}
