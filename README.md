# NNPRO E-Learning System

E-learningová platforma postavená na **ASP.NET Core (.NET 10)**, **Vue.js (SPA)**, **MariaDB**, **Redis** a reverzní proxy **Caddy** běžící v Dockeru.

## 1. Architektura a přehled prostředí

Projekt využívá sdílený základ **`docker-compose.base.yaml`**, ze kterého pomocí klíče `extends` dědí jednotlivé spustitelné konfigurace podle účelu použití:

| Soubor | Účel | Spuštěné služby |
| :--- | :--- | :--- |
| `docker-compose.dev.backend.yaml` | Lokální vývoj backendu v IDE (pouze DB + Redis v Dockeru) | `db`, `redis` |
| `docker-compose.dev.frontend.yaml` | Lokální vývoj frontendu (`npm run dev` na `:5173`, celý backend stack v Dockeru) | `db`, `redis`, `backend` (Development, HTTP `:8080`) |
| `docker-compose.yaml` | Kompletní vývojový stack v Dockeru včetně HTTPS proxy | `caddy`, `frontend`, `backend` (Development), `db`, `redis` |
| `docker-compose.prod.yaml` | Ostré produkční nasazení na serveru (Azure) | `caddy`, `frontend`, `backend` (2× Production), `db`, `redis` |

> **Poznámka k `docker-compose.base.yaml`:** Základní soubor definuje společné parametry služeb (image, healthchecky, interní síť `app_net`, připojení `dp_keys` a výchozí proměnné prostředí). Nespouští se samostatně — jednotlivá prostředí z něj dědí a doplňují pouze své rozdíly (mapování portů, počet replik, `target` buildu, `depends_on` a deklarace volumes).

### Síťová architektura a TLS

* **Jediný vstupní bod (Caddy):** V konfiguracích `docker-compose.yaml` a `docker-compose.prod.yaml` vystavuje porty `80/tcp`, `443/tcp` a `443/udp` reverzní proxy **Caddy**. V produkci automaticky získává a obnovuje certifikáty od **Let's Encrypt**; při lokálním spuštění (`DOMAIN=localhost`) si automaticky generuje vlastní lokální certifikát přes interní CA.
* **Interní komunikace po HTTP:** Uvnitř privátní Docker sítě `app_net` komunikují služby čistě po HTTP (backend na portu `8080`) — v žádném Docker Compose prostředí není potřeba generovat ani připojovat `.pfx` certifikáty.
* **Load balancing:** V `docker-compose.prod.yaml` Caddy rozkládá požadavky na `/api/*` (**round-robin**) mezi **2 repliky backendu**, zatímco ostatní cesty (`/*`) směruje na **Nginx** kontejner s Vue.js aplikací.

## 2. Prerekvizity a konfigurace prostředí

Všechny Docker příkazy spouštěj z kořene repozitáře. Před prvním spuštěním vytvoř konfigurační soubor `.env` ze sdílené šablony (používá se jak pro lokální vývoj, tak pro produkci):

```bash
cp .env.example .env
```

> **Poznámka k `.env`:** Soubor `.env` nepodporuje interpolaci proměnných — pokud změníš `DB_PASSWORD`, nezapomeň stejné heslo aktualizovat také uvnitř `DOCKER_DB_CONNECTION_STRING` (`Password=...`) a případně i v `ConnectionStrings__DefaultConnection`. Podpisový klíč `JwtOptions__SecretKey` musí mít alespoň 64 znaků.

* **Vývoj v Dockeru bez certifikátů:** Pro spuštění `docker-compose.yaml` ani žádného ze souborů `docker-compose.*.yaml` není nutné vytvářet žádné SSL certifikáty.
* **Vývoj přímo na hostiteli (mimo Docker):** Frontend (`npm run dev`) běží lokálně čistě přes HTTP (`http://localhost:5173`) a nevyžaduje žádný certifikát. Kořenový `.env` obsahuje sdílené proměnné pro Docker (`DOCKER_DB_CONNECTION_STRING`, `REDIS_DOCKER_CONNECTION`), produkční proměnné pro Caddy (`DOMAIN`, `ACME_EMAIL`) i lokální `ConnectionStrings__*` na `localhost` pro běh backendu z IDE. Vývojový profil v `appsettings.Development.json` má již povolený CORS jak pro Vite (`http://localhost:5173`, `https://localhost:5173`), tak pro lokální Caddy proxy (`https://localhost`). Pokud spouštíš backend přímo z IDE přes HTTPS profil, vygeneruj si vývojový certifikát příkazem:
  ```bash
  dotnet dev-certs https --trust
  ```

