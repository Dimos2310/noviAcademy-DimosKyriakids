# Debugging exercise — the bug in `IWalletRepository.Add`

This walks through a realistic bug in `InMemoryWalletRepository.Add`, and how to
find it with the debugger's **Add Watch** and **Call Stack** windows — exactly
the technique from the Day 3 theory.

## The bug

Look at `Add` and imagine the highlighted line is missing:

```csharp
public void Add(Wallet wallet, int playerId)
{
    ArgumentNullException.ThrowIfNull(wallet);

    if (!_walletsByPlayer.TryGetValue(playerId, out var wallets))
    {
        wallets = new List<Wallet>();
        // _walletsByPlayer[playerId] = wallets;   ← IMAGINE THIS LINE IS GONE
    }

    if (wallets.Any(w => w.Currency == wallet.Currency))
        throw new DuplicateWalletException(playerId, wallet.Currency);

    wallets.Add(wallet);   // adds to a list that is NEVER stored anywhere
}
```

### Why it's nasty

- **No exception. No crash. No stack trace.** The program says
  *"Added EUR wallet to Alice."* — everything looks fine.
- But when the player is **new** (first wallet), `wallets` is a brand-new local
  list that was never put back into the dictionary. `wallets.Add(wallet)`
  mutates that orphan list, and the moment `Add` returns, the list is
  garbage-collected.
- Later, **List player's wallets** shows *"Alice has no wallets."* — the data
  silently vanished.

This is the worst kind of bug: **silent data loss**. You can't read a stack
trace because nothing threw. You have to *inspect live state* — which is what
the debugger is for.

## Reproduce it

1. In `Repositories/InMemoryWalletRepository.cs`, comment out the line
   `_walletsByPlayer[playerId] = wallets;`.
2. Run the app: `dotnet run`
3. **Add player** → `Alice`
4. **Add wallet to player** → `alice`, `EUR` → it says *"Added EUR wallet."*
5. **List player's wallets** → `alice` → *"Alice has no wallets."* 🐞

## Find it with the debugger

### Step 1 — breakpoint
Set a breakpoint on the line `wallets.Add(wallet);` inside `Add`.
Run (F5), then drive the menu: add Alice, then add an EUR wallet. Execution
pauses on the breakpoint.

### Step 2 — Add Watch (inspect live state)
Right-click each of these → **Add Watch**:

| Watch expression | What you'll see | What it tells you |
|---|---|---|
| `playerId` | `1` | the id we're adding under |
| `wallets` | `Count = 0`, about to become 1 | the LOCAL list |
| `wallets.Count` | `0` | it has nothing yet |
| `_walletsByPlayer` | `Count = 0` | **the dictionary is still EMPTY** ← the smell |
| `_walletsByPlayer.ContainsKey(playerId)` | `false` | player 1 has no entry at all |

The tell is the mismatch: you're about to `wallets.Add(...)`, but
`_walletsByPlayer` still has **`Count = 0`**. The list you're mutating isn't the
one the dictionary keeps — because it was never stored.

### Step 3 — Call Stack (understand how you got here)
Open the **Call Stack** window. Top to bottom you'll see:

```
InMemoryWalletRepository.Add(wallet, playerId)   ← you are here
Program.<Main>$g__AddWallet                        ← the menu handler
Program.<Main>$                                     ← the menu loop
```

This confirms the call came from **AddWallet** with a valid `player.Id`, so the
input is fine — the fault is *inside* `Add`, not in the caller. That narrows it
down to the dictionary-store logic.

### Step 4 — step over and confirm
Press **F10** to step over `wallets.Add(wallet)`. Watch `wallets.Count` become
`1`, but `_walletsByPlayer.Count` **stays `0`**. There it is: the wallet went
into a list nobody keeps.

## The fix

Restore the one line so the new list is stored back in the dictionary:

```csharp
if (!_walletsByPlayer.TryGetValue(playerId, out var wallets))
{
    wallets = new List<Wallet>();
    _walletsByPlayer[playerId] = wallets;   // ← keep the list
}
```

Re-run the reproduction: **List player's wallets** now shows the EUR wallet.

## The lesson (from the Day 3 theory)

- A crash gives you a stack trace; **silent bugs give you nothing** — the
  debugger's **Add Watch** lets you see the state a `Console.WriteLine` never
  would.
- The **Call Stack** answers *"how did I get here?"* and separates a caller bug
  from a callee bug.
- Reach for **structured logging** for the errors you can predict, and the
  **debugger** for the ones you can't.
