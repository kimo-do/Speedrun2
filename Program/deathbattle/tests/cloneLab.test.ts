import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { Deathbattle } from "../target/types/deathbattle";
import { assert } from "chai";

describe("deathbattle", () => {
  // Configure the client to use the local cluster.
  anchor.setProvider(anchor.AnchorProvider.env());

  const program = anchor.workspace.Deathbattle as Program<Deathbattle>;

  it("It can create a Clone Lab", async () => {
    const authority = anchor.web3.Keypair.generate();
    let airdrop = await anchor.getProvider().connection.requestAirdrop(authority.publicKey, 10000000000);
    await anchor.getProvider().connection.confirmTransaction(airdrop, "finalized");

    const cloneLabAddress = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("clone_lab"), authority.publicKey.toBuffer()],
      program.programId
    );
    
    const tx = await program.methods
    .createCloneLab()
    .accounts({
      cloneLab: cloneLabAddress[0],
      payer: authority.publicKey,
    })
    .signers([authority])
    .rpc();

    // console.log("Your transaction signature", tx);
    
    const cloneLab = await program.account.cloneLab.fetch(cloneLabAddress[0]);
    // console.log("Clone Lab:", cloneLab);

    assert.equal(cloneLab.bump, cloneLabAddress[1]);
    assert.isEmpty(cloneLab.brawlers);
  });
});
