use anchor_lang::prelude::*;
use strum_macros::EnumIter;

use crate::MAX_NAME_LENGTH;

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Debug, EnumIter)]
pub enum CharacterType {
    Default,
    Male1,
    Female1,
    Bonki,
    SolBlaze,
    Male2,
    Female2,
    Cop,
    Gangster,
}

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Debug, EnumIter)]
pub enum BrawlerType {
    Saber,
    Pistol,
    Hack,
    Katana,
    Virus,
    Laser,
}

#[account]
pub struct Brawler {
    /// The PDA bump.
    pub bump: u8,
    /// The owner of the clone.
    pub owner: Pubkey,
    /// The character type of the clone.
    pub character_type: CharacterType,
    /// The brawler type of the clone.
    pub brawler_type: BrawlerType,
    /// The name of the clone.
    pub name: String,
}

impl Brawler {
    /// The length of the Brawler account.
    pub const LEN: usize = 8 + // 8 byte discriminator
        1 + // 1 byte bump
        32 + // 32 byte owner
        1 + // 1 byte character type
        1 + // 1 byte brawler type
        4 + // 4 byte length of the name
        MAX_NAME_LENGTH; // The max length of the name
}
