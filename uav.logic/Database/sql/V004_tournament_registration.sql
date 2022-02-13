CREATE TABLE known_users (
    user_id BIGINT PRIMARY KEY,
    user_name TEXT CHARSET utf8mb4,
    user_nick TEXT CHARSET utf8mb4
);

CREATE TABLE tournament (
    tournament_id INT AUTO_INCREMENT PRIMARY KEY,
    start_time DATETIME
);

CREATE TABLE tournament_registration (
    tournament_id INT,
    user_id BIGINT,
    joined DATETIME,
    removed DATETIME,
    requested_by BIGINT,
    PRIMARY KEY(tournament_id, user_id),
    FOREIGN KEY(tournament_id) REFERENCES tournament(tournament_id),
    FOREIGN KEY(user_id) REFERENCES known_users(user_id),
    FOREIGN KEY(requested_by) REFERENCES known_users(user_id)
);