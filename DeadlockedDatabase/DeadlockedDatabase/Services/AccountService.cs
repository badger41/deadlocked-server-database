using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeadlockedDatabase.Services
{
    public class AccountService
    {
        public AccountDTO toAccountDTO(Account account)
        {
            return new AccountDTO()
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                Friends = account.AccountFriend.Select(f => toAccountRelationDTO(f.AccountId, f.Account.AccountName)).ToList(),
                Ignored = account.AccountIgnored.Select(i => toAccountRelationDTO(i.AccountId, i.Account.AccountName)).ToList(),
                AccountWideStats = account.AccountStat.OrderBy(a => a.StatId).Select(a => a.StatValue).ToList(),
                MediusStats = account.MediusStats,
                MachineId = account.MachineId,
                AppId = account.AppId,
            };
        }

        public AccountRelationDTO toAccountRelationDTO(int AccountId, string AccountName)
        {
            return new AccountRelationDTO()
            {
                AccountId = AccountId,
                AccountName = AccountName,
            };
        }
    }
}
