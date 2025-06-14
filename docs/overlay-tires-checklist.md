# Checklist de Dados Necessários para Overlay Tires

Esta checklist resume os dados de pneus que o backend deve enviar para que as overlays de pneus exibam informações corretas.

## Pressões Atuais
- **lfPress** (float) – Pressão do pneu dianteiro esquerdo
- **rfPress** (float) – Pressão do pneu dianteiro direito
- **lrPress** (float) – Pressão do pneu traseiro esquerdo
- **rrPress** (float) – Pressão do pneu traseiro direito

## Pressões Frias ao Sair do Pit
- **lfColdPress** (float) – Pressão fria registrada ao deixar o pit
- **rfColdPress** (float) – Pressão fria registrada ao deixar o pit
- **lrColdPress** (float) – Pressão fria registrada ao deixar o pit
- **rrColdPress** (float) – Pressão fria registrada ao deixar o pit

## Desgaste ou Sulco dos Pneus
- **lfTreadRemainingParts** (float[]) – Array com as partes do sulco restante do pneu dianteiro esquerdo
- **rfTreadRemainingParts** (float[]) – Array com as partes do sulco restante do pneu dianteiro direito
- **lrTreadRemainingParts** (float[]) – Array com as partes do sulco restante do pneu traseiro esquerdo
- **rrTreadRemainingParts** (float[]) – Array com as partes do sulco restante do pneu traseiro direito

## Últimas Pressões e Temperaturas Quentes
- **lfLastHotPress** (float) – Última pressão registrada com o pneu quente
- **rfLastHotPress** (float) – Última pressão registrada com o pneu quente
- **lrLastHotPress** (float) – Última pressão registrada com o pneu quente
- **rrLastHotPress** (float) – Última pressão registrada com o pneu quente
- **lfLastHotTemp** (float) – Última temperatura registrada com o pneu quente
- **rfLastHotTemp** (float) – Última temperatura registrada com o pneu quente
- **lrLastHotTemp** (float) – Última temperatura registrada com o pneu quente
- **rrLastHotTemp** (float) – Última temperatura registrada com o pneu quente

Esses campos permitem que o frontend mostre pressões em tempo real, comparações com pressões frias e o desgaste acumulado de cada pneu.
