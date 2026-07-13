using Microsoft.AspNetCore.Mvc;
using WorldRand.API.Contracts;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;

namespace WorldRank.API.Controllers
{
    [ApiController]
    [Route("api/players/{playerId:int}/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public IActionResult GetWalletsOfPlayer(int playerId)
        {
            try
            {
                var wallets = _walletService.GetWalletsOfPlayer(playerId);

                if (wallets.Count == 0)
                    return NotFound();

                return Ok(wallets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult AddWalletToPlayer(int playerId, [FromBody] AddWalletRequest request)
        {
            try
            {
                _walletService.AddWalletToPlayer(playerId, request.Currency, request.InitialBalance);
                return CreatedAtAction(nameof(GetWalletsOfPlayer), new { playerId }, null);
            }
            catch (WalletException ex)
            {
                return MapWalletException(ex);
            }
            catch (PlayerNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{currency}/deposit")]
        public IActionResult Deposit(int playerId, Currency currency, [FromBody] WalletAmountRequest request)
        {
            return RunWalletOperation(() =>
            {
                _walletService.Deposit(playerId, currency, request.Amount);
                return Ok();
            });
        }

        [HttpPost("{currency}/withdraw")]
        public IActionResult Withdraw(int playerId, Currency currency, [FromBody] WalletAmountRequest request)
        {
            return RunWalletOperation(() =>
            {
                _walletService.Withdraw(playerId, currency, request.Amount);
                return Ok();
            });
        }

        [HttpPost("{currency}/block")]
        public IActionResult Block(int playerId, Currency currency)
        {
            return RunWalletOperation(() =>
            {
                _walletService.Block(playerId, currency);
                return Ok();
            });
        }

        [HttpPost("{currency}/unblock")]
        public IActionResult Unblock(int playerId, Currency currency)
        {
            return RunWalletOperation(() =>
            {
                _walletService.Unblock(playerId, currency);
                return Ok();
            });
        }

        [HttpPut("{currency}/balance")]
        public IActionResult UpdateBalance(int playerId, Currency currency, [FromBody] UpdateWalletBalanceRequest request)
        {
            return RunWalletOperation(() =>
            {
                _walletService.UpdateBalance(playerId, currency, request.NewBalance);
                return Ok();
            });
        }

        [HttpPost("{currency}/apply-funds")]
        public IActionResult ApplyFunds(int playerId, Currency currency, [FromBody] ApplyFundsRequest request)
        {
            return RunWalletOperation(() =>
            {
                _walletService.ApplyFunds(playerId, currency, request.Operation, request.Amount);
                return Ok();
            });
        }

        // Runs a wallet operation and turns any domain (WalletException) failure into the matching HTTP response.
        private IActionResult RunWalletOperation(Func<IActionResult> operation)
        {
            try
            {
                return operation();
            }
            catch (WalletException ex)
            {
                return MapWalletException(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private IActionResult MapWalletException(WalletException ex) => ex switch
        {
            WalletNotFoundException => NotFound(ex.Message),
            DuplicateWalletException => Conflict(ex.Message),
            _ => BadRequest(ex.Message) // InsufficientFundsException, InvalidAmountException, WalletBlockedException, etc.
        };
    }
}
