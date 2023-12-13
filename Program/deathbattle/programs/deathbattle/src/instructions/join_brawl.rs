use anchor_lang::prelude::*;
use anchor_spl::token::{Mint, TokenAccount};

use crate::{error::BrawlError, Brawl, Brawler, MAX_BRAWLERS};

#[derive(Accounts)]
pub struct JoinBrawl<'info> {
    #[account(mut)]
    pub brawl: Account<'info, Brawl>,
    #[account(
        init_if_needed,
        payer=payer,
        space=Brawl::LEN,
        seeds=[b"brawler".as_ref(), mint.key().as_ref(), payer.key().as_ref()],
        bump
    )]
    pub brawler: Account<'info, Brawler>,
    pub mint: Account<'info, Mint>,
    #[account(
        token::mint = mint,
        token::authority = payer,
    )]
    pub token_account: Account<'info, TokenAccount>,
    #[account(signer, mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> JoinBrawl<'info> {
    pub fn handler(ctx: Context<JoinBrawl>) -> Result<()> {
        ctx.accounts.brawler.bump = ctx.bumps.brawler;

        if ctx.accounts.brawl.queue.len() < (MAX_BRAWLERS - 1) {
            ctx.accounts.brawl.queue.push(ctx.accounts.brawler.key());
        } else {
            err!(BrawlError::BrawlFull)?;
        }

        ctx.accounts.brawler.mint = ctx.accounts.mint.key();
        ctx.accounts.brawler.owner = ctx.accounts.payer.key();

        Ok(())
    }
}
