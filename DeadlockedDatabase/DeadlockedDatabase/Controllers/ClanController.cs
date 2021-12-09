﻿using System;
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
            AccountService aServ = new AccountService();
            ClanService cs = new ClanService();

            Clan clan = (from c in db.Clan
                           .Include(c => c.ClanMember)
                           .Include(c => c.ClanMessage)
                           .Include(c => c.ClanStat)
                           .Include(c => c.ClanLeaderAccount)
                           .Include(c => c.ClanInvitation)
                           .ThenInclude(ci => ci.Account)
                         where c.ClanId == ClanId
                         select c).FirstOrDefault();

            ClanDTO response = new ClanDTO()
            {
                AppId = clan.AppId ?? 11184,
                ClanId = clan.ClanId,
                ClanName = clan.ClanName,
                ClanLeaderAccount = aServ.toAccountDTO(clan.ClanLeaderAccount),
                ClanMemberAccounts = clan.ClanMember.Select(cm => aServ.toAccountDTO(cm.Account)).ToList(),
                ClanMediusStats = clan.MediusStats,
                ClanWideStats = clan.ClanStat.OrderBy(stat => stat.Id).Select(cs => cs.StatValue).ToList(),
                ClanMessages = clan.ClanMessage.OrderByDescending(cm => cm.Id).Select(cm => cs.toClanMessageDTO(cm)).ToList(),
                ClanMemberInvitations = clan.ClanInvitation.Select(ci => cs.toClanInvitationDTO(ci)).ToList(),
            };

            return response;
        }

        [Authorize("database")]
        [HttpGet, Route("searchClanByName")]
        public async Task<dynamic> searchClanByName(string ClanName)
        {
            int clanId = db.Clan.Where(c => c.IsActive == true && c.ClanName == ClanName).Select(c => c.ClanId).FirstOrDefault();

            if (clanId != 0)
            {
                return await getClan((int)clanId);
            }

            return NotFound();
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

        [Authorize("database")]
        [HttpGet, Route("deleteClan")]
        public async Task<dynamic> deleteClan(int accountId, int clanId)
        {
            DateTime now = DateTime.UtcNow;
            Clan target = db.Clan.Where(c => c.ClanId == clanId)
                                    .Include(c => c.ClanMember)
                                    .Include(c => c.ClanInvitation)
                                    .FirstOrDefault();

            if (target != null)
            {
                target.IsActive = false;
                target.ModifiedBy = accountId;
                target.ModifiedDt = now;
                target.ClanMember.ToList().ForEach(cm =>
                {
                    cm.IsActive = false;
                    cm.ModifiedBy = accountId;
                    cm.ModifiedDt = now;
                });
                target.ClanInvitation.ToList().ForEach(ci =>
                {
                    ci.IsActive = false;
                    ci.ModifiedBy = accountId;
                    ci.ModifiedDt = now;
                });
                db.SaveChanges();
            }

            return Ok();
        }

        [Authorize("database")]
        [HttpPost, Route("transferLeadership")]
        public async Task<dynamic> transferLeadership(ClanTransferLeadershipDTO req) 
        {
            DateTime now = DateTime.UtcNow;
            var target = (from c in db.Clan where c.ClanId == req.ClanId select c).FirstOrDefault();

            target.ClanLeaderAccountId = req.NewLeaderAccountId;
            target.ModifiedBy = req.AccountId;
            target.ModifiedDt = now;

            await db.SaveChangesAsync();

            return Ok();
        }

        [Authorize("database")]
        [HttpPost, Route("createInvitation")]
        public async Task<dynamic> createInvitation(ClanInvitationDTO req)
        {
            DateTime now = DateTime.UtcNow;
            ClanInvitation invite = new ClanInvitation()
            {
                ClanId = req.ClanId,
                AccountId = req.TargetAccountId,
                InviteMsg = req.Message,
                ResponseId = 0,
                IsActive = true,
            };
            db.ClanInvitation.Add(invite);
            db.SaveChanges();

            return Ok();
        }

        [Authorize("database")]
        [HttpGet, Route("invitations")]
        public async Task<dynamic> getInvitesByAccountId(int AccountId)
        {
            ClanService cs = new ClanService();

            var invites = db.ClanInvitation.Where(ci => ci.AccountId == AccountId && ci.ResponseId == 0)
                                            .Include(ci => ci.Clan)
                                            .ThenInclude(c => c.ClanLeaderAccount)
                                            .Include(ci => ci.Account)
                                            .Select(ci => cs.toAccountClanInvitationDTO(ci))
                                            .ToList();

            return invites;

        }

        [Authorize("database")]
        [HttpPost, Route("respondInvitation")]
        public async Task<dynamic> respondInvitation(ClanInvitationResponseDTO req)
        {
            DateTime now = DateTime.UtcNow;
            var target = (from ci in db.ClanInvitation where ci.Id == req.InvitationId select ci).FirstOrDefault();

            if(target != null)
            {
                target.ResponseDt = now;
                target.ResponseMsg = req.ResponseMessage;
                target.ResponseId = req.Response;
                target.IsActive = false;
                target.ModifiedBy = req.AccountId;
                target.ModifiedDt = now;

                db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        [Authorize("database")]
        [HttpPost, Route("revokeInvitation")]
        public async Task<dynamic> revokeInvitation(int FromAccountId, int ClanId, int TargetAccountId)
        {
            DateTime now = DateTime.UtcNow;
            var target = (from ci in db.ClanInvitation where ci.AccountId == TargetAccountId && ci.ClanId == ClanId select ci).FirstOrDefault();

            if (target != null)
            {
                target.ResponseDt = now;
                target.InviteMsg = "Invitation Revoked";
                target.IsActive = false;
                target.ModifiedBy = FromAccountId;
                target.ModifiedDt = now;

                db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        [Authorize("database")]
        [HttpGet, Route("messages")]
        public async Task<dynamic> getClanMessages(int AccountId, int ClanId, int start, int pageSize)
        {
            ClanService cs = new ClanService();

            int totalMessages = db.ClanMessage.Where(cm => cm.ClanId == ClanId && cm.IsActive == true).Count();

            int totalPages = (int) Math.Ceiling((decimal) totalMessages / pageSize);

            if (start < totalPages)
            {
                var skip = start * pageSize;

                var result = db.ClanMessage.Where(cm => cm.ClanId == ClanId && cm.IsActive == true)
                                            .Skip(skip)
                                            .Take(pageSize)
                                            .Select(cm => cs.toClanMessageDTO(cm))
                                            .ToList();

                return result;
            }

            return NotFound($"Page index exceeds total of {totalPages}.");

        }

        [Authorize("database")]
        [HttpPost, Route("addMessage")]
        public async Task<dynamic> createClanMessage(int AccountId, int ClanId, [FromBody] ClanMessageDTO req)
        {
            ClanMessage newMessage = new ClanMessage()
            {
                ClanId = ClanId,
                Message = req.Message,
                CreatedBy = AccountId,
                IsActive = true,
            };

            db.ClanMessage.Add(newMessage);
            db.SaveChanges();

            return Ok();
        }
    }
}
