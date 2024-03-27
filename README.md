# reservation-api
### Coding challenge (March 2024)

<br>

#### Programming language: C#
#### Project type: .NET Web API
#### Target framework: .NET 8.0
#### Database: SQLite
#### Code repository: https://github.com/weili-projects/reservation-api

<br><br>

#### Architecture
Implement a controller-service-repository architecture pattern with the Entity Framework.

<br><br>

#### Endpoints

##### 1. POST:  /api/availability
For adding availabilities. Although a slot is 15 minutes long, a provider doesn't have to send multiple requests (each with a single slot), or a list of slots in one request. The code is designed to accept a list of consecutive time ranges (e.g. from 9 am to 5 pm on 3/8/2024). The service layer slices the ranges into 15-minute slots. The sanity checks ensure the valid slots start from the 0th, 15th, 30th, or 45th minute in an hour. If the slot already exists, no duplicate slot will be added, and the added slot needs to be at a future time.    
  
HTTP status codes: 201 (Created, with the list of slots created), 404 (Not Found)  
  
##### Example (returning 201)  

request URL:  
  
http://localhost:5000/api/availability

request payload:
```json
{
  "ProviderId": 1,
  "AvailabilityRanges": [
    {
      "StartTime": "2024-03-26T09:00:00",
      "EndTime": "2024-03-26T09:45:00"
    }
  ]
}
```

response payload:
```json
[
    {
        "availabilityId": 75,
        "startTime": "2024-03-26T09:00:00",
        "endTime": "2024-03-26T09:15:00"
    },
    {
        "availabilityId": 76,
        "startTime": "2024-03-26T09:15:00",
        "endTime": "2024-03-26T09:30:00"
    },
    {
        "availabilityId": 77,
        "startTime": "2024-03-26T09:30:00",
        "endTime": "2024-03-26T09:45:00"
    }
]
```

<br>

##### 2. GET:   /api/availability/{pid}
For retrieving a provider's availability slots. The pid is the provider ID. Assume that the client is interested in a particular provider. See possible enhancements for this endpoint in the last section.  
  
HTTP status codes: 200 (OK, with the list of available slots), 204 (No Content), 404 (Not Found)  
  
##### Example (returning 200)  
  
request URL:  
  
http://localhost:5000/api/availability/1
  
response payload:
```json
[
    {
        "availabilityId": 1,
        "startTime": "2024-03-15T09:00:00",
        "endTime": "2024-03-15T09:15:00"
    },
    {
        "availabilityId": 2,
        "startTime": "2024-03-15T09:15:00",
        "endTime": "2024-03-15T09:30:00"
    },
    {
        "availabilityId": 4,
        "startTime": "2024-03-15T09:45:00",
        "endTime": "2024-03-15T10:00:00"
    }
]
```

<br>

##### 3. POST:  /api/appointment
For making an appointment by providing an available appointment slot. Reservation needs to be 24 hours in advance, compared to the slot start time. If a slot has already been confirmed or is pending for confirmation (not expired), it will be unavailable for a new reservation, unless the unconfirmed slot expires after 30 minutes. Apply BeginTransactionAsync to mitigate the race condition. Use "BeginTransactionAsync" over "lock" because it is related to database operations.  
  
HTTP status codes: 201 (Created, with the created appointment), 404 (Not Found)  
  
##### Example (returning 201)  

request URL:  
  
http://localhost:5000/api/appointment
  
request payload:
```json
{
  "AvailabilityId": 2,
  "ClientId": 3
}
```
response payload:
```json
{
    "appointmentId": 1,
    "availabilityId": 2,
    "appointmentTime": "2024-03-15T09:15:00",
    "clientId": 3,
    "clientName": "C3",
    "isConfirm": false,
    "reservationTime": "2024-03-08T05:17:56.0355047-08:00",
    "expirationTime": "2024-03-08T05:47:56.0355047-08:00"
}
```

<br>

##### 4. PATCH: /api/appointment/{id}
To confirm the pending reservation. Use PATCH instead of PUT since it is idempotent. We only need to update the IsConfirmed attribute. The expiration time is set to 30 minutes after the Reservation time, once it is passed, it will expire and cannot be confirmed any longer, which means the slot will be released for a new reservation. The expiration time is saved to the database because it can be checked in each request. Alternatively, we may schedule a time on the service layer (C# code) to fire 30 minutes after the appointment is created, but I think it will be less stable than using the database, as it depends on the running of the application.  
  
HTTP status codes: 200 (OK, with the updated appointment), 404 (Not Found)  
  
##### Example (returning 200)  

request URL:  
  
http://localhost:5000/api/appointment/1
  
request payload:
```json
{
  "AvailabilityId": 2,
  "ClientId": 3
}
```
response payload:
```json
{
    "appointmentId": 1,
    "availabilityId": 2,
    "appointmentTime": "2024-03-15T09:15:00",
    "clientId": 3,
    "clientName": "C3",
    "isConfirm": true,
    "reservationTime": "2024-03-08T05:17:56.0355047-08:00",
    "expirationTime": "2024-03-08T05:47:56.0355047-08:00"
}
```

<br><br>

#### Implementation features
- Take advantage of .NET 8's web API features, including dependency injection, and incorporate the entity framework to handle data operations.
- CRUD to the database is expensive, so use asynchronous calls substantially.
- To handle the possible race condition at reservation, implement _context.Database.BeginTransactionAsync() to group a set of database operations to ensure atomicity.
- Use SQLite since it is lightweight and portable.
- Use DTO for data transfer to format the input and output, reducing the traffic. For example, a provider may provide a range of available start/end time pairs, and the service layer will parse and divide it into 15-minute slots.
- Return the 404 (Not Found) HTTP status code in negative use cases. This is mainly for security purposes so that we hide the actual error or error code from users. Use a custom exception called ReservationException for flows we expect to fail. This way, we can distinguish the expected exceptions from the unexpected ones, and control what message to send back to the client side, otherwise, only generic messages will be sent.
- All the positive and negative flows above have been verified by tests on Postman.

<br><br>
  
#### Future Considerations and Improvements

Due to time constraints, several areas can be considered for future improvements.
- Add authentication and authorization filters to enforce that only particular groups or users can call particular endpoint(s). For example, clients can only make appointments for themselves. Only providers can submit availabilities.
- It would be beneficial to engage in a detailed discussion regarding the integration and contractual agreements between the front-end and back-end systems, particularly if the front-end components are included within the scope of the delivery.
- Some business logic can be revisited and updated depending on the business needs. For example, if a provider provides a partially valid availability range, the valid parts will be honored. Alternatively, this can also be filtered on the client side. 
- Based on the business needs, for retrieving the availabilities, currently, a user can only search by provider ID. For functional improvement, there is potential to enable users to refine their queries by incorporating filters such as date range and provider name. For performance optimization, consider implementing features such as limiting results, introducing sorting options, and/or adding pagination capabilities.
- Add Unit tests and integration tests. It is straightforward to test controllers, services, and repositories because they are separated under the current architecture and implementation, with the dependency injection done by the .NET Core framework. In addition, consider mocking some of the dependencies (e.g. by using Moq) to test different parts.
- Consider implementing more parallelism and caching for scalability if the throughput is high in the real world.
