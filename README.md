# MindFlow Backend

REST API for MindFlow, a mental wellness application that helps users track their emotional well-being through journaling, habit tracking, AI-powered insights, and real-time chat support.

## Tech Stack

- **.NET 10** (ASP.NET Core Web API)
- **MySQL** (via Entity Framework Core)
- **Google Gemini** (AI responses, weekly summaries, habit suggestions)
- **Stripe** (subscription management)
- **Firebase Cloud Messaging** (push notifications)
- **Cloudinary** (media file storage)
- **QuestPDF** (PDF report generation)
- **BCrypt** (password hashing)
- **JWT** (authentication)

## Architecture

The project follows **Domain-Driven Design (DDD)** with bounded contexts:

```
Mindflow-backend/
├── iam/                  # Identity & Access Management (auth, users, PIN)
├── journal/              # Journal entries, tags, media uploads
├── habits/               # Habit tracking, completion logs, streaks
├── analytics/            # Weekly analytics, word cloud, mood calendar
├── Chat/                 # AI-powered conversations
├── AiFeedback/           # User ratings on AI-generated content
├── AiIntegration/        # Gemini API integration & metrics
├── Notifications/        # FCM push notifications & notification history
├── Support/              # Support tickets with email confirmation
├── Subscriptions/        # Stripe checkout, webhooks, premium plans
├── Reporting/            # PDF & CSV export (premium)
├── WellnessEngine/       # Stress check & automatic habit adjustment
└── shared/               # Base repositories, UoW, encryption, interceptors
```

Each bounded context follows the layered structure:
- **Domain** — Entities, aggregates, value objects
- **Application** — Commands, queries, handlers, DTOs, services
- **Infrastructure** — Repositories, external service integrations
- **Interfaces** — REST controllers

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.0+
- (Optional) Stripe account for subscriptions
- (Optional) Google Cloud project for Gemini AI and Google Auth
- (Optional) Firebase project for push notifications
- (Optional) Cloudinary account for media uploads

## Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/user/MindFlow-Backend.git
   cd MindFlow-Backend
   ```

2. **Configure environment variables** in `appsettings.Development.json` or user secrets:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=mindflow;User=root;Password=yourpassword"
     },
     "TokenSettings": {
       "Secret": "your-jwt-secret-min-32-characters-long"
     },
     "AiSettings": {
       "GeminiApiKey": "your-gemini-api-key",
       "GeminiModel": "gemini-2.0-flash"
     },
     "Stripe": {
       "SecretKey": "sk_test_...",
       "PremiumPriceId": "price_...",
       "WebhookSecret": "whsec_..."
     },
     "Google": {
       "ClientId": "your-google-client-id"
     },
     "Firebase": {
       "ProjectId": "your-project-id",
       "ServiceAccountJson": "{ ... }"
     },
     "Cloudinary": {
       "CloudName": "your-cloud",
       "ApiKey": "your-key",
       "ApiSecret": "your-secret"
     },
     "Email": {
       "From": "noreply@example.com",
       "Host": "smtp.gmail.com",
       "Port": "587",
       "Username": "your-email",
       "Password": "your-app-password"
     },
     "Encryption": {
       "AesKey": "base64-encoded-32-byte-key"
     },
     "FrontendUrl": "http://localhost:5173"
   }
   ```

3. **Run the application**
   ```bash
   dotnet run --project Mindflow-backend
   ```
   The database is migrated automatically on startup. Swagger is available at `/swagger`.

## API Endpoints

### Authentication (`/api/v1/users`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/sign-up` | No | Register with email, password, and optional name |
| POST | `/sign-in` | No | Login, returns JWT token |
| POST | `/google-auth` | No | Google OAuth login |
| POST | `/forgot-password` | No | Send password reset email |
| POST | `/reset-password` | No | Reset password with token |
| GET | `/profile` | Yes | Get user profile |
| PUT | `/profile` | Yes | Update name and occupation |
| DELETE | `/` | Yes | Delete account and all related data |
| POST | `/pin` | Yes | Set security PIN |
| POST | `/pin/verify` | Yes | Verify PIN |
| DELETE | `/pin` | Yes | Remove PIN |
| GET | `/pin/status` | Yes | Check if PIN is configured |

