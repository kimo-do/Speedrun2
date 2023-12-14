use anchor_lang::prelude::*;

use crate::CloneLab;

#[derive(Accounts)]
pub struct CreateCloneLab<'info> {
    /// The Clone Lab account. This is keyed to the creator.
    #[account(
        init,
        payer=payer,
        space=CloneLab::INIT_LEN,
        seeds=[CloneLab::PREFIX.as_ref(), payer.key().as_ref()],
        bump
    )]
    pub clone_lab: Account<'info, CloneLab>,

    /// The creator of the Clone Lab. In most cases this will be the game's keypair managing the public lobby.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> CreateCloneLab<'info> {
    pub fn handler(ctx: Context<CreateCloneLab>) -> Result<()> {
        ctx.accounts.clone_lab.bump = ctx.bumps.clone_lab;

        Ok(())
    }
}
