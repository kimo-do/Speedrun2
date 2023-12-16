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

    /// 1003 - Name too long.
    #[msg("Name too long.")]
    NameTooLong,

    /// 1003 - Invalid Brawl.
    #[msg("Invalid Brawl.")]
    InvalidBrawl,

    /// 1004 - Invalid Owner.
    #[msg("Invalid Owner of the Brawler.")]
    InvalidOwner,

    /// 1005 - Numerical overflow error.
    #[msg("Numerical overflow error.")]
    NumericalOverflowError,

    #[msg("Winner not determined")]
    WinnerNotDetermined
}
