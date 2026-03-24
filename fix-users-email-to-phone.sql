-- Run this in Supabase SQL Editor to rename Users.Email to PhoneNumber
-- (Only runs if Email column exists - safe to run multiple times)

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'Users' AND column_name = 'Email'
    ) THEN
        ALTER TABLE "Users" RENAME COLUMN "Email" TO "PhoneNumber";
        RAISE NOTICE 'Renamed Users.Email to PhoneNumber';
    ELSE
        RAISE NOTICE 'Users already has PhoneNumber (or table/column not found)';
    END IF;
END $$;
