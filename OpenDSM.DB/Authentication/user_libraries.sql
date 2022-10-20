CREATE TABLE user_libraries
(
    product_id INT NOT NULL,
    user_id INT NOT NULL,
    purchase_price FLOAT NOT NULL,
    purchase_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_used DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    use_time BIGINT NOT NULL DEFAULT 0
);