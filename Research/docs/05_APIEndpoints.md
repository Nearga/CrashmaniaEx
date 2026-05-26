# CrashMania API Endpoints Reference

## Base URLs

| Service | URL |
|---------|-----|
| Main API | `https://api.crashmania.com/api/` |
| Crash Game API | `https://crash.crashmania.com/api` |
| File CDN | `https://files.crashmania.com/` |
| Frontend | `https://game.crashmania.com/` |

## Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/` | Email/password authentication |
| POST | `/api/token/refresh` | Refresh JWT token |
| POST | `/social/login` | Social login (Google/Facebook) |
| POST | `/social/register` | Social registration |

### Third-Party Auth Providers
- **Google**: Uses `https://accounts.google.com/gsi/client`
- **Facebook**: Uses `https://connect.facebook.net/en_US/sdk.js`

## Lobby & Catalog

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/lobby` | Lobby page data (featured games, categories) |
| GET | `/api/catalog` | Full game catalog |
| GET | `/api/catalog/games` | Games listing with filters |

### Expected Lobby Response Structure (inferred)
```json
{
  "categories": [
    {
      "id": "string",
      "name": "string",
      "icon": "url",
      "games": [
        {
          "id": "string",
          "name": "string",
          "thumbnail": "url",
          "provider": "string",
          "type": "crash|slot|table|etc",
          "isNew": boolean,
          "isFeatured": boolean
        }
      ]
    }
  ],
  "banners": [...],
  "topGames": [...]
}
```

## Player

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/player` | Player profile & balance |
| GET | `/api/stats/` | Player statistics |

## Store & Payments

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/stores` | Available store packages |
| GET | `/api/Offers` | Current offers/deals |
| POST | `/api/payment` | Process payment |
| GET | `/api/payment-provider/current` | Get active payment provider |
| GET | `/api/transactions` | Transaction history |

### Payment Providers
- **SafeCharge**: `https://cdn.safecharge.com/safecharge_resources/v1/websdk/safecharge.js`
- **Pay.com**: `https://js.pay.com/v1.js`
- **Smart2Pay**: `https://apitest.smart2pay.com` (test environment found)

## Bonuses & Rewards

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/hourly-bonus` | Hourly bonus status/timer |
| POST | `/hourly-bonus/claim` | Claim hourly bonus |
| GET | `/weekly-streak-bonus` | Weekly streak status |
| POST | `/weekly-streak-bonus/claim` | Claim weekly streak bonus |
| GET | `/monthly-calendar-bonus` | Monthly calendar status |
| POST | `/monthly-calendar-bonus/claim` | Claim calendar bonus |
| GET | `/welcome-bonus/active` | Check active welcome bonus |
| POST | `/welcome-bonus/claim` | Claim welcome bonus |
| GET | `/welcome-bonus/logged-in` | Welcome bonus for returning users |
| GET | `/coinback` | Coinback reward status |
| POST | `/coinback/claim` | Claim coinback |
| POST | `/coinback/presented` | Mark coinback as seen |
| POST | `/levelup-rewards/claim` | Claim level-up reward |
| GET | `/rolling-offers` | Time-limited rolling offers |

## Promotions & Events

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/promotions` | Active promotions |
| GET | `/api/events` | Active events |

## Social & Referrals

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/referrals` | Referral program data |
| GET | `/rate_us` | App rating prompt data |
| GET | `/api/app-ratings` | App ratings |

## Game Providers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/mg` | Mancala Games integration |

## Verification (KYC)

- **Provider**: Sumsub
- **SDK URL**: `https://in.sumsub.com`
- **Route**: `/idensic` and `/verify`

## CDN Assets

### WebGL Builds
```
https://files.crashmania.com/WebglBuilds/CrashManiaProd/
```

### Lobby Assets
```
/lobby-images/    - Promotional images, banners
/lobby-icons/     - Category and filter icons
/lobby-bg/        - Background assets
```

## Analytics & Monitoring

| Service | URL |
|---------|-----|
| AWS RUM | `https://dataplane.rum.us-east-1.amazonaws.com` |
| Facebook Pixel | `https://connect.facebook.net/en_US/fbevents.js` |
| Intercom | `https://api-iam.intercom.io` (+ EU/AU endpoints) |

## Notes

1. All API endpoints require authentication (returned 404 without auth tokens)
2. Auth appears to use JWT with refresh token mechanism
3. The API follows RESTful patterns
4. CDN URLs for game assets are likely signed/temporary
5. WebSocket connections may be used for the crash game real-time updates (crash.crashmania.com)
