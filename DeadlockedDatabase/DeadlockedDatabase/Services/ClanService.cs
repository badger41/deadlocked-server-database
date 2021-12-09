using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.Services
{
    public class ClanService
    {

        public ClanMessageDTO toClanMessageDTO(ClanMessage message)
        {
            return new ClanMessageDTO()
            {
                Id = message.Id,
                Message = message.Message,
            };
        }

        public ClanInvitationDTO toClanInvitationDTO(ClanInvitation invite)
        {
            return new ClanInvitationDTO()
            {
                ClanId = invite.ClanId,
                ClanName = invite.Clan.ClanName,
                TargetAccountId = invite.AccountId,
                TargetAccountName = invite.Account.AccountName,
                Message = invite.InviteMsg,
                ResponseMessage = invite.ResponseMsg,
                ResponseTime = invite.ResponseDt != null ? (int)((DateTimeOffset)invite.ResponseDt).ToUnixTimeSeconds() : 0,
                ResponseStatus = invite.ResponseId ?? 0,
            };
        }

        public AccountClanInvitationDTO toAccountClanInvitationDTO(ClanInvitation invite)
        {
            return new AccountClanInvitationDTO()
            {
                LeaderAccountId = invite.Clan.ClanLeaderAccountId,
                LeaderAccountName = invite.Clan.ClanLeaderAccount.AccountName,
                Invitation = toClanInvitationDTO(invite),
            };
        }
    }
}
