using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using System.Collections.Generic;

namespace DeadlockedDatabase.Models
{
    public class AuthenticationResponse
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }


        public AuthenticationResponse(UserDTO user, string token)
        {
            AccountId = user.AccountId;
            AccountName = user.AccountName;
            Roles = user.Roles;
            Token = token;
        }
    }
}