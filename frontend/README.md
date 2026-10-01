# Vehicle Management: React frontend

A React + TypeScript app, built with Vite. It talks to the backend only through the REST API in
[docs/api-contract.md](../docs/api-contract.md).

## Requirements

- Node.js 24 LTS (with npm)
- The backend API running on `http://localhost:5080` (see the main README)

## Run

```bash
cd frontend
npm install
npm run dev
```

The app opens on `http://localhost:5173`. It calls the API with relative paths such as `/api/vehicles`; in
development, Vite forwards anything under `/api` to `http://localhost:5080` (see `vite.config.ts`). So the app code
never contains the backend's address, and the backend needs no knowledge of the frontend.

In VS Code, "Backend + frontend" in the Run and Debug panel starts both.

## How it's organised

| Folder | What's in it |
|---|---|
| `src/api/` | `types.ts` (the JSON shapes), `client.ts` (calls the API, turns errors into `ApiError`), `vehicleManagementApi.ts` (one function per REST endpoint). The only place that knows the API's paths |
| `src/vehicles/` | The Vehicles tab: the sortable list and the add-vehicle form, which previews the category as the weight is typed |
| `src/categories/` | The Categories tab: the list with edit (name and icon) and delete, and the add-category form |
| `src/components/` | Shared pieces: category icon, icon picker, error messages |
| `App.tsx` | The page title and the two tabs |

The server checks every rule. Its messages are shown next to the field they're about, and messages that aren't about
one field (such as a gap between categories) are shown above the button. Icons are shown through an `<img>`, so an
SVG can't run scripts.

## Commands

| Command | What it does |
|---|---|
| `npm run dev` | Starts the app with instant reload |
| `npm run lint` | Checks the code for common mistakes (oxlint) |
| `npm run build` | Type-checks and builds the production files into `dist/` |
