using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DeadlockedDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public AccountController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [HttpGet, Route("getAccountExists")]
        public async Task<bool> getAccountExists(string AccountName)
        {
            Account acc = (from a in db.Account
                           where a.AccountName == AccountName
                           && a.IsActive == true
                           select a).FirstOrDefault();
            return acc != null;
        }
        [HttpGet, Route("getActiveAccountCountByAppId")]
        public async Task<int> getActiveAccountCountByAppId(int AppId)
        {
            int accountCount = (from a in db.Account
                                where a.AppId == AppId
                                && a.IsActive == true
                                select a).Count();
            return accountCount;
        }


        [HttpGet, Route("getAccount")]
        public async Task<AccountDTO> getAccount(int AccountId)
        {
            Account existingAccount = db.Account.Include(a => a.AccountFriend)
                                                .Include(a => a.AccountIgnored)
                                                .Include(a => a.AccountStat)
                                                .Where(a => a.AccountId == AccountId)
                                                .FirstOrDefault();

            AccountDTO account = new AccountDTO()
            {
                AccountId = existingAccount.AccountId,
                AccountName = existingAccount.AccountName,
                AccountPassword = existingAccount.AccountPassword,
                Friends = (from f in existingAccount.AccountFriend
                           join a in db.Account
                            on f.FriendAccountId equals a.AccountId
                           select new AccountRelationDTO
                           {
                               AccountId = f.FriendAccountId,
                               AccountName = a.AccountName
                           }).ToList(),
                Ignored = (from f in existingAccount.AccountIgnored
                            join a in db.Account
                            on f.IgnoredAccountId equals a.AccountId
                           select new AccountRelationDTO
                           {
                               AccountId = f.IgnoredAccountId,
                               AccountName = a.AccountName
                           }).ToList(),
                AccountWideStats = existingAccount.AccountStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                MediusStats = existingAccount.MediusStats,
                IsBanned = false,
                AppId = existingAccount.AppId,
            };

            return account;
        }

        [HttpPost, Route("createAccount")]
        public async Task<dynamic> createAccount([FromBody] AccountRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Where(a => a.AccountName == request.AccountName).FirstOrDefault();
            if (existingAccount == null || existingAccount.IsActive == false)
            {
                if (existingAccount == null)
                {
                    Account acc = new Account()
                    {
                        AccountName = request.AccountName,
                        AccountPassword = request.AccountPassword,
                        CreateDt = now,
                        LastSignInDt = now,
                        MachineId = request.MachineId,
                        MediusStats = request.MediusStats,
                        AppId = request.AppId,
                    };

                    db.Account.Add(acc);
                    db.SaveChanges();


                    List<AccountStat> newStats = (from ds in db.DimStats
                                                  select new AccountStat()
                                                  {
                                                      AccountId = acc.AccountId,
                                                      StatId = ds.StatId,
                                                      StatValue = ds.DefaultValue
                                                  }).ToList();
                    db.AccountStat.AddRange(newStats);
                    db.SaveChanges();
                    return await getAccount(acc.AccountId);
                } else
                {
                    existingAccount.IsActive = true;
                    existingAccount.AccountPassword = request.AccountPassword;
                    existingAccount.ModifiedDt = now;
                    existingAccount.MediusStats = request.MediusStats;
                    existingAccount.AppId = request.AppId;
                    existingAccount.MachineId = request.MachineId;
                    existingAccount.LastSignInDt = now;
                    db.Account.Attach(existingAccount);
                    db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    List<AccountStat> newStats = (from ds in db.DimStats
                                                  select new AccountStat()
                                                  {
                                                      AccountId = existingAccount.AccountId,
                                                      StatId = ds.StatId,
                                                      StatValue = ds.DefaultValue
                                                  }).ToList();
                    db.AccountStat.AddRange(newStats);

                    db.SaveChanges();
                    return await getAccount(existingAccount.AccountId);
                }

            } else
            {
                return StatusCode(403, $"Account {request.AccountName} already exists.");
            }
        }

        [HttpGet, Route("deleteAccount")]
        public async Task<dynamic> deleteAccount(string AccountName)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Where(a => a.AccountName == AccountName).FirstOrDefault();
            if(existingAccount == null || existingAccount.IsActive == false)
            {
                return StatusCode(403, "Cannot delete an account that doesn't exist.");
            }

            existingAccount.IsActive = false;
            existingAccount.ModifiedDt = now;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            AccountDTO otherData = await getAccount(existingAccount.AccountId);

            List<AccountStat> existingStats = db.AccountStat.Where(s => s.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingStats);

            List<AccountFriend> existingFriends = db.AccountFriend.Where(s => s.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingFriends);

            List<AccountIgnored> existingIgnores = db.AccountIgnored.Where(ai => ai.AccountId == existingAccount.AccountId).ToList();
            db.RemoveRange(existingIgnores);

            db.SaveChanges();
            return Ok("Account Deleted");
        }

        [HttpGet, Route("searchAccountByName")]
        public async Task<dynamic> searchAccountByName(string AccountName)
        {
            Account existingAccount = db.Account.Where(a => a.AccountName == AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            return await getAccount(existingAccount.AccountId);
        }

        [HttpPost, Route("postMediusStats")]
        public async Task<dynamic> postMediusStats([FromBody] string StatsString, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.MediusStats = StatsString;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [HttpPost, Route("postAccountSignInDate")]
        public async Task<dynamic> postAccountSignInDate([FromBody] DateTime SignInDt, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();

            if (existingAccount == null)
                return NotFound();

            existingAccount.LastSignInDt = SignInDt;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();

            return Ok();
        }

        [HttpGet, Route("getAccountStatus")]
        public async Task<dynamic> getAccountStatus(int AccountId)
        {
            AccountStatus existingData = db.AccountStatus.Where(acs => acs.AccountId == AccountId).FirstOrDefault();
            if (existingData == null)
                return NotFound();

            return existingData;
        }

        [HttpPost, Route("postAccountStatusUpdates")]
        public async Task<dynamic> postAccountStatusUpdates([FromBody] AccountStatusDTO StatusData)
        {
            AccountStatus existingData = db.AccountStatus.Where(acs => acs.AccountId == StatusData.AccountId).FirstOrDefault();
            if(existingData != null)
            {
                existingData.LoggedIn = StatusData.LoggedIn;
                existingData.GameId = StatusData.GameId;
                existingData.ChannelId = StatusData.ChannelId;
                existingData.WorldId = StatusData.WorldId;
                db.AccountStatus.Attach(existingData);
                db.Entry(existingData).State = EntityState.Modified;
            } else
            {
                AccountStatus newStatusData = new AccountStatus()
                {
                    AccountId = StatusData.AccountId,
                    LoggedIn = StatusData.LoggedIn,
                    GameId = StatusData.GameId,
                    ChannelId = StatusData.ChannelId,
                    WorldId = StatusData.WorldId
                };
                db.AccountStatus.Add(newStatusData);
            }
            db.SaveChanges();

            return await getAccountStatus(StatusData.AccountId);
        }
    }
}
