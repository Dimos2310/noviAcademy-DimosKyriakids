using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Application.Services;

public class WalletService
{
	private readonly IWalletRepository _walletRepository;
	private readonly IPlayerRepository _playerRepository;
	private readonly ILogger<WalletService> _logger;
	private readonly IReadOnlyDictionary<FundsOperation, IFundsStrategy> _fundsStrategies;

	public WalletService(
		IWalletRepository walletRepository,
		IPlayerRepository playerRepository,
		IEnumerable<IFundsStrategy> strategies,
		ILogger<WalletService> logger)
	{
		_walletRepository = walletRepository;
		_playerRepository = playerRepository;
		_logger = logger;

		// Index every registered strategy by the operation it implements.
		_fundsStrategies = strategies.ToDictionary(strategy => strategy.Operation);
	}

	public void AddWalletToPlayer()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		var balance = Prompts.PromptAmount("Initial balance");
		if (balance is null)
			return;

		try
		{
			if (_playerRepository.FindPlayer(playerId.Value) is null)
				throw new PlayerNotFoundException(playerId.Value);

			var wallet = new Wallet(playerId.Value, currency.Value, balance.Value);
			_walletRepository.Add(wallet);
			Console.WriteLine("Wallet added successfully.");
		}
		catch (PlayerNotFoundException ex)
		{
			_logger.LogWarning(ex, "Could not add wallet, player {PlayerId} not found", playerId);
			Console.WriteLine($"Error: {ex.Message}");
		}
		catch (WalletException ex)
		{
			_logger.LogWarning(ex, "Could not add wallet for player {PlayerId} in {Currency}", playerId, currency);
			Console.WriteLine($"Error: {ex.Message}");
		}
	}

	public void GetWalletsOfPlayer()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var wallets = _walletRepository.GetAllWalletsByPlayerId(playerId.Value);

		if (wallets.Count == 0)
		{
			Console.WriteLine("No wallets found for this player.");
			return;
		}

		foreach (var wallet in wallets)
			Console.WriteLine($"Wallet Number {wallets.IndexOf(wallet)} {wallet}");
	}

	public void DepositToWallet()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		var amount = Prompts.PromptAmount("Amount to deposit");
		if (amount is null)
			return;

		RunWalletOperation(() =>
		{
			ApplyStrategy(FundsOperation.Add, playerId.Value, currency.Value, amount.Value);
			Console.WriteLine("Deposit successful.");
		});
	}

	public void WithdrawFromWallet()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		var amount = Prompts.PromptAmount("Amount to withdraw");
		if (amount is null)
			return;

		RunWalletOperation(() =>
		{
			ApplyStrategy(FundsOperation.Subtract, playerId.Value, currency.Value, amount.Value);
			Console.WriteLine("Withdrawal successful.");
		});
	}

	public void BlockWallet()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		RunWalletOperation(() =>
		{
			_walletRepository.Block(playerId.Value, currency.Value);
			Console.WriteLine("Wallet blocked.");
		});
	}

	public void UnblockWallet()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		RunWalletOperation(() =>
		{
			_walletRepository.Unblock(playerId.Value, currency.Value);
			Console.WriteLine("Wallet unblocked.");
		});
	}

	public void UpdateWalletBalance()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		var newBalance = Prompts.PromptAmount("New balance");
		if (newBalance is null)
			return;

		RunWalletOperation(() =>
		{
			_walletRepository.UpdateBalance(playerId.Value, currency.Value, newBalance.Value);
			Console.WriteLine("Balance updated.");
		});
	}

	public void ApplyFundsOperation()
	{
		var playerId = Prompts.PromptPlayerId();
		if (playerId is null)
			return;

		var currency = Prompts.PromptCurrency();
		if (currency is null)
			return;

		var operation = Prompts.PromptFundsOperation();
		if (operation is null)
			return;

		var amount = Prompts.PromptAmount("Amount");
		if (amount is null)
			return;

		RunWalletOperation(() =>
		{
			ApplyStrategy(operation.Value, playerId.Value, currency.Value, amount.Value);
			Console.WriteLine($"{operation} operation applied.");
		});
	}

	// Resolves the wallet and runs the strategy registered for the given operation.
	private void ApplyStrategy(FundsOperation operation, int playerId, Domain.Enums.Currency currency, decimal amount)
	{
		var wallet = _walletRepository.GetWallet(playerId, currency);
		var strategy = _fundsStrategies[operation];
		strategy.Execute(wallet, amount);
		_logger.LogInformation("Applied {Strategy} of {Amount} to player {PlayerId} {Currency} wallet (balance {Balance})",
			strategy.GetType().Name, amount, playerId, currency, wallet.Balance);
	}

	// Runs a wallet operation and turns any domain (WalletException) failure into a friendly message + log.
	private void RunWalletOperation(Action operation)
	{
		try
		{
			operation();
		}
		catch (WalletException ex)
		{
			_logger.LogWarning(ex, "Wallet operation failed");
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
}
