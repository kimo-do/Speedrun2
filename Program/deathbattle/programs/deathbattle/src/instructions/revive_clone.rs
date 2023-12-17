use anchor_lang::{
    prelude::*,
    solana_program::{program::invoke, system_instruction},
};

use crate::{resize_or_reallocate_account_raw, Brawler, CloneLab, Graveyard, Profile};

// #[derive(AnchorSerialize, AnchorDeserialize, Clone, Default, PartialEq)]
// pub struct ReviveCloneArgs {
//     /// The name of the clone.
//     pub name: String,
// }

#[derive(Accounts)]
pub struct ReviveClone<'info> {
    /// The Clone Lab account. This will be used to store the clone.
    #[account(
        mut,
        // realloc=clone_lab.len() + 32,
        // realloc::payer=payer,
        // realloc::zero=false
    )]
    pub clone_lab: Account<'info, CloneLab>,

    /// The Graveyard account. This will be where the clone is revived from.
    #[account(
        mut,
        // realloc=graveyard.len() - 32,
        // realloc::payer=payer,
        // realloc::zero=false
    )]
    pub graveyard: Account<'info, Graveyard>,

    /// The Clone account. This is the account that will be created.
    #[account(mut)]
    pub brawler: Account<'info, Brawler>,

    /// The profile of the owner of the new clone.
    #[account(
        seeds=[Profile::PREFIX.as_ref(), payer.key().as_ref()],
        bump=profile.bump
    )]
    pub profile: Account<'info, Profile>,

    /// The player who is creating the clone and adding it to the Clone Lab.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> ReviveClone<'info> {
    pub fn handler(ctx: Context<ReviveClone>) -> Result<()> {
        if let Some(index) = ctx
            .accounts
            .graveyard
            .brawlers
            .iter()
            .position(|value| *value == ctx.accounts.brawler.key())
        {
            ctx.accounts.graveyard.brawlers.swap_remove(index);
        }

        ctx.accounts
            .clone_lab
            .brawlers
            .push(ctx.accounts.brawler.key());

        ctx.accounts.brawler.owner = ctx.accounts.payer.key();
        ctx.accounts.brawler.name = ctx.accounts.profile.username.clone();

        // Transfer 0.05 SOL fee to the Clone Lab.
        let ix = system_instruction::transfer(
            &ctx.accounts.payer.key(),
            &ctx.accounts.clone_lab.key(),
            50_000_000,
        );

        invoke(
            &ix,
            &[
                ctx.accounts.payer.to_account_info(),
                ctx.accounts.clone_lab.to_account_info(),
                ctx.accounts.system_program.to_account_info(),
            ],
        )?;

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
            ctx.accounts.clone_lab.len() - 32,
        )?;

        Ok(())
    }
}
