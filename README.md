This API is basic and serves as a foundation for further development. The primary goal was to showcase my expertise in building robust systems with ASP.NET. Of course, for optimization purposes, throttling could be added to individual endpoints, as well as features like an endpoint responsible for fetching songs based on a submitted array of song IDs, etc.

## Getting Started
Clone the project to your local machine using the following command:
```
git clone <REPOSITORY_URL>
```

In the root directory of the project, create a .env file and configure the required environment variables. Example configuration below.
```bash
DB_PORT=5432
DB_HOST=localhost
DB_DATABASE=spotify_api
DB_USERNAME=spotify_api_user
DB_PASSWORD=passwordExample-123456789101234567890123456
REDIS_PORT=6379
FRONTEND_DOMAIN=http://localhost:3000

SPOTIFY_API_CONNECTION_STRING=Host=localhost;Port=5432;Database=spotify_api;Username=spotify_api_user;Password=passwordExample-123456789101234567890123456

ACCESS_TOKEN_SECRET_KEY=secretKeyExample-123456789101234567890123456
REFRESH_TOKEN_SECRET_KEY=refreshTokenKeyExample-123456789101234567890123456
PASSWORD_RESET_TOKEN_SECRET_KEY=passwordResetKeyExample-123456789101234567890123456

SMTP_SERVER=smtp.gmail.com
SMTP_PORT=465
CLIENT_URL=http://localhost:3000
SENDER_NAME=Spotify
SENDER_EMAIL=YOUR_EMAIL
SENDER_EMAIL_PASSWORD=YOUR_EMAIL_PASSWORD
```

Install the required dependencies by running the following command in the project directory:
```
dotnet restore
```
This command will download and install all the dependencies listed in the *.csproj file.

After installing the dependencies, start the application using:
```
dotnet run
```

## Backend Features
The ASP.NET Core backend includes the following features:

- User registration and login
- Password reset via email (sends a reset link)
- Token-based authentication (access and refresh tokens)
- Authorization
- Fetching songs from the iTunes API
- Creating playlists
- Editing playlists (name, description, privacy settings)
- Deleting playlists
- Adding songs to playlists
- Adding and managing playlist collaborators
