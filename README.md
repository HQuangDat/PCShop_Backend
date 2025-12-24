# PCShop_Backend

This is the backend for the PCShop application, a fully-featured e-commerce platform for computer components. It's built with ASP.NET Core and provides a RESTful API for all the functionalities of the shop.

## Features

*   **Authentication:** Secure user registration and login using JWT tokens.
*   **Product Management:** Manage a catalog of PC components, including categories and specifications.
*   **Shopping Cart:** A persistent shopping cart for each user.
*   **Order Management:** Create and manage orders, including receipts and order items.
*   **User Management:** Manage user accounts and roles.
*   **Support Tickets:** A support ticket system for users to get help.
*   **PC Builder:** A feature to build a custom PC from the available components.

## Technologies Used

*   **Framework:** ASP.NET Core 8
*   **Database:** Entity Framework Core with a relational database (e.g., SQL Server).
*   **Authentication:** JWT (JSON Web Tokens)
*   **API:** RESTful API with Swagger/OpenAPI documentation.
*   **Caching:** Redis for distributed caching to improve performance.
*   **Error Handling:** Global exception handling middleware.
*   **Validation:** Data validation using FluentValidation.

## API Endpoints

The API provides endpoints for the following resources:

*   `/api/Auth`: User authentication (login, register, reset password).
*   `/api/Products`: Manage products, categories, and PC builds.
*   `/api/Cart`: Manage the user's shopping cart.
*   `/api/Orders`: Create and manage orders.
*   `/api/Users`: Manage users and roles.
*   `/api/Support`: Manage support tickets.

For a detailed API documentation, you can run the project and navigate to `/swagger`.

## Getting Started

To get the project up and running, follow these steps:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/HQuangDat/PCShop_Backend.git
    ```
2.  **Configure the database:**
    *   Update the connection string in `appsettings.Development.json`.
    *   Run the database migrations:
        ```bash
        dotnet ef database update
        ```
3.  **Run the application:**
    ```bash
    dotnet run
    ```
4.  **Run Redis:**
    *   Make sure you have Docker installed.
    *   Run the following command to start a Redis container:
        ```bash
        docker-compose -f redis-docker-compose.yml up -d
        ```

Now you can access the API at `https://localhost:5001` (or the port configured in `launchSettings.json`).