## 3. Spuštění jednotlivých prostředí

> **Poznámka:** Při přepínání mezi různými režimy vždy nejprve ukonči předchozí stack (`docker compose down` nebo `docker compose -f <predchozi-soubor> down`), aby nedošlo ke kolizi portů nebo názvů kontejnerů.

### A) Lokální vývoj backendu v IDE (`docker-compose.dev.backend.yaml`)

Spustí pouze databázi MariaDB a Redis v Dockeru, aby bylo možné spouštět a ladit backend přímo z IDE na `http://localhost:8080` a frontend přes `npm run dev` na `http://localhost:5173`:

```bash
docker compose -f docker-compose.dev.backend.yaml up -d
```

```mermaid
flowchart LR
    vite("<b>Frontend (Vite na hostiteli)</b><br/>http://localhost:5173<br/>npm run dev (HTTP)"):::frontend
    backend("<b>Backend (.NET 10 v IDE)</b><br/>http://localhost:8080<br/>Scalar: / -> /scalar"):::backend

    subgraph app_net["Docker Host — interní síť: app_net (bridge)"]
        direction LR
        db("<b>db</b><br/>mariadb:12.3<br/>Port hostitele: 3306:3306"):::db
        redis("<b>redis</b><br/>redis:7-alpine<br/>Port hostitele: 127.0.0.1:6379"):::redis

        mariadb_data[("<b>mariadb_data</b><br/>/var/lib/mysql<br/>+ ./database/init (ro)")]:::vol
        redis_data[("<b>redis_data</b><br/>/data")]:::vol

        mariadb_data -.- db
        redis -.- redis_data
    end

    vite -->|"HTTP :8080 (/api/*)"| backend
    backend -->|"TCP :3306"| db
    backend -->|"TCP :6379"| redis

    style app_net fill:#f8f9fa,stroke:#666666,stroke-dasharray: 5 5,color:#1f2328
    classDef frontend fill:#fff2cc,stroke:#d6b656,color:#1f2328
    classDef backend fill:#e1d5e7,stroke:#9673a6,color:#1f2328
    classDef db fill:#f8cecc,stroke:#b85450,color:#1f2328
    classDef redis fill:#ffe6cc,stroke:#d79b00,color:#1f2328
    classDef vol fill:#f5f5f5,stroke:#666666,color:#1f2328
```

### B) Lokální vývoj frontendu (`docker-compose.dev.frontend.yaml`)

Spustí celý backendový stack (`db`, `redis`, `backend` v režimu `Development` na HTTP portu `8080`) pro lokální vývoj frontendu přes `npm run dev` na `http://localhost:5173`. Databáze a Redis zůstávají přístupné pouze uvnitř sítě `app_net`, takže nekolidují s případnými lokálními instalacemi na hostiteli:

```bash
docker compose -f docker-compose.dev.frontend.yaml up -d --build
```

```mermaid
flowchart LR
    vite("<b>Frontend (Vite na hostiteli)</b><br/>http://localhost:5173<br/>npm run dev (HTTP)"):::frontend
    ide("<b>Vývojář / Prohlížeč</b><br/>Scalar: http://localhost:8080/"):::client

    subgraph app_net["Docker Host — interní síť: app_net (bridge)"]
        direction LR
        backend("<b>backend</b><br/>.NET 10 (Development)<br/>Kestrel HTTP :8080<br/>Port hostitele: 8080:8080"):::backend
        db("<b>db</b><br/>mariadb:12.3<br/>Interní port: 3306"):::db
        redis("<b>redis</b><br/>redis:7-alpine<br/>Interní port: 6379"):::redis

        dp_keys[("<b>dp_keys</b><br/>/home/app/dp_keys")]:::vol
        mariadb_data[("<b>mariadb_data</b><br/>/var/lib/mysql<br/>+ ./database/init (ro)")]:::vol
        redis_data[("<b>redis_data</b><br/>/data")]:::vol

        backend -->|"TCP :3306"| db
        backend -->|"TCP :6379"| redis
        backend -.- dp_keys
        mariadb_data -.- db
        redis -.- redis_data
    end

    vite -->|"HTTP :8080 (/api/*)"| backend
    ide -.->|"HTTP :8080 (/ -> /scalar)"| backend

    style app_net fill:#f8f9fa,stroke:#666666,stroke-dasharray: 5 5,color:#1f2328
    classDef client fill:#dae8fc,stroke:#6c8ebf,color:#1f2328
    classDef frontend fill:#fff2cc,stroke:#d6b656,color:#1f2328
    classDef backend fill:#e1d5e7,stroke:#9673a6,color:#1f2328
    classDef db fill:#f8cecc,stroke:#b85450,color:#1f2328
    classDef redis fill:#ffe6cc,stroke:#d79b00,color:#1f2328
    classDef vol fill:#f5f5f5,stroke:#666666,color:#1f2328
```

