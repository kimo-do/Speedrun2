use anchor_lang::{prelude::*, solana_program};

use crate::{
    error::BrawlError, rand_choice, resize_or_reallocate_account_raw, Brawl, CloneLab, Colosseum,
    Graveyard,
};

#[derive(Accounts)]
pub struct RunMatch<'info> {
    /// The Clone Lab account. The winner will go back here.
    #[account(
        mut,
        // realloc=clone_lab.len() + 32,
        // realloc::payer=payer,
        // realloc::zero=false
    )]
    pub clone_lab: Account<'info, CloneLab>,

    /// The Colosseum account. The brawl will transition from pending to ended.
    #[account(mut)]
    pub colosseum: Account<'info, Colosseum>,

    /// The Graveyard account. The losing clones will go here.
    #[account(
        mut,
        // realloc=graveyard.len() + (32 * 7),
        // realloc::payer=payer,
        // realloc::zero=false
    )]
    pub graveyard: Account<'info, Graveyard>,

    #[account(mut)]
    pub brawl: Account<'info, Brawl>,

    #[account(mut)]
    pub payer: Signer<'info>,

    pub system_program: Program<'info, System>,

    /// CHECK: Checked in the instruction.
    pub slot_hashes: UncheckedAccount<'info>,
}

impl<'info> RunMatch<'info> {
    pub fn handler(ctx: Context<RunMatch>) -> Result<()> {
        assert!(*ctx.accounts.slot_hashes.key == solana_program::sysvar::slot_hashes::ID);

        // let mut brawlers = vec![];
        // let queue = ctx.accounts.brawl.queue.clone();
        // for brawler in queue.iter() {
        //     for account in ctx.remaining_accounts.iter() {
        //         if brawler == account.key {
        //             brawlers.push(ctx.accounts.brawl.queue.pop().unwrap());
        //         }
        //     }
        // }

        // if !queue.is_empty() {
        //     err!(BrawlError::MissingBrawlerAccounts)?;
        // }

        let mut brawlers = ctx.accounts.brawl.queue.clone();

        match rand_choice(&brawlers, &ctx.accounts.slot_hashes.to_account_info()) {
            Ok(winner) => {
                ctx.accounts.brawl.winner = winner;
                // Remove the winner from the brawlers list and add it back to the Clone Lab.
                if let Some(index) = brawlers.iter().position(|value| *value == winner) {
                    brawlers.swap_remove(index);
                    ctx.accounts.clone_lab.brawlers.push(winner.key());
                };
            }
            Err(_e) => {
                err!(BrawlError::MissingBrawlerAccounts)?;
            }
        }

        // Send the losers to the Graveyard.
        for loser in brawlers.iter() {
            ctx.accounts.graveyard.brawlers.push(loser.key());
        }

        if let Some(index) = ctx
            .accounts
            .colosseum
            .active_brawls
            .iter()
            .position(|value| *value == ctx.accounts.brawl.key())
        {
            ctx.accounts.colosseum.active_brawls.swap_remove(index);
            ctx.accounts
                .colosseum
                .ended_brawls
                .push(ctx.accounts.brawl.key());
        } else {
            err!(BrawlError::InvalidBrawl)?;
        }

        resize_or_reallocate_account_raw(
            &ctx.accounts.clone_lab.to_account_info(),
            &ctx.accounts.payer.to_account_info(),
            &ctx.accounts.system_program.to_account_info(),
            ctx.accounts.clone_lab.len() + 32,
        )?;

        resize_or_reallocate_account_raw(
            &ctx.accounts.graveyard.to_account_info(),
            &ctx.accounts.payer.to_account_info(),
            &ctx.accounts.system_program.to_account_info(),
            ctx.accounts.clone_lab.len() + (32 * 7),
        )?;

        Ok(())
    }
}
