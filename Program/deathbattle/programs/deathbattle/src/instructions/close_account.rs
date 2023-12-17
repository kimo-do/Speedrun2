use anchor_lang::prelude::*;

use crate::AUTH_PUBKEY;

#[derive(Accounts)]
pub struct CloseAccount<'info> {
    /// CHECK: YOLO!
    #[account(mut)]
    pub account: UncheckedAccount<'info>,

    #[account(mut, address = AUTH_PUBKEY)]
    pub payer: Signer<'info>,
}

impl<'info> CloseAccount<'info> {
    pub fn handler(ctx: Context<CloseAccount>) -> Result<()> {
        close_account_raw(&ctx.accounts.payer, &ctx.accounts.account)?;
        Ok(())
    }
}

/// Close src_account and transfer lamports to dst_account, lifted from Solana Cookbook
pub fn close_account_raw<'a>(
    dest_account_info: &AccountInfo<'a>,
    src_account_info: &AccountInfo<'a>,
) -> Result<()> {
    let dest_starting_lamports = dest_account_info.lamports();
    **dest_account_info.lamports.borrow_mut() = dest_starting_lamports
        .checked_add(src_account_info.lamports())
        .unwrap();
    **src_account_info.lamports.borrow_mut() = 0;

    let mut src_data = src_account_info.data.borrow_mut();
    src_data.fill(0);

    Ok(())
}
