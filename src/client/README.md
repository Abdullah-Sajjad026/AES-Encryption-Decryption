# AES Encryption Project - Frontend Client

This is a Vite + React application built with TypeScript and Tailwind CSS v4. It provides a user interface for the AES Encryption API.

## Features

- **Authentication**: Login and Registration forms with validation (Zod + React Hook Form).
- **Dashboard**: Secure file encryption (AES-GCM), decryption, and Google Drive integration.
- **State Management**: Uses React Query (TanStack Query) for efficient data fetching and caching.
- **Reactive Forms**: Form validation and submission handled by `react-hook-form`.

## Tech Stack

- **Framework**: [Vite](https://vitejs.dev/) + [React](https://react.dev/)
- **Language**: [TypeScript](https://www.typescriptlang.org/)
- **Styling**: [Tailwind CSS v4](https://tailwindcss.com/)
- **State Management**: [@tanstack/react-query](https://tanstack.com/query/latest) + [React Context](https://react.dev/reference/react/useContext)
- **HTTP Client**: [Axios](https://axios-http.com/)
- **Routing**: [React Router v6](https://reactrouter.com/en/main)
- **Validation**: [Zod](https://zod.dev/) + [React Hook Form](https://react-hook-form.com/)

## Project Structure

- **src/api**: Axios client configuration and interceptors.
- **src/components**: Reusable UI components.
- **src/context**: React Context providers (AuthContext).
- **src/pages**: Application pages (Login, Dashboard, Onboard).
- **src/hooks**: Custom React hooks.

## Environment Variables

Create a `.env` file in the root of the project to configure API endpoints:

```env
VITE_API_BASE_URL=http://localhost:5000/api/v1
```

## Running the Application

1.  **Install Dependencies**:
    ```bash
    npm install
    ```
2.  **Start Development Server**:
    ```bash
    npm run dev
    ```
    The application will run on `http://localhost:5173`.
3.  **Build for Production**:
    ```bash
    npm run build
    ```
    This generates static files in the `dist/` directory.

## Deployment

To deploy the frontend, build the project and serve the `dist` folder using a static file server (e.g., Nginx, Apache) or integrate it with the ASP.NET Core backend using `UseStaticFiles`.
