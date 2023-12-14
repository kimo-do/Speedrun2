use anchor_lang::prelude::*;

use crate::MAX_NAME_LENGTH;

#[account]
pub struct Profile {
    /// The PDA bump.
    pub bump: u8,
    /// The name of the clone.
    pub username: String,
}

impl Profile {
    /// The string prefix of the profile
    pub const PREFIX: &'static str = "profile";
    /// The length of the Profile account.
    pub const LEN: usize = 8 + // 8 byte discriminator
        1 + // 1 byte bump
        32 + // 32 byte owner
        4 + // 4 byte length of the name
        MAX_NAME_LENGTH; // The max length of the name
}
