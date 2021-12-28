using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using DeadlockedDatabase.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeadlockedDatabase.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        private IAuthService authService;
        public StatsController(Ratchet_DeadlockedContext _db, IAuthService _authService)
        {
            db = _db;
            authService = _authService;
        }

        [Authorize("database")]
        [HttpGet, Route("initStats")]
        public async Task<dynamic> initStats(int AccountId)
        {
            AccountController ac = new AccountController(db, authService);
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

        [Authorize]
        [HttpGet, Route("getPlayerLeaderboardIndex")]
        public async Task<dynamic> getPlayerLeaderboardIndex(int AccountId, int StatId)
        {
            List<AccountStat> stats = db.AccountStat.Where(s => s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).ToList();
            AccountStat statForAccount = stats.Where(s => s.AccountId == AccountId).FirstOrDefault();
            Account acc = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            AccountController ac = new AccountController(db, authService);
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

        [Authorize]
        [HttpGet, Route("getClanLeaderboardIndex")]
        public async Task<dynamic> getClanLeaderboardIndex(int ClanId, int StatId)
        {
            List<ClanStat> stats = db.ClanStat.Where(s => s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).ToList();
            ClanStat statForClan = stats.Where(s => s.ClanId == ClanId).FirstOrDefault();
            Clan clan = db.Clan.Where(a => a.ClanId == ClanId).FirstOrDefault();
            ClanController cc = new ClanController(db, authService);
            int totalClans = await cc.getActiveClanCountByAppId((int)clan.AppId);
            if (clan.IsActive == true)
            {
                return new ClanLeaderboardDTO()
                {
                    TotalRankedClans = totalClans,
                    ClanId = ClanId,
                    Index = stats.IndexOf(statForClan),
                    StartIndex = stats.IndexOf(statForClan),
                    StatValue = statForClan.StatValue,
                    ClanName = clan.ClanName,
                    MediusStats = clan.MediusStats
                };
            }
            return StatusCode(400, $"Clan {ClanId} is inactive.");
        }

        [Authorize]
        [HttpGet, Route("getLeaderboard")]
        public async Task<List<LeaderboardDTO>> getLeaderboard(int StatId, int StartIndex, int Size)
        {
            List<AccountStat> stats = db.AccountStat.Where(s => s.Account.IsActive == true && s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.AccountId).Skip(StartIndex).Take(Size).ToList();
            AccountController ac = new AccountController(db, authService);

            List<LeaderboardDTO> board = (from s in stats
                                          join a in db.Account
                                            on s.AccountId equals a.AccountId
                                          where a.IsActive == true
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

        [Authorize]
        [HttpGet, Route("getClanLeaderboard")]
        public async Task<List<ClanLeaderboardDTO>> getClanLeaderboard(int StatId, int StartIndex, int Size)
        {
            List<ClanStat> stats = db.ClanStat.Where(s => s.Clan.IsActive == true && s.StatId == StatId).OrderByDescending(s => s.StatValue).ThenBy(s => s.ClanId).Skip(StartIndex).Take(Size).ToList();

            List<ClanLeaderboardDTO> board = (from s in stats
                                          join c in db.Clan
                                            on s.ClanId equals c.ClanId
                                            where c.IsActive == true
                                          select new ClanLeaderboardDTO()
                                          {
                                              TotalRankedClans = 0,
                                              StartIndex = StartIndex,
                                              Index = StartIndex + stats.IndexOf(s),
                                              ClanId = s.ClanId,
                                              ClanName = c.ClanName,
                                              StatValue = s.StatValue,
                                              MediusStats = c.MediusStats
                                          }).ToList();

            return board;
        }

        [Authorize("database")]
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

        [Authorize("database")]
        [HttpPost, Route("postClanStats")]
        public async Task<dynamic> postClanStats([FromBody] ClanStatPostDTO statData)
        {
            DateTime modifiedDt = DateTime.UtcNow;
            List<ClanStat> clanStats = db.ClanStat.Where(s => s.ClanId == statData.ClanId).OrderBy(s => s.StatId).Select(s => s).ToList();

            int badStats = clanStats.Where(s => s.StatValue < 0).Count();
            if (badStats > 0)
                return BadRequest("Found a negative stat in array. Can't have those!");

            foreach (ClanStat cStat in clanStats)
            {

                int newValue = statData.stats[cStat.StatId - 1];
                cStat.ModifiedDt = modifiedDt;
                cStat.StatValue = newValue;

                db.ClanStat.Attach(cStat);
                db.Entry(cStat).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            db.SaveChanges();
            return Ok();

        }

    }
}