### Journal (`/journal`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/entries` | Yes | List entries (supports `_sort`, `_order`, `_limit`, `q`) |
| GET | `/entries/{id}` | Yes | Get entry by ID |
| POST | `/entries` | Yes | Create entry (auto-detects sentiment) |
| PUT | `/entries/{id}` | Yes | Update entry |
| DELETE | `/entries/{id}` | Yes | Soft-delete entry |
| GET | `/tags` | Yes | List user's tags |
| GET | `/entry-tags` | Yes | List entry-tag associations |
| POST | `/entry-tags` | Yes | Associate tag with entry |
| DELETE | `/entry-tags/{id}` | Yes | Remove tag association |
| GET | `/media` | Yes | List media for an entry |
| POST | `/media` | Yes | Create media record |
| POST | `/media/upload` | Yes | Upload file (max 10MB) |

### Habits (`/habits`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Yes | List habits (streak and status recalculated in real-time) |
| GET | `/{id}` | Yes | Get habit by ID |
| POST | `/` | Yes | Create habit |
| PUT | `/{id}` | Yes | Update name, category, frequency |
| DELETE | `/{id}` | Yes | Delete habit |
| GET | `/streak-summary` | Yes | Streak summary for all habits |
| POST | `/suggestions` | Yes | AI-generated habit suggestions |

### Habit Logs (`/habit-logs`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Yes | List logs (filter by `habit_id`) |
| POST | `/` | Yes | Create completion log (recalculates streak) |
| GET | `/{id}` | Yes | Get log by ID |
| PUT | `/{id}` | Yes | Update log |
| DELETE | `/{id}` | Yes | Delete log (recalculates streak) |

### Analytics

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/analyticsCache` | Yes | Get weekly analytics (computes on cache miss) |
| POST | `/analyticsCache/compute` | Yes | Force recomputation |
| GET | `/wordCloud` | Yes | Get word cloud from journal entries |
| POST | `/wordCloud/compute` | Yes | Force word cloud recomputation |
| GET | `/moodCalendar` | Yes | Mood calendar by month (`year`, `month`) |

### Chat (`/chat`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/conversations` | Yes | Create conversation with first message |
| GET | `/conversations` | Yes | List conversations |
| DELETE | `/conversations/{id}` | Yes | Delete conversation |
| POST | `/conversations/{id}/messages` | Yes | Send message (AI responds) |
| GET | `/conversations/{id}/messages` | Yes | Get conversation messages |

### Notifications (`/notifications`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Yes | List notifications (last 50) |
| PATCH | `/{id}/read` | Yes | Mark notification as read |
| POST | `/register-device` | Yes | Register FCM device token |
| DELETE | `/unregister-device` | Yes | Unregister device token |

### AI Feedback (`/api/v1/ai-feedback`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/` | Yes | Submit rating (1-5) for AI content |
| GET | `/` | Yes | List user's ratings |
| GET | `/summary` | Yes | Rating distribution summary |

### Wellness (`/wellness`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/stress-check` | Yes | Run stress analysis; auto-pauses/resumes habits |

### Support (`/api/v1/support`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/tickets` | Yes | Create support ticket (sends email confirmation) |
| GET | `/tickets` | Yes | List user's tickets |

### Subscriptions (`/api/v1/subscriptions`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/checkout` | Yes | Create Stripe checkout session |
| GET | `/me` | Yes | Get subscription status |
| POST | `/verify-session` | Yes | Verify Stripe session after payment |
| POST | `/cancel` | Yes | Cancel subscription |
| POST | `/webhook` | No | Stripe webhook handler |

### Reporting (`/api/v1/reporting`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/export/pdf` | Yes | Export journal as PDF (premium) |
| GET | `/export/csv` | Yes | Export journal as CSV (premium) |

## Background Services

- **WeeklySummaryScheduler** — Computes analytics for all users every Sunday at 06:00 UTC
- **HydrationReminderService** — Sends hydration push notifications every 2 hours

## Testing

```bash
dotnet test
```

## Database Migrations

```bash
dotnet ef migrations add MigrationName --project Mindflow-backend --startup-project Mindflow-backend
dotnet ef database update --project Mindflow-backend --startup-project Mindflow-backend
```
