use anchor_lang::prelude::*;

use crate::{error::BrawlError, Brawl, Brawler, CloneLab, MAX_BRAWLERS};

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
    pub brawl: Account<'info, Brawl>,
    pub brawler: Account<'info, Brawler>,
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> JoinBrawl<'info> {
    pub fn handler(ctx: Context<JoinBrawl>, args: JoinBrawlArgs) -> Result<()> {
        if let Some(index) = ctx
            .accounts
            .clone_lab
            .brawlers
            .iter()
            .position(|value| *value == args.brawler)
        {
            ctx.accounts.clone_lab.brawlers.swap_remove(index);
        } else {
            return err!(BrawlError::InvalidBrawler);
        }

        if ctx.accounts.brawl.queue.len() == MAX_BRAWLERS {
            return err!(BrawlError::BrawlFull);
        } else {
            ctx.accounts.brawl.queue.push(args.brawler);
        }

        Ok(())
    }
}
