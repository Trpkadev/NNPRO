# NNPRO E-Learning System

E-learningová platforma postavená na **ASP.NET Core (.NET 10)**, **Vue.js (SPA)**, **MariaDB**, **Redis** a reverzní proxy **Caddy** běžící v Dockeru.

---

## 1. Architektura a přehled prostředí

Projekt je rozdělen do čtyř samostatných Docker Compose konfigurací podle účelu použití:

| Soubor | Účel | Spuštěné služby |
| :--- | :--- | :--- |
| `docker-compose.dev.backend.yaml` | Vývoj a ladění backendu přímo v IDE (Rider / VS) | `db`, `redis` |
| `docker-compose.dev.yaml` | Lokální vývoj (zejména práce na frontendu) | `db`, `redis`, `backend` (Development) |
| `docker-compose.prod.local.yaml` | Lokální test kompletní produkční architektury | `caddy`, `frontend`, `backend` (2× Production), `db`, `redis` |
| `docker-compose.prod.yaml` | Ostré produkční nasazení na serveru (Azure) | `caddy`, `frontend`, `backend` (2× Production), `db`, `redis` |

### Produkční síťová architektura
* Jediným vstupním bodem z internetu je **Caddy** (`80/tcp`, `443/tcp`, `443/udp`), který automaticky zajišťuje HTTPS certifikáty přes **Let's Encrypt** (v lokálním produkčním režimu přes vlastní interní CA).
* Uvnitř privátní Docker sítě `app_net` komunikují všechny kontejnery čistě po HTTP bez interních certifikátů.
* Požadavky na `/api/*` Caddy rozkládá (**round-robin load balancing**) mezi **2 repliky backendu**, ostatní cesty (`/*`) směruje na **Nginx** kontejner s Vue.js aplikací.

---

## 2. Prerekvizity pro lokální vývoj (HTTPS certifikát)

Vývojový certifikát `./Backend/aspnetapp.pfx` je vyžadován **pouze** při spouštění `docker-compose.dev.yaml` (nebo při běhu backendu lokálně v IDE). Pro frontend ani pro produkční sestavení (`prod` / `prod.local`) není potřeba.

V kořenovém adresáři projektu vygeneruj lokální certifikát (soubor je v `.gitignore`):

```bash
dotnet dev-certs https --trust -ep ./Backend/aspnetapp.pfx -p nnpro
```

### Windows
* Dvojklikem otevři vygenerovaný certifikát a potvrď systémové okno pro přidání mezi důvěryhodné certifikáty (případně restartuj prohlížeč/systém).

### Linux (Mint / Ubuntu)
V kořenovém adresáři projektu:
```bash
sudo install -m 0644 "$HOME"/.aspnet/dev-certs/trust/aspnetcore-localhost-*.pem /usr/local/share/ca-certificates/aspnetcore-localhost.crt
sudo update-ca-certificates
```
Poté restart systému 

---

## 3. Spuštění jednotlivých prostředí

Všechny Docker příkazy spouštěj z kořene repozitáře. Před prvním lokálním spuštěním vytvoř konfigurační soubor z šablony:

```bash
cp .env.example .env
```

> **Poznámka:** Při přepínání mezi různými režimy vždy nejprve ukonči předchozí stack (`docker compose -f <predchozi-soubor> down`), aby nedošlo ke kolizi portů nebo názvů kontejnerů.

### A) Vývoj backendu v IDE (Rider / Visual Studio)
Spustí pouze databázi a Redis s vystavenými porty na `localhost`, backend spouštíš přímo z IDE:

```bash
docker compose -f docker-compose.dev.backend.yaml up -d
```

### B) Lokální vývoj s backendem v Dockeru
Spustí databázi, Redis a vývojový backend s lokálním `.pfx` certifikátem (vhodné při spuštění frontendu přes `npm run dev`):

```bash
docker compose -f docker-compose.dev.yaml up -d --build
```

### C) Lokální test produkčního stacku
Spustí kompletní produkční architekturu včetně Caddy, Nginx frontendu a 2 replik backendu v režimu `Production` na `https://localhost` (využívá lokální `.env`):

```bash
docker compose -f docker-compose.prod.local.yaml up -d --build
```

### D) Ostré produkční nasazení (Server)
1. Vytvoř produkční konfiguraci na serveru a vyplň reálnou doménu, e-mail pro Let's Encrypt a silná hesla/klíče:
   ```bash
   cp .env.prod.example .env.prod
   ```
