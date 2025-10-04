-- Add avatar_path column to users table
ALTER TABLE users ADD COLUMN avatar_path VARCHAR(255);

-- Create avatars directory in wwwroot/images if it doesn't exist
-- This will be handled by the application when the first avatar is uploaded
