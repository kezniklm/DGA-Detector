# DGA-Detector API Documentation

The DGA-Detector API serves as the backend for the Detector block, providing essential functionalities for user authentication, blacklist and whitelist management, and handling domain analysis results.

## Getting Started

To integrate with the DGA-Detector API, follow the steps below:

### Prerequisites
- .NET Core 8.0+ SDK installed
- MongoDB instance running
- MySQL instance running (optional)

### Installing

1. Clone the DGA-Detector API repository:
```
$ git clone https://github.com/kezniklm/DGA-Detector
```
2. Navigate to the Web application directory:
```
$ cd DGA-Detector/src/Web application/
```
3. Install dependencies and build the project:
```
$ dotnet build
```
4. Configure the appsettings.json file with MongoDB and MySQL (optional) connection details.
5. Run the DGA-Detector API:
```
$ dotnet run
```

## Authentication

The API includes a built-in authentication controller supporting login, registration, logout, and user identity verification. Authentication is required for accessing the majority of endpoints and is typically achieved through the use of authorization tokens.

### Authorization Method

The API supports authentication via authorization tokens. Depending on the configuration, this can be through Bearer tokens, cookies, or session cookies. Ensure that your requests include the appropriate authorization headers or cookie values as required.

### Endpoints

- `POST /register`: Registers a new user.
- `POST /login`: Logs in a user.
- `POST /refresh`: Refreshes the authentication token.
- `GET /confirmEmail`: Confirms user email.
- `POST /resendConfirmationEmail`: Resends confirmation email.
- `POST /forgotPassword`: Sends a password reset link to the user's email.
- `POST /resetPassword`: Resets user password.
- `POST /manage/2fa`: Manages two-factor authentication.
- `GET /manage/info`: Gets user information.
- `POST /manage/info`: Updates user information.
- `POST /logout`: Logs out the current user.
- `GET /pingauth`: Returns the email of the authenticated user.

## Blacklist Management

The Blacklist Controller manages operations related to the blacklist, such as retrieving, creating, updating, and deleting blacklist entries.

### Endpoints

- `GET /blacklist`: Retrieves all blacklist entries.
- `GET /blacklist/{id}`: Retrieves a specific blacklist entry by ID.
- `POST /blacklist`: Creates a new blacklist entry.
- `PATCH /blacklist`: Updates an existing blacklist entry.
- `DELETE /blacklist/{id}`: Deletes a blacklist entry by ID.
- `GET /blacklist/{max}/{page}/{filter?}`: Retrieves blacklist entries with pagination and optional filtering.
- `GET /blacklist/count`: Gets the total count of blacklist entries.

## Result Management

The Result Controller handles operations related to the results of domain analysis, such as retrieving and managing analysis results.

### Endpoints

- `GET /result`: Retrieves all analysis results.
- `GET /result/{id}`: Retrieves a specific analysis result by ID.
- `POST /result`: Creates a new analysis result.
- `PATCH /result`: Updates an existing analysis result.
- `DELETE /result/{id}`: Deletes an analysis result by ID.
- `GET /result/{max}/{page}/{filter?}`: Retrieves analysis results with pagination and optional filtering.
- `GET /result/NumberOfDomainsToday`: Gets the number of domains analyzed today.
- `GET /result/PositiveResultsToday`: Gets the number of positive detections today.
- `GET /result/FilteredByBlacklist`: Gets the count of results filtered by the blacklist.
- `GET /result/count`: Gets the total count of analysis results.

## User Management

The User Controller provides endpoints for managing user profiles, including updating user information and changing passwords.

### Endpoints

- `PUT /user/update`: Updates the current user's profile.
- `POST /user/delete/{userId}`: Deletes a user by user ID.
- `POST /user/change-password`: Changes the current user's password.

## Whitelist Management

The Whitelist Controller manages operations related to the whitelist, similar to the Blacklist Controller, including retrieving, creating, updating, and deleting whitelist entries.

### Endpoints

- `GET /whitelist`: Retrieves all whitelist entries.
- `GET /whitelist/{id}`: Retrieves a specific whitelist entry by ID.
- `POST /whitelist`: Creates a new whitelist entry.
- `PATCH /whitelist`: Updates an existing whitelist entry.
- `DELETE /whitelist/{id}`: Deletes a whitelist entry by ID.
- `GET /whitelist/{max}/{page}/{filter?}`: Retrieves whitelist entries with pagination and optional filtering.
- `GET /whitelist/count`: Gets the total count of whitelist entries.

## Technologies

The API is built using ASP.NET Core and employs MongoDB for data persistence of Blacklist, Whitelist and Results collections and SQLite or MySQL for persistence of User information. It utilizes `System.Security.Claims` for claims-based identity management, along with Microsoft's `Identity` system for user authentication and authorization.

Please ensure that your requests to the API are properly authorized using the supported method (Bearer token, cookies, or session cookies) to access the protected resources.

## Logging

Logging within the DGA-Detector API is configured using Serilog. Log entries are enriched with contextual information and written to the console. For Windows platforms, errors are also logged to the Event Log, while for other platforms, they are logged to the local syslog.

Ensure to review the logging configuration in the `ConfigureLogging` method of the application startup for customization or adjustments according to your requirements.

## Author

- **Matej Keznikl** -  [kezniklm](https://github.com/kezniklm)

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](../../../LICENSE) file for details.
