use anchor_lang::prelude::*;

#[repr(C)]
#[derive(AnchorSerialize, AnchorDeserialize, Clone, Default)]
pub enum BrawlerState {
    #[default]
    Alive,
    Battling,
    Dead,
}

#[account]
pub struct Brawler {
    /// The PDA bump.
    pub bump: u8,
    /// The Mint of the NFT.
    pub mint: Pubkey,
    /// The owner of the NFT.
    pub owner: Pubkey,
    /// The current state of the Brawler.
    pub state: BrawlerState,
}

impl Brawler {
    /// The length of the Brawler account.
    pub const LEN: usize = 8 + // 8 byte discriminator
        32 + // 32 byte mint
        32 + // 32 byte owner
        1 + // 1 byte bump
        1; // 1 byte state
}
