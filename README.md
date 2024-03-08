# reservation-api

coding challenge (March, 2024)

Programming language: C#
Project type: .NET Web API
Target framework: .NET 8.0
Database: SQLite

Design:
follow the controller-service-repository architecture with the entity framework

Endpoints:
1. POST:  /api/availability
For adding availabilities. Although the a slot is 15-minute log, a provider doesn't have to send multiple requests (each with a single slot), or a list of slots in one request. The code is designed to accept a list of consecutive time ranges (e.g. from 9 am to 5 pm on 3/8/2024). The service layer slices the ranges into 15-minute slots. The sanity checks ensure the valid slots start from the 0th, 15th, 30th, or 45th minute in an hour, an existing slot will not be added repeatedly, and the slots need to be at future times.
HTTP status code: 201 (Created, with the list of slots created), 404 (Not Found)
Example:
http://localhost:5000/api/availability
request:
{
  "ProviderId": 1,
  "AvailabilityRanges": [
    {
      "StartTime": "2024-03-26T09:00:00",
      "EndTime": "2024-03-26T09:45:00"
    }
  ]
}
response:
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

2. GET:   /api/availability/{pid}
Pid is the provider ID. Assume that the client is interested in a particular provider. See possible enhancements for this endpoint in the last section.
HTTP status code: 200 (Ok, with the list of available slots), 204 (No Content), 404 (Not Found)
Example:
http://localhost:5000/api/availability/1

response:
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

3. POST:  /api/appointment
For making an appointment by providing an available appointment slot. Reservation needs to be 24 hours in advance compared to the slot start time. If a slot has already been confirmed or is pending for confirmation (not expired), it will be unavailable for a new reservation, unless the unconfirmed slot expires after 30 minutes. Apply BeginTransactionAsync to mitigate the race condition. Use "BeginTransactionAsync" over "lock" because it is related to database operations.
HTTP status code: 201 (Created, with the created appointment), 404 (Not Found)
Example:
http://localhost:5000/api/appointment
request
{
  "AvailabilityId": 2,
  "ClientId": 3
}
response
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

4. PATCH: /api/appointment/{id}
To confirm the pending reservation. Using PATCH instead of PUT since it is idempotent and we only need to update the IsConfirmed attribute. The expiration time is set to 30 minutes after the Reservation time, once it is passed, it will expire and cannot be confirmed any longer, which means the slot will be released for a new reservation. The expiration time is saved to the database because it can be checked in each request. Alternatively, we may schedule a time on the service layer (C# code) to fire 30 minutes after the appointment is created, but I think it will not be as stable as using the database, as it depends on the running of the application.
HTTP status code: 200 (Ok, with the updated appointment), 404 (Not Found)
Example
http://localhost:5000/api/appointment/1
request:
{
  "AvailabilityId": 2,
  "ClientId": 3
}
response:
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

Features:
- Take advantage of .NET 8's web API features, including dependency injection, and incorporate the entity framework to handle data operations.
- CRUD to the database is expensive, so use asynchronous calls substantially.
- To handle the possible race condition at reservation, implement _context.Database.BeginTransactionAsync() to group a set of database operations to ensure atomicity.
- Use SQLite since it is lightweight and portable.
- Use DTO for data transfer to format the input and output, reducing the traffic. For example, a provider may just provide a range of available start/end time pairs, and the service layer will parse and divide it into 15-minute slots.
- For HTTP status codes in the negative flows, use the 404 (Not Found) error for different scenarios. This is mainly for security purposes so that we hide the actual error or error code from users. Customize the exception called ReservationException so that for flows we expect to fail, so that we can control what message to send back to the client side, otherwise, only generic messages will be sent.
- All the positive and negative flows above have been verified by tests on Postman.


Future Considerations and Improvements:
Due to time constraints, several areas can be considered for future improvements.
- Add authentication and/or authorization filters so that only a particular group or user can call some endpoint. For example, clients can only make appointments for themselves. Only providers can submit availabilities.
- If frontend is also involved, it would be ideal to discuss the contracts between frontend and backend.
- Based on the business needs, some business logic can be revisited and updated. For example, if the provider provides a partially valid availability range, the valid parts may be honored. But this needs to be filtered on the client side. 
- Based on the business needs, for availability retrieval, currently user can only search by provider ID. In the future, users can combine with date range and/or provider name. For performance enhancement, limit, sorting, and/or pagination may also be implemented.
- Consider Unit tests and integration tests. The controller, service, and repository parts are separated and the dependency injection is done by the framework, and also consider mocking some of the dependencies (e.g. by using Moq) to test different parts.
