CREATE TABLE user_banking
(
    account_balance FLOAT NOT NULL DEFAULT 0,
    payout_period BIT NOT NULL DEFAULT 0,
    payout_routing VARCHAR(64) NULL,
    payout_account VARCHAR(64) NULL,
    account_card VARCHAR(64) NULL,
    account_cvv VARCHAR(64) NULL,
    account_name VARCHAR(255) NULL,
);