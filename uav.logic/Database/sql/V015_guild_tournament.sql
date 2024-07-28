-- Guild tournament table
-- This table maps the user to whether they are a temporary or permanent member of a guild.

CREATE TABLE IF NOT EXISTS uav.guild_members (
    user_id BIGINT PRIMARY KEY,
    is_temporary INTEGER NOT NULL DEFAULT 0,
    added_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES known_users (user_id)
);