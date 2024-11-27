# Consul Demo Application

This repository contains a demo application that demonstrates how to load configuration from Consul, monitor for configuration changes, and use feature flags in an ASP.NET Core application.

## Getting Started

### Clone the Repository

Clone this repository to your local machine:

```bash
git clone https://github.com/yourusername/consul-demo.git
cd consul-demo
```

### Build and Run the Application

Use Docker Compose to build and start the services:

```bash
docker-compose up --build
```

This command will:
- Build the `consul-demo` application image.
- Start two instances of the application (`consul-demo-1` and `consul-demo-2`).
- Start a Consul server.
- Load initial configuration into Consul using the `consul-loader` service.

## Services

The Docker Compose setup includes the following services:
- **consul-demo-1**: First instance of the demo application running on port 8080.
- **consul-demo-2**: Second instance of the demo application running on port 8081.
- **consul**: Consul server for configuration storage and service discovery.
- **consul-loader**: Initializes the Consul KV store with configuration data from `demo-config.json`.

## Endpoints

The demo application exposes the following endpoints:

### GET /config

**Description**: Retrieves the current application configuration.

**URL**:
- Instance 1: `http://localhost:8080/config`
- Instance 2: `http://localhost:8081/config`

**Response**: JSON object containing the application configuration.

**Example Request**:

```bash
curl http://localhost:8080/config
```

**Example Response**:

```json
{
    "apiCredentialConfiguration": {
        "appA": {
            "baseUrl": "https://api.appa.com",
            "apiKey": "your-api-key",
            "methodType": "GET",
            "isAnonymous": false
        },
        "appB": {
            "baseUrl": "https://api.example.com",
            "apiKey": "your-api-key",
            "methodType": "POST",
            "isAnonymous": false
        },
        "appC": {
            "baseUrl": "https://api.test.com",
            "apiKey": "your-api-key",
            "methodType": "PUT",
            "isAnonymous": true
        }
    },
    "featureManagement": {
        "useFeatureA": true,
        "useFeatureB": false,
        "useFeatureC": true
    },
    "logging": {
        "logging": {
            "includeScopes": true,
            "logLevel": {
                "default": "Information",
                "system": "Information",
                "microsoft": "Information"
            },
            "console": {
                "includeScopes": true,
                "logLevel": {
                    "default": "Information",
                    "system": "Information",
                    "microsoft": "Information"
                }
            }
        }
    }
}
```

### GET /feature

**Description**: Checks if a specific feature (UseFeatureA) is enabled.

**URL**:
- Instance 1: `http://localhost:8080/feature`
- Instance 2: `http://localhost:8081/feature`

**Response**:
- `200 OK`: Feature A is enabled.
- `423 Locked`: Feature A is disabled.

**Example Request**:

```bash
curl http://localhost:8080/feature
```

**Example Response (Feature Enabled)**:

```
Feature A is enabled.
```

**Example Response (Feature Disabled)**:

```
Status Code: 423 Locked
```

## Updating Configuration

The application uses Consul's KV store to load and monitor configuration. Changes to the configuration in Consul are automatically picked up by the application instances without restarting them.

### Steps to Update Configuration

1. **Access the Consul UI**:
     - Navigate to `http://localhost:8500/ui` in your web browser.

2. **Navigate to the KV Store**:
     - Click on **Key/Value** in the Consul UI.
     - Locate the key `appsettings/myapp`.

3. **Edit the Configuration**:
     - Click on the key `appsettings/myapp`.
     - Click the **Edit** button to modify the configuration.
     - Make your desired changes in the JSON configuration.

     **Example**: Toggle the `useFeatureA` flag:

     ```json
     "featureManagement": {
         "useFeatureA": false,
         "useFeatureB": false,
         "useFeatureC": true
     }
     ```

4. **Save the Changes**:
     - Click **Save** to apply the changes.

5. **Verify the Changes in the Application**:
     - Wait a few seconds for the application to pick up the changes.
     - Access the `/config` endpoint to verify the new configuration.
     - Access the `/feature` endpoint to see if `UseFeatureA` is now disabled.

## Consul UI

Consul provides a web UI for managing and viewing the KV store and services.

**URL**: `http://localhost:8500/ui`

**Features**:
- **KV Store Management**: View, edit, and delete key-value pairs.
- **Service Discovery**: See registered services and their health status.