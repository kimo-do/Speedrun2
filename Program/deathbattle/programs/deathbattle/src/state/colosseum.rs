use anchor_lang::prelude::*;

#[account]
pub struct Colosseum {
    /// The PDA bump.
    pub bump: u8,
    /// The counter of total brawls.
    pub num_brawls: u32,
    /// A list of the brawls filling up.
    pub pending_brawls: Vec<Pubkey>,
    /// A list of the brawls ready to go.
    pub active_brawls: Vec<Pubkey>,
    /// A list of the ended brawls.
    pub ended_brawls: Vec<Pubkey>,
}

impl Colosseum {
    pub const PREFIX: &str = "colosseum";
    pub const INIT_LEN: usize = 8 // The 8 byte discriminator
    + 1 // The 1 byte bump
    + 4 // The 4 byte brawl counter
    + 4 // The 4 byte length of the pending brawls vec
    + 4 // The 4 byte length of the active brawls vec
    + 4; // The 4 byte length of the ended brawls vec

    #[allow(clippy::len_without_is_empty)]
    pub fn len(&self) -> usize {
        Self::INIT_LEN
            + (32 * self.pending_brawls.len())
            + (32 * self.active_brawls.len())
            + (32 * self.ended_brawls.len())
    }
}
