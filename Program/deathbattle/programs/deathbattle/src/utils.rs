use anchor_lang::prelude::*;
use arrayref::array_ref;

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
