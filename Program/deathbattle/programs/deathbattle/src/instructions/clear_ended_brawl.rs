use anchor_lang::prelude::*;

use crate::{error::BrawlError, Brawl, Colosseum, AUTH_PUBKEY};

#[derive(Accounts)]
pub struct ClearEndedBrawl<'info> {
    #[account(
        mut,
        realloc=colosseum.len() - 32,
        realloc::payer=authority,
        realloc::zero=false
    )]
    pub colosseum: Account<'info, Colosseum>,

    /// The brawl to remove
    pub brawl: Account<'info, Brawl>,

    #[account(mut)]
    pub payer: Signer<'info>,

    #[account(mut, address = AUTH_PUBKEY)]
    pub authority: SystemAccount<'info>,

    pub system_program: Program<'info, System>,
}

impl<'info> ClearEndedBrawl<'info> {
    pub fn handler(ctx: Context<ClearEndedBrawl>) -> Result<()> {
        if let Some(index) = ctx
            .accounts
            .colosseum
            .ended_brawls
            .iter()
            .position(|value| *value == ctx.accounts.brawl.key())
        {
            ctx.accounts.colosseum.active_brawls.swap_remove(index);
        } else {
            return err!(BrawlError::InvalidBrawl);
        }

        Ok(())
    }
}
