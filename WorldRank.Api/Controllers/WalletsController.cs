using Microsoft.AspNetCore.Mvc;
using WorldRank.Api.Contracts;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Api.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var wallet = await _walletService.GetWalletByIdAsync(id, cancellationToken);

                if (wallet is null)
                    return NotFound();

                return Ok(WalletResponse.From(wallet));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByPlayer([FromQuery] int playerId, CancellationToken cancellationToken)
        {
            try
            {
                var wallets = await _walletService.GetWalletsOfPlayerAsync(playerId, cancellationToken);

                if (wallets.Count == 0)
                    return NotFound();

                return Ok(wallets.Select(WalletResponse.From));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddWalletRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var wallet = await _walletService.AddWalletToPlayerAsync(request.PlayerId, request.Currency, request.InitialBalance, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = wallet.Id }, WalletResponse.From(wallet));
            }
            catch (PlayerNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpPost("{id:int}/deposit")]
        public Task<IActionResult> Deposit(int id, [FromBody] WalletAmountRequest request, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.DepositByIdAsync(id, request.Amount, ct), cancellationToken);

        [HttpPost("{id:int}/withdraw")]
        public Task<IActionResult> Withdraw(int id, [FromBody] WalletAmountRequest request, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.WithdrawByIdAsync(id, request.Amount, ct), cancellationToken);

        [HttpPost("{id:int}/block")]
        public Task<IActionResult> Block(int id, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.BlockByIdAsync(id, ct), cancellationToken);

        [HttpPost("{id:int}/unblock")]
        public Task<IActionResult> Unblock(int id, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.UnblockByIdAsync(id, ct), cancellationToken);

        [HttpPut("{id:int}/balance")]
        public Task<IActionResult> UpdateBalance(int id, [FromBody] UpdateWalletBalanceRequest request, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.UpdateBalanceByIdAsync(id, request.NewBalance, ct), cancellationToken);

        [HttpPost("{id:int}/apply-funds")]
        public Task<IActionResult> ApplyFunds(int id, [FromBody] ApplyFundsRequest request, CancellationToken cancellationToken)
            => RunWalletOperation(id, ct => _walletService.ApplyFundsByIdAsync(id, request.Operation, request.Amount, ct), cancellationToken);

        // Runs a wallet-mutating operation and turns any domain (WalletException) failure into the matching HTTP response.
        private async Task<IActionResult> RunWalletOperation(int id, Func<CancellationToken, Task<Wallet>> operation, CancellationToken cancellationToken)
        {
            try
            {
                var wallet = await operation(cancellationToken);
                return Ok(WalletResponse.From(wallet));
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
            WalletNotFoundByIdException => NotFound(ex.Message),
            DuplicateWalletException => Conflict(ex.Message),
            _ => BadRequest(ex.Message) // InsufficientFundsException, InvalidAmountException, WalletBlockedException, etc.
        };
    }
}
