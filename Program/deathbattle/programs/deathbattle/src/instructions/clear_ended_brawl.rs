use anchor_lang::prelude::*;

use crate::{error::BrawlError, Brawl, CloneLab, Colosseum, AUTH_PUBKEY};

#[derive(Accounts)]
pub struct ClearEndedBrawl<'info> {
    #[account(mut)]
    pub clone_lab: Account<'info, CloneLab>,

    #[account(
        mut,
        realloc=colosseum.len() - 32,
        realloc::payer=authority,
        realloc::zero=false
    )]
    pub colosseum: Account<'info, Colosseum>,

    /// The brawl to remove
    pub brawl: Account<'info, Brawl>,

    ///CHECK: The winner of the brawl, checked in the instruction.
    #[account(mut)]
    pub winner: UncheckedAccount<'info>,

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
            ctx.accounts.colosseum.ended_brawls.swap_remove(index);
        } else {
            return err!(BrawlError::InvalidBrawl);
        }

        if ctx.accounts.winner.key() != ctx.accounts.brawl.winner {
            return err!(BrawlError::InvalidWinner);
        }

        Ok(())
    }
}

pub fn pay_winnings<'a>(
    dest_account_info: &AccountInfo<'a>,
    src_account_info: &AccountInfo<'a>,
) -> Result<()> {
    let src_starting_lamports = src_account_info.lamports();
    let dest_starting_lamports = dest_account_info.lamports();
    **dest_account_info.lamports.borrow_mut() =
        dest_starting_lamports.checked_add(200_000_000).unwrap();
    **src_account_info.lamports.borrow_mut() =
        src_starting_lamports.checked_sub(200_000_000).unwrap();

    Ok(())
}
