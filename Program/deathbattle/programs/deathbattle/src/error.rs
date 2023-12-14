use anchor_lang::prelude::*;

#[error_code]
pub enum BrawlError {
    /// 1000 - The Brawl is full.
    #[msg("The Brawl is full.")]
    BrawlFull,

    /// 1001 - Missing Brawler accounts.
    #[msg("Missing Brawler accounts.")]
    MissingBrawlerAccounts,

    /// 1002 - Invalid Brawler.
    #[msg("Invalid Brawler.")]
    InvalidBrawler,

    /// 1003 - Brawler name too long.
    #[msg("Brawler name too long.")]
    BrawlerNameTooLong,

    /// 1003 - Invalid Brawl.
    #[msg("Invalid Brawl.")]
    InvalidBrawl,
}
