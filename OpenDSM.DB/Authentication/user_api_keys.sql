CREATE TABLE api_keys
(
    user_id INT NOT NULL,
    api_key VARCHAR(255) NOT NULL,
    calls BIGINT NOT NULL DEFAULT 0
);