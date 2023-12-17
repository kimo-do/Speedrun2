pub mod constants;
pub mod error;
pub mod instructions;
pub mod state;
pub mod utils;

use anchor_lang::prelude::*;

use crate::instructions::*;
pub use constants::*;
pub use state::*;
pub use utils::*;

declare_id!("BRAWLHsgvJBQGx4EzNuqKpbbv8q3LhcYbL1bHqbgVtaJ");

#[program]
pub mod deathbattle {

    use super::*;

    pub fn create_profile(ctx: Context<CreateProfile>, args: CreateProfileArgs) -> Result<()> {
        CreateProfile::handler(ctx, args)
    }

    pub fn create_clone_lab(ctx: Context<CreateCloneLab>) -> Result<()> {
        CreateCloneLab::handler(ctx)
    }

    pub fn create_colosseum(ctx: Context<CreateColosseum>) -> Result<()> {
        CreateColosseum::handler(ctx)
    }

    pub fn create_graveyard(ctx: Context<CreateGraveyard>) -> Result<()> {
        CreateGraveyard::handler(ctx)
    }

    pub fn create_clone(ctx: Context<CreateClone>) -> Result<()> {
        CreateClone::handler(ctx)
    }

    pub fn revive_clone(ctx: Context<ReviveClone>) -> Result<()> {
        ReviveClone::handler(ctx)
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

    pub fn close_account(ctx: Context<CloseAccount>) -> Result<()> {
        CloseAccount::handler(ctx)
    }

    pub fn clear_ended_brawl(ctx: Context<ClearEndedBrawl>) -> Result<()> {
        ClearEndedBrawl::handler(ctx)
    }
}
