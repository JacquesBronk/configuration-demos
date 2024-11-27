# OpenFeature Demos Documentation

This guide provides documentation for two demos of the OpenFeature API using different technologies: Redis Pub/Sub and Consul with Kafka. Both implementations enable feature flag management and dynamic updates, showcasing scalable and robust systems.

## Demo 1: OpenFeature API with Redis Pub/Sub

### Overview

This demo uses Redis Pub/Sub to manage feature flags dynamically. It supports real-time updates across multiple API instances, demonstrating how Redis can be used for efficient feature management.

### Prerequisites

- Docker and Docker Compose installed.
- Basic knowledge of terminal commands.

### Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/JacquesBronk/configuration-demos.git
    cd open-feature-api
    ```
2. Start the application:
    ```sh
    docker-compose up
    ```
3. Access the API:
    - Instance 1: [http://localhost:5000/swagger/index.html](http://localhost:5000/swagger/index.html)
    - Instance 2: [http://localhost:5001/swagger/index.html](http://localhost:5001/swagger/index.html)

### API Endpoints

- **Get All Feature Flags:** `/feature-flags` (GET)
- **Update Feature Flag:** `/feature-flags/{key}` (POST)
- **Use Feature:** `/use-feature` (GET)

Refer to the full Redis demo [documentation](/Option-1-Redis/Option1.md) for detailed API usage examples.

## Demo 2: OpenFeature API with Consul and Kafka

### Overview

This demo integrates Consul for configuration management and Kafka for feature flag updates. It highlights how Consul's KV store and Kafka's message streaming capabilities enable scalable feature management.

### Prerequisites

- Docker and Docker Compose installed.
- Basic knowledge of terminal commands.

### Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/consul-demo.git
    cd consul-demo
    ```
2. Start the application:
    ```sh
    docker-compose up --build
    ```
3. Access the application:
    - Instance 1: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)
    - Instance 2: [http://localhost:8081/swagger/index.html](http://localhost:8081/swagger/index.html)
    - Consul UI: [http://localhost:8500](http://localhost:8500)

### API Endpoints

- **Get Configuration:** `/config` (GET)
- **Use Feature:** `/feature` (GET)

Refer to the full Consul demo [documentation](/Option-2-Consul/Option2.md) for detailed API usage examples.

## Comparison

| Feature             | Redis Pub/Sub Demo               | Consul + Kafka Demo               |
|---------------------|----------------------------------|-----------------------------------|
| **Feature Store**   | Redis                            | Consul                            |
| **Real-Time Updates** | Redis Pub/Sub                   | Consul KV                            |
| **Configuration UI** | N/A                              | Consul UI                         |
| **API Instances**   | 2 Instances (Ports 5000, 5001)   | 2 Instances (Ports 8080, 8081)    |
| **Scalability**     | High (Redis caching and Pub/Sub) | High (Consul KV and Kafka messages) |

## Additional Notes

- Both demos highlight scalable, real-time feature flag management approaches.
- Use the Swagger UI in the Redis demo for interactive API exploration.
- Use Consul UI in the Consul demo for KV management and service discovery.
