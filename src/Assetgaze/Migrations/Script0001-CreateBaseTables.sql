-- Users Table
CREATE TABLE "Users" (
                         "Id" UUID PRIMARY KEY,
                         "Email" VARCHAR(255) NOT NULL,
                         "PasswordHash" TEXT NOT NULL
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