### C) Kompletní vývojový stack (`docker-compose.yaml`)

Spustí celý stack včetně reverzní proxy Caddy (`https://localhost/`), Nginx frontendu a backendu v režimu `Development`:

```bash
docker compose up -d --build
```

```mermaid
flowchart LR
    client("<b>Klient / Prohlížeč</b><br/>https://localhost<br/>JWT + HttpOnly Secure Cookie"):::client
    ide("<b>Vývojář / IDE</b><br/>Přímý přístup na hostiteli<br/>Scalar :8080 | DB :3306 | Redis :6379"):::client

    subgraph app_net["Docker Host — interní síť: app_net (bridge)"]
        direction LR
        caddy("<b>caddy</b><br/>caddy:2-alpine<br/>TLS (Interní CA) + Reverse Proxy<br/>Porty: 80, 443, 443/udp"):::caddy
        frontend("<b>frontend</b><br/>Vue.js SPA (nginx)<br/>Interní port: 8080 (HTTP)"):::frontend
        backend("<b>backend</b><br/>.NET 10 (Development)<br/>Kestrel HTTP :8080 (host :8080)<br/>ForwardedHeaders + JWT"):::backend
        db("<b>db</b><br/>mariadb:12.3<br/>Interní :3306 (host :3306)"):::db
        redis("<b>redis</b><br/>redis:7-alpine<br/>Interní :6379 (host 127.0.0.1:6379)"):::redis

        caddy_vol[("<b>Volumes & Config</b><br/>caddy_data (/data)<br/>caddy_config (/config)<br/>./Caddyfile (ro)")]:::vol
        dp_keys[("<b>dp_keys</b><br/>/home/app/dp_keys")]:::vol
        mariadb_data[("<b>mariadb_data</b><br/>/var/lib/mysql<br/>+ ./database/init (ro)")]:::vol
        redis_data[("<b>redis_data</b><br/>/data")]:::vol

        caddy -->|"HTTP :8080 (/*)"| frontend
        caddy -->|"HTTP :8080 (/api/*)<br/>X-Forwarded-Proto: https"| backend
        backend -->|"TCP :3306"| db
        backend -->|"TCP :6379"| redis

        caddy -.- caddy_vol
        backend -.- dp_keys
        mariadb_data -.- db
        redis -.- redis_data
    end

    client -->|"HTTPS :443<br/>HTTP :80"| caddy
    ide -.->|"HTTP :8080 / TCP :3306, :6379"| backend

    style app_net fill:#f8f9fa,stroke:#666666,stroke-dasharray: 5 5,color:#1f2328
    classDef client fill:#dae8fc,stroke:#6c8ebf,color:#1f2328
    classDef caddy fill:#d5e8d4,stroke:#82b366,color:#1f2328
    classDef frontend fill:#fff2cc,stroke:#d6b656,color:#1f2328
    classDef backend fill:#e1d5e7,stroke:#9673a6,color:#1f2328
    classDef db fill:#f8cecc,stroke:#b85450,color:#1f2328
    classDef redis fill:#ffe6cc,stroke:#d79b00,color:#1f2328
    classDef vol fill:#f5f5f5,stroke:#666666,color:#1f2328
```

### D) Ostré produkční nasazení (`docker-compose.prod.yaml`)

1. Vytvoř konfigurační soubor `.env` na serveru, v produkční sekci vyplň reálnou doménu (`DOMAIN`), e-mail pro Let's Encrypt (`ACME_EMAIL`) a ve sdílené části silná hesla a podpisový JWT klíč (CORS origin `https://${DOMAIN}` se propisuje automaticky):
   ```bash
   cp .env.example .env
   ```
2. Ověř, že ve firewallu / Azure Network Security Group jsou otevřené příchozí porty **`80/tcp`**, **`443/tcp`** a **`443/udp`** a DNS záznam `DOMAIN` směřuje na IP adresu serveru.
3. Spusť produkční stack (Docker Compose načte soubor `.env` automaticky):
   ```bash
   docker compose -f docker-compose.prod.yaml up -d --build
   ```

