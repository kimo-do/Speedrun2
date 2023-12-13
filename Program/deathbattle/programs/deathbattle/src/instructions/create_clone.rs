use anchor_lang::prelude::*;

use crate::{Brawler, CloneLab};

#[derive(Accounts)]
pub struct CreateClone<'info> {
    /// The Clone Lab account. This will be used to store the clone.
    #[account(mut)]
    pub clone_lab: Account<'info, CloneLab>,

    /// The Clone account. This is the account that will be created.
    #[account(
        init,
        payer=payer,
        space=Brawler::LEN,
        seeds=[b"brawler".as_ref(), clone_lab.key().as_ref(), clone_lab.num_brawlers.to_le_bytes().as_ref()],
        bump
    )]
    pub brawler: Account<'info, Brawler>,

    /// The player who is creating the clone and adding it to the Clone Lab.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> CreateClone<'info> {
    pub fn handler(ctx: Context<CreateClone>) -> Result<()> {
        ctx.accounts.brawler.bump = ctx.bumps.brawler;
        ctx.accounts.clone_lab.num_brawlers += 1;
        ctx.accounts
            .clone_lab
            .brawlers
            .push(ctx.accounts.brawler.key());

        Ok(())
    }
}
