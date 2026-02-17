# Stargate Web

Angular frontend for the Stargate Astronaut Career Tracking System (ACTS).

## Prerequisites

- Node.js (v18 or higher recommended)
- npm (comes with Node.js)

## Installation

Install dependencies:

```bash
npm install
```

## Configuration

The application uses environment files to configure the API connection:

- **Development** (`src/environments/environment.ts`): Configured for `http://localhost:5204`
- **Production** (`src/environments/environment.prod.ts`): Update `apiBaseUrl` for your production deployment

Edit these files directly to change the API base URL.

## Development Server

Run the development server:

```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Build

Build the project:

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory. By default, builds use the production configuration.

## Project Structure

```
src/
├── app/
│   ├── components/           # UI components
│   │   ├── astronaut-search/ # Search bar component
│   │   └── astronaut-duty-list/ # Duty list component
│   ├── models/              # TypeScript interfaces
│   ├── services/            # API services
│   └── app.*                # Root component
├── environments/            # Environment configuration files
└── styles.scss             # Global styles
```

## Running Tests

Run unit tests:

```bash
npm test
```