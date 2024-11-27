# Open Feature API Documentation

## Overview

The Open Feature API is a feature flag management system that allows you to enable or disable features in your application dynamically. This API uses Redis for storing feature flags and provides endpoints to manage and utilize these flags. It is designed to be scalable and can run multiple instances concurrently.

## Getting Started

### Prerequisites

- Docker and Docker Compose installed on your machine.
- Basic knowledge of using the terminal/command prompt.

### Starting the Application

1. Clone the Repository (if applicable):

    ```bash
    git clone https://github.com/your-repo/open-feature-api.git
    cd open-feature-api
    ```

2. Open a Terminal in the project directory.

3. Run Docker Compose to build and start the containers:

    ```bash
    docker-compose up
    ```

    This command will:
    - Build the open-feature-api Docker image.
    - Start two instances of the Open Feature API service.
    - Start a Redis instance.

### Access the API Endpoints

The API is available on port 5000 and port 5001. You can use either port to interact with the API.

Swagger UI is available at:
- [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)
- [http://localhost:5001/swagger/index.html](http://localhost:5001/swagger/index.html)

## API Endpoints

### 1. Get All Feature Flags

- **URL:** `/feature-flags`
- **Method:** `GET`
- **Description:** Retrieves all feature flags stored in Redis.

    ```sh
    curl -X GET http://localhost:5000/feature-flags
    # or
    curl -X GET http://localhost:5001/feature-flags
    ```

### 2. Update Feature Flag

- **URL:** `/feature-flags/{key}`
- **Method:** `POST`
- **Description:** Updates or creates a feature flag with the specified key and value.
- **Parameters:**
  - `{key}`: The key/name of the feature flag.
  - **Body:** A JSON string representing the value of the feature flag (e.g., `"true"` or `"false"`).

    ```sh
    curl -X POST http://localhost:5000/feature-flags/{key} \
    -H "Content-Type: application/json" \
    -d "\"value\""
    # or
    curl -X POST http://localhost:5001/feature-flags/{key} \
    -H "Content-Type: application/json" \
    -d "\"value\""
    ```

### 3. Use Feature

- **URL:** `/use-feature`
- **Method:** `GET`
- **Description:** Checks if the feature `feature-a` is enabled and returns a response accordingly.

    ```sh
    curl -X GET http://localhost:5000/use-feature
    # or
    curl -X GET http://localhost:5001/use-feature
    ```

## Example Usage

### 1. Retrieve All Feature Flags

    ```sh
    curl -X GET http://localhost:5000/feature-flags
    # or
    curl -X GET http://localhost:5001/feature-flags
    ```

### 2. Update a Feature Flag

    ```sh
    curl -X POST http://localhost:5000/feature-flags/feature-a \
    -H "Content-Type: application/json" \
    -d "\"true\""
    # or
    curl -X POST http://localhost:5001/feature-flags/feature-a \
    -H "Content-Type: application/json" \
    -d "\"true\""
    ```

### 3. Check Feature Usage

    ```sh
    curl -X GET http://localhost:5000/use-feature
    # or
    curl -X GET http://localhost:5001/use-feature
    ```

**Expected Response:**

If the feature `feature-a` is enabled (set to `"true"`), you will receive:

    ```sh
    hoorah
    ```

If the feature is not enabled, you will receive an HTTP status code `423 Locked`.

## Docker Compose Configuration

The application is configured to run using Docker Compose. Here's the relevant part of the `docker-compose.yml` file:

    ```yaml
    version: '3.8'

    networks:
      open-demo:
        driver: bridge

    services:
      open-feature-api-1:
        image: open-feature-api
        container_name: open-feature-api-1
        build:
          context: .
          dockerfile: Open-Feature-Api/Dockerfile
        ports:
          - "5000:8080"
        networks:
          - open-demo

      open-feature-api-2:
        image: open-feature-api
        container_name: open-feature-api-2
        build:
          context: .
          dockerfile: Open-Feature-Api/Dockerfile
        ports:
          - "5001:8080"
        networks:
          - open-demo

      redis:
        image: redis:alpine
        ports:
          - "6379:6379"
        networks:
          - open-demo
    ```

### Explanation

**Networks:**

- `open-demo`: A bridge network that allows containers to communicate.

**Services:**

- `open-feature-api-1`: Builds and runs an instance of the Open Feature API. Exposes port 5000 (mapped to container port 8080).
- `open-feature-api-2`: A second instance of the Open Feature API. Exposes port 5001 (also mapped to container port 8080).
- `redis`: Runs a Redis instance using the `redis:alpine` image. Exposes port 6379.

## Dependencies

- **ASP.NET Core:** The web framework used to build the API.
- **StackExchange.Redis:** A high-performance Redis client for .NET.
- **OpenFeature:** The OpenFeature SDK for feature flag evaluation.

## Configuration

### Redis Connection

The application connects to Redis using the following configuration:

    ```vbnet
    redis:6379,defaultDatabase=1,abortConnect=false,connectTimeout=1000,asyncTimeout=5000,syncTimeout=5000,connectRetry=3,keepAlive=30
    ```

- **Redis Host:** `redis` (as per the Docker Compose network).

### Memory Cache

An in-memory cache is used to store feature flags locally in each API instance. This improves performance by reducing the number of calls to Redis. The cache is invalidated via Redis Pub/Sub when feature flags are updated.

### Logging

- **Console Logging:** The application is configured to output logs to the console.
- **Log Levels:** Configured to capture debug and information logs.

### Swagger UI

Access Swagger UI:
- [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)
- [http://localhost:5001/swagger/index.html](http://localhost:5001/swagger/index.html)

**Description:** Swagger UI provides an interactive interface for exploring and testing the API endpoints.

## Notes

- **Multiple Instances:** Running multiple instances of the API demonstrates scalability and load balancing (if configured).
- **Feature Flag Consistency:** Feature flags are stored in Redis and cached in each API instance. Updates to flags are propagated to all instances via Redis Pub/Sub.

## Troubleshooting

### Docker Issues:

- If you encounter issues with Docker, ensure Docker Desktop is running and you have the latest version.
- Use `docker-compose down` to stop and remove containers if needed.

### Port Conflicts:

- If ports 5000, 5001, or 6379 are in use, adjust the ports section in the `docker-compose.yml` file.

### Redis Connection Errors:

- Ensure that the Redis container is running and accessible.
- Check network configurations if running in a different environment.

## Cleaning Up

To stop the application and remove the containers, run:

    ```bash
    docker-compose down
    ```

## Conclusion

This documentation provides an overview of the Open Feature API, instructions to start the application, details on API endpoints, and example commands to interact with the API. By following these steps, you can manage feature flags dynamically in your application using Redis and the Open Feature API.