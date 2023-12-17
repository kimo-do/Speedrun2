use anchor_lang::prelude::*;

use crate::MAX_MATCHES;

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Default, PartialEq)]
pub struct Match {
    /// The index of the first brawler.
    pub brawler0: u8,
    /// The index of the second brawler.
    pub brawler1: u8,
    /// The winner of the match.
    pub winner: u8,
}

impl Match {
    /// 1 byte winner + 2 * 1 byte indexes.
    pub const LEN: usize = 1 // The first brawler index
        + 1 // The second brawler index
        + 1; // The winner index
}

#[account]
pub struct Brawl {
    /// The PDA bump
    pub bump: u8,
    /// The queue of Brawler Pubkeys.
    pub queue: Vec<Pubkey>,
    /// The winner
    pub winner: Pubkey,
    /// The match up list
    pub matches: Vec<Match>,
}

impl Brawl {
    pub const LEN: usize = 8 // The 8 byte discriminator
    + 1 // The 1 byte bump
    + 4 // The 4 byte length of the queue vec
    + (32 * 8) // The 32 byte length of each Pubkey in the queue
    + 32 // The 32 byte length of the winner
    + 4 // The 4 byte length of the matches vec
    + Match::LEN * MAX_MATCHES; // The number of matchups
}
