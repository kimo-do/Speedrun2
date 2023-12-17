use anchor_lang::prelude::*;

use crate::Graveyard;

#[derive(Accounts)]
pub struct CreateGraveyard<'info> {
    /// The Graveyard account. This is keyed to the creator.
    #[account(
        init,
        payer=payer,
        space=Graveyard::INIT_LEN,
        seeds=[Graveyard::PREFIX.as_ref(), payer.key().as_ref()],
        bump
    )]
    pub graveyard: Account<'info, Graveyard>,

    /// The creator of the Graveyard. In most cases this will be the game's keypair managing the public lobby.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> CreateGraveyard<'info> {
    pub fn handler(ctx: Context<CreateGraveyard>) -> Result<()> {
        ctx.accounts.graveyard.bump = ctx.bumps.graveyard;

        Ok(())
    }
}
