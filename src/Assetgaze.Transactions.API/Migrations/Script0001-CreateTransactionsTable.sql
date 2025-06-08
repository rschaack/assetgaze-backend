CREATE TABLE "Transactions" (
                                "Id" UUID PRIMARY KEY,
                                "Ticker" VARCHAR(10) NOT NULL,
                                "Quantity" INT NOT NULL,
                                "Price" DECIMAL(18, 4) NOT NULL,
                                "TransactionType" VARCHAR(10) NOT NULL,
                                "TransactionDate" TIMESTAMPTZ NOT NULL
);