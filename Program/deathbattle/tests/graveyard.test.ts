import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { Deathbattle } from "../target/types/deathbattle";
import { assert } from "chai";

describe("deathbattle", () => {
  // Configure the client to use the local cluster.
  anchor.setProvider(anchor.AnchorProvider.env());

  const program = anchor.workspace.Deathbattle as Program<Deathbattle>;

  it("It can create a Graveyard", async () => {
    const authority = anchor.web3.Keypair.generate();
    let airdrop = await anchor.getProvider().connection.requestAirdrop(authority.publicKey, 10000000000);
    await anchor.getProvider().connection.confirmTransaction(airdrop, "finalized");

    const graveyardAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("graveyard"), authority.publicKey.toBuffer()],
      program.programId
    );
    
    const tx = await program.methods
    .createGraveyard()
    .accounts({
      graveyard: graveyardAddress[0],
      payer: authority.publicKey,
    })
    .signers([authority])
    .rpc();

    // console.log("Your transaction signature", tx);
    
    const graveyard = await program.account.graveyard.fetch(graveyardAddress[0]);
    // console.log("Graveyard:", graveyard);

    assert.equal(graveyard.bump, graveyardAddress[1]);
    assert.isEmpty(graveyard.brawlers);
  });
});
