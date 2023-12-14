mod brawl;
mod brawler;
mod clone_lab;
mod colosseum;
mod graveyard;
mod profile;

pub use brawl::*;
pub use brawler::*;
pub use clone_lab::*;
pub use colosseum::*;
pub use graveyard::*;
pub use profile::*;

pub const MAX_BRAWLERS: usize = 8;
pub const MAX_MATCHES: usize = 4 + 2 + 1;
pub const MAX_NAME_LENGTH: usize = 32;
