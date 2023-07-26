CREATE TABLE user_config (
  user_id BIGINT(20) NOT NULL,
  lounge_level INT NOT NULL DEFAULT 0,
  has_exodus BOOLEAN NOT NULL DEFAULT false,
  credits_1 INT NOT NULL DEFAULT 0,
  credits_2 INT NOT NULL DEFAULT 0,
  credits_3 INT NOT NULL DEFAULT 0,
  credits_4 INT NOT NULL DEFAULT 0,
  credits_5 INT NOT NULL DEFAULT 0,
  credits_6 INT NOT NULL DEFAULT 0,
  credits_7 INT NOT NULL DEFAULT 0,
  credits_8 INT NOT NULL DEFAULT 0,
  FOREIGN KEY (user_id) REFERENCES known_users(user_id)
);