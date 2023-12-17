use anchor_lang::prelude::*;
use arrayref::array_ref;
use solana_program::{program::invoke, system_instruction};

use crate::error::BrawlError;

pub fn rand_choice<T: Clone>(choices: &Vec<T>, slot_hashes: &AccountInfo) -> Result<T> {
    let data = slot_hashes.data.borrow();
    let most_recent = array_ref![data, 12, 8];

    let clock = Clock::get()?;
    // seed for the random number is a combination of the slot_hash - timestamp
    let seed = usize::from_le_bytes(*most_recent).saturating_sub(clock.unix_timestamp as usize);

    let remainder: usize = seed
        .checked_rem(choices.len())
        .ok_or(BrawlError::NumericalOverflowError)?;

    Ok(choices[remainder].clone())
}

/// Resize an account using realloc, lifted from Solana Cookbook
pub fn resize_or_reallocate_account_raw<'a>(
    target_account: &AccountInfo<'a>,
    funding_account: &AccountInfo<'a>,
    system_program: &AccountInfo<'a>,
    new_size: usize,
) -> Result<()> {
    let rent = Rent::get()?;
    let new_minimum_balance = rent.minimum_balance(new_size);

    let lamports_diff = new_minimum_balance.saturating_sub(target_account.lamports());
    invoke(
        &system_instruction::transfer(funding_account.key, target_account.key, lamports_diff),
        &[
            funding_account.clone(),
            target_account.clone(),
            system_program.clone(),
        ],
    )?;

    target_account.realloc(new_size, false)?;

    Ok(())
}