2. Ověř, že ve firewallu / Azure Network Security Group jsou otevřené příchozí porty **`80/tcp`**, **`443/tcp`** a **`443/udp`** a DNS záznam `DOMAIN` směřuje na IP adresu serveru.
3. Spusť produkční stack:
   ```bash
   docker compose -f docker-compose.prod.yaml --env-file .env.prod up -d --build
   ```

---

## 4. Dostupné služby a endpointy

### Vývojové prostředí (`docker-compose.dev.yaml`)
* **MariaDB:** `localhost:3306`
* **Redis:** `127.0.0.1:6379`
* **Backend HTTP:** `http://localhost:8080`
* **Backend HTTPS:** `https://localhost:8081`
* **OpenAPI JSON:** `https://localhost:8081/openapi/v1.json` *(pouze Development)*
* **Scalar dokumentace:** `https://localhost:8081/scalar` *(pouze Development)*
* **Health check:** `https://localhost:8081/health`

### Produkční prostředí (`prod` / `prod.local`)
* **Frontend (Vue.js SPA):** `https://localhost/` (lokálně) nebo `https://<DOMAIN>/` (produkce)
* **Backend API (Load Balanced):** `https://localhost/api/...` nebo `https://<DOMAIN>/api/...`
* Databáze a Redis nejsou vystaveny mimo interní síť `app_net`.

---

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

```js
headers: { Authorization: `Bearer ${accessToken}` }
```

Refresh token backend nastavuje jako `HttpOnly; Secure; SameSite=Lax` cookie s cestou `/api/auth`; v databázi uchovává pouze jeho hash. JavaScript cookie nepřečte. U požadavků z jiné originy použij ve `fetch` `credentials: "include"`, v Axiosu `withCredentials: true` (login, registrace, refresh i logout). CORS povoluje credentials jen nakonfigurovaným originům.

Po obnovení stránky frontend zavolá `/api/auth/refresh` a access token uloží z odpovědi. Při `401` z chráněného API může frontend jednou obnovit token a původní požadavek zopakovat; pokud refresh vrátí `401`, musí uživatele odhlásit. Refresh endpoint vyžaduje hlavičku `Origin` odpovídající nakonfigurovanému originu nebo originu samotného API.

Refresh tokeny se při obnově rotují. Opětovné použití spotřebovaného tokenu zneplatní jeho tokenovou rodinu (konkrétní relaci), ne ostatní relace uživatele. Access tokeny se při revokaci blokují v Redis do konce jejich platnosti.

Cookie s `SameSite=Lax` funguje pro frontend a API na stejné doméně (v produkci obojí běží pod `https://<DOMAIN>` přes Caddy). Pro cross-site vývoj je třeba povolit přesný origin frontendu v `CorsSettings:AllowedOrigins`.

### Administrátorské funkce (Role: Admin)

Administrátoři mohou spravovat relace uživatelů:

* `GET /api/admin/users/{userId}/sessions` – Výpis všech aktivních relací uživatele.
* `DELETE /api/admin/users/{userId}/sessions/{sessionId}` – Ukončení jedné konkrétní relace (`sessionId` odpovídá ID rodiny tokenů z výpisu).
* `DELETE /api/admin/users/{userId}/sessions` – Okamžité odhlášení uživatele ze všech zařízení.

### Vývojové prostředí (Seeding)

Při spuštění v režimu `Development` se automaticky vytvoří testovací účty: `admin/admin`, `student/student` a `instructor/instructor`. Tento seed se v produkčním prostředí nespouští.

---

## 6. Důležité poznámky pro vývojáře a provoz

* **Automatické EF Core migrace:** Databázové schéma se aplikuje automaticky při startu backendu (`dbContext.Database.MigrateAsync()`). Při současném startu 2 produkčních replik si EF Core zamkne tabulku `__EFMigrationsHistory` a migrace proběhne bezpečně jednou.
* **Bezstavový backend (2 repliky):** Obě produkční repliky backendu sdílejí volume `dp_keys` (pro DataProtection klíče) a veškerý sdílený stav ukládají do **Redis** a **MariaDB**.
* **MariaDB verze:** Databáze používá příznak `MARIADB_AUTO_UPGRADE=1` pro bezproblémové aktualizace systémových tabulek databáze.
* **Lokální self-signed certifikát v prohlížeči:** Při lokálním testu přes `docker-compose.prod.local.yaml` (`https://localhost`) prohlížeč zobrazí varování před lokálním certifikátem vygenerovaným Caddy — stačí potvrdit pokračování na stránku (případně lze pro vývoj v Chrome/Edge zapnout `chrome://flags/#allow-insecure-localhost`).
* **Git:** Soubory `*.pfx`, `.env` a `.env.prod` obsahují citlivé údaje nebo lokální certifikáty a nesmí být součástí Git repozitáře.