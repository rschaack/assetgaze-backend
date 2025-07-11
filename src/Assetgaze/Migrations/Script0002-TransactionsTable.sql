CREATE TABLE "Transactions" (
                                "Id" UUID PRIMARY KEY,
                                "TransactionType" VARCHAR(50) NOT NULL,
                                "BrokerDealReference" VARCHAR(255),
                                "BrokerId" UUID NOT NULL,
                                "AccountId" UUID NOT NULL,
                                "TaxWrapper" VARCHAR(50) NOT NULL,
                                "ISIN" VARCHAR(12) NOT NULL, -- Storing the asset identifier directly
                                "TransactionDate" TIMESTAMPTZ NOT NULL,
                                "Quantity" DECIMAL(28, 10),
                                "NativePrice" DECIMAL(28, 10),
                                "LocalPrice" DECIMAL(28, 10),
                                "Consideration" DECIMAL(28, 10) NOT NUll,
                                "BrokerCharge" DECIMAL(28, 10),
                                "StampDuty" DECIMAL(28, 10),
                                "FxCharge" DECIMAL(28, 10),
                                "AccruedInterest" DECIMAL(28, 10),

                                CONSTRAINT "FK_Transactions_Brokers" FOREIGN KEY ("BrokerId") REFERENCES "Brokers"("Id"),
                                CONSTRAINT "FK_Transactions_Accounts" FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id")
);