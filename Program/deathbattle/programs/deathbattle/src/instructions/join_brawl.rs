use anchor_lang::prelude::*;

use crate::{error::BrawlError, Brawl, Brawler, CloneLab, Colosseum, MAX_BRAWLERS};

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Default, PartialEq)]
pub struct JoinBrawlArgs {
    /// The address of the Brawler.
    pub brawler: Pubkey,
    /// The rough index of the brawler in the Brawl queue.
    pub index_hint: Option<u8>,
}

#[derive(Accounts)]
pub struct JoinBrawl<'info> {
    #[account(mut)]
    pub clone_lab: Account<'info, CloneLab>,

    #[account(mut)]
    pub colosseum: Account<'info, Colosseum>,

    #[account(
        mut,
        realloc=Brawl::LEN,
        realloc::payer=payer,
        realloc::zero=false
    )]
    pub brawl: Account<'info, Brawl>,

    pub brawler: Account<'info, Brawler>,

    #[account(mut)]
    pub payer: Signer<'info>,

    pub system_program: Program<'info, System>,
}

impl<'info> JoinBrawl<'info> {
    pub fn handler(ctx: Context<JoinBrawl>, args: JoinBrawlArgs) -> Result<()> {
        // Remove the brawler from the list of brawlers in the Clone Lab.
        if let Some(index) = ctx
            .accounts
            .clone_lab
            .brawlers
            .iter()
            .position(|value| *value == args.brawler)
        {
            if ctx.accounts.brawler.owner == ctx.accounts.payer.key() {
                ctx.accounts.clone_lab.brawlers.swap_remove(index);
            } else {
                return err!(BrawlError::InvalidOwner);
            }
        } else {
            return err!(BrawlError::InvalidBrawler);
        }

        // Add the brawler to the Brawl queue if the queue in the provided Brawl isn't full.
        if ctx.accounts.brawl.queue.len() == MAX_BRAWLERS {
            return err!(BrawlError::BrawlFull);
        } else {
            ctx.accounts.brawl.queue.push(args.brawler);
        }

        // Remove the Brawl from the list of pending Brawls in the Colosseum if it has been filled,
        // and insert it into the list of active Brawls.
        if ctx.accounts.brawl.queue.len() == MAX_BRAWLERS {
            if let Some(index) = ctx
                .accounts
                .colosseum
                .pending_brawls
                .iter()
                .position(|value| *value == ctx.accounts.brawl.key())
            {
                ctx.accounts.colosseum.pending_brawls.swap_remove(index);
                ctx.accounts
                    .colosseum
                    .active_brawls
                    .push(ctx.accounts.brawl.key());
            } else {
                return err!(BrawlError::InvalidBrawl);
            }
        }

        Ok(())
    }
}
