-- alter known_users to include a datetime column for last_seen defaulting to the minimum time for mysql

ALTER TABLE known_users ADD COLUMN last_seen DATETIME NOT NULL DEFAULT '1000-01-01 00:00:00';

-- set a trigger to update last_seen on any update to known_users.

DELIMITER $$
CREATE TRIGGER known_users_update_last_seen BEFORE UPDATE ON known_users
FOR EACH ROW
BEGIN
  SET NEW.last_seen = UTC_TIMESTAMP();
END$$
DELIMITER ;
