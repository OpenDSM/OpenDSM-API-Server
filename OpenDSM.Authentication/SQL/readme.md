# Users 👤
| Name           	| Type     	| Size 	| Plain Text |
|----------------	|----------	|------	| ---- |
| username    	| `varchar`  	| 32   	|✔️
| email       	| `varchar`  	| 320  	|✔️
| password    	| `varchar`  	| 255  	|❌
| about       	| `varchar`  	| 4000 	|❌
| last_online 	| `datetime` 	| N/A  	|✔️
| joined_date 	| `datetime` 	| N/A  	|✔️

# User Library 🔖
| Name           	| Type     	| Size 	| Plain Text |
|----------------	|----------	|------	| ---- |
| product_id     	| `int32`    	| N/A  	|✔️
| user_id        	| `int32`    	| N/A  	|✔️
| purchase_price 	| `float`    	| N/A  	|✔️
| purchase_date  	| `datetime` 	| N/A  	|✔️
| last_used      	| `datetime` 	| N/A  	|✔️
| use_time       	| `int64`    	| N/A  	|✔️

# User Authorized Clients 💾
| Name           	| Type     	| Size 	| Plain Text |
|----------------	|----------	|------	| ---- |
| user_id        	| `int32`    	| N/A  	|✔️
| client_name    	| `varchar`  	| 255  	|✔️
| client_address 	| `varchar`  	| 255  	|❌
| last_connected 	| `datetime` 	| N/A  	|✔️

# User API Keys 🔑
| Name           	| Type     	| Size 	| Plain Text |
|----------------	|----------	|------	| ---- |
| user_id        	| `int32`    	| N/A  	|✔️
| api_key        	| `varchar`  	| 255  	|✔️
| calls          	| `int64`    	| N/A  	|✔️

# User Banking 🏦
| Name           	| Type     	| Size 	| Plain Text |
|----------------	|----------	|------	| ---- |
| account_balance | `float` | N/A | ✔️
| payout_period | `byte` | N/A | ✔️
| payout_routing | `varchar` | 64 | ❌
| payout_account | `varchar` | 64 | ❌
| account_card | `varchar` | 64 | ❌
| account_cvv | `varchar` | 64 | ❌
| account_name | `varchar` | 255 | ❌

