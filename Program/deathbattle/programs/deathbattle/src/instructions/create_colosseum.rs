use anchor_lang::prelude::*;

use crate::Colosseum;

#[derive(Accounts)]
pub struct CreateColosseum<'info> {
    /// The Colosseum account. This is keyed to the creator.
    #[account(
        init,
        payer=payer,
        space=Colosseum::INIT_LEN,
        seeds=[Colosseum::PREFIX.as_ref(), payer.key().as_ref()],
        bump
    )]
    pub colosseum: Account<'info, Colosseum>,

    /// The creator of the Colosseum. In most cases this will be the game's keypair managing the public lobby.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> CreateColosseum<'info> {
    pub fn handler(ctx: Context<CreateColosseum>) -> Result<()> {
        ctx.accounts.colosseum.bump = ctx.bumps.colosseum;

        Ok(())
    }
}
