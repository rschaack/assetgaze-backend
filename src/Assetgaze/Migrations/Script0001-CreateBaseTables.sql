-- Users Table
CREATE TABLE "Users" (
                         "Id" UUID PRIMARY KEY,
                         "Email" VARCHAR(255) NOT NULL,
                         "PasswordHash" TEXT NOT NULL,
                         "CreatedDate" TIMESTAMPTZ NOT NULL,
                         "LastLoginDate" TIMESTAMPTZ,
                         "LastPasswordChangeDate" TIMESTAMPTZ,
                         "FailedLoginAttempts" INT NOT NULL DEFAULT 0,
                         "LoginCount" INT NOT NULL DEFAULT 0,
                         "LockoutEndDateUtc" TIMESTAMPTZ -- Nullable by default
);
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

-- Brokers Table
CREATE TABLE "Brokers" (
                           "Id" UUID PRIMARY KEY,
                           "Name" VARCHAR(255) NOT NULL
);

-- Accounts Table
CREATE TABLE "Accounts" (
                            "Id" UUID PRIMARY KEY,
                            "Name" VARCHAR(255) NOT NULL
);