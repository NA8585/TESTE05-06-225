# Checklist de Dados Necessários para Overlay Radar

Esta checklist descreve os campos que devem ser enviados pelo backend para que a overlay **Radar** possa calcular a distância e a classe dos carros ao redor do jogador.

## Informações do Jogador
- **playerCarIdx** (int) – Índice do carro do jogador
- **playerCarClassID** (int) – Classe do carro do jogador
- **carSpeed** (float) – Velocidade atual do jogador
- **lap** (int) – Volta atual do jogador
- **carIdxLap** (int[]) – Array com a volta de cada carro
- **carIdxLapDistPct** (float[]) – Percentual percorrido de cada carro na volta

## Dados de Pista
- **trackDisplayName** (string) – Nome da pista
- **trackLength** (float) – Comprimento da pista em quilômetros

## Informações por Carro
- **carIdxCarClassIds** (int[]) – Classe de cada carro
- **carIdxCarClassEstLapTimes** (float[]) – Estimativa de tempo de volta por classe

Esses dados permitem que a overlay determine a posição relativa, identifique carros de classes diferentes e a proximidade em relação ao jogador. Caso algum campo esteja ausente, a overlay utiliza dados de exemplo, o que pode resultar em informações incorretas.
