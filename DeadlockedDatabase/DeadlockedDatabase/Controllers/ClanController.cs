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
    [Route("[controller]")]
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
        public async Task<ClanDTO> getClan(int clanId)
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
                         where c.ClanId == clanId
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
        public async Task<dynamic> searchClanByName(string clanName, int appId)
        {
            int clanId = db.Clan.Where(c => c.IsActive == true && c.AppId == appId && c.ClanName.ToLower() == clanName.ToLower()).Select(c => c.ClanId).FirstOrDefault();

            if (clanId != 0)
            {
                return await getClan((int)clanId);
            }

            return NotFound();
        }

        [Authorize("database")]
        [HttpPost, Route("createClan")]
        public async Task<dynamic> createClan(int accountId, string clanName, int appId)
        {
            // verify not already in clan
            var member = db.ClanMember.Where(c => c.IsActive == true && c.AccountId == accountId && c.Clan.AppId == appId)
                .FirstOrDefault();
            if (member != null)
                return BadRequest();

            Clan newClan = new Clan()
            {
                ClanLeaderAccountId = accountId,
                ClanName = clanName,
                AppId = appId,
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
            Clan target = db.Clan.Where(c => c.ClanId == clanId && c.ClanLeaderAccountId == accountId)
                                    .Include(c => c.ClanMember)
                                    .Include(c => c.ClanInvitation)
                                    .FirstOrDefault();

            // not found
            if (target == null)
                return NotFound();

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

            return Ok();
        }

        [Authorize("database")]
        [HttpPost, Route("transferLeadership")]
        public async Task<dynamic> transferLeadership([FromBody] ClanTransferLeadershipDTO req) 
        {
            DateTime now = DateTime.UtcNow;
            var target = (from c in db.Clan where c.ClanId == req.ClanId && c.ClanLeaderAccountId == req.AccountId select c).FirstOrDefault();

            if (target == null)
                return NotFound();

            target.ClanLeaderAccountId = req.NewLeaderAccountId;
            target.ModifiedBy = req.AccountId;
            target.ModifiedDt = now;

            await db.SaveChangesAsync();
            return Ok();
        }

        [Authorize("database")]
        [HttpPost, Route("createInvitation")]
        public async Task<dynamic> createInvitation(int accountId, [FromBody] ClanInvitationDTO req)
        {
            Clan target = db.Clan.Where(c => c.ClanId == req.ClanId && c.ClanLeaderAccountId == accountId)
                                    .FirstOrDefault();

            var existingInvitation = db.ClanInvitation.Where(c => c.ClanId == req.ClanId && c.IsActive == true && c.AccountId == accountId)
                                    .FirstOrDefault();

            // prevent inviting someone twice
            if (existingInvitation != null)
            {
                return BadRequest();
            }

            if (target != null)
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

            return this.ValidationProblem();
        }

        [Authorize("database")]
        [HttpGet, Route("invitations")]
        public async Task<dynamic> getInvitesByAccountId(int accountId)
        {
            ClanService cs = new ClanService();

            var invites = db.ClanInvitation.Where(ci => ci.AccountId == accountId && ci.ResponseId == 0)
                                            .Include(ci => ci.Clan)
                                            .ThenInclude(c => c.ClanLeaderAccount)
                                            .Include(ci => ci.Account)
                                            .Select(ci => cs.toAccountClanInvitationDTO(ci))
                                            .ToList();

            return invites;

        }

        [Authorize("database")]
        [HttpPost, Route("respondInvitation")]
        public async Task<dynamic> respondInvitation([FromBody] ClanInvitationResponseDTO req)
        {
            DateTime now = DateTime.UtcNow;
            var target = (from ci in db.ClanInvitation where ci.Id == req.InvitationId && ci.AccountId == req.AccountId select ci).FirstOrDefault();

            if(target != null)
            {
                // client accepted invitation
                if (req.Response == 1 && target.IsActive == true)
                {
                    Clan clan = db.Clan.Where(c => c.ClanId == target.ClanId)
                                    .Include(c => c.ClanMember)
                                    .Include(c => c.ClanInvitation)
                                   .FirstOrDefault();
                    
                    if (clan != null)
                    {
                        clan.ClanMember.Add(new ClanMember()
                        {
                            ClanId = target.ClanId,
                            AccountId = target.AccountId,
                        });
                    }
                }

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
        public async Task<dynamic> revokeInvitation(int fromAccountId, int clanId, int targetAccountId)
        {
            DateTime now = DateTime.UtcNow;
            var target = (from ci in db.ClanInvitation where ci.AccountId == targetAccountId && ci.ClanId == clanId select ci).FirstOrDefault();

            if (target != null)
            {
                target.ResponseDt = now;
                target.InviteMsg = "Invitation Revoked";
                target.IsActive = false;
                target.ModifiedBy = fromAccountId;
                target.ModifiedDt = now;

                db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        [Authorize("database")]
        [HttpGet, Route("messages")]
        public async Task<dynamic> getClanMessages(int accountId, int clanId, int start, int pageSize)
        {
            ClanService cs = new ClanService();

            int totalMessages = db.ClanMessage.Where(cm => cm.ClanId == clanId && cm.IsActive == true).Count();

            int totalPages = (int) Math.Ceiling((decimal) totalMessages / pageSize);

            if (start < totalPages)
            {
                var skip = start * pageSize;

                var result = db.ClanMessage.Where(cm => cm.ClanId == clanId && cm.IsActive == true)
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
        public async Task<dynamic> createClanMessage(int accountId, int clanId, [FromBody] ClanMessageDTO req)
        {
            ClanMessage newMessage = new ClanMessage()
            {
                ClanId = clanId,
                Message = req.Message,
                CreatedBy = accountId,
                IsActive = true,
            };

            db.ClanMessage.Add(newMessage);
            db.SaveChanges();

            return Ok();
        }

        [Authorize("database")]
        [HttpPut, Route("editMessage")]
        public async Task<dynamic> editClanMessage(int accountId, int clanId, [FromBody] ClanMessageDTO req)
        {
            var target = db.ClanMessage.Where(c => c.ClanId == clanId && c.Id == req.Id)
                .FirstOrDefault();

            if (target == null)
                return NotFound();

            target.Message = req.Message;

            db.ClanMessage.Update(target);
            db.SaveChanges();

            return Ok();
        }
    }
}
