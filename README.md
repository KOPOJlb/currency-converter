1. How to run:
    - Open the project in your IDE.
    - Ensure that CurrencyConverter.Api is set as your startup project.
    - Select your preferred run option (I recommend IIS Express).
    - Run the project.
    - Use the following token to get access to the API:
    eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJhdWQiOiI5NzdmNmMxOC0yNzY4LTRmZTYtYTBjNS00MTA0MjNmODA5NjkiLCJyb2xlcyI6WyJsYXRlc3QiLCJjb252ZXJ0IiwibGlzdCJdLCJpc3MiOiJodHRwczovL2p3dC5pby8iLCJjbGllbnRfaWQiOiJ0ZXN0X2NsaWVudCJ9.bayaQmW0T8Y71NhlDN1MSu4somTT7hD5A6vt4O9Pek0

2. Assumptions made:
    - All configurations were made just to demonstrate functionality, not for production use.
    - Using a token without an expiration date (just for simplicity).
    - Using a fixed rate limiter for easy testing.
    - The time in the Frankfurter service and in my solution is exactly the same (which is not possible, but I need it for caching the latest rates. How to avoid this? This can be an enhancement option).
    - Avoid writing test cases for all possible scenarios.

3. Possible future enhancements:
    - The main suggestion from my side is to use a database on our side and create a synchronization mechanism to fetch new data from the service. This will allow us to build a reliable solution that doesn’t depend on an unstable third-party service.
    - Never use the latest endpoint. Always use a rate explicitly defined for a specific date.
    - In the current approach, each instance of the service maintains a separate copy of the cache, which is not efficient in terms of resources (we could consider caching using Redis).
    - Also, the current caching strategy is very simple and straightforward. We can consider a more intelligent or efficient caching strategy.