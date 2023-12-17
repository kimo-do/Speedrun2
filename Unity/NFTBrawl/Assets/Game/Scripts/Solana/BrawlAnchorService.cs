using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Frictionless;
using Game.Scripts.Ui;
using Lumberjack;
using Lumberjack.Accounts;
using Lumberjack.Program;
using Solana.Unity.Programs;
using Solana.Unity.Programs.Models;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.SessionKeys.GplSession.Accounts;
using Solana.Unity.Wallet;
using Services;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Deathbattle;
using Deathbattle.Accounts;
using Deathbattle.Program;
using Deathbattle.Types;
using System.Linq;

public class BrawlAnchorService : MonoBehaviour
{
    public static PublicKey AnchorProgramIdPubKey = new("BRAWLHsgvJBQGx4EzNuqKpbbv8q3LhcYbL1bHqbgVtaJ");

    // Needs to be the same constants as in the anchor program
    public const int TIME_TO_REFILL_ENERGY = 60;
    public const int MAX_ENERGY = 100;
    public const int MAX_WOOD_PER_TREE = 100000;

    public static BrawlAnchorService Instance { get; private set; }
    public static Action<PlayerData> OnPlayerDataChanged;
    public static Action<CloneLab> OnCloneLabChanged;
    public static Action<Graveyard> OnGraveyardChanged;
    public static Action<Profile> OnProfileChanged;
    public static Action<Colosseum> OnColosseumChanged;

    public static Action OnInitialDataLoaded;

    public bool IsAnyBlockingTransactionInProgress => blockingTransactionsInProgress > 0 || IsAnyBlockingProgress;
    public bool IsAnyNonBlockingTransactionInProgress => nonBlockingTransactionsInProgress > 0;

    public bool IsAnyBlockingProgress { get; set; }
    public PlayerData CurrentPlayerData { get; private set; }
    public CloneLab CurrentCloneLab { get; private set; }
    public Profile CurrentProfile { get; private set; }
    public Graveyard CurrentGraveyard { get; private set; }
    public Colosseum CurrentColosseum { get; private set; }

    public int BlockingTransactionsInProgress => blockingTransactionsInProgress;
    public int NonBlockingTransactionsInProgress => nonBlockingTransactionsInProgress;
    public long LastTransactionTimeInMs => lastTransactionTimeInMs;
    public string LastError { get; set; }

    public SessionWallet sessionWallet;
    private PublicKey AdminPubkey = new("braw1mRTFfPNedZHiDMWsZgB2pwS3bss91QUB6oy4FX");
    private PublicKey PlayerDataPDA;
    private PublicKey GameDataPDA;
    public PublicKey ProfilePDA;
    public PublicKey CloneLabPDA;
    public PublicKey ColosseumPDA;
    public PublicKey GraveyardPDA;
    public PublicKey BrawlPDA;
    private bool _isInitialized;
    private DeathbattleClient anchorClient;
    private int blockingTransactionsInProgress;
    private int nonBlockingTransactionsInProgress;
    private long? sessionValidUntil;
    private string sessionKeyPassword = "inGame"; // Would be better to generate and save in playerprefs
    private string levelSeed = "level_2";
    private ushort transactionCounter = 0;

