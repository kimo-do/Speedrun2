use anchor_lang::prelude::*;

#[account]
pub struct CloneLab {
    /// The PDA bump.
    pub bump: u8,
    /// The number of brawlers in the Clone Lab.
    pub num_brawlers: u16,
    /// A lists of the brawlers in the clone lab.
    pub brawlers: Vec<Pubkey>,
}

impl CloneLab {
    pub const PREFIX: &str = "clone_lab";
    pub const INIT_LEN: usize = 8 // The 8 byte discriminator
    + 1 // The 1 byte bump
    + 2 // The 2 byte brawler counter
    + 4; // The 4 byte length of the brawlers vec

    #[allow(clippy::len_without_is_empty)]
    pub fn len(&self) -> usize {
        Self::INIT_LEN + (32 * self.brawlers.len())
    }
}
