-- mysql add approved_by (foreign key to known_users' user_id) and approved_at (datetime) to hints

ALTER TABLE hints
  ADD COLUMN approved_by BIGINT(20) NULL,
  ADD COLUMN approved_at DATETIME NULL,
  ADD CONSTRAINT hints_approved_by FOREIGN KEY (approved_by) REFERENCES known_users(user_id);

-- and then a trigger to automatically set approved_at to the current time when approved_by is set
DELIMITER $$
CREATE TRIGGER hints_approved_at BEFORE UPDATE ON hints
FOR EACH ROW
BEGIN
    IF (NEW.approved_by IS NOT NULL AND OLD.approved_by IS NULL) THEN
        SET NEW.approved_at = CURRENT_TIMESTAMP;
    END IF;
END;
$$
DELIMITER ;
