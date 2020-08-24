using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeadlockedDatabase.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public StatsController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [HttpGet, Route("initStats")]
        public async Task<dynamic> initStats(int AccountId)
        {
            AccountController ac = new AccountController(db);
            AccountDTO existingAcc = await ac.getAccount(AccountId);

            if (existingAcc == null)
                return BadRequest($"Account Id {AccountId} doesn't exist.");

            if (existingAcc.AccountWideStats.Count() > 0)
                return StatusCode(403, "This account already has stats.");

            List<AccountStat> newStats = (from ds in db.DimStats
                                          select new AccountStat()
                                          {
                                              AccountId = existingAcc.AccountId,
                                              StatId = ds.StatId,
                                              StatValue = ds.DefaultValue
                                          }).ToList();
            db.AccountStat.AddRange(newStats);
            db.SaveChanges();

            return Ok("Stats Created");
        }

        [HttpGet, Route("getPlayerLeaderboardIndex")]
        public async Task<dynamic> getPlayerLeaderboardIndex(int AccountId, int StatId)
        {
            List<AccountStat> stats = db.AccountStat.Where(s => s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).ToList();
            AccountStat statForAccount = stats.Where(s => s.AccountId == AccountId).FirstOrDefault();
            Account acc = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            AccountController ac = new AccountController(db);
            int totalAccounts = await ac.getActiveAccountCountByAppId((int)acc.AppId);
            if (acc.IsActive == true)
            {
                return new LeaderboardDTO()
                {
                    TotalRankedAccounts = totalAccounts,
                    AccountId = AccountId,
                    Index = stats.IndexOf(statForAccount),
                    StartIndex = stats.IndexOf(statForAccount),
                    StatValue = statForAccount.StatValue,
                    AccountName = acc.AccountName,
                    MediusStats = acc.MediusStats
                };
            }
            return StatusCode(400, $"Account {AccountId} is inactive.");
        }

        [HttpGet, Route("getLeaderboard")]
        public async Task<List<LeaderboardDTO>> getLeaderboard(int StatId, int StartIndex, int Size)
        {
            List<AccountStat> stats = db.AccountStat.Where(s => s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).Skip(StartIndex).Take(Size).ToList();
            AccountController ac = new AccountController(db);

            List<LeaderboardDTO> board = (from s in stats
                                          join a in db.Account
                                            on s.AccountId equals a.AccountId
                                          select new LeaderboardDTO()
                                          {
                                              TotalRankedAccounts = 0,
                                              StartIndex = StartIndex,
                                              Index = StartIndex + stats.IndexOf(s),
                                              AccountId = s.AccountId,
                                              AccountName = a.AccountName,
                                              StatValue = s.StatValue,
                                              MediusStats = a.MediusStats
                                          }).ToList();

            return board;
        }

        [HttpPost, Route("postStats")]
        public async Task<dynamic> postStats([FromBody] StatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<AccountStat> playerStats = db.AccountStat.Where(s => s.AccountId == statData.AccountId).OrderBy(s => s.StatId).Select(s => s).ToList();

            int badStats = playerStats.Where(s => s.StatValue < 0).Count();
            if(badStats > 0)
                return BadRequest("Found a negative stat in array. Can't have those!");

            foreach (AccountStat pStat in playerStats)
            {
                
                int newValue = statData.stats[pStat.StatId - 1];
                pStat.ModifiedDt = modifiedDt;
                pStat.StatValue = newValue;

                db.AccountStat.Attach(pStat);
                db.Entry(pStat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
            return Ok();

        }
    }
}
