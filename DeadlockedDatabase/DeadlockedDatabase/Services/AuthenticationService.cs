using DeadlockedDatabase.DTO;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DeadlockedDatabase.Models;
using DeadlockedDatabase.Entities;
using DeadlockedDatabase.Controllers;
using Microsoft.EntityFrameworkCore;
using DeadlockedDatabase.Helpers;
using Microsoft.Extensions.Options;

namespace DeadlockedDatabase.Services
{
    public interface IAuthService
    {
        AuthenticationResponse Authenticate(AuthenticationRequest model);
        UserDTO GetById(int id);
    }

    public class AuthenticationService : IAuthService
    {

        private readonly AppSettings appSettings;
        private readonly Ratchet_DeadlockedContext db;

        public AuthenticationService(IOptions<AppSettings> _appSettings, Ratchet_DeadlockedContext _db)
        {
            appSettings = _appSettings.Value;
            db = _db;
        }

        public AuthenticationResponse Authenticate(AuthenticationRequest model)
        {
            Account acc = db.Account.Where(a => a.AccountName == model.AccountName && a.AccountPassword == Crypto.ComputeSHA256(model.Password)).FirstOrDefault();

            // return null if user not found
            if (acc == null) return null;

            UserDTO user = GetById(acc.AccountId);

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticationResponse(user, token);
        }

        public UserDTO GetById(int AccountId)
        {
            Account existingAccount = db.Account.Where(a => a.AccountId == AccountId)
                                                .FirstOrDefault();
            DateTime now = DateTime.UtcNow;

            UserDTO account = new UserDTO()
            {
                AccountId = existingAccount.AccountId,
                AccountName = existingAccount.AccountName,
                Roles = (from ur in db.UserRole
                         join r in db.Roles
                            on ur.RoleId equals r.RoleId
                         where ur.AccountId == AccountId 
                         && ur.FromDt <= now && (ur.ToDt == null || ur.ToDt >= now)
                         select r.RoleName).ToList()
            };

            return account;
        }

        // helper methods

        private string generateJwtToken(UserDTO user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.AccountId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
