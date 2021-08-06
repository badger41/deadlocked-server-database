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

namespace DeadlockedDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private Ratchet_DeadlockedContext db;
        public GameController(Ratchet_DeadlockedContext _db)
        {
            db = _db;
        }

        [Authorize("discord_bot")]
        [HttpGet, Route("list")]
        public async Task<dynamic> getGames()
        {
            var games = db.Game.ToList();

            return games;
        }

        [Authorize("discord_bot")]
        [HttpGet, Route("/{gameId}")]
        public async Task<dynamic> getGame(int gameId)
        {
            var existingGame = db.Game.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                return existingGame;
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize("database")]
        [HttpPost, Route("create")]
        public async Task<dynamic> createGame([FromBody] GameDTO game)
        {
            Game newGame = new Game()
            {
                GameId = game.GameId,
                AppId = game.AppId,
                MinPlayers = game.MinPlayers,
                MaxPlayers = game.MaxPlayers,
                PlayerCount = game.PlayerCount,
                GameLevel = game.GameLevel,
                PlayerSkillLevel = game.PlayerSkillLevel,
                GameStats = game.GameStats,
                GameName = game.GameName,
                RuleSet = game.RuleSet,
                PlayerListCurrent = game.PlayerListCurrent,
                PlayerListStart = game.PlayerListStart,
                GenericField1 = game.GenericField1,
                GenericField2 = game.GenericField2,
                GenericField3 = game.GenericField3,
                GenericField4 = game.GenericField4,
                GenericField5 = game.GenericField5,
                GenericField6 = game.GenericField6,
                GenericField7 = game.GenericField7,
                GenericField8 = game.GenericField8,
                WorldStatus = game.WorldStatus,
                GameHostType = game.GameHostType,
                Metadata = game.Metadata,
                GameCreateDt = game.GameCreateDt ?? DateTime.UtcNow,
            };
            db.Game.Add(newGame);
            db.SaveChanges();

            return Ok();
        }

        [Authorize("database")]
        [HttpPut, Route("update/{gameId}")]
        public async Task<dynamic> updateGame(int gameId, [FromBody] GameDTO game)
        {
            var existingGame = db.Game.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                // Catalog the historical game
                if (game.GameEndDt != null)
                {
                    GameHistory newHistoricalGame = new GameHistory()
                    {
                        GameId = game.GameId,
                        AppId = game.AppId,
                        MinPlayers = game.MinPlayers,
                        MaxPlayers = game.MaxPlayers,
                        PlayerCount = game.PlayerCount,
                        PlayerListCurrent = game.PlayerListCurrent,
                        PlayerListStart = game.PlayerListStart,
                        GameLevel = game.GameLevel,
                        PlayerSkillLevel = game.PlayerSkillLevel,
                        GameStats = game.GameStats,
                        GameName = game.GameName,
                        RuleSet = game.RuleSet,
                        GenericField1 = game.GenericField1,
                        GenericField2 = game.GenericField2,
                        GenericField3 = game.GenericField3,
                        GenericField4 = game.GenericField4,
                        GenericField5 = game.GenericField5,
                        GenericField6 = game.GenericField6,
                        GenericField7 = game.GenericField7,
                        GenericField8 = game.GenericField8,
                        WorldStatus = game.WorldStatus,
                        GameHostType = game.GameHostType,
                        Metadata = existingGame.Metadata,
                        GameCreateDt = game.GameCreateDt,
                        GameStartDt = game.GameStartDt,
                        GameEndDt = game.GameEndDt,
                    };
                    db.GameHistory.Add(newHistoricalGame);
                    db.SaveChanges();

                    await deleteGame(existingGame.GameId);

                    return Ok();
                }
                else
                {

                    existingGame.GameId = game.GameId;
                    existingGame.AppId = game.AppId;
                    existingGame.MinPlayers = game.MinPlayers;
                    existingGame.MaxPlayers = game.MaxPlayers;
                    existingGame.PlayerCount = game.PlayerCount;
                    existingGame.PlayerListCurrent = game.PlayerListCurrent;
                    existingGame.PlayerListStart = game.PlayerListStart;
                    existingGame.GameLevel = game.GameLevel;
                    existingGame.GameName = game.GameName;
                    existingGame.RuleSet = game.RuleSet;
                    existingGame.GenericField1 = game.GenericField1;
                    existingGame.GenericField2 = game.GenericField2;
                    existingGame.GenericField3 = game.GenericField3;
                    existingGame.GenericField4 = game.GenericField4;
                    existingGame.GenericField5 = game.GenericField5;
                    existingGame.GenericField6 = game.GenericField6;
                    existingGame.GenericField7 = game.GenericField7;
                    existingGame.GenericField8 = game.GenericField8;
                    existingGame.WorldStatus = game.WorldStatus;
                    existingGame.GameHostType = game.GameHostType;
                    existingGame.GameCreateDt = game.GameCreateDt;
                    existingGame.GameStartDt = game.GameStartDt;

                    db.SaveChanges();

                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize("database")]
        [HttpPut, Route("updateMetaData/{gameId}")]
        public async Task<dynamic> updateGame(int gameId, [FromBody] string MetaData)
        {
            var existingGame = db.Game.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {

                existingGame.Metadata = MetaData;

                db.SaveChanges();

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize("database")]
        [HttpDelete, Route("clear")]
        public async Task<dynamic> clearGames()
        {
            var games = db.Game.ToList();
            db.Game.RemoveRange(games);
            db.SaveChanges();

            return Ok();
        }

        [Authorize("database")]
        [HttpDelete, Route("delete/{gameId}")]
        public async Task<dynamic> deleteGame(int gameId)
        {
            var existingGame = db.Game.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                db.Game.Remove(existingGame);
                db.SaveChanges();

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
