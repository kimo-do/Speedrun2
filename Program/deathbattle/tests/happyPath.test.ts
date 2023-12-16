import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { Deathbattle } from "../target/types/deathbattle";
import { assert } from "chai";

describe("deathbattle", () => {
  // Configure the client to use the local cluster.
  anchor.setProvider(anchor.AnchorProvider.env());

  const program = anchor.workspace.Deathbattle as Program<Deathbattle>;

  it("It can run the happy path", async () => {
    // Set up the authority and give it funds.
    const authority = anchor.web3.Keypair.generate();
    let airdropAuthority = await anchor.getProvider().connection.requestAirdrop(authority.publicKey, 10000000000);
    await anchor.getProvider().connection.confirmTransaction(airdropAuthority, "finalized");
    // Set up the player and give it funds.
    const player = anchor.web3.Keypair.generate();
    let airdropPlayer = await anchor.getProvider().connection.requestAirdrop(player.publicKey, 10000000000);
    await anchor.getProvider().connection.confirmTransaction(airdropPlayer, "finalized");

    // Create the player account.
    const profileAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("profile"), player.publicKey.toBuffer()],
      program.programId
    );
    // const createProfileTx = await anchor.getProvider().sendAndConfirm(
    await program.methods
      .createProfile({
        username: "Blockiosaurus"
      })
      .accounts({
        profile: profileAddress[0],
        payer: player.publicKey,
      })
      .signers([player])
      .rpc({ skipPreflight: true });
    //   [player]
    // );

    // Create the Clone Lab, Colosseum, and Graveyard to initialize global state.
    const cloneLabAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("clone_lab"), authority.publicKey.toBuffer()],
      program.programId
    );
    const colosseumAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("colosseum"), authority.publicKey.toBuffer()],
      program.programId
    );
    const graveyardAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("graveyard"), authority.publicKey.toBuffer()],
      program.programId
    );

    const createCloneLabTx = await program.methods
      .createCloneLab()
      .accounts({
        cloneLab: cloneLabAddress[0],
        payer: authority.publicKey,
      })
      .signers([authority])
      .rpc();

    const createColosseumTx = await program.methods
      .createColosseum()
      .accounts({
        colosseum: colosseumAddress[0],
        payer: authority.publicKey,
      })
      .signers([authority])
      .rpc();

    const createGraveyardTx = await program.methods
      .createGraveyard()
      .accounts({
        graveyard: graveyardAddress[0],
        payer: authority.publicKey,
      })
      .signers([authority])
      .rpc();

    await printState(program, cloneLabAddress[0], colosseumAddress[0], graveyardAddress[0]);

    let brawlerAddresses: [anchor.web3.PublicKey, number][] = [];
    let brawlers = [];
    // Create brawlers in the Clone Lab.
    for (let i = 0; i < 8; i++) {
      let cloneLab = await program.account.cloneLab.fetch(cloneLabAddress[0]);
      let numBrawlersSeed = new anchor.BN(cloneLab.numBrawlers).toBuffer("le", 2);
      brawlerAddresses.push(anchor.web3.PublicKey.findProgramAddressSync(
        [Buffer.from("brawler"), cloneLabAddress[0].toBuffer(), numBrawlersSeed],
        program.programId
      ));
      const createBrawlerTx = await program.methods
        .createClone()
        .accounts({
          cloneLab: cloneLabAddress[0],
          brawler: brawlerAddresses[i][0],
          profile: profileAddress[0],
          payer: player.publicKey,
          slotHashes: anchor.web3.SYSVAR_SLOT_HASHES_PUBKEY,
        })
        .signers([player])
        .rpc({ skipPreflight: true });
      brawlers.push(await program.account.brawler.fetch(brawlerAddresses[i][0]));
      console.log("Brawler:", brawlers[i]);
    }

    await printState(program, cloneLabAddress[0], colosseumAddress[0], graveyardAddress[0]);

    // Create a brawl in the Colosseum.
    let brawlAddresses: [anchor.web3.PublicKey, number][] = [];
    const colosseum = await program.account.colosseum.fetch(colosseumAddress[0]);
    let numBrawlsSeed = new anchor.BN(colosseum.numBrawls).toBuffer("le", 4);
      brawlAddresses.push(anchor.web3.PublicKey.findProgramAddressSync(
        [Buffer.from("brawl"), colosseumAddress[0].toBuffer(), numBrawlsSeed],
        program.programId
      ));
    const createBrawlTx = await program.methods
      .startBrawl()
      .accounts({
        brawl: brawlAddresses[0][0],
        colosseum: colosseumAddress[0],
        payer: player.publicKey,
      })
      .signers([player])
      .rpc({ skipPreflight: true });
    const brawl = await program.account.brawl.fetch(brawlAddresses[0][0]);
    console.log("Brawl:", brawl);

    await printState(program, cloneLabAddress[0], colosseumAddress[0], graveyardAddress[0]);
  });

  async function printState(program: Program<Deathbattle>, cloneLabAddress: anchor.web3.PublicKey, colosseumAddress: anchor.web3.PublicKey, graveyardAddress: anchor.web3.PublicKey) {
    const cloneLab = await program.account.cloneLab.fetch(cloneLabAddress);
    console.log("Clone Lab:", cloneLab);
    const colosseum = await program.account.colosseum.fetch(colosseumAddress);
    console.log("Colosseum:", colosseum);
    const graveyard = await program.account.graveyard.fetch(graveyardAddress);
    console.log("Graveyard:", graveyard);
  }
});

