-- Run this in Supabase SQL Editor to add Roles/Claims and link Users

-- 1. Create Roles table
CREATE TABLE IF NOT EXISTS "Roles" (
  "Id" serial PRIMARY KEY,
  "Name" text NOT NULL
);

-- 2. Create Claims table
CREATE TABLE IF NOT EXISTS "Claims" (
  "Id" serial PRIMARY KEY,
  "Name" text NOT NULL
);

-- 3. Create RoleClaims (many-to-many)
CREATE TABLE IF NOT EXISTS "RoleClaims" (
  "RoleId" int NOT NULL REFERENCES "Roles"("Id") ON DELETE CASCADE,
  "ClaimId" int NOT NULL REFERENCES "Claims"("Id") ON DELETE CASCADE,
  PRIMARY KEY ("RoleId", "ClaimId")
);

-- 4. Add RoleId to Users (if not exists)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'RoleId') THEN
    ALTER TABLE "Users" ADD COLUMN "RoleId" int;
  END IF;
END $$;

-- 5. Seed Roles (skip if already has data)
INSERT INTO "Roles" ("Id", "Name") 
SELECT 1, 'Admin' WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Id" = 1);
INSERT INTO "Roles" ("Id", "Name") 
SELECT 2, 'General Manager' WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Id" = 2);
INSERT INTO "Roles" ("Id", "Name") 
SELECT 3, 'Social Media Manager' WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE "Id" = 3);

-- 6. Seed Claims
INSERT INTO "Claims" ("Id", "Name") SELECT 1, 'ManageUsers' WHERE NOT EXISTS (SELECT 1 FROM "Claims" WHERE "Id" = 1);
INSERT INTO "Claims" ("Id", "Name") SELECT 2, 'ManageCustomers' WHERE NOT EXISTS (SELECT 1 FROM "Claims" WHERE "Id" = 2);
INSERT INTO "Claims" ("Id", "Name") SELECT 3, 'ManageCars' WHERE NOT EXISTS (SELECT 1 FROM "Claims" WHERE "Id" = 3);
INSERT INTO "Claims" ("Id", "Name") SELECT 4, 'ManagePosts' WHERE NOT EXISTS (SELECT 1 FROM "Claims" WHERE "Id" = 4);
INSERT INTO "Claims" ("Id", "Name") SELECT 5, 'ViewDashboard' WHERE NOT EXISTS (SELECT 1 FROM "Claims" WHERE "Id" = 5);

-- 7. Seed RoleClaims
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 1,1 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=1 AND "ClaimId"=1);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 1,2 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=1 AND "ClaimId"=2);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 1,3 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=1 AND "ClaimId"=3);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 1,4 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=1 AND "ClaimId"=4);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 1,5 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=1 AND "ClaimId"=5);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 2,2 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=2 AND "ClaimId"=2);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 2,3 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=2 AND "ClaimId"=3);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 2,4 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=2 AND "ClaimId"=4);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 2,5 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=2 AND "ClaimId"=5);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 3,4 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=3 AND "ClaimId"=4);
INSERT INTO "RoleClaims" ("RoleId", "ClaimId") SELECT 3,5 WHERE NOT EXISTS (SELECT 1 FROM "RoleClaims" WHERE "RoleId"=3 AND "ClaimId"=5);

-- 8. Migrate Users: if Role column exists, use it; else set RoleId=1
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Role') THEN
    UPDATE "Users" SET "RoleId" = 1 WHERE "Role" = 'Admin';
    UPDATE "Users" SET "RoleId" = 2 WHERE "Role" = 'General Manager';
    UPDATE "Users" SET "RoleId" = 3 WHERE "Role" = 'Social Media Manager';
    UPDATE "Users" SET "RoleId" = 1 WHERE "RoleId" IS NULL;
    ALTER TABLE "Users" DROP COLUMN "Role";
  END IF;
  UPDATE "Users" SET "RoleId" = 1 WHERE "RoleId" IS NULL;
END $$;

-- 9. Make RoleId required and add FK
ALTER TABLE "Users" ALTER COLUMN "RoleId" SET NOT NULL;
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Users_Roles') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Roles" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id");
  END IF;
END $$;

-- 10. Tell EF the migration ran (so Update-Database won't try again)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
SELECT '20260314150000_AddRolesAndClaims', '8.0.24'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260314150000_AddRolesAndClaims');
