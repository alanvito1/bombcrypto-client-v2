# Relatório de Auditoria Técnica - Bombcrypto Game Client

## 1. Visão Geral do Projeto
Este projeto é o **Game Client** do jogo **Bombcrypto**, desenvolvido na engine **Unity 2022.3** com foco na plataforma **WebGL**.
Trata-se de uma aplicação cliente que depende estritamente de um backend (servidor) para lógica de negócios, validação e persistência de dados.

*   **Plataforma Alvo:** WebGL (Browser).
*   **Arquitetura:** Component-Entity System (Engine própria) + Service Locator.
*   **Integrações:** Web3 (Blockchain), Networking HTTP/Socket.

## 2. Dependência do Servidor (Client-Server)
A auditoria confirmou que este cliente **não funciona de forma autônoma**. Ele atua como uma interface visual para lógica processada remotamente.

*   **Evidências:**
    *   **API Manager (`DefaultApiManager.cs`):** O cliente realiza chamadas HTTP constantes para endpoints como `coin_balance`, `pvp-matching`, `ccu` (usuários online).
    *   **Autenticação:** O login e logout são gerenciados via comunicação com um wrapper React/Web (`Utils.KickToConnectScene`, `ReactCommand.LOGOUT`).
    *   **Blockchain:** Verificações de saldo e transações dependem de confirmação externa (`WaitForBalanceChange`).

**Conclusão:** Qualquer modificação na lógica de "ganhos" ou "regras de jogo" no cliente é apenas visual. A validação real ocorre no servidor. Para alterar o funcionamento do jogo, é necessário acesso e modificação no código do servidor (backend).

## 3. Estrutura e Principais Funções

### 3.1. Engine (`Assets/Scripts/Engine`)
Núcleo do gameplay. Implementa um sistema personalizado de Entidades e Componentes.
*   **`Entity.cs`:** Classe base para objetos do jogo. Gerencia estado de vida (`IsAlive`, `Kill`, `Resurrect`) e container de componentes.
*   **`IEntityComponent`:** Interface para comportamentos acopláveis às entidades (movimento, animação, colisão).
*   **Gerenciamento:** Utiliza `EntityManager` para controlar o ciclo de vida dos objetos na cena.

### 3.2. Serviços (`Assets/Scripts/Services`)
Camada de lógica de negócios e comunicação externa.
*   **`DefaultApiManager.cs`:** Centraliza as chamadas REST API. Define URLs de produção e teste.
*   **`Utils.cs`:** Contém lógica crítica de networking, formatação de dados (moedas, datas) e helpers de segurança (redação de dados sensíveis em logs).
*   **Managers:** Existem gerenciadores específicos para cada funcionalidade (`ShopManager`, `InventoryManager`, `HeroManager`), acessados via padrão **Service Locator**.

### 3.3. Integração Web3/Blockchain
*   Módulos para integração com carteiras (Wallet) e verificação de tokens.
*   Suporte a múltiplas redes/chains (pastas `Binance`, `Polygon`, `Solana`, `Ton`).

### 3.4. Inicialização (`Assets/Scripts/Main`)
*   **`MainLoader.cs`:** Ponto de entrada (Entry Point). Inicializa os serviços globais (`FirstClassServices`) antes de carregar a lógica da cena.

## 4. Oportunidades de Otimização e Melhorias

Após análise estática do código, foram identificados os seguintes pontos para melhoria de performance e manutenção:

### 4.1. Otimização de Performance (Crítico para WebGL)
1.  **Uso de `Resources.Load`:**
    *   *Problema:* O projeto utiliza `Resources.Load` em diversos pontos (ex: carregar prefabs de diálogos em `StoryMode`, configurações em `AppConfig`). Isso aumenta o uso de memória na inicialização e o tamanho do binário.
    *   *Sugestão:* Migrar para o sistema de **Addressables** da Unity, permitindo carregamento assíncrono e melhor gerenciamento de memória.

2.  **Chamadas `GetComponent` e `FindObjectOfType`:**
    *   *Problema:* Identificou-se uso de `FindObjectOfType` (lento) em scripts como `LevelScene.cs` e `TaskTonManager.cs`. Alguns componentes fazem `GetComponent` em tempo de execução.
    *   *Sugestão:* Cachear referências no método `Awake/Start` ou injetar dependências via Service Locator para evitar buscas na hierarquia durante o gameplay.

3.  **Parsing de JSON:**
    *   *Problema:* O uso de `JObject.Parse` (Newtonsoft) na thread principal pode causar "engasgos" (spikes) se as respostas da API forem grandes.
    *   *Sugestão:* Utilizar `JsonUtility` (nativo da Unity e mais rápido) onde possível, ou garantir que o parsing pesado seja feito fora da thread principal (embora difícil em WebGL sem Threads reais, pode-se usar Corrotinas fracionadas).

4.  **Polling de Saldo:**
    *   *Problema:* `Utils.WaitForBalanceChange` usa um loop com `Task.Delay` para verificar saldo.
    *   *Sugestão:* Implementar um sistema de notificação via Socket ou Eventos para atualizar o saldo apenas quando houver mudança real, reduzindo chamadas desnecessárias.

### 4.2. Melhorias de Código e Manutenção
1.  **Logs em Produção:**
    *   Existem chamadas de `Debug.Log` espalhadas (ex: `PingUtils.cs`, `BotMove.cs`). Estas devem ser removidas ou encapsuladas em um Logger condicional (`Debug.isDebugBuild`) para não poluir o console do navegador e economizar CPU.
2.  **Hardcoded Strings:**
    *   Endereços de API e caminhos de recursos estão "chumbados" no código. Mover para `ScriptableObjects` de configuração facilitaria a manutenção e troca de ambientes.

## 5. Conclusão da Auditoria
O projeto possui uma base sólida e modular (Service Pattern), adequada para um jogo WebGL de médio porte. No entanto, para escalar ou melhorar a fluidez em dispositivos móveis/web, recomenda-se fortemente a migração de `Resources` para `Addressables` e uma revisão nas chamadas de busca de componentes (`Find`/`Get`).

Lembre-se: **Modificações neste código alteram apenas a apresentação visual.** Regras de ganho, drop rates e economia estão seguras no servidor.
