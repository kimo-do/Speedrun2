use anchor_lang::prelude::*;

use crate::{Brawl, Colosseum};

#[derive(Accounts)]
pub struct StartBrawl<'info> {
    #[account(
        init,
        payer=payer,
        space=Brawl::LEN,
        seeds=[
            b"brawl".as_ref(),
            payer.key().as_ref(),
            colosseum.key().as_ref(),
            colosseum.num_brawls.to_le_bytes().as_ref()
        ],
        bump
    )]
    pub brawl: Account<'info, Brawl>,

    #[account(mut)]
    pub colosseum: Account<'info, Colosseum>,

    #[account(mut)]
    pub payer: Signer<'info>,

    pub system_program: Program<'info, System>,
}

impl<'info> StartBrawl<'info> {
    pub fn handler(ctx: Context<StartBrawl>) -> Result<()> {
        ctx.accounts.brawl.bump = ctx.bumps.brawl;

        ctx.accounts.colosseum.num_brawls += 1;

        Ok(())
    }
}
