use anchor_lang::prelude::*;

pub const MAX_NAME_LENGTH: usize = 32;

#[account]
pub struct Brawler {
    /// The PDA bump.
    pub bump: u8,
    /// The owner of the clone.
    pub owner: Pubkey,
    /// The name of the clone.
    pub name: String,
}

impl Brawler {
    /// The length of the Brawler account.
    pub const LEN: usize = 8 + // 8 byte discriminator
        1 + // 1 byte bump
        32 + // 32 byte owner
        4 + // 4 byte length of the name
        MAX_NAME_LENGTH; // The max length of the name
}
