use anchor_lang::prelude::*;

use crate::{error::BrawlError, Profile, MAX_NAME_LENGTH};

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Default, PartialEq)]
pub struct CreateProfileArgs {
    /// The name of the profile.
    pub username: String,
}

#[derive(Accounts)]
pub struct CreateProfile<'info> {
    /// The Profile account. This is keyed to the creator.
    #[account(
        init,
        payer=payer,
        space=Profile::LEN,
        seeds=[Profile::PREFIX.as_ref(), payer.key().as_ref()],
        bump
    )]
    pub profile: Account<'info, Profile>,

    /// The user creating the Profile.
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

impl<'info> CreateProfile<'info> {
    pub fn handler(ctx: Context<CreateProfile>, args: CreateProfileArgs) -> Result<()> {
        ctx.accounts.profile.bump = ctx.bumps.profile;

        if ctx.accounts.profile.username.len() > MAX_NAME_LENGTH {
            return err!(BrawlError::NameTooLong);
        } else {
            ctx.accounts.profile.username = args.username;
        }

        Ok(())
    }
}
