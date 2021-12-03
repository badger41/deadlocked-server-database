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
using DeadlockedDatabase.Services;

namespace DeadlockedDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClanController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public ClanController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [Authorize("database")]
        [HttpGet, Route("getClan")]
        public async Task<ClanDTO> getClan(int ClanId)
        {
            //var response = (from c in db.Clan
            //                where c.ClanId == ClanId
            //                select new ClanDTO()
            //                {
            //                    ClanId = c.ClanId,
            //                    ClanLeaderAccount = new AccountDTO()
            //                    {
            //                        AccountId = c.ClanLeaderAccountId,
            //                        AccountName = c.ClanLeaderAccount.AccountName,
            //                        AppId = c.ClanLeaderAccount.AppId,
            //                    },

            //                })

            var response = (from c in db.Clan
                            where c.ClanId == ClanId
                            select c).FirstOrDefault();
            return new ClanDTO();
        }

        [Authorize("database")]
        [HttpGet, Route("getClantest")]
        public async Task<ClanDTO> getClantest(int ClanId)
        {
            //var response = (from c in db.Clan
            //                where c.ClanId == ClanId
            //                select new ClanDTO()
            //                {
            //                    ClanId = c.ClanId,
            //                    ClanLeaderAccount = new AccountDTO()
            //                    {
            //                        AccountId = c.ClanLeaderAccountId,
            //                        AccountName = c.ClanLeaderAccount.AccountName,
            //                        AppId = c.ClanLeaderAccount.AppId,
            //                    },

            //                })
            AccountService aServ = new AccountService();

            Clan clan = ( from c in db.Clan
                            .Include(c => c.ClanMember)
                            .Include(c => c.ClanMessage)
                            .Include(c => c.ClanStat)
                            .Include(c => c.ClanLeaderAccount)
                            .Include(c => c.ClanInvitation)
                            where c.ClanId == ClanId
                            select c).FirstOrDefault();

            ClanDTO response = new ClanDTO()
            {
                ClanId = clan.ClanId,
                ClanLeaderAccount = aServ.toAccountDTO(clan.ClanLeaderAccount),
                ClanMemberAccounts = clan.ClanMember.Select(cm => aServ.toAccountDTO(cm.Account)).ToList(),
                ClanMediusStats = clan.MediusStats,
                ClanWideStats = clan.ClanStat.Select(cs => cs.StatValue).ToList(),
                // ClanMessages = clan.ClanMessage.Select(cm => )

            };

            return response;
        }

        [Authorize("database")]
        [HttpPost, Route("createClan")]
        public async Task<ClanDTO> createClan(int accountId, string clanName)
        {
            Clan newClan = new Clan()
            {
                ClanLeaderAccountId = accountId,
                ClanName = clanName,
                AppId = 11184,
                CreatedBy = accountId,
            };
            db.Clan.Add(newClan);
            db.SaveChanges();

            ClanMember newMember = new ClanMember()
            {
                ClanId = newClan.ClanId,
                AccountId = accountId,

            };
            db.ClanMember.Add(newMember);

            List<ClanStat> newStats = (from ds in db.DimStats
                                          select new ClanStat()
                                          {
                                              ClanId = newClan.ClanId,
                                              StatId = ds.StatId,
                                              StatValue = ds.DefaultValue
                                          }).ToList();
            db.ClanStat.AddRange(newStats);


            db.SaveChanges();
            return await getClan(newClan.ClanId);
        }



    }
}
