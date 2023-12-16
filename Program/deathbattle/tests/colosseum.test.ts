import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { Deathbattle } from "../target/types/deathbattle";
import { assert } from "chai";

describe("deathbattle", () => {
  // Configure the client to use the local cluster.
  anchor.setProvider(anchor.AnchorProvider.env());

  const program = anchor.workspace.Deathbattle as Program<Deathbattle>;

  it("It can create a Colosseum", async () => {
    const authority = anchor.web3.Keypair.generate();
    let airdrop = await anchor.getProvider().connection.requestAirdrop(authority.publicKey, 10000000000);
    await anchor.getProvider().connection.confirmTransaction(airdrop, "finalized");

    const colosseumAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("colosseum"), authority.publicKey.toBuffer()],
      program.programId
    );
    
    const tx = await program.methods
    .createColosseum()
    .accounts({
      colosseum: colosseumAddress[0],
      payer: authority.publicKey,
    })
    .signers([authority])
    .rpc();

    // console.log("Your transaction signature", tx);
    
    const colosseum = await program.account.colosseum.fetch(colosseumAddress[0]);
    // console.log("Colosseum:", colosseum);

    assert.equal(colosseum.bump, colosseumAddress[1]);
    assert.isEmpty(colosseum.activeBrawls);
    assert.isEmpty(colosseum.pendingBrawls);
    assert.equal(colosseum.numBrawls, 0);
  });
});
