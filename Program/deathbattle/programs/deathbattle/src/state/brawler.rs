use anchor_lang::prelude::*;

#[account]
pub struct Brawler {
    /// The PDA bump.
    pub bump: u8,
    /// The owner of the clone.
    pub owner: Pubkey,
}

impl Brawler {
    /// The length of the Brawler account.
    pub const LEN: usize = 8 + // 8 byte discriminator
        1 + // 1 byte bump
        32; // 32 byte owner
}
