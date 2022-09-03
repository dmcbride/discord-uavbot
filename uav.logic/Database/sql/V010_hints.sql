CREATE TABLE hints (
    user_id BIGINT,
    hint_name CHAR(255),
    title TEXT,
    hint_text TEXT,
    created DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, hint_name),
    FOREIGN KEY (user_id) REFERENCES known_users (user_id),
    index (user_id)
);
