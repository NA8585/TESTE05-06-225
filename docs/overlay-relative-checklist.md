# Checklist de Dados Necessários para Overlay Relative

Este documento resume os campos que o backend deve enviar para que a overlay **Relative** funcione corretamente.

## Informações Gerais
- **playerCarIdx** (int) – Índice do carro do jogador
- **sessionNum** (int) – Número da sessão atual
- **sessionTime** (float) – Tempo total da sessão
- **sessionTimeRemain** (double) – Tempo restante da sessão
- **lap** (int) – Volta atual do jogador
- **totalLaps** (int) – Total de voltas da sessão (pode ser -1 se ilimitado)
- **trackDisplayName** (string) – Nome da pista
- **dcBrakeBias** (float) – Bias de freio do jogador

## Dados Ambientais
- **trackAirTemp** (float) – Temperatura do ar
- **trackSurfaceTemp** (float) – Temperatura da pista

## Arrays por CarIdx
Os seguintes arrays devem ter o mesmo tamanho e conter a informação de todos os carros na sessão.
- **carIdxPosition** (int[]) – Posição geral de cada carro
- **carIdxLap** (int[]) – Volta atual de cada carro
- **carIdxLapDistPct** (float[]) – Percentual percorrido na volta de cada carro
- **carIdxOnPitRoad** (bool[]) – Indica se o carro está no pit
- **carIdxTrackSurface** (int[]) – Estado da superfície do carro (0 a 7)
- **carIdxLastLapTime** (float[]) – Último tempo de volta
- **carIdxF2Time** (float[]) – Diferença de tempo F2 para o jogador
- **carIdxCarClassEstLapTimes** (float[]) – Tempo estimado de volta de cada classe

> **Importante:** verifique no frontend se o tamanho dos arrays é adequado antes de acessar os valores para evitar erros.

## YAML da Sessão
O campo **sessionInfoYaml** deve conter a string completa retornada por `GetSessionInfo()` do iRacing. Dela podem ser extraídos dados como:
- `DriverInfo.Drivers[i]` com `CarIdx`, `UserName`, `CarNumberRaw`, `IRating`, `LicString`, `CarClassShortName` e `CarClassEstLapTime`.
- `WeekendInfo.TrackDisplayName` e `WeekendInfo.TrackLength`.
- `WeekendInfo.NumCarClasses` e `WeekendInfo.IncidentLimit`.
- `SessionInfo.Sessions[i].SessionType` e outros detalhes de cada sessão.

Sempre que o YAML mudar, o backend deve atualizar este campo para que a overlay possa reaproveitar as novas informações.

## Dicas
- Mantenha coerência entre `sessionTime`, temperaturas, voltas e o restante da sessão.
- Campos derivados do YAML podem ser enviados já processados para simplificar a lógica no frontend.
