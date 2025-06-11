# Telemetry Overlays

This project contains a .NET backend that streams telemetry data through WebSockets and an Electron + React frontend used to display several overlays.

## Prerequisites

- **.NET 6 SDK** – required to build and run the backend
- **Node.js** with **npm** – required for the Electron/React frontend

## Running the backend

```bash
# from the repository root
 dotnet run --project backend/SuperBackendNR85IA.csproj
```

This starts the WebSocket server on `http://localhost:5221`. Overlays connect to the `/ws` endpoint on that port. You can provide a custom WebSocket URL to the overlays by setting the environment variable `OVERLAY_WS_URL` before launching the Electron app or by defining `window.OVERLAY_WS_URL` in a browser.

## Running the Electron frontend

```bash
cd telemetry-frontend
npm install
npm run dev
```

The `dev` script launches the backend, the Vite development server and Electron simultaneously. When Electron starts, a menu will appear listing the available overlays.

## Session YAML Exposure

Every WebSocket payload includes the raw `sessionInfoYaml` string from iRacing **and** several parsed objects derived from that YAML. The backend exposes these objects directly so overlays do not need to parse the YAML themselves. Available fields are:

- `yamlPlayerDriver` – information about the current player driver
- `yamlWeekendInfo` – track and weather details for the event
- `yamlSessionInfo` – data for the current session, including lap counts
- `yamlSectorInfo` – sector configuration and best sector times
- `yamlDrivers` – an array with basic info on all drivers

A sample payload is available in `ws/messages/overlay_message.json` and the latest YAML dump is written to `yamls/input_current.yaml` whenever the backend updates.

## Overlays

Each overlay corresponds to an HTML file in `telemetry-frontend/public/overlays` and can be opened from the Electron menu. A brief description of each overlay follows:

- **Inputs** – shows throttle, brake and other driver inputs.
- **Delta** – lap delta comparison bar.
- **Relative** – relative positions to nearby cars.
- **Sessao** – session dashboard with sector times.
- **Combustivel** – fuel calculator and usage display.
- **Tires & Freio** – tire temperatures and brake information with ABS/TC.
- **Tires Garage** – garage view of tire and brake data.
- **Standings** – table of race standings and session info.
- **Classificação** – shows full race classification with lap gaps.
- **Calculadora** – another compact fuel calculator.
- **Base** – basic classification template overlay.
- **Radar** – circular radar indicating nearby cars with alerts.
- **Teste Final** – diagnostic overlay combining various widgets.

To open an overlay, start the Electron application and click its name in the menu. Windows can be moved, pinned or closed individually.

## Tests

No automated tests are currently configured for this project. Running `npm test` in `telemetry-frontend` simply prints a placeholder message.
