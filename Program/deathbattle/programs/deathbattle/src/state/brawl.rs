use anchor_lang::prelude::*;

#[account]
pub struct Brawl {
    /// The PDA bump
    pub bump: u8,
    /// The queue of Brawler Pubkeys.
    pub queue: Vec<Pubkey>,
}

impl Brawl {
    /// 8 byte discriminator + 1 byte bump + 4 byte length + 8 * 32 byte pubkeys.
    pub const LEN: usize = 8 + 4 + (32 * 8);
}
