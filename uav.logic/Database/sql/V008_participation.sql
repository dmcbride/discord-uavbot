CREATE TABLE participation_history (
    user_id BIGINT NOT NULL,
    yyyymm INT NOT NULL,
    count INT DEFAULT 1 NOT NULL,
    PRIMARY KEY (user_id, yyyymm),
    FOREIGN KEY(user_id) REFERENCES known_users(user_id)
);

CREATE INDEX participation_month ON participation_history (yyyymm);

ALTER TABLE known_users ADD COLUMN is_mod BOOLEAN DEFAULT FALSE;