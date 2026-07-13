using WorldRank.Application.Interfaces;
using WorldRank.Application.Strategies;
using WorldRank.Domain.Entities;
using WorldRank.Domain.Enums;
using WorldRank.Domain.Exceptions;

namespace WorldRank.Application.Services;

// Use-case logic only. No console / presentation concerns: inputs come in as
// parameters, failures surface as domain exceptions for the caller to handle.
public class WalletService : IWalletService
{
	private readonly IWalletRepository _walletRepository;
	private readonly IPlayerRepository _playerRepository;
	private readonly IReadOnlyDictionary<FundsOperation, IFundsStrategy> _fundsStrategies;

	public WalletService(
		IWalletRepository walletRepository,
		IPlayerRepository playerRepository,
		IEnumerable<IFundsStrategy> strategies)
	{
		_walletRepository = walletRepository;
		_playerRepository = playerRepository;

		// Index every registered strategy by the operation it implements.
		_fundsStrategies = strategies.ToDictionary(strategy => strategy.Operation);
	}

	public void AddWalletToPlayer(int playerId, Currency currency, decimal initialBalance)
	{
		if (_playerRepository.FindPlayer(playerId) is null)
			throw new PlayerNotFoundException(playerId);

		// No id here: the store (database identity or in-memory repository) assigns it.
		var wallet = new Wallet(playerId, currency, initialBalance);
		_walletRepository.Add(wallet);
	}

	public IReadOnlyList<Wallet> GetWalletsOfPlayer(int playerId)
	{
		return _walletRepository.GetAllWalletsByPlayerId(playerId);
	}

	public void Deposit(int playerId, Currency currency, decimal amount)
	{
		_walletRepository.Deposit(playerId, currency, amount);
	}

	public void Withdraw(int playerId, Currency currency, decimal amount)
	{
		_walletRepository.Withdraw(playerId, currency, amount);
	}

	public void Block(int playerId, Currency currency)
	{
		_walletRepository.Block(playerId, currency);
	}

	public void Unblock(int playerId, Currency currency)
	{
		_walletRepository.Unblock(playerId, currency);
	}

	public void UpdateBalance(int playerId, Currency currency, decimal newBalance)
	{
		_walletRepository.UpdateBalance(playerId, currency, newBalance);
	}

	public void ApplyFunds(int playerId, Currency currency, FundsOperation operation, decimal amount)
	{
		// Pick the strategy that matches the chosen operation (resolved from DI, no factory).
		var strategy = _fundsStrategies[operation];

		// The repository applies the strategy and persists it, just like Deposit/Withdraw,
		// so there is no separate "save" step for the caller to remember.
		_walletRepository.ApplyStrategy(playerId, currency, strategy, amount);
	}
}
