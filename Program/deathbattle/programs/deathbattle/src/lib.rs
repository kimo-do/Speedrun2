pub mod constants;
pub mod error;
pub mod instructions;
pub mod state;

use anchor_lang::prelude::*;

use crate::instructions::*;
pub use constants::*;
pub use state::*;

declare_id!("BRAWLHsgvJBQGx4EzNuqKpbbv8q3LhcYbL1bHqbgVtaJ");

#[program]
pub mod deathbattle {

    use super::*;

    pub fn create_clone_lab(ctx: Context<CreateCloneLab>) -> Result<()> {
        CreateCloneLab::handler(ctx)
    }

    pub fn create_clone(ctx: Context<CreateClone>, args: CreateCloneArgs) -> Result<()> {
        CreateClone::handler(ctx, args)
    }

    pub fn start_brawl(ctx: Context<StartBrawl>) -> Result<()> {
        StartBrawl::handler(ctx)
    }

    pub fn join_brawl(ctx: Context<JoinBrawl>, args: JoinBrawlArgs) -> Result<()> {
        JoinBrawl::handler(ctx, args)
    }

    pub fn run_match(ctx: Context<RunMatch>) -> Result<()> {
        RunMatch::handler(ctx)
    }
}
