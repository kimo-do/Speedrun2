pub mod constants;
pub mod error;
pub mod instructions;
pub mod state;

use anchor_lang::prelude::*;

use crate::instructions::*;
pub use constants::*;
pub use state::*;

declare_id!("HyThTjyyi6DdCxbF7KeegYsnLogSR2CKfkzVVeJRaGk6");

#[program]
pub mod deathbattle {

    use super::*;

    pub fn start_brawl(ctx: Context<StartBrawl>) -> Result<()> {
        StartBrawl::handler(ctx)
    }

    pub fn join_brawl(ctx: Context<JoinBrawl>) -> Result<()> {
        JoinBrawl::handler(ctx)
    }

    pub fn run_match(ctx: Context<RunMatch>) -> Result<()> {
        RunMatch::handler(ctx)
    }
}
