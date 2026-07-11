using Microsoft.Extensions.Logging;
using WorldRank.Application.Interfaces;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Infrastructure.Repositories;

public class InMemoryWalletRepository : IWalletRepository
{
	private readonly ILogger<InMemoryWalletRepository> _logger;

	private readonly List<Wallet> _wallets = new();

	public InMemoryWalletRepository(ILogger<InMemoryWalletRepository> logger)
	{
		_logger = logger;
	}

	public void Add(Wallet wallet)
	{
		var exists = _wallets.Any(item => item.PlayerId == wallet.PlayerId && item.Currency == wallet.Currency);

		if (exists)
		{
			throw new DuplicateWalletException(wallet.PlayerId, wallet.Currency);
		}

		_wallets.Add(wallet);
		_logger.LogInformation("Wallet created for player {PlayerId} in {Currency} with balance {Balance}", wallet.PlayerId, wallet.Currency, wallet.Balance);
	}

	public Wallet[] GetAll()
	{
		return _wallets.ToArray();
	}

	public List<Wallet> GetAllWalletsByPlayerId(int playerId)
	{
		return _wallets.Where(item => item.PlayerId == playerId).ToList();
	}

	public void UpdateBalance(int playerId, Currency currency, decimal newBalance)
	{
		GetWallet(playerId, currency).SetBalance(newBalance);
		_logger.LogInformation("Player {PlayerId} {Currency} wallet balance set to {Balance}", playerId, currency, newBalance);
	}

	public void Deposit(int playerId, Currency currency, decimal amount)
	{
		var wallet = GetWallet(playerId, currency);
		wallet.Deposit(amount);
		_logger.LogInformation("Deposited {Amount} to player {PlayerId} {Currency} wallet (balance {Balance})", amount, playerId, currency, wallet.Balance);
	}

	public void Withdraw(int playerId, Currency currency, decimal amount)
	{
		var wallet = GetWallet(playerId, currency);
		wallet.Withdraw(amount);
		_logger.LogInformation("Withdrew {Amount} from player {PlayerId} {Currency} wallet (balance {Balance})", amount, playerId, currency, wallet.Balance);
	}

	public void Block(int playerId, Currency currency)
	{
		GetWallet(playerId, currency).Block();
		_logger.LogInformation("Player {PlayerId} {Currency} wallet blocked", playerId, currency);
	}

	public void Unblock(int playerId, Currency currency)
	{
		GetWallet(playerId, currency).Unblock();
		_logger.LogInformation("Player {PlayerId} {Currency} wallet unblocked", playerId, currency);
	}

	public Wallet GetWallet(int playerId, Currency currency)
	{
		var wallet = _wallets.SingleOrDefault(item => item.PlayerId == playerId && item.Currency == currency);

		if (wallet is null)
		{
			throw new WalletNotFoundException(playerId, currency);
		}

		return wallet;
	}
}
