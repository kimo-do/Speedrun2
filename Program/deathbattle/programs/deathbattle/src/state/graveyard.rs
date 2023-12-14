use anchor_lang::prelude::*;

#[account]
pub struct Graveyard {
    /// The PDA bump.
    pub bump: u8,
    /// A lists of the brawlers in the clone lab.
    pub brawlers: Vec<Pubkey>,
}

impl Graveyard {
    pub const PREFIX: &str = "graveyard";
    pub const INIT_LEN: usize = 8 // The 8 byte discriminator
    + 1 // The 1 byte bump
    + 4; // The 4 byte length of the brawlers vec

    #[allow(clippy::len_without_is_empty)]
    pub fn len(&self) -> usize {
        Self::INIT_LEN + (32 * self.brawlers.len())
    }
}
