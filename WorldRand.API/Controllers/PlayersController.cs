using Microsoft.AspNetCore.Mvc;
using WorldRand.API.Contracts;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;

namespace WorldRank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _playerService
                    .GetAllPlayers()
                    .ToList();

                if (result.Count == 0)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("grouped-by-score")]
        public IActionResult GetGroupedByScore()
        {
            try
            {
                var groups = _playerService
                    .GroupPlayersByScore()
                    .Select(group => new { score = group.Key, players = group.ToList() })
                    .ToList();

                if (groups.Count == 0)
                    return NotFound();

                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("search")]
        public IActionResult FindByName([FromQuery] string name)
        {
            try
            {
                var result = _playerService.FindPlayerByName(name);

                if (result is null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{playerId:int}")]
        public IActionResult GetPlayerById(int playerId)
        {
            try
            {
                var result = _playerService.FindPlayerById(playerId);

                if (result is null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult AddPlayer([FromBody] AddPlayerRequest request)
        {
            try
            {
                var player = _playerService.AddPlayer(request.Name, request.Score);
                return CreatedAtAction(nameof(GetPlayerById), new { playerId = player.Id }, player);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{playerId:int}")]
        public IActionResult DeletePlayer(int playerId)
        {
            try
            {
                _playerService.DeletePlayer(playerId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}