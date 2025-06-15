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

## Composto e Stagger
- **tireCompound** / **compound** (string) – Composto atual do pneu do jogador
- **frontStagger** (float) – Stagger dianteiro calculado a partir das alturas de rodagem
- **rearStagger** (float) – Stagger traseiro calculado a partir das alturas de rodagem

Esses campos permitem que o frontend mostre pressões em tempo real, comparações com pressões frias e o desgaste acumulado de cada pneu.

As pressões quentes (`lfHotPressure`, `rfHotPressure`, `lrHotPressure` e `rrHotPressure`) são informadas em psi.

## Conversão de Pressão (kPa → psi)

Para converter as pressões quentes dos pneus obtidas em kPa para psi, utilize a fórmula:

```
psi = kPa × 0.1450377
```

Esta fórmula aplica-se especificamente às pressões quentes dos pneus.

Esse fator de conversão (0.1450377) equivale à relação entre 1 kPa e 1 psi.