```mermaid
flowchart LR
    client("<b>Klient / Prohlížeč</b><br/>https://${DOMAIN}<br/>JWT + HttpOnly Secure Cookie"):::client

    subgraph app_net["Docker Host — interní síť: app_net (bridge)"]
        direction LR
        caddy("<b>caddy</b><br/>caddy:2-alpine<br/>TLS (Let's Encrypt) + Load Balancer<br/>Porty: 80, 443, 443/udp"):::caddy
        frontend("<b>frontend</b><br/>Vue.js SPA (nginx)<br/>Interní port: 8080 (HTTP)"):::frontend
        backend("<b>backend (2× repliky)</b><br/>.NET 10 (Production)<br/>Kestrel HTTP :8080 (Round-Robin)<br/>ForwardedHeaders + JWT"):::backend
        db("<b>db</b><br/>mariadb:12.3<br/>Interní port: 3306"):::db
        redis("<b>redis</b><br/>redis:7-alpine (AOF)<br/>Interní port: 6379"):::redis

        caddy_vol[("<b>Volumes & Config</b><br/>caddy_data (/data)<br/>caddy_config (/config)<br/>./Caddyfile (ro)")]:::vol
        dp_keys[("<b>dp_keys (sdílený)</b><br/>/home/app/dp_keys")]:::vol
        mariadb_data[("<b>mariadb_data</b><br/>/var/lib/mysql<br/>+ ./database/init (ro)")]:::vol
        redis_data[("<b>redis_data</b><br/>/data")]:::vol

        caddy -->|"HTTP :8080 (/*)"| frontend
        caddy -->|"HTTP :8080 (/api/*)<br/>X-Forwarded-Proto: https"| backend
        backend -->|"TCP :3306"| db
        backend -->|"TCP :6379"| redis

        caddy -.- caddy_vol
        backend -.- dp_keys
        mariadb_data -.- db
        redis -.- redis_data
    end

    client -->|"HTTPS :443<br/>HTTP :80"| caddy

    style app_net fill:#f8f9fa,stroke:#666666,stroke-dasharray: 5 5,color:#1f2328
    classDef client fill:#dae8fc,stroke:#6c8ebf,color:#1f2328
    classDef caddy fill:#d5e8d4,stroke:#82b366,color:#1f2328
    classDef frontend fill:#fff2cc,stroke:#d6b656,color:#1f2328
    classDef backend fill:#e1d5e7,stroke:#9673a6,color:#1f2328
    classDef db fill:#f8cecc,stroke:#b85450,color:#1f2328
    classDef redis fill:#ffe6cc,stroke:#d79b00,color:#1f2328
    classDef vol fill:#f5f5f5,stroke:#666666,color:#1f2328
```

## 4. Dostupné služby a endpointy

### Vývojová prostředí (`docker-compose.yaml`, `docker-compose.dev.frontend.yaml` a `docker-compose.dev.backend.yaml`)

* **Aplikace přes Caddy (pouze `docker-compose.yaml`):** `https://localhost/` (API na `https://localhost/api/...`)
* **Backend API (přímý přístup):** `http://localhost:8080/api/...` *(otevření kořenové cesty `http://localhost:8080/` v režimu Development automaticky přesměruje na dokumentaci `/scalar`)*
* **OpenAPI JSON:** `http://localhost:8080/openapi/v1.json` *(pouze Development)*
* **Scalar dokumentace:** `http://localhost:8080/scalar` *(pouze Development; dostupné i přes přesměrování z `/`)*
* **Health check:** `http://localhost:8080/health`
* **MariaDB:** `127.0.0.1:3306` *(pouze `docker-compose.yaml` a `docker-compose.dev.backend.yaml`)*
* **Redis:** `127.0.0.1:6379` *(pouze `docker-compose.yaml` a `docker-compose.dev.backend.yaml`)*

### Produkční prostředí (`docker-compose.prod.yaml`)

* **Frontend (Vue.js SPA):** `https://<DOMAIN>/`
* **Backend API (Load Balanced):** `https://<DOMAIN>/api/...`
* Backend (`8080`), databáze ani Redis nejsou vystaveny mimo interní síť `app_net`.

## 5. Autentizace a správa relací

Backend využívá krátkodobé JWT access tokeny (výchozí platnost 10 minut) a dlouhodobé refresh tokeny (1 den). Hodnoty lze upravit v `appsettings.json`. Podpisový klíč a identita tokenu se konfigurují přes proměnné prostředí (`JwtOptions__SecretKey`, `JwtOptions__Issuer`, `JwtOptions__Audience`). Podpisový klíč musí mít z bezpečnostních důvodů alespoň 64 znaků.

### Auth API Endpointy

