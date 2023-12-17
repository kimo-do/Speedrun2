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

            public PublicKey Winner { get; set; }

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

                result.Winner = _data.GetPubKey(offset);
                offset += 32;
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

            public CharacterType CharacterType { get; set; }

            public BrawlerType BrawlerType { get; set; }

            public string Name { get; set; }

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
                result.CharacterType = (CharacterType)_data.GetU8(offset);
                offset += 1;
                result.BrawlerType = (BrawlerType)_data.GetU8(offset);
                offset += 1;
                offset += _data.GetBorshString(offset, out var resultName);
                result.Name = resultName;
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

        public partial class Colosseum
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 315776606391033891UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{35, 224, 80, 132, 38, 221, 97, 4};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "713aDhnJNK5";
            public byte Bump { get; set; }

            public uint NumBrawls { get; set; }

            public PublicKey[] PendingBrawls { get; set; }

            public PublicKey[] ActiveBrawls { get; set; }

            public PublicKey[] EndedBrawls { get; set; }

            public static Colosseum Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Colosseum result = new Colosseum();
                result.Bump = _data.GetU8(offset);
                offset += 1;
                result.NumBrawls = _data.GetU32(offset);
                offset += 4;
                int resultPendingBrawlsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.PendingBrawls = new PublicKey[resultPendingBrawlsLength];
                for (uint resultPendingBrawlsIdx = 0; resultPendingBrawlsIdx < resultPendingBrawlsLength; resultPendingBrawlsIdx++)
                {
                    result.PendingBrawls[resultPendingBrawlsIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                int resultActiveBrawlsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.ActiveBrawls = new PublicKey[resultActiveBrawlsLength];
                for (uint resultActiveBrawlsIdx = 0; resultActiveBrawlsIdx < resultActiveBrawlsLength; resultActiveBrawlsIdx++)
                {
                    result.ActiveBrawls[resultActiveBrawlsIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                int resultEndedBrawlsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.EndedBrawls = new PublicKey[resultEndedBrawlsLength];
                for (uint resultEndedBrawlsIdx = 0; resultEndedBrawlsIdx < resultEndedBrawlsLength; resultEndedBrawlsIdx++)
                {
                    result.EndedBrawls[resultEndedBrawlsIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                return result;
            }
        }

        public partial class Graveyard
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 12796968033564238423UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{87, 34, 160, 18, 160, 246, 151, 177};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "FaKdG6FbYdW";
            public byte Bump { get; set; }

            public PublicKey[] Brawlers { get; set; }

            public static Graveyard Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Graveyard result = new Graveyard();
                result.Bump = _data.GetU8(offset);
                offset += 1;
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

        public partial class Profile
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 13582644681592104376UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{184, 101, 165, 188, 95, 63, 127, 188};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "XqtBdGS7oVD";
            public byte Bump { get; set; }

            public string Username { get; set; }

            public static Profile Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Profile result = new Profile();
                result.Bump = _data.GetU8(offset);
                offset += 1;
                offset += _data.GetBorshString(offset, out var resultUsername);
                result.Username = resultUsername;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum DeathbattleErrorKind : uint
        {
            BrawlFull = 6000U,
            MissingBrawlerAccounts = 6001U,
            InvalidBrawler = 6002U,
            NameTooLong = 6003U,
            InvalidBrawl = 6004U,
            InvalidOwner = 6005U,
            NumericalOverflowError = 6006U,
            WinnerNotDetermined = 6007U,
            InvalidWinner = 6008U
        }
    }

    namespace Types
    {
        public partial class CreateProfileArgs
        {
            public string Username { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                offset += _data.WriteBorshString(Username, offset);
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out CreateProfileArgs result)
            {
                int offset = initialOffset;
                result = new CreateProfileArgs();
                offset += _data.GetBorshString(offset, out var resultUsername);
                result.Username = resultUsername;
                return offset - initialOffset;
            }
        }

        public partial class JoinBrawlArgs
        {
            public PublicKey Brawler { get; set; }

            public byte? IndexHint { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WritePubKey(Brawler, offset);
                offset += 32;
                if (IndexHint != null)
                {
                    _data.WriteU8(1, offset);
                    offset += 1;
                    _data.WriteU8(IndexHint.Value, offset);
                    offset += 1;
                }
                else
                {
                    _data.WriteU8(0, offset);
                    offset += 1;
                }

                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out JoinBrawlArgs result)
            {
                int offset = initialOffset;
                result = new JoinBrawlArgs();
                result.Brawler = _data.GetPubKey(offset);
                offset += 32;
                if (_data.GetBool(offset++))
                {
                    result.IndexHint = _data.GetU8(offset);
                    offset += 1;
                }

                return offset - initialOffset;
            }
        }

        public partial class Match
        {
            public byte Brawler0 { get; set; }

            public byte Brawler1 { get; set; }

            public byte Winner { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU8(Brawler0, offset);
                offset += 1;
                _data.WriteU8(Brawler1, offset);
                offset += 1;
                _data.WriteU8(Winner, offset);
                offset += 1;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out Match result)
            {
                int offset = initialOffset;
                result = new Match();
                result.Brawler0 = _data.GetU8(offset);
                offset += 1;
                result.Brawler1 = _data.GetU8(offset);
                offset += 1;
                result.Winner = _data.GetU8(offset);
                offset += 1;
                return offset - initialOffset;
            }
        }

        public enum CharacterType : byte
        {
            Default,
            Male1,
            Female1,
            Bonki,
            SolBlaze,
            Male2,
            Female2,
            Cop,
            Gangster
        }

        public enum BrawlerType : byte
        {
            Saber,
            Pistol,
            Hack,
            Katana,
            Virus,
            Laser
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

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Colosseum>>> GetColosseumsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Colosseum.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Colosseum>>(res);
            List<Colosseum> resultingAccounts = new List<Colosseum>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Colosseum.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Colosseum>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Graveyard>>> GetGraveyardsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Graveyard.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Graveyard>>(res);
            List<Graveyard> resultingAccounts = new List<Graveyard>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Graveyard.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Graveyard>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Profile>>> GetProfilesAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Profile.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Profile>>(res);
            List<Profile> resultingAccounts = new List<Profile>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Profile.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Profile>>(res, resultingAccounts);
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

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Colosseum>> GetColosseumAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Colosseum>(res);
            var resultingAccount = Colosseum.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Colosseum>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Graveyard>> GetGraveyardAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Graveyard>(res);
            var resultingAccount = Graveyard.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Graveyard>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Profile>> GetProfileAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Profile>(res);
            var resultingAccount = Profile.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Profile>(res, resultingAccount);
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

        public async Task<SubscriptionState> SubscribeColosseumAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Colosseum> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Colosseum parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Colosseum.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeGraveyardAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Graveyard> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Graveyard parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Graveyard.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeProfileAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Profile> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Profile parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Profile.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<RequestResult<string>> SendCreateProfileAsync(CreateProfileAccounts accounts, CreateProfileArgs args, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateProfile(accounts, args, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCreateCloneLabAsync(CreateCloneLabAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateCloneLab(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCreateColosseumAsync(CreateColosseumAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateColosseum(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCreateGraveyardAsync(CreateGraveyardAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateGraveyard(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCreateCloneAsync(CreateCloneAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CreateClone(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendReviveCloneAsync(ReviveCloneAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.ReviveClone(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendStartBrawlAsync(StartBrawlAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.StartBrawl(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendJoinBrawlAsync(JoinBrawlAccounts accounts, JoinBrawlArgs args, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.JoinBrawl(accounts, args, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendRunMatchAsync(RunMatchAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.RunMatch(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCloseAccountAsync(CloseAccountAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.CloseAccount(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendClearEndedBrawlAsync(ClearEndedBrawlAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.DeathbattleProgram.ClearEndedBrawl(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        protected override Dictionary<uint, ProgramError<DeathbattleErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<DeathbattleErrorKind>>{{6000U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.BrawlFull, "The Brawl is full.")}, {6001U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.MissingBrawlerAccounts, "Missing Brawler accounts.")}, {6002U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.InvalidBrawler, "Invalid Brawler.")}, {6003U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.NameTooLong, "Name too long.")}, {6004U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.InvalidBrawl, "Invalid Brawl.")}, {6005U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.InvalidOwner, "Invalid Owner of the Brawler.")}, {6006U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.NumericalOverflowError, "Numerical overflow error.")}, {6007U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.WinnerNotDetermined, "Winner not determined")}, {6008U, new ProgramError<DeathbattleErrorKind>(DeathbattleErrorKind.InvalidWinner, "Invalid Winner")}, };
        }
    }

    namespace Program
    {
        public class CreateProfileAccounts
        {
            public PublicKey Profile { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateCloneLabAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateColosseumAccounts
        {
            public PublicKey Colosseum { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateGraveyardAccounts
        {
            public PublicKey Graveyard { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class CreateCloneAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Brawler { get; set; }

            public PublicKey Profile { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }

            public PublicKey SlotHashes { get; set; }
        }

        public class ReviveCloneAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Graveyard { get; set; }

            public PublicKey Brawler { get; set; }

            public PublicKey Profile { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class StartBrawlAccounts
        {
            public PublicKey Brawl { get; set; }

            public PublicKey Colosseum { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class JoinBrawlAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Colosseum { get; set; }

            public PublicKey Brawl { get; set; }

            public PublicKey Brawler { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class RunMatchAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Colosseum { get; set; }

            public PublicKey Graveyard { get; set; }

            public PublicKey Brawl { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }

            public PublicKey SlotHashes { get; set; }
        }

        public class CloseAccountAccounts
        {
            public PublicKey Account { get; set; }

            public PublicKey Payer { get; set; }
        }

        public class ClearEndedBrawlAccounts
        {
            public PublicKey CloneLab { get; set; }

            public PublicKey Colosseum { get; set; }

            public PublicKey Brawl { get; set; }

            public PublicKey Winner { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public static class DeathbattleProgram
        {
            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateProfile(CreateProfileAccounts accounts, CreateProfileArgs args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Profile, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(15866949021771419105UL, offset);
                offset += 8;
                offset += args.Serialize(_data, offset);
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

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

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateColosseum(CreateColosseumAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Colosseum, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(16936109742051122458UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateGraveyard(CreateGraveyardAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Graveyard, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17170173318506891256UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CreateClone(CreateCloneAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawler, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Profile, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SlotHashes, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(16122771911115072516UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ReviveClone(ReviveCloneAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Graveyard, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawler, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Profile, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(8007496213246540780UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction StartBrawl(StartBrawlAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Colosseum, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(14009454267979503751UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction JoinBrawl(JoinBrawlAccounts accounts, JoinBrawlArgs args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Colosseum, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Brawler, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2932231159124621166UL, offset);
                offset += 8;
                offset += args.Serialize(_data, offset);
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction RunMatch(RunMatchAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Colosseum, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Graveyard, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SlotHashes, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(10808022854363113096UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CloseAccount(CloseAccountAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Account, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(1749686311319895933UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ClearEndedBrawl(ClearEndedBrawlAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.CloneLab, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Colosseum, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Brawl, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Winner, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(7537756438295858546UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}