    // Only used to show transaction speed. Feel free to remove
    private Dictionary<ushort, Stopwatch> stopWatches = new();
    private long lastTransactionTimeInMs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Web3.OnLogin += OnLogin;
    }

    private void OnDestroy()
    {
        Web3.OnLogin -= OnLogin;
    }

    private async void OnLogin(Account account)
    {
        Debug.Log("Logged in with pubkey: " + account.PublicKey);


        await RequestAirdropIfSolValueIsLow();

        sessionWallet = await SessionWallet.GetSessionWallet(AnchorProgramIdPubKey, sessionKeyPassword);
        await UpdateSessionValid();

        FindPDAs(account);

        anchorClient = new DeathbattleClient(Web3.Rpc, Web3.WsRpc, AnchorProgramIdPubKey);

        //await SubscribeToPlayerDataUpdates();
        
        await SubscribeToProfileUpdates();
        /*
        await SubscribeToCloneLabUpdates();
        await SubscribeToGraveyardUpdates();
        await SubscribeToColosseumUpdates();
        */

        OnInitialDataLoaded?.Invoke();

        //BrawlAnchorService.Instance.IsAnyBlockingProgress = false;
    }

    public async void SubscribeToUpdates()
    {
        await SubscribeToCloneLabUpdates();
        await SubscribeToGraveyardUpdates();
        await SubscribeToColosseumUpdates();
    }

    private bool conditionMet = false;

    private async Task WaitForCondition()
    {
        var tcs = new TaskCompletionSource<bool>();

        while (!Instance.IsSessionValid())
        {
            await Task.Delay(100); // Check the condition every 100ms
            if (Instance.IsSessionValid())
            {
                tcs.SetResult(true);
            }
        }

        await tcs.Task;
    }

    private void FindPDAs(Account account)
    {
        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("player"), account.PublicKey.KeyBytes},
            AnchorProgramIdPubKey, out PlayerDataPDA, out byte bump);

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes(levelSeed)},
            AnchorProgramIdPubKey, out GameDataPDA, out byte bump2);

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("profile"), account.PublicKey.KeyBytes},
            AnchorProgramIdPubKey, out ProfilePDA, out byte profileBump);

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("clone_lab"), AdminPubkey},
            AnchorProgramIdPubKey, out CloneLabPDA, out byte cloneLabBump);

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("colosseum"), AdminPubkey},
            AnchorProgramIdPubKey, out ColosseumPDA, out byte colosseumBump);

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("graveyard"), AdminPubkey},
            AnchorProgramIdPubKey, out GraveyardPDA, out byte graveyardBump);
    }

    private static async Task RequestAirdropIfSolValueIsLow()
    {
        var solBalance = await Web3.Instance.WalletBase.GetBalance();
        if (solBalance < 0.8f)
        {
            Debug.Log("Not enough sol. Requesting airdrop");
            var result = await Web3.Instance.WalletBase.RequestAirdrop(commitment: Commitment.Confirmed);
            if (!result.WasSuccessful)
            {
                Debug.Log("Airdrop failed. You can go to faucet.solana.com and request sol for this key: " + Web3.Instance.WalletBase.Account.PublicKey);
            }
            else
            {
                Debug.Log("Airdrop succesful.");

                if (solBalance < 0.8f)
                {
                    Debug.Log("Sol balance still too low. You can go to faucet.solana.com and request sol for this key: " + Web3.Instance.WalletBase.Account.PublicKey);
                }
            }
        }
        else
        {
            Debug.Log("Sufficient Sol in wallet: " + solBalance.ToString());
        }
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    public long GetSessionKeysEndTime()
    {
        return DateTimeOffset.UtcNow.AddDays(6).ToUnixTimeSeconds();
    }

    //private async Task SubscribeToPlayerDataUpdates()
    //{
    //    AccountResultWrapper<PlayerData> playerData = null;

    //    try
    //    {
    //        playerData = await anchorClient.GetPlayerDataAsync(PlayerDataPDA, Commitment.Confirmed);
    //        if (playerData.ParsedResult != null)
    //        {
    //            CurrentPlayerData = playerData.ParsedResult;
    //            OnPlayerDataChanged?.Invoke(playerData.ParsedResult);
    //            _isInitialized = true;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("Probably playerData not available " + e.Message);
    //    }

    //    if (playerData != null)
    //    {
    //        await anchorClient.SubscribePlayerDataAsync(PlayerDataPDA, (state, value, playerData) =>
    //        {
    //            OnReceivedPlayerDataUpdate(playerData);
    //        }, Commitment.Processed);
    //    }
    //}

    private void OnReceivedPlayerDataUpdate(PlayerData playerData)
    {
        Debug.Log($"Socket Message: Player has {playerData.Wood} wood now.");
        stopWatches[playerData.LastId].Stop();
        lastTransactionTimeInMs = stopWatches[playerData.LastId].ElapsedMilliseconds;
        CurrentPlayerData = playerData;
        OnPlayerDataChanged?.Invoke(playerData);
    }

    private async Task SubscribeToCloneLabUpdates()
    {
        AccountResultWrapper<CloneLab> cloneData = null;

        try
        {
            cloneData = await anchorClient.GetCloneLabAsync(CloneLabPDA, Commitment.Finalized);
            if (cloneData.ParsedResult != null)
            {
                CurrentCloneLab = cloneData.ParsedResult;
                OnCloneLabChanged?.Invoke(cloneData.ParsedResult);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Probably clone lab not available " + e.Message);
        }

        if (cloneData != null)
        {
            await anchorClient.SubscribeCloneLabAsync(CloneLabPDA, (state, value, gameData) =>
            {
                OnReceivedCloneLabUpdate(gameData);
            }, Commitment.Finalized);
        }
    }

    private void OnReceivedCloneLabUpdate(CloneLab cloneLab)
    {
        Debug.Log($"Socket Message: Total available brawlers: {cloneLab.Brawlers}.");
        CurrentCloneLab = cloneLab;
        OnCloneLabChanged?.Invoke(cloneLab);
    }

    private async Task SubscribeToProfileUpdates()
    {
        AccountResultWrapper<Profile> profileData = null;

        try
        {
            profileData = await anchorClient.GetProfileAsync(ProfilePDA, Commitment.Confirmed);
            if (profileData.ParsedResult != null)
            {
                _isInitialized = true;
                CurrentProfile = profileData.ParsedResult;
                OnProfileChanged?.Invoke(profileData.ParsedResult);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Probably profile not available: " + e.Message);
        }

        if (profileData != null)
        {
            await anchorClient.SubscribeProfileAsync(ProfilePDA, (state, value, gameData) =>
            {
                OnReceivedProfileUpdate(gameData);
            }, Commitment.Processed);
        }
    }

    private void OnReceivedProfileUpdate(Profile profile)
    {
        Debug.Log($"Socket Message: Profile username: {profile.Username}.");
        CurrentProfile = profile;
        OnProfileChanged?.Invoke(profile);
    }

    private async Task SubscribeToGraveyardUpdates()
    {
        AccountResultWrapper<Graveyard> graveyardData = null;

        try
        {
            graveyardData = await anchorClient.GetGraveyardAsync(GraveyardPDA, Commitment.Confirmed);
            if (graveyardData.ParsedResult != null)
            {
                CurrentGraveyard = graveyardData.ParsedResult;
                OnGraveyardChanged?.Invoke(graveyardData.ParsedResult);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Probably graveyard not available: " + e.Message);
        }

        if (graveyardData != null)
        {
            await anchorClient.SubscribeGraveyardAsync(GraveyardPDA, (state, value, gameData) =>
            {
                OnReceivedGraveyardUpdate(gameData);
            }, Commitment.Processed);
        }
    }

    private void OnReceivedGraveyardUpdate(Graveyard graveyard)
    {
        Debug.Log($"Socket Message: Graveyard fallen brawlers: {graveyard.Brawlers.Length}.");
        CurrentGraveyard = graveyard;
        OnGraveyardChanged?.Invoke(graveyard);
    }

    private async Task SubscribeToColosseumUpdates()
    {
        AccountResultWrapper<Colosseum> colosseumData = null;

        try
        {
            colosseumData = await anchorClient.GetColosseumAsync(ColosseumPDA, Commitment.Confirmed);
            if (colosseumData.ParsedResult != null)
            {
                CurrentColosseum = colosseumData.ParsedResult;
                OnColosseumChanged?.Invoke(colosseumData.ParsedResult);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Probably colosseum not available: " + e.Message);
        }

        if (colosseumData != null)
        {
            await anchorClient.SubscribeColosseumAsync(ColosseumPDA, (state, value, gameData) =>
            {
                OnReceivedColosseumUpdate(gameData);
            }, Commitment.Processed);
        }
    }

    private void OnReceivedColosseumUpdate(Colosseum colosseum)
    {
        Debug.Log($"Socket Message: Colosseum found pending lobbies: {colosseum.PendingBrawls.Length}.");
        Debug.Log($"Socket Message: Colosseum found active lobbies: {colosseum.ActiveBrawls.Length}.");
        Debug.Log($"Socket Message: Colosseum found total lobbies: {colosseum.EndedBrawls.Length}.");

        CurrentColosseum = colosseum;
        OnColosseumChanged?.Invoke(colosseum);
    }

    public async Task<List<Brawler>> FetchAllReadyBrawlers(CloneLab cloneLab)
    {
        List<Brawler> brawlers = new List<Brawler>();
        foreach (var brawler in cloneLab.Brawlers)
        {
            var brawlerData = await anchorClient.GetBrawlerAsync(brawler, Commitment.Confirmed);
            if (brawlerData.ParsedResult != null && brawlerData.ParsedResult.Owner == Web3.Account.PublicKey)
            {
                brawlers.Add(brawlerData.ParsedResult);
            }
        }

        return brawlers;
    }

    public async Task<List<Brawler>> FetchAllBrawlersFromBrawl(Brawl brawl)
    {
        List<Brawler> brawlers = new List<Brawler>();
        foreach (var brawler in brawl.Queue)
        {
            var brawlerData = await anchorClient.GetBrawlerAsync(brawler, Commitment.Confirmed);
            if (brawlerData.ParsedResult != null)
            {
                brawlers.Add(brawlerData.ParsedResult);
            }
        }

        return brawlers;
    }

    public async Task<List<Brawler>> FetchAllDeadBrawlers(Graveyard graveyard)
    {
        List<Brawler> brawlers = new List<Brawler>();
        foreach (var brawler in graveyard.Brawlers)
        {
            var brawlerData = await anchorClient.GetBrawlerAsync(brawler, Commitment.Confirmed);
            if (brawlerData.ParsedResult != null)
            {
                brawlers.Add(brawlerData.ParsedResult);
            }
        }

        return brawlers;
    }

    public async Task<List<Brawl>> FetchAllPendingBrawls(Colosseum colosseum)
    {
        List<Brawl> brawls = new List<Brawl>();
        foreach (var brawl in colosseum.PendingBrawls)
        {
            var brawlData = await anchorClient.GetBrawlAsync(brawl, Commitment.Confirmed);
            if (brawlData.ParsedResult != null)
            {
                brawls.Add(brawlData.ParsedResult);
            }
        }

        return brawls;
    }

    public async Task<List<Brawl>> FetchAllActiveBrawls(Colosseum colosseum)
    {
        List<Brawl> brawls = new List<Brawl>();
        foreach (var brawl in colosseum.ActiveBrawls)
        {
            var brawlData = await anchorClient.GetBrawlAsync(brawl, Commitment.Confirmed);
            if (brawlData.ParsedResult != null)
            {
                brawls.Add(brawlData.ParsedResult);
            }
        }

        return brawls;
    }

    public async Task<Brawl> FetchBrawl(PublicKey brawlKey)
    {
        Brawl fetchedBrawl = null;

        var brawlData = await anchorClient.GetBrawlAsync(brawlKey, Commitment.Confirmed);

        if (brawlData.ParsedResult != null)
        {
            fetchedBrawl = brawlData.ParsedResult;
        }

        return fetchedBrawl;
    }

    public async Task<Brawler> FetchBrawler(PublicKey brawler)
    {
        Brawler foundBrawler = null;

        var brawlerData = await anchorClient.GetBrawlerAsync(brawler, Commitment.Processed);

        if (brawlerData.ParsedResult != null)
        {
            Debug.Log($"Fetched brawler: {brawler.ToString()}");
        }

        if (brawlerData.ParsedResult != null && brawlerData.ParsedResult.Owner == Web3.Account.PublicKey)
        {
            Debug.Log($"I am the owner: {brawler.ToString()}");

            foundBrawler = brawlerData.ParsedResult;
        }

        return foundBrawler;
    }

    public async Task InitAccounts(bool useSession, string username)
    {
        var tx = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash()
        };

        CreateProfileAccounts cpaAccounts = new CreateProfileAccounts
        {
            Payer = Web3.Account,
            SystemProgram = SystemProgram.ProgramIdKey,
            Profile = ProfilePDA
        };

        CreateProfileArgs cpaArgs = new CreateProfileArgs
        {
            Username = username
        };

        var initTx = DeathbattleProgram.CreateProfile(cpaAccounts, cpaArgs, AnchorProgramIdPubKey);
        tx.Add(initTx);

        if (true)
        {
            if (!(await IsSessionTokenInitialized()))
            {
                var topUp = true;

                var validity = GetSessionKeysEndTime();
                var createSessionIX = sessionWallet.CreateSessionIX(topUp, validity);
                cpaAccounts.Payer = Web3.Account.PublicKey;
                tx.Add(createSessionIX);
                Debug.Log("Has no session -> partial sign");
                tx.PartialSign(new[] { Web3.Account, sessionWallet.Account });
            }
        }

        bool success = await SendAndConfirmTransaction(Web3.Wallet, tx, "initialize",
            () => { Debug.Log("Init account was successful"); }, s => { Debug.LogError("Init was not successful"); });

        await UpdateSessionValid();
        await SubscribeToProfileUpdates();
        await SubscribeToGraveyardUpdates();
        await SubscribeToCloneLabUpdates();
        await SubscribeToColosseumUpdates();
    }

    public async Task<bool> SendAndConfirmTransaction(WalletBase wallet, Transaction transaction, string label = "",
        Action onSucccess = null, Action<string> onError = null, bool isBlocking = true)
    {
        (isBlocking ? ref blockingTransactionsInProgress : ref nonBlockingTransactionsInProgress)++;
        LastError = String.Empty;

        Debug.Log("Sending and confirming transaction: " + label);
        RequestResult<string> res;
        try
        {
            res = await wallet.SignAndSendTransaction(transaction, commitment: Commitment.Confirmed);
        }
        catch (Exception e)
        {
            Debug.Log("Transaction exception " + e);
            blockingTransactionsInProgress--;
            (isBlocking ? ref blockingTransactionsInProgress : ref nonBlockingTransactionsInProgress)--;
            LastError = e.Message;
            onError?.Invoke(e.ToString());
            return false;
        }

        if (res.WasSuccessful && res.Result != null)
        {
            Debug.Log($"Transaction sent: {res.RawRpcResponse } signature: {res.Result}" );
            await Web3.Rpc.ConfirmTransaction(res.Result, Commitment.Confirmed);
        }
        else
        {
            Debug.LogError("Transaction failed: " + res.RawRpcResponse);
            if (res.RawRpcResponse.Contains("InsufficientFundsForRent"))
            {
                Debug.Log("Trigger session top up (Not implemented)");
                // TODO: this can probably happen when the session key runs out of funds. 
                //TriggerTopUpTransaction();
            }

            LastError = res.RawRpcResponse;
            (isBlocking ? ref blockingTransactionsInProgress : ref nonBlockingTransactionsInProgress)--;

            onError?.Invoke(res.RawRpcResponse);
            return false;
        }

        Debug.Log($"Send transaction {label} with response: {res.RawRpcResponse}");
        (isBlocking ? ref blockingTransactionsInProgress : ref nonBlockingTransactionsInProgress)--;
        onSucccess?.Invoke();
        return true;
    }

    public async Task RevokeSession()
    {
        await sessionWallet.CloseSession();
        Debug.Log("Session closed");
    }

    public async void StartBrawl(Action onBrawlCreated)
    {
        var transaction = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash(maxSeconds: 15)
        };

        PublicKey.TryFindProgramAddress(new[]
               {Encoding.UTF8.GetBytes("brawl"), ColosseumPDA.KeyBytes, BitConverter.GetBytes(CurrentColosseum.NumBrawls)},
           AnchorProgramIdPubKey, out BrawlPDA, out byte brawlBump);

        StartBrawlAccounts startBrawlAccounts = new StartBrawlAccounts
        {
            Brawl = BrawlPDA,
            Colosseum = ColosseumPDA,
            Payer = Web3.Account,
            SystemProgram = SystemProgram.ProgramIdKey
        };

        var startBrawlIX = DeathbattleProgram.StartBrawl(startBrawlAccounts, AnchorProgramIdPubKey);
        transaction.Add(startBrawlIX);
        Debug.Log("Sign and send start brawl");
        await SendAndConfirmTransaction(Web3.Wallet, transaction, "Start brawl.", onBrawlCreated);
    }

    public async void JoinBrawl(PublicKey brawlPDA, PublicKey brawler)
    {
        var transaction = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash(maxSeconds: 15)
        };

        JoinBrawlAccounts joinBrawlAccounts = new JoinBrawlAccounts
        {
            Brawl = brawlPDA,
            Brawler = brawler,
            CloneLab = CloneLabPDA,
            Colosseum = ColosseumPDA,
            Payer = Web3.Account,
            SystemProgram = SystemProgram.ProgramIdKey
        };

        JoinBrawlArgs joinBrawlArgs = new JoinBrawlArgs()
        {
            Brawler = brawler
        };

        var startBrawlIX = DeathbattleProgram.JoinBrawl(joinBrawlAccounts, joinBrawlArgs, AnchorProgramIdPubKey);
        transaction.Add(startBrawlIX);
        Debug.Log("Sign and send join brawl");
        await SendAndConfirmTransaction(Web3.Wallet, transaction, "Join brawl.");
    }

    //public async void JoinBrawl(bool useSession, PublicKey brawler, PublicKey pendingBrawlLobby, Action onSuccess)
    //{
    //    if (!Instance.IsSessionValid())
    //    {
    //        await Instance.UpdateSessionValid();
    //        ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
    //        return;
    //    }

    //    var transaction = new Transaction()
    //    {
    //        FeePayer = Web3.Account,
    //        Instructions = new List<TransactionInstruction>(),
    //        RecentBlockHash = await Web3.BlockHash(maxSeconds: 15)
    //    };

    //    JoinBrawlAccounts joinBrawlAccounts = new JoinBrawlAccounts
    //    {
    //        Brawl = pendingBrawlLobby,
    //        Brawler = brawler,
    //        Payer = Web3.Account,
    //        Colosseum = ColosseumPDA,
    //        CloneLab = CloneLabPDA,
    //        SystemProgram = SystemProgram.ProgramIdKey
    //    };

    //    JoinBrawlArgs jbArgs = new JoinBrawlArgs
    //    {
    //        Brawler = brawler,
    //        IndexHint = 0,
    //    };

    //    if (useSession)
    //    {
    //        transaction.FeePayer = sessionWallet.Account.PublicKey;
    //        joinBrawlAccounts.Signer = sessionWallet.Account.PublicKey;
    //        joinBrawlAccounts.SessionToken = sessionWallet.SessionTokenPDA;
    //        var joinBrawlIX = DeathbattleProgram.JoinBrawl(joinBrawlAccounts, jbArgs, AnchorProgramIdPubKey);
    //        transaction.Add(joinBrawlIX);
    //        Debug.Log("Sign and send join brawl with session");
    //        await SendAndConfirmTransaction(sessionWallet, transaction, "Join brawl with session.", isBlocking: false, onSucccess: onSuccess);
    //    }
    //    else
    //    {
    //        transaction.FeePayer = Web3.Account.PublicKey;
    //        joinBrawlAccounts.Signer = Web3.Account.PublicKey;
    //        var chopInstruction = LumberjackProgram.ChopTree(joinBrawlAccounts, levelSeed, transactionCounter, AnchorProgramIdPubKey);
    //        transaction.Add(chopInstruction);
    //        Debug.Log("Sign and send init without session");
    //        await SendAndConfirmTransaction(Web3.Wallet, transaction, "Chop Tree without session.", onSucccess: onSuccess);
    //    }

    //    if (CurrentCloneLab == null)
    //    {
    //        await SubscribeToCloneLabUpdates();
    //    }
    //}

    public async void CreateBrawler(bool useSession, Action onSuccess)
    {
        if (!Instance.IsSessionValid())
        {
            await Instance.UpdateSessionValid();
            ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
            return;
        }

        PublicKey BrawlerPDA;

        var tx = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash()
        };

        PublicKey.TryFindProgramAddress(new[]
                {Encoding.UTF8.GetBytes("brawler"), BrawlAnchorService.Instance.CloneLabPDA.KeyBytes, BitConverter.GetBytes(BrawlAnchorService.Instance.CurrentCloneLab.NumBrawlers)},
            BrawlAnchorService.AnchorProgramIdPubKey, out BrawlerPDA, out byte brawlerBump);

        CreateCloneAccounts ccAccounts = new CreateCloneAccounts
        {
            CloneLab = CloneLabPDA,
            Brawler = BrawlerPDA,
            Profile = ProfilePDA,
            Payer = Web3.Account.PublicKey,
            SystemProgram = SystemProgram.ProgramIdKey,
            SlotHashes = new PublicKey("SysvarS1otHashes111111111111111111111111111"),
        };

        var initTx = DeathbattleProgram.CreateClone(ccAccounts, BrawlAnchorService.AnchorProgramIdPubKey);
        tx.Add(initTx);

        if (true)
        {
            if (!(await BrawlAnchorService.Instance.IsSessionTokenInitialized()))
            {
                var topUp = true;

                var validity = BrawlAnchorService.Instance.GetSessionKeysEndTime();
                var createSessionIX = BrawlAnchorService.Instance.sessionWallet.CreateSessionIX(topUp, validity);
                ccAccounts.Payer = Web3.Account.PublicKey;
                tx.Add(createSessionIX);
                Debug.Log("Has no session -> partial sign");
                tx.PartialSign(new[] { Web3.Account, BrawlAnchorService.Instance.sessionWallet.Account });
            }
        }

        bool success = await BrawlAnchorService.Instance.SendAndConfirmTransaction(Web3.Wallet, tx, "create_clone",
            onSuccess, s => { Debug.LogError("Create Clone was not successful"); });
    }

    public async void ReviveBrawler(bool useSession, Action onSuccess)
    {
        if (!Instance.IsSessionValid())
        {
            await Instance.UpdateSessionValid();
            ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
            return;
        }

        PublicKey BrawlerPDA = CurrentGraveyard.Brawlers.ElementAt(0);

        var tx = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash()
        };

        // PublicKey.TryFindProgramAddress(new[]
        //         {Encoding.UTF8.GetBytes("brawler"), BrawlAnchorService.Instance.CloneLabPDA.KeyBytes, BitConverter.GetBytes(BrawlAnchorService.Instance.CurrentCloneLab.NumBrawlers)},
        //     BrawlAnchorService.AnchorProgramIdPubKey, out BrawlerPDA, out byte brawlerBump);

        ReviveCloneAccounts rcAccounts = new ReviveCloneAccounts
        {
            CloneLab = CloneLabPDA,
            Graveyard = GraveyardPDA,
            Brawler = BrawlerPDA,
            Profile = ProfilePDA,
            Payer = Web3.Account.PublicKey,
            SystemProgram = SystemProgram.ProgramIdKey,
        };

        var initTx = DeathbattleProgram.ReviveClone(rcAccounts, BrawlAnchorService.AnchorProgramIdPubKey);
        tx.Add(initTx);

        if (true)
        {
            if (!(await BrawlAnchorService.Instance.IsSessionTokenInitialized()))
            {
                var topUp = true;

                var validity = BrawlAnchorService.Instance.GetSessionKeysEndTime();
                var createSessionIX = BrawlAnchorService.Instance.sessionWallet.CreateSessionIX(topUp, validity);
                rcAccounts.Payer = Web3.Account.PublicKey;
                tx.Add(createSessionIX);
                Debug.Log("Has no session -> partial sign");
                tx.PartialSign(new[] { Web3.Account, BrawlAnchorService.Instance.sessionWallet.Account });
            }
        }

        bool success = await BrawlAnchorService.Instance.SendAndConfirmTransaction(Web3.Wallet, tx, "revive_clone",
            onSuccess, s => { Debug.LogError("Revive Clone was not successful"); });
    }

    public async void RunMatch(bool useSession, Action onSuccess, PublicKey brawlPDA)
    {
        if (!Instance.IsSessionValid())
        {
            await Instance.UpdateSessionValid();
            ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
            return;
        }

        var tx = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash()
        };

        // PublicKey.TryFindProgramAddress(new[]
        //         {Encoding.UTF8.GetBytes("brawler"), BrawlAnchorService.Instance.CloneLabPDA.KeyBytes, BitConverter.GetBytes(BrawlAnchorService.Instance.CurrentCloneLab.NumBrawlers)},
        //     BrawlAnchorService.AnchorProgramIdPubKey, out BrawlerPDA, out byte brawlerBump);

        RunMatchAccounts rmAccounts = new RunMatchAccounts
        {
            CloneLab = CloneLabPDA,
            Colosseum = ColosseumPDA,
            Graveyard = GraveyardPDA,
            Brawl = brawlPDA,
            Payer = Web3.Account.PublicKey,
            SystemProgram = SystemProgram.ProgramIdKey,
            SlotHashes = new PublicKey("SysvarS1otHashes111111111111111111111111111"),
        };

        var initTx = DeathbattleProgram.RunMatch(rmAccounts, BrawlAnchorService.AnchorProgramIdPubKey);
        tx.Add(initTx);

        if (true)
        {
            if (!(await BrawlAnchorService.Instance.IsSessionTokenInitialized()))
            {
                var topUp = true;

                var validity = BrawlAnchorService.Instance.GetSessionKeysEndTime();
                var createSessionIX = BrawlAnchorService.Instance.sessionWallet.CreateSessionIX(topUp, validity);
                rmAccounts.Payer = Web3.Account.PublicKey;
                tx.Add(createSessionIX);
                Debug.Log("Has no session -> partial sign");
                tx.PartialSign(new[] { Web3.Account, BrawlAnchorService.Instance.sessionWallet.Account });
            }
        }

        bool success = await BrawlAnchorService.Instance.SendAndConfirmTransaction(Web3.Wallet, tx, "run_match",
            () => { Debug.Log("Run Match was successful"); }, s => { Debug.LogError("Run Match was not successful"); });
    }

    public async void ClearEndedBrawl(bool useSession, Action onSuccess, PublicKey brawlPDA, PublicKey winner)
    {
        if (!Instance.IsSessionValid())
        {
            await Instance.UpdateSessionValid();
            ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
            return;
        }

        var tx = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash()
        };

        // PublicKey.TryFindProgramAddress(new[]
        //         {Encoding.UTF8.GetBytes("brawler"), BrawlAnchorService.Instance.CloneLabPDA.KeyBytes, BitConverter.GetBytes(BrawlAnchorService.Instance.CurrentCloneLab.NumBrawlers)},
        //     BrawlAnchorService.AnchorProgramIdPubKey, out BrawlerPDA, out byte brawlerBump);

        ClearEndedBrawlAccounts cebAccounts = new ClearEndedBrawlAccounts
        {
            CloneLab = CloneLabPDA,
            Colosseum = ColosseumPDA,
            Brawl = brawlPDA,
            Winner = winner,
            Payer = Web3.Account.PublicKey,
            Authority = AdminPubkey,
            SystemProgram = SystemProgram.ProgramIdKey,
        };

        var initTx = DeathbattleProgram.ClearEndedBrawl(cebAccounts, BrawlAnchorService.AnchorProgramIdPubKey);
        tx.Add(initTx);

        if (true)
        {
            if (!(await BrawlAnchorService.Instance.IsSessionTokenInitialized()))
            {
                var topUp = true;

                var validity = BrawlAnchorService.Instance.GetSessionKeysEndTime();
                var createSessionIX = BrawlAnchorService.Instance.sessionWallet.CreateSessionIX(topUp, validity);
                cebAccounts.Payer = Web3.Account.PublicKey;
                tx.Add(createSessionIX);
                Debug.Log("Has no session -> partial sign");
                tx.PartialSign(new[] { Web3.Account, BrawlAnchorService.Instance.sessionWallet.Account });
            }
        }

        bool success = await BrawlAnchorService.Instance.SendAndConfirmTransaction(Web3.Wallet, tx, "revive_clone",
            () => { Debug.Log("Clear Ended Brawl was successful"); }, s => { Debug.LogError("Clear Ended Brawl was not successful"); });
    }

    public async void ChopTree(bool useSession, Action onSuccess)
    {
        if (!Instance.IsSessionValid())
        {
            await Instance.UpdateSessionValid();
            ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.SessionPopup, new SessionPopupUiData());
            return;
        }

        // only for time tracking feel free to remove 
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        stopWatches[++transactionCounter] = stopWatch;

        var transaction = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash(maxSeconds: 15)
        };

        ChopTreeAccounts chopTreeAccounts = new ChopTreeAccounts
        {
            Player = PlayerDataPDA,
            GameData = GameDataPDA,
            SystemProgram = SystemProgram.ProgramIdKey
        };

        if (useSession)
        {
            transaction.FeePayer = sessionWallet.Account.PublicKey;
            chopTreeAccounts.Signer = sessionWallet.Account.PublicKey;
            chopTreeAccounts.SessionToken = sessionWallet.SessionTokenPDA;
            var chopInstruction = LumberjackProgram.ChopTree(chopTreeAccounts, levelSeed, transactionCounter, AnchorProgramIdPubKey);
            transaction.Add(chopInstruction);
            Debug.Log("Sign and send chop tree with session");
            await SendAndConfirmTransaction(sessionWallet, transaction, "Chop Tree with session.", isBlocking: false, onSucccess: onSuccess);
        }
        else
        {
            transaction.FeePayer = Web3.Account.PublicKey;
            chopTreeAccounts.Signer = Web3.Account.PublicKey;
            var chopInstruction = LumberjackProgram.ChopTree(chopTreeAccounts, levelSeed, transactionCounter, AnchorProgramIdPubKey);
            transaction.Add(chopInstruction);
            Debug.Log("Sign and send init without session");
            await SendAndConfirmTransaction(Web3.Wallet, transaction, "Chop Tree without session.", onSucccess: onSuccess);
        }

        if (CurrentCloneLab == null)
        {
            await SubscribeToCloneLabUpdates();
        }
    }

    public async Task<bool> IsSessionTokenInitialized()
    {
        var sessionTokenData = await Web3.Rpc.GetAccountInfoAsync(sessionWallet.SessionTokenPDA, Commitment.Confirmed);
        if (sessionTokenData.Result != null && sessionTokenData.Result.Value != null)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateSessionValid()
    {
        SessionToken sessionToken = await RequestSessionToken();

        if (sessionToken == null) return false;

        Debug.Log("Session token valid until: " + (new DateTime(1970, 1, 1)).AddSeconds(sessionToken.ValidUntil) +
                  " Now: " + DateTimeOffset.UtcNow);
        sessionValidUntil = sessionToken.ValidUntil;
        return IsSessionValid();
    }

    public async Task<SessionToken> RequestSessionToken()
    {
        ResponseValue<AccountInfo> sessionTokenData =
            (await Web3.Rpc.GetAccountInfoAsync(sessionWallet.SessionTokenPDA, Commitment.Confirmed)).Result;

        if (sessionTokenData == null) return null;
        if (sessionTokenData.Value == null || sessionTokenData.Value.Data[0] == null)
        {
            return null;
        }

        var sessionToken = SessionToken.Deserialize(Convert.FromBase64String(sessionTokenData.Value.Data[0]));

        return sessionToken;
    }

    private bool IsSessionValid()
    {
        return sessionValidUntil != null && sessionValidUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private async Task RefreshSessionWallet()
    {
        sessionWallet = await SessionWallet.GetSessionWallet(AnchorProgramIdPubKey, sessionKeyPassword,
            Web3.Wallet);
    }

    public async Task CreateNewSession()
    {
        var sessionToken = await Instance.RequestSessionToken();
        if (sessionToken != null)
        {
            await sessionWallet.CloseSession();
        }

        var transaction = new Transaction()
        {
            FeePayer = Web3.Account,
            Instructions = new List<TransactionInstruction>(),
            RecentBlockHash = await Web3.BlockHash(Commitment.Confirmed, false)
        };

        SessionWallet.Instance = null;
        await RefreshSessionWallet();
        var sessionIx = sessionWallet.CreateSessionIX(true, GetSessionKeysEndTime());
        transaction.Add(sessionIx);
        transaction.PartialSign(new[] { Web3.Account, sessionWallet.Account });

        var res = await Web3.Wallet.SignAndSendTransaction(transaction, commitment: Commitment.Confirmed);

        Debug.Log("Create session wallet: " + res.RawRpcResponse);
        await Web3.Wallet.ActiveRpcClient.ConfirmTransaction(res.Result, Commitment.Confirmed);
        var sessionValid = await UpdateSessionValid();
        Debug.Log("After create session, the session is valid: " + sessionValid);
    }
}