CREATE TABLE auth_clients
(
    user_id INT NOT NULL,
    client_name VARCHAR(255) NOT NULL,
    client_address VARCHAR(255) NOT NULL,
    last_connected DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);