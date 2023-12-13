use anchor_lang::prelude::*;

use crate::Brawl;

#[derive(Accounts)]
pub struct StartBrawl<'info> {
    #[account(
        init,
        payer=payer,
        space=Brawl::LEN,
        seeds=[b"brawl".as_ref(), payer.key().as_ref()],
        bump
    )]
    pub brawl: Account<'info, Brawl>,
    #[account(signer, mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> StartBrawl<'info> {
    pub fn handler(ctx: Context<StartBrawl>) -> Result<()> {
        ctx.accounts.brawl.bump = ctx.bumps.brawl;

        Ok(())
    }
}
