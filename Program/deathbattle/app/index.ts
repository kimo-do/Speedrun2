import { web3, Program, AnchorProvider, Wallet } from '@coral-xyz/anchor'
import { readFileSync } from 'fs';

const { Command } = require('commander');
const idl = require('../target/idl/deathbattle.json');

const programID = new web3.PublicKey('BRAWLHsgvJBQGx4EzNuqKpbbv8q3LhcYbL1bHqbgVtaJ');
const program = new Command();

program
    .name('Rebirth Rumble Manager')
    .description('CLI to manage the Rebirth Rumble program')
    .version('0.1.0');

program.command('create_clone_lab')
    .description('Create a clone lab')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const cloneLabAddress = web3.PublicKey.findProgramAddressSync(
            [Buffer.from("clone_lab"), provider.wallet.publicKey.toBuffer()],
            program.programId
        );

        const tx = await program.methods
            .createCloneLab()
            .accounts({
                cloneLab: cloneLabAddress[0],
                payer: provider.wallet.publicKey,
            })
            // .signers([provider.wallet])
            .rpc();

        console.log("Your transaction signature", tx);

        const cloneLab = await program.account.cloneLab.fetch(cloneLabAddress[0]);
        console.log("Clone Lab:", cloneLab);
    });

program.command('create_colosseum')
    .description('Close a Colosseum')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const colosseumAddress = web3.PublicKey.findProgramAddressSync(
            [Buffer.from("colosseum"), provider.wallet.publicKey.toBuffer()],
            program.programId
        );

        const tx = await program.methods
            .createColosseum()
            .accounts({
                colosseum: colosseumAddress[0],
                payer: provider.wallet.publicKey,
            })
            // .signers([provider.wallet])
            .rpc();

        console.log("Your transaction signature", tx);

        const colosseum = await program.account.colosseum.fetch(colosseumAddress[0]);
        console.log("Colosseum:", colosseum);
    });

program.command('print_colosseum')
    .description('Print a Colosseum')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const colosseumAddress = web3.PublicKey.findProgramAddressSync(
            [Buffer.from("colosseum"), provider.wallet.publicKey.toBuffer()],
            program.programId
        );

        const colosseum = await program.account.colosseum.fetch(colosseumAddress[0]);
        console.log("Colosseum:", colosseum);
    });

program.command('close_colosseum')
    .description('Create a Colosseum')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const colosseumAddress = web3.PublicKey.findProgramAddressSync(
            [Buffer.from("colosseum"), provider.wallet.publicKey.toBuffer()],
            program.programId
        );

        const tx = await program.methods
            .closeAccount()
            .accounts({
                account: colosseumAddress[0],
                payer: provider.wallet.publicKey,
            })
            // .signers([provider.wallet])
            .rpc();

        console.log("Your transaction signature", tx);

        const colosseum = await program.account.colosseum.fetch(colosseumAddress[0]);
        console.log("Colosseum:", colosseum);
    });

program.command('create_graveyard')
    .description('Create a Graveyard')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const graveyardAddress = web3.PublicKey.findProgramAddressSync(
            [Buffer.from("graveyard"), provider.wallet.publicKey.toBuffer()],
            program.programId
        );

        const tx = await program.methods
            .createGraveyard()
            .accounts({
                graveyard: graveyardAddress[0],
                payer: provider.wallet.publicKey,
            })
            // .signers([provider.wallet])
            .rpc();

        console.log("Your transaction signature", tx);

        const graveyard = await program.account.graveyard.fetch(graveyardAddress[0]);
        console.log("Graveyard:", graveyard);
    });

program.command('close_all_brawls')
    .description('Close all brawls')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const allBrawls = await program.account.brawl.all();
        console.log("All Brawls:", allBrawls);

        allBrawls.forEach(async (brawl) => {
            const tx = await program.methods
                .closeAccount()
                .accounts({
                    account: brawl.publicKey,
                    payer: provider.wallet.publicKey,
                })
                // .signers([provider.wallet])
                .rpc();

            console.log("Your transaction signature", tx);
        });
    });

    program.command('print_all_brawls')
    .description('Close all brawls')
    .option('-r --rpc <string>', 'The endpoint to connect to.')
    .option('-k --keypair <string>', 'Solana wallet location')
    .action(async (str: any, options: any) => {
        const { rpc, keypair } = options.opts();

        const connection = new web3.Connection(rpc, 'confirmed');
        const provider = new AnchorProvider(connection, loadWalletKey(keypair), {});
        const program = new Program(idl, programID, provider);

        const allBrawls = await program.account.brawl.all();
        // console.log("All Brawls:", allBrawls);
        allBrawls.forEach(async (brawl) => {
            const brawlAccount = await program.account.brawl.fetch(brawl.publicKey);
            console.log("Brawl:", brawlAccount);
        });
    });

program.parse(process.argv);

export function loadWalletKey(keypair: string): Wallet {
    if (!keypair || keypair == '') {
        throw new Error('Keypair is required!');
    }
    const loaded = web3.Keypair.fromSecretKey(
        new Uint8Array(JSON.parse(readFileSync(keypair).toString()))
    );
    return new Wallet(loaded);
}