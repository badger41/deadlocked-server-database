using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using DeadlockedDatabase.DTO;
using DeadlockedDatabase.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        [Authorize]
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

            return Ok("Log Saved!");
        }

        [Route("error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult error()
        {
            var context = HttpContext;

            var exceptionContext = HttpContext.Features.Get<IExceptionHandlerFeature>();
            context.Request.Body.Position = 0;
            using (StreamReader stream = new StreamReader(context.Request.Body))
            {
                string rbody = stream.ReadToEndAsync().Result;


                var payload = new
                {
                    query = context.Request.QueryString.Value,
                    body = rbody,
                    innerException = exceptionContext.Error.InnerException
                };
                context.Request.Body.Position = 0;
                string payloadString = JsonConvert.SerializeObject(payload, Formatting.Indented);

                RollBack();
                ServerLog log = new ServerLog()
                {
                    LogDt = DateTime.UtcNow,
                    AccountId = null,
                    MethodName = context.Request.Path.ToString(),
                    LogTitle = "Internal Error",
                    LogMsg = exceptionContext.Error.Message,
                    LogStacktrace = exceptionContext.Error.StackTrace,
                    Payload = payloadString,
                };
                db.ServerLog.Add(log);
                db.SaveChanges();

            }



            return Problem();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void RollBack()
        {
            var context = db;
            var changedEntries = context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
