mod brawl;
mod brawler;
mod clone_lab;

pub use brawl::*;
pub use brawler::*;
pub use clone_lab::*;

pub const MAX_BRAWLERS: usize = 8;
pub const MAX_MATCHES: usize = 4 + 2 + 1;
