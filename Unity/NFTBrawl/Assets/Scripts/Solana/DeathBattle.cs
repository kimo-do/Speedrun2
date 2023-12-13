using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using Deathbattle;
using Deathbattle.Program;
using Deathbattle.Errors;
using Deathbattle.Accounts;
using Deathbattle.Types;

namespace Deathbattle
{
    namespace Accounts
    {
        public partial class Brawl
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 17425906383086167482UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{186, 37, 16, 109, 135, 65, 213, 241};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "Y8qaYpZ1o4k";
            public byte Bump { get; set; }

            public PublicKey[] Queue { get; set; }

            public Match[] Matches { get; set; }

            public static Brawl Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Brawl result = new Brawl();
                result.Bump = _data.GetU8(offset);
                offset += 1;
                int resultQueueLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Queue = new PublicKey[resultQueueLength];
                for (uint resultQueueIdx = 0; resultQueueIdx < resultQueueLength; resultQueueIdx++)
                {
                    result.Queue[resultQueueIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                int resultMatchesLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Matches = new Match[resultMatchesLength];
                for (uint resultMatchesIdx = 0; resultMatchesIdx < resultMatchesLength; resultMatchesIdx++)
                {
                    offset += Match.Deserialize(_data, offset, out var resultMatchesresultMatchesIdx);
                    result.Matches[resultMatchesIdx] = resultMatchesresultMatchesIdx;
                }

                return result;
            }
        }

        public partial class Brawler
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 9059971701405562958UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{78, 184, 94, 185, 66, 124, 187, 125};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "EAghfwkfvhS";
            public byte Bump { get; set; }

            public PublicKey Owner { get; set; }

            public static Brawler Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Brawler result = new Brawler();
                result.Bump = _data.GetU8(offset);
                offset += 1;
                result.Owner = _data.GetPubKey(offset);
                offset += 32;
                return result;
            }
        }

        public partial class CloneLab
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 11407528245530951168UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{0, 126, 27, 232, 127, 174, 79, 158};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "15nBSiXoJKw";
            public byte Bump { get; set; }

            public ushort NumBrawlers { get; set; }

            public PublicKey[] Brawlers { get; set; }

            public static CloneLab Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                CloneLab result = new CloneLab();
                result.Bump = _data.GetU8(offset);
                offset += 1;
                result.NumBrawlers = _data.GetU16(offset);
                offset += 2;
                int resultBrawlersLength = (int)_data.GetU32(offset);
                offset += 4;
                result.Brawlers = new PublicKey[resultBrawlersLength];
                for (uint resultBrawlersIdx = 0; resultBrawlersIdx < resultBrawlersLength; resultBrawlersIdx++)
                {
                    result.Brawlers[resultBrawlersIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                return result;
            }
        }
    }

    namespace Errors
    {
        public enum DeathbattleErrorKind : uint
        {
            BrawlFull = 6000U,
            MissingBrawlerAccounts = 6001U
        }
    }

    namespace Types
    {
        public partial class Match
        {
            public (byte,byte) Brawlers { get; set; }

            public byte Winner { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                offset += SerializeBrawlers(_data, offset);
                _data.WriteU8(Winner, offset);
                offset += 1;
                return offset - initialOffset;
            }

            private int SerializeBrawlers(byte[] data, int offset)
            {
                data[offset] = Brawlers.Item1;
                data[offset + 1] = Brawlers.Item2;
                return 2; // Two bytes were written
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out Match result)
            {
                int offset = initialOffset;
                result = new Match();
                DeserializeBrawlers(_data, offset, out var resultBrawlers);
                result.Brawlers = resultBrawlers;
                offset += 2; // Increase offset by 2, as two bytes are read for Brawlers
                result.Winner = _data.GetU8(offset);
                offset += 1;
                return offset - initialOffset;
            }

            private static void DeserializeBrawlers(ReadOnlySpan<byte> data, int offset, out (byte, byte) brawlers)
            {
                brawlers = (data[offset], data[offset + 1]);
            }
        }
    }

    public partial class DeathbattleClient : TransactionalBaseClient<DeathbattleErrorKind>
    {
        public DeathbattleClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawl>>> GetBrawlsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Brawl.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawl>>(res);
            List<Brawl> resultingAccounts = new List<Brawl>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Brawl.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawl>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawler>>> GetBrawlersAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Brawler.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawler>>(res);
            List<Brawler> resultingAccounts = new List<Brawler>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Brawler.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Brawler>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<CloneLab>>> GetCloneLabsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = CloneLab.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<CloneLab>>(res);
            List<CloneLab> resultingAccounts = new List<CloneLab>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => CloneLab.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<CloneLab>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Brawl>> GetBrawlAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Brawl>(res);
            var resultingAccount = Brawl.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Brawl>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Brawler>> GetBrawlerAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Brawler>(res);
            var resultingAccount = Brawler.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Brawler>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<CloneLab>> GetCloneLabAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<CloneLab>(res);
            var resultingAccount = CloneLab.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<CloneLab>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeBrawlAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Brawl> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Brawl parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Brawl.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeBrawlerAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Brawler> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Brawler parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Brawler.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeCloneLabAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, CloneLab> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                CloneLab parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = CloneLab.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<RequestResult<string>> SendCreateCloneLabAsync(CreateCloneLabAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateCloneLab(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCreateCloneAsync(CreateCloneAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateClone(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendStartBrawlAsync(StartBrawlAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.StartBrawl(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendJoinBrawlAsync(JoinBrawlAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.JoinBrawl(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendRunMatchAsync(RunMatchAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.RunMatch(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        protected override Dictionary<uint, ProgramError<DeathbattleErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<DeathbattleErrorKind>>{{6000U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.BrawlFull, "The Brawl is full.")}, {6001U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.MissingBrawlerAccounts, "Missing Brawler accounts.")}, };
        }
    }

    namespace Program
    {
        public class CreateCloneLabAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateCloneAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Brawler { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class StartBrawlAccounts
        {
            public PublicKey Brawl { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class JoinBrawlAccounts
        {
            public PublicKey Brawl { get; set; }

            public PublicKey Brawler { get; set; }

            public PublicKey Mint { get; set; }

            public PublicKey TokenAccount { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class RunMatchAccounts
        {
            public PublicKey Brawl { get; set; }

            public PublicKey Payer { get; set; }
        }

        public static class DeathbattleProgram
        {
            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateCloneLab(CreateCloneLabAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(1848630009599871168UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateClone(CreateCloneAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawler, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(16122771911115072516UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction StartBrawl(StartBrawlAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(14009454267979503751UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction JoinBrawl(JoinBrawlAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawler, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Mint, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2932231159124621166UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction RunMatch(RunMatchAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(10808022854363113096UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}