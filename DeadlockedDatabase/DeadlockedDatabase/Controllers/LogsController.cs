using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeadlockedDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public LogsController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [HttpPost, Route("submitLog")]
        public async Task<dynamic> submitLog([FromBody] LogDTO LogData)
        {
            ServerLog log = new ServerLog()
            {
                LogDt = LogData.Timestamp,
                AccountId = LogData.AccountId,
                MethodName = LogData.MethodName,
                LogTitle = LogData.LogTitle,
                LogMsg = LogData.LogMsg,
                LogStacktrace = LogData.LogStacktrace,
                Payload = LogData.Payload
            };
            db.ServerLog.Add(log);
            db.SaveChanges();

            return Ok("Log Saved!");
        }
    }
}
