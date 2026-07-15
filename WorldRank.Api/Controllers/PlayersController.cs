using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorldRank.Api.Contracts;
using WorldRank.Application.Commands.Players;
using WorldRank.Application.Queries.Players;

namespace WorldRank.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlayersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = (await _mediator.Send(new GetAllPlayersQuery(), cancellationToken)).ToList();

                if (result.Count == 0)
                    return NotFound();

                return Ok(result.Select(PlayerResponse.From));
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
                var groups = (await _mediator.Send(new GetPlayersGroupedByScoreQuery(), cancellationToken))
                    .Select(group => new { score = group.Key, players = group.Select(PlayerResponse.From).ToList() })
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
                var result = await _mediator.Send(new GetPlayerByNameQuery(name), cancellationToken);

                if (result is null)
                    return NotFound();

                return Ok(PlayerResponse.From(result));
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
                var result = await _mediator.Send(new GetPlayerByIdQuery(playerId), cancellationToken);

                if (result is null)
                    return NotFound();

                return Ok(PlayerResponse.From(result));
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
                var playerId = await _mediator.Send(new AddPlayerCommand(request.Name, request.Score), cancellationToken);

                var player = await _mediator.Send(new GetPlayerByIdQuery(playerId), cancellationToken);
                return CreatedAtAction(nameof(GetPlayerById), new { playerId }, PlayerResponse.From(player!));
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
                await _mediator.Send(new DeletePlayerCommand(playerId), cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
