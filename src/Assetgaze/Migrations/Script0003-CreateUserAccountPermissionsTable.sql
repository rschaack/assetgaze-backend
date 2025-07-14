CREATE TABLE "UserAccountPermissions" (
                                          "UserId" UUID NOT NULL,
                                          "AccountId" UUID NOT NULL,
                                          PRIMARY KEY ("UserId", "AccountId"),
                                          CONSTRAINT "FK_UserAccountPermissions_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
                                          CONSTRAINT "FK_UserAccountPermissions_Accounts" FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id")
);