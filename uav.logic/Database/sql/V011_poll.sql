-- tables for polls

-- Table: poll
CREATE TABLE polls (
    poll_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    guild_id BIGINT NOT NULL,
    channel_id BIGINT NOT NULL,
    msg_id BIGINT NOT NULL,
    poll_user_key VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    options JSON NOT NULL,
    max_options INT NOT NULL,
    created_date TIMESTAMP NOT NULL DEFAULT UTC_TIMESTAMP(),
    end_date TIMESTAMP NOT NULL,
    completed BOOLEAN NOT NULL DEFAULT FALSE,
);

-- Table: poll_vote
CREATE TABLE poll_votes (
    poll_vote_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    poll_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    vote INT NOT NULL,
    vote_time TIMESTAMP NOT NULL DEFAULT UTC_TIMESTAMP(),
    CONSTRAINT poll_vote_poll_id_fkey FOREIGN KEY (poll_id)
        REFERENCES polls (poll_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT poll_vote_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES known_users (user_id)
        ON UPDATE CASCADE ON DELETE CASCADE
);
