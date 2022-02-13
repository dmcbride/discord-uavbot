CREATE TABLE history (
    user_id BIGINT,
    activity_time DATETIME DEFAULT now(),
    command TEXT CHARSET utf8mb4,
    response TEXT CHARSET utf8mb4,
    PRIMARY KEY (user_id, activity_time),
    FOREIGN KEY(user_id) REFERENCES known_users(user_id)
);

ALTER TABLE history ADD COLUMN options TEXT CHARSET utf8mb4 AFTER command;

CREATE OR REPLACE VIEW hist AS SELECT
    h.activity_time,
    COALESCE(u.user_nick, u.user_name) AS nickname,
    h.command,
    h.options,
    REPLACE(REPLACE(h.response, '\n\n', ' ||| '), '\n', ' || ') AS response
FROM
    history h
LEFT JOIN
    known_users u ON h.user_id = u.user_id
;