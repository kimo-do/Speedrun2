use anchor_lang::prelude::*;

use crate::{error::BrawlError, Brawl};

#[derive(Accounts)]
pub struct RunMatch<'info> {
    #[account(mut)]
    pub brawl: Account<'info, Brawl>,
    #[account(signer, mut)]
    pub payer: Signer<'info>,
}

impl<'info> RunMatch<'info> {
    pub fn handler(ctx: Context<RunMatch>) -> Result<()> {
        let queue = ctx.accounts.brawl.queue.clone();
        for brawler in queue.iter() {
            for account in ctx.remaining_accounts.iter() {
                if brawler == account.key {
                    ctx.accounts.brawl.queue.pop();
                }
            }
        }

        if !queue.is_empty() {
            err!(BrawlError::MissingBrawlerAccounts)?;
        }

        Ok(())
    }
}
