# PaymentsServiceForModulbank

![Static Badge](https://img.shields.io/badge/language-C%23-red)
![Static Badge](https://img.shields.io/badge/powered_by-.NET_10-blue)
![Static Badge](https://img.shields.io/badge/platforms-Windows,Linux-purple)
![Static Badge](https://img.shields.io/badge/version-1.0-orange)
![Static Badge](https://img.shields.io/badge/developer-sergxlove-green)
![Static Badge](https://img.shields.io/badge/year-2026-green)

## About 

This project is an implementation of a reliable payment service in C# (.NET 10) that interacts with an external provider via HTTP requests and processes callback quittances to confirm the final status of operations, while ensuring data consistency in the event of network failures, concurrent requests, and system restarts. The service provides a mandatory REST API for creating operations, sending them, retrieving their status and event history, uses a persistent storage to save the state of operations, sending intentions, and events, and implements the idempotency pattern via Idempotency-Key when calling the provider, as well as correctly handling early, repeated, and conflicting callback receipts, ensuring that no more than one real payment corresponds to a single operation, and the final result is determined solely by the provider’s confirmation. The project is packaged in Docker Compose with the candidate-service and provider-simulator services, which ensure automatic recovery of unfinished operations after a restart and complete data preservation in the volume. The task of this project is [here](https://github.com/sergxlove/PaymentsServiceForModulbank/blob/master/task.md).

## Install 

The program requires Docker/ 

1. Clone this repository using the command:

```git clone https://github.com/sergxlove/PaymentsServiceForModulbank.git```

  or 

```git clone git@github.com:sergxlove/PaymentsServiceForModulbank.git```

2. Go to the folder of this repository using the command:

```cd PaymentsServiceForModulbank``` 

3. Run the Docker container using the command:

```docker compose up --build```

4. Open swagger at the address http://localhost:8080 
   
