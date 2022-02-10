CREATE TABLE history (
    user_id BIGINT,
    activity_time DATETIME DEFAULT now(),
    command TEXT,
    response TEXT,
    PRIMARY KEY (user_id, activity_time),
    FOREIGN KEY(user_id) REFERENCES known_users(user_id)
);
