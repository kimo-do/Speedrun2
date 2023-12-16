use anchor_lang::prelude::*;

use crate::{error::BrawlError, rand_choice, Brawl};

#[derive(Accounts)]
pub struct RunMatch<'info> {
    #[account(mut)]
    pub brawl: Account<'info, Brawl>,
    #[account(signer, mut)]
    pub payer: Signer<'info>,

    pub slot_hashes: UncheckedAccount<'info>,
}

impl<'info> RunMatch<'info> {
    pub fn handler(ctx: Context<RunMatch>) -> Result<()> {
        let mut brawlers = vec![];
        let queue = ctx.accounts.brawl.queue.clone();
        for brawler in queue.iter() {
            for account in ctx.remaining_accounts.iter() {
                if brawler == account.key {
                    brawlers.push(ctx.accounts.brawl.queue.pop().unwrap());
                }
            }
        }

        if !queue.is_empty() {
            err!(BrawlError::MissingBrawlerAccounts)?;
        }

        match rand_choice(&brawlers, &ctx.accounts.slot_hashes.to_account_info()) {
            Ok(winner) => {
                ctx.accounts.brawl.winner = winner;
            }
            Err(e) => {
                err!(BrawlError::MissingBrawlerAccounts)?;
            }
        } 

        Ok(())
    }
}
