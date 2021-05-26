using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DeadlockedDatabase.Models;
using DeadlockedDatabase.Services;

namespace DeadlockedDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        private IAuthService authService;
        public AccountController(Ratchet_DeadlockedContext _db, IAuthService _authService)
        {
            db = _db;
            authService = _authService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticationRequest model)
        {
            var response = authService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet, Route("getAccountExists")]
        public async Task<bool> getAccountExists(string AccountName)
        {
            Account acc = (from a in db.Account
                           where a.AccountName == AccountName
                           && a.IsActive == true
                           select a).FirstOrDefault();
            return acc != null;
        }

        [Authorize]
        [HttpGet, Route("getActiveAccountCountByAppId")]
        public async Task<int> getActiveAccountCountByAppId(int AppId)
        {
            int accountCount = (from a in db.Account
                                where a.AppId == AppId
                                && a.IsActive == true
                                select a).Count();
            return accountCount;
        }

        [Authorize]
        [HttpGet, Route("getAccount")]
        public async Task<dynamic> getAccount(int AccountId)
        {
            DateTime now = DateTime.UtcNow;
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            //Account existingAccount = db.Account//.Include(a => a.AccountFriend)
            //                                    //.Include(a => a.AccountIgnored)
            //                                    .Include(a => a.AccountStat)
            //                                    .Where(a => a.AccountId == AccountId)
            //                                    .FirstOrDefault();



            if (existingAccount == null)
                return NotFound();

            var existingBan = (from b in db.Banned where b.AccountId == existingAccount.AccountId && b.FromDt <= now && (b.ToDt == null || b.ToDt > now) select b).FirstOrDefault();
            var accountList = db.Account.ToList();

            AccountDTO account2 = (from a in db.Account
                                   where a.AccountId == AccountId
                                   select new AccountDTO()
                                   {
                                       AccountId = a.AccountId,
                                       AccountName = a.AccountName,
                                       AccountPassword = a.AccountPassword,
                                       AccountWideStats = a.AccountStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
                                       Friends = new List<AccountRelationDTO>(),
                                       Ignored = new List<AccountRelationDTO>(),
                                       //Friends = a.AccountFriend.Select(af => new AccountRelationDTO()
                                       //{
                                       //    AccountId = af.FriendAccountId,
                                       //}).ToList(),
                                       //Ignored = a.AccountIgnored.Select(ai => new AccountRelationDTO()
                                       //{
                                       //    AccountId = ai.IgnoredAccountId,
                                       //}).ToList(),
                                       MediusStats = existingAccount.MediusStats,
                                       MachineId = existingAccount.MachineId,
                                       IsBanned = existingBan != null ? true : false,
                                       AppId = existingAccount.AppId,
                                   }).FirstOrDefault();
            List<int> friendIds = db.AccountFriend.Where(a => a.AccountId == AccountId).Select(a => a.FriendAccountId).ToList();
            List<int> ignoredIds = db.AccountIgnored.Where(a => a.AccountId == AccountId).Select(a => a.IgnoredAccountId).ToList();
            foreach (int friendId in friendIds)
            {
                AccountRelationDTO friendDTO = new AccountRelationDTO()
                {
                    AccountId = friendId,
                    AccountName = accountList.Where(a => a.AccountId == friendId).Select(a => a.AccountName).FirstOrDefault()
                };
                account2.Friends.Add(friendDTO);
            }
            foreach (int ignoredId in ignoredIds)
            {
                AccountRelationDTO friendDTO = new AccountRelationDTO()
                {
                    AccountId = ignoredId,
                    AccountName = accountList.Where(a => a.AccountId == ignoredId).Select(a => a.AccountName).FirstOrDefault()
                };
                account2.Friends.Add(friendDTO);
            }
            //foreach (AccountRelationDTO ignored in account2.Ignored)
            //{
            //    ignored.AccountName = accountList.Where(a => a.AccountId == ignored.AccountId).Select(a => a.AccountName).FirstOrDefault();
            //}

            //AccountDTO account = new AccountDTO()
            //{
            //    AccountId = existingAccount.AccountId,
            //    AccountName = existingAccount.AccountName,
            //    AccountPassword = existingAccount.AccountPassword,
            //    //Friends = (from f in existingAccount.AccountFriend
            //    //           join a in db.Account
            //    //            on f.FriendAccountId equals a.AccountId
            //    //           select new AccountRelationDTO
            //    //           {
            //    //               AccountId = f.FriendAccountId,
            //    //               AccountName = a.AccountName
            //    //           }).ToList(),
            //    //Ignored = (from f in existingAccount.AccountIgnored
            //    //            join a in db.Account
            //    //            on f.IgnoredAccountId equals a.AccountId
            //    //           select new AccountRelationDTO
            //    //           {
            //    //               AccountId = f.IgnoredAccountId,
            //    //               AccountName = a.AccountName
            //    //           }).ToList(),
            //    AccountWideStats = existingAccount.AccountStat.OrderBy(s => s.StatId).Select(s => s.StatValue).ToList(),
            //    MediusStats = existingAccount.MediusStats,
            //    MachineId = existingAccount.MachineId,
            //    IsBanned = existingBan != null ? true : false,
            //    AppId = existingAccount.AppId,
            //};

            return account2;
        }

        [Authorize("database")]
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
                        AccountPassword = request.PasswordPreHashed ? request.AccountPassword : Crypto.ComputeSHA256(request.AccountPassword),
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

                    AccountStatus newStatusData = new AccountStatus()
                    {
                        AccountId = acc.AccountId,
                        LoggedIn = false,
                        GameId = null,
                        ChannelId = null,
                        WorldId = null
                    };
                    db.AccountStatus.Add(newStatusData);

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

        [Authorize("database")]
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

        [Authorize]
        [HttpGet, Route("searchAccountByName")]
        public async Task<dynamic> searchAccountByName(string AccountName)
        {
            Account existingAccount = db.Account.Where(a => a.AccountName == AccountName && a.IsActive == true).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            return await getAccount(existingAccount.AccountId);
        }

        [Authorize("database")]
        [HttpPost, Route("postMachineId")]
        public async Task<dynamic> postMachineId([FromBody] string MachineId, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.MachineId = MachineId;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [Authorize("database")]
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
        [Authorize("database")]
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
        [Authorize("database")]
        [HttpPost, Route("postAccountIp")]
        public async Task<dynamic> postAccountIp([FromBody] string IpAddress, int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            existingAccount.LastSignInIp = IpAddress;
            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return Ok();
        }

        [Authorize]
        [HttpGet, Route("getAccountStatus")]
        public async Task<dynamic> getAccountStatus(int AccountId)
        {
            AccountStatus existingData = db.AccountStatus.Where(acs => acs.AccountId == AccountId).FirstOrDefault();
            if (existingData == null)
                return NotFound();

            return existingData;
        }

        [Authorize("database")]
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
                existingData.GameName = StatusData.GameName;
                db.AccountStatus.Attach(existingData);
                db.Entry(existingData).State = EntityState.Modified;
            }
            db.SaveChanges();

            return await getAccountStatus(StatusData.AccountId);
        }

        [Authorize]
        [HttpPost, Route("clearAccountStatuses")]
        public async Task<dynamic> clearAccountStatuses()
        {
            await db.AccountStatus.ForEachAsync(a =>
            {
                a.GameId = null;
                a.LoggedIn = false;
                a.WorldId = null;
                a.ChannelId = null;
                a.GameName = null;
            });

            db.SaveChanges();

            return Ok();
        }

        [Authorize("discord_bot")]
        [HttpGet, Route("getOnlineAccounts")]
        public async Task<dynamic> getOnlineAccounts()
        {
            var results = db.AccountStatus
                .Where(acs => acs.LoggedIn)
                .Select(s => new
                {
                    s.AccountId,
                    db.Account.FirstOrDefault(a => a.AccountId == s.AccountId).AccountName,
                    s.WorldId,
                    s.GameId,
                    s.GameName,
                    s.ChannelId,
                });

            return results;
        }

        [Authorize]
        [HttpPost, Route("changeAccountPassword")]
        public async Task<dynamic> changeAccountPassword([FromBody] AccountPasswordRequest PasswordRequest)
        {
            Account existingAccount = db.Account.Where(acs => acs.AccountId == PasswordRequest.AccountId).FirstOrDefault();
            if (existingAccount == null)
                return NotFound();

            if (Crypto.ComputeSHA256(PasswordRequest.OldPassword) != existingAccount.AccountPassword)
                return StatusCode(401, "The password you provided is incorrect.");

            if (PasswordRequest.NewPassword != PasswordRequest.ConfirmNewPassword)
                return StatusCode(400, "The new and confirmation passwords do not match each other. Please try again.");

            existingAccount.AccountPassword = Crypto.ComputeSHA256(PasswordRequest.NewPassword);
            existingAccount.ModifiedDt = DateTime.UtcNow;

            db.Account.Attach(existingAccount);
            db.Entry(existingAccount).State = EntityState.Modified;
            db.SaveChanges();

            return Ok("Password Updated");

        }

        [Authorize]
        [HttpPost, Route("getIpIsBanned")]
        public async Task<bool> getIpIsBanned([FromBody] string IpAddress)
        {
            DateTime now = DateTime.UtcNow;
            BannedIp ban = (from b in db.BannedIp
                            where b.IpAddress == IpAddress
                            && b.FromDt <= now
                            && (b.ToDt == null || b.ToDt > now)
                            select b).FirstOrDefault();
            return ban != null ? true : false;
        }

        [Authorize]
        [HttpPost, Route("getMacIsBanned")]
        public async Task<bool> getMacIsBanned([FromBody] string MacAddress)
        {
            DateTime now = DateTime.UtcNow;
            BannedMac ban = (from b in db.BannedMac
                             where b.MacAddress == MacAddress
                            && b.FromDt <= now
                            && (b.ToDt == null || b.ToDt > now)
                            select b).FirstOrDefault();
            return ban != null ? true : false;
        }

        [Authorize]
        [HttpPost, Route("banIp")]
        public async Task<dynamic> banIp([FromBody] BanRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            BannedIp newBan = new BannedIp()
            {
                IpAddress = request.IpAddress,
                FromDt = now,
                ToDt = request.ToDt
            };
            db.BannedIp.Add(newBan);
            db.SaveChanges();
            return Ok("Ip Banned");
        }

        [Authorize]
        [HttpPost, Route("banMac")]
        public async Task<dynamic> banMac([FromBody] BanRequestDTO request)
        {
            DateTime now = DateTime.UtcNow;
            BannedMac newBan = new BannedMac()
            {
                MacAddress = request.MacAddress,
                FromDt = now,
                ToDt = request.ToDt
            };
            db.BannedMac.Add(newBan);
            db.SaveChanges();
            return Ok("Mac Banned");
        }
    }
}
