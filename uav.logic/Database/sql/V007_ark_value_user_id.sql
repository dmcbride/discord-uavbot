ALTER TABLE ark_value
ADD COLUMN user_id BIGINT;

ALTER TABLE ark_value
ADD CONSTRAINT FOREIGN KEY(user_id) REFERENCES known_users(user_id);