| Metoda | Endpoint | Účel |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Registrace nového účtu (heslo min. 8 znaků). |
| `POST` | `/api/auth/login` | Přihlášení přes uživatelské jméno nebo e-mail a heslo. Pokud při loginu pošleš dosavadní access token jako Bearer, jeho relace se po úspěšném loginu ukončí. |
| `POST` | `/api/auth/refresh` | Obnova access tokenu a rotace refresh cookie; tělo požadavku se neposílá. |
| `POST` | `/api/auth/logout` | Odhlášení aktuální relace. |

### Integrace s frontendem a bezpečnost

Access token backend vrací v JSON pod `data.accessToken`. Frontend ho drží pouze v paměti a posílá jako Bearer token:

```javascript
headers: { Authorization: `Bearer ${accessToken}` }
```

Refresh token backend nastavuje jako `HttpOnly; Secure; SameSite=Lax` cookie s cestou `/api/auth`; v databázi uchovává pouze jeho hash. JavaScript cookie nepřečte. U *cross-origin* požadavků použij ve `fetch` `credentials: "include"`, v Axiosu `withCredentials: true` (login, registrace, refresh i logout). CORS povoluje credentials jen nakonfigurovaným originům.

Po obnovení stránky frontend zavolá `/api/auth/refresh` a access token uloží z odpovědi. Při `401` z chráněného API může frontend jednou obnovit token a původní požadavek zopakovat; pokud refresh vrátí `401`, musí uživatele odhlásit. Refresh endpoint vyžaduje hlavičku `Origin` odpovídající nakonfigurovanému originu nebo originu samotného API.

Refresh tokeny se při obnově rotují. Opětovné použití spotřebovaného tokenu zneplatní jeho tokenovou rodinu (konkrétní relaci), ne ostatní relace uživatele. Access tokeny se při revokaci blokují v Redis do konce jejich platnosti.

Cookie s `SameSite=Lax` funguje pro frontend a API na stejné doméně/`site` (v produkci i v `docker-compose.yaml` běží obojí pod jednou originou `https://<DOMAIN>` / `https://localhost` přes Caddy; při lokálním vývoji přes Vite na `http://localhost:5173` proti portu `8080` jde stále o stejný `site` `localhost` a moderní prohlížeče považují `localhost` za důvěryhodný kontext i pro `Secure` cookies). Pro *cross-origin* vývoj je potřeba povolit přesný origin frontendu v `CorsSettings:AllowedOrigins`.

### Administrátorské funkce (Role: Admin)

Administrátoři mohou spravovat relace uživatelů:

* `GET /api/admin/users/{userId}/sessions` – Výpis všech aktivních relací uživatele.
* `DELETE /api/admin/users/{userId}/sessions/{sessionId}` – Ukončení jedné konkrétní relace (`sessionId` odpovídá ID rodiny tokenů z výpisu).
* `DELETE /api/admin/users/{userId}/sessions` – Okamžité odhlášení uživatele ze všech zařízení.

### Vývojové prostředí (Seeding)

Při spuštění v režimu `Development` se automaticky vytvoří testovací účty (`admin/admin`, `student/student`, `instructor/instructor`, `student2/student2`, `instructor2/instructor2`) a ukázkové kurzy. Tento seed se v produkčním prostředí nespouští.

## 6. Důležité poznámky pro vývojáře a provoz

* **Automatické EF Core migrace a bezpečný seeding (Development):** Při startu v režimu `Development` backend automaticky aplikuje EF Core migrace (`dbContext.Database.MigrateAsync()`) a naplní testovací data přes `DbInitializer` chráněný databázovým advisory lockem `GET_LOCK` a transakcí.
* **Bezstavový backend (2 repliky) a komprese odpovědí:** Obě produkční repliky backendu sdílejí volume `dp_keys` (`/home/app/dp_keys` nakonfigurovaný v `docker-compose.base.yaml` pro DataProtection klíče) a veškerý sdílený stav ukládají do **Redis** a **MariaDB**. Backend automaticky komprimuje odpovědi pomocí **Brotli** a **Gzip** (`CompressionLevel.Fastest`, včetně `application/problem+json`).
* **MariaDB verze:** Databáze používá image `mariadb:12.3` s příznakem `MARIADB_AUTO_UPGRADE=1` pro bezproblémové aktualizace systémových tabulek databáze.
* **Lokální self-signed certifikát v prohlížeči:** Při lokálním spuštění přes Caddy (`docker-compose.yaml` na `https://localhost`) prohlížeč zobrazí varování před lokálním certifikátem vygenerovaným Caddy — stačí potvrdit pokračování na stránku (případně lze v Chrome/Edge zapnout `chrome://flags/#allow-insecure-localhost`).
* **Git:** Soubor `.env` obsahuje citlivé údaje (hesla k databázi, podpisový JWT klíč) a nesmí být součástí Git repozitáře.