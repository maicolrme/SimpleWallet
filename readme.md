# Wallet MVP

Simple digital wallet built with ASP.NET Core MVC.

**Version:** v2

## Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQLite
- Tailwind CSS CDN
- Swashbuckle (Swagger / OpenAPI)
- Docker

## Features

- User registration
- Authentication (cookie)
- Wallet
- Balance
- Transfers
- Transactions
- Profile
- Admin panel with side menu
- Roles and permissions
- Multi-language UI: English, Spanish, Italian
- REST API with bearer authentication
- Swagger / OpenAPI documentation

## Run

docker compose up -d

Application:

http://localhost:8080

Swagger:

http://localhost:8080/swagger

Stop with:

docker compose down

The SQLite database and the data-protection keys survive container
recreation through the `wallet_data` volume, so users stay signed in.

## Demo

Admin:

admin@example.com
Admin123!

User:

user@example.com
User123!

## Language

The language switcher lives in the top navigation (EN / ES / IT).
It writes a culture cookie (`AspNetCore.Culture`) and the request
localization middleware picks it up on the next request.

Supported cultures: `en-US`, `es-ES`, `it-IT`.

Strings are stored in `Localization/Texts.cs`, keyed by the English
text. Any string without a translation simply falls back to English.
Money always uses `.` for decimals and `,` for groups in every
language, so balances never break.

## API

All endpoints are JSON and live under `/api`.

| Method | Route                  | Auth    | Description |
|--------|------------------------|---------|-------------|
| POST   | `/api/token`           | none    | Exchange email + password for a bearer token |
| GET    | `/api/me`              | bearer  | Profile and wallet of the current user |
| GET    | `/api/wallet`          | bearer  | Balance, currency and address |
| POST   | `/api/wallet/deposit`  | bearer  | Add money to the wallet |
| POST   | `/api/wallet/withdraw` | bearer  | Remove money from the wallet |
| GET    | `/api/transactions`    | bearer  | History, `?filter=all\|received\|sent` |
| POST   | `/api/transfers`       | bearer  | Send money to another user |
| GET    | `/api/admin/stats`     | admin   | Platform statistics |
| GET    | `/api/admin/users`     | admin   | All users with wallets |

Get a token:

```
POST /api/token
Content-Type: application/json

{ "email": "user@example.com", "password": "User123!" }
```

Response:

```json
{
  "token": "...",
  "tokenType": "Bearer",
  "expiresAt": "2026-10-05T12:00:00Z"
}
```

Use it:

```
GET /api/wallet
Authorization: Bearer <token>
```

Tokens are stateless: they are protected with ASP.NET Core Data
Protection, carry the user id and the expiry, and live 24 hours by
default (`Api:TokenLifetimeHours` in `appsettings.json`). API error
messages are always English.

Interactive documentation is available at `/swagger`.

## Architecture

MVC + Services

Controller
    ↓
Service
    ↓
DbContext
    ↓
SQLite

API follows the same path:

ApiController → Service → DbContext → SQLite

See `AGENTS.md` for development and coding guidelines.
