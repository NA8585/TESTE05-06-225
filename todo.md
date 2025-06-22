# Refactoring Progress

- [x] backend/Program.cs
- [x] backend/Repositories/ICarTrackRepository.cs
- [x] backend/Services/CarTrackDataStore.cs
- [x] backend/Services/IRacingTelemetryService.cs
- [x] backend/Services/IRacingTelemetryService.Data.cs
- [x] backend/Services/IRacingTelemetryService.Calculations.cs
- [x] backend/Services/SessionYamlParser.cs
- [x] backend/Utilities/DataValidator.cs
- [x] backend/Services/TelemetryBroadcaster.cs
- [x] backend/Services/IRacingTelemetryService.DriverArrays.cs
- [x] backend/Services/IRacingTelemetryService.AllExtras.cs
- [x] backend/Services/IRacingTelemetryService.Persistence.cs
- [x] backend/Services/IRacingTelemetryService.Snapshot.cs
- [x] backend/Services/EnumTranslations.cs
- [x] backend/Models/DamageData.cs
- [x] backend/Models/DriverInfo.cs
- [x] backend/Models/FrontendDataPayload.cs
- [x] backend/Models/Proximity.cs
- [x] backend/Models/ResultPosition.cs
- [x] backend/Models/SessionData.cs
- [x] backend/Models/SessionDetailFromYaml.cs
- [x] backend/Models/SessionInfo.cs
- [x] backend/Models/SectorInfo.cs
- [x] backend/Models/TelemetryCalculations.cs
- [x] backend/Models/TelemetryCalculationsOverlay.cs
- [x] backend/Models/TelemetryExtras.Models.cs
- [x] backend/Models/TelemetryModel.cs
- [x] backend/Models/TyreData.cs
- [x] backend/Models/VehicleData.cs
- [x] backend/Models/WeekendInfo.cs
- [x] Review overall frontend architecture and component structure
- [x] Profile each overlay to measure CPU and memory usage
- [x] Create a shared render scheduler using `requestAnimationFrame`
- [x] Refactor WebSocket handling to batch messages per frame
- [x] Replace layout-changing styles with `transform` animations
 - [x] Audit event listeners and apply delegation where possible
- [x] Implement lazy loading or virtual scrolling for large lists
- [x] Migrate heavy calculations to Web Workers if needed
- [x] Optimize React overlays with `memo`, `useCallback`, and `useMemo`
- [x] Ensure CSS containment and hardware acceleration hints (`will-change`)
- [x] Review individual overlay files:
  - [x] `overlay-calculadora.html`
  - [x] `overlay-classificacao.html`
  - [x] `overlay-dadoscompletos.html`
  - [x] `overlay-delta.html`
  - [x] `overlay-diagnostico-raw.html`
  - [x] `overlay-inputs.html`
  - [x] `overlay-radar.html`
  - [x] `overlay-relative.html`
  - [x] `overlay-sessao.html`
  - [x] `overlay-standings.html`
  - [x] `overlay-tanque.html`
  - [x] `overlay-tiresandbrakes.html`
  - [x] `overlay-tiresgarage.html`
  - [x] `overlay-tiresnapshot.html`
  - [x] `overlay-tiresraw.html`
  - [x] `overlay-tiresyaml.html`
  - [x] `overlay-tirewear.html`
  - [x] `overlaybase.html`
  - [x] `supermegaultra.html`
- [x] Validate no visual regressions after optimizations
- [x] Document performance metrics before and after changes


## Optimization Plan Completed

## Refactor & Correction Roadmap

### Backend
- [x] Isolar logica de captura do IRSDK em um servico `TelemetryReader`
- [x] Aplicar configuracao de `UpdateInterval` via `appsettings.json`
 - [x] Simplificar `BuildTelemetryModelAsync` separando calculos em classes
 - [x] Revisar serializacao JSON e remover dados nao utilizados
 - [x] Implementar testes unitarios para `SessionYamlParser`
 - [x] Documentar estrutura dos payloads no README

- [x] Centralizar estado de telemetria em um `TelemetryContext`
- [x] Revisar cada overlay para consumir o contexto em vez de acessar `window`
 - [x] Extrair utilitarios de animacao para `utils/animation.js`
 - [x] Adicionar script `npm run lint` com configuracao do ESLint
 - [x] Criar checklist visual para validar cada overlay
