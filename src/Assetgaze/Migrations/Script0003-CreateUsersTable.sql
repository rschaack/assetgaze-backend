
CREATE TABLE "Users" (
                         "Id" UUID PRIMARY KEY,
                         "Email" VARCHAR(255) NOT NULL,
                         "PasswordHash" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");