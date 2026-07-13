using Microsoft.AspNetCore.Mvc;
using WorldRank.Api.Contracts;
using WorldRank.Application.Interfaces;

namespace WorldRank.Api.Controllers
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
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = (await _playerService.GetAllPlayersAsync(cancellationToken)).ToList();

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
        public async Task<IActionResult> GetGroupedByScore(CancellationToken cancellationToken)
        {
            try
            {
                var groups = (await _playerService.GroupPlayersByScoreAsync(cancellationToken))
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
        public async Task<IActionResult> FindByName([FromQuery] string name, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _playerService.FindPlayerByNameAsync(name, cancellationToken);

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
        public async Task<IActionResult> GetPlayerById(int playerId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _playerService.FindPlayerByIdAsync(playerId, cancellationToken);

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
        public async Task<IActionResult> AddPlayer([FromBody] AddPlayerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var player = await _playerService.AddPlayerAsync(request.Name, request.Score, cancellationToken);
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
        public async Task<IActionResult> DeletePlayer(int playerId, CancellationToken cancellationToken)
        {
            try
            {
                await _playerService.DeletePlayerAsync(playerId, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
