# NNPRO Backend

Backend e-learningového systému v ASP.NET Core (.NET 10), MariaDB a Redis běžící v Dockeru.

---

## 1. První spuštění (Prerekvizity)

Před prvním spuštěním kontejnerů je nutné vygenerovat vývojový HTTPS certifikát přímo na hostitelském počítači. Certifikát se uloží do `./Backend/aspnetapp.pfx` (tento soubor je v `.gitignore` a nahrává ho každý vývojář lokálně).

V kořenovém adresáři projektu spusť v terminálu:

```bash
dotnet dev-certs https --trust -ep ./Backend/aspnetapp.pfx -p nnpro

```

*(Potvrď systémové okno pro důvěru certifikátu).*

---

## 2. Spuštění pomocí Docker Compose

Všechny Docker příkazy spouštěj z kořene repozitáře. Před prvním spuštěním vytvoř lokální konfiguraci z šablony: `cp .env.example .env`. Hodnoty v `.env` jsou určeny pouze pro lokální vývoj; pro produkci nastav vlastní tajné údaje. Backend spuštěný přímo přes Rider načte tento kořenový `.env` automaticky v prostředí Development.

```bash
docker compose up -d --build

```

### Přepínání prostředí (Development / Production)

Aktivní build target se nastavuje v `docker-compose.yaml` u služby `backend`:

```yaml
# Pro Development:
target: development
# target: production

```

Pro produkční sestavení prohoď komentáře:

```yaml
# target: development
target: production

```

Po každé změně targetu image znovu sestav:

```bash
docker compose up -d --build

```

---

## 3. Dostupné služby a endpointy

Docker Compose spouští databázi **MariaDB** (`3306`), **Redis** (`6379`) a **backend** na portech:

* **HTTP:** `http://localhost:8080`
* **HTTPS:** `https://localhost:8081`
* **OpenAPI JSON:** `https://localhost:8081/openapi/v1.json` *(pouze Development)*
* **Scalar dokumentace:** `https://localhost:8081/scalar` *(pouze Development)*
* **Health check:** `https://localhost:8081/health`

---

## 4. Autentizace a správa relací

Backend využívá krátkodobé JWT access tokeny (výchozí platnost 10 minut) a dlouhodobé refresh tokeny (1 den). Hodnoty lze upravit v `appsettings.json`. Podpisový klíč a identita tokenu se konfigurují přes proměnné prostředí (`Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`). Podpisový klíč musí mít z bezpečnostních důvodů alespoň 32 bajtů.

### Auth API Endpointy

| Metoda | Endpoint | Účel |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Registrace nového účtu (heslo min. 8 znaků). |
| `POST` | `/api/auth/login` | Přihlášení přes uživatelské jméno nebo e-mail a heslo. Pokud při loginu pošleš dosavadní access token jako Bearer, jeho relace se po úspěšném loginu ukončí. |
| `POST` | `/api/auth/refresh` | Obnova access tokenu a rotace refresh cookie; tělo požadavku se neposílá. |
| `POST` | `/api/auth/logout` | Odhlášení aktuální relace (vyžaduje zaslání access tokenu). |

### Integrace s frontendem a bezpečnost

Access token backend vrací v JSON pod `data.accessToken`. FE ho má držet pouze v paměti a posílat jako Bearer token:

```js
headers: { Authorization: `Bearer ${accessToken}` }
```

Refresh token backend nastavuje jako `HttpOnly; Secure; SameSite=Lax` cookie s cestou `/api/auth`; v databázi uchovává pouze jeho hash. JavaScript cookie nepřečte. U požadavků z jiné originy použij ve `fetch` `credentials: "include"`, v Axiosu `withCredentials: true` (login, registrace, refresh i logout). CORS povoluje credentials jen nakonfigurovaným originům.

Po obnovení stránky FE zavolá `/api/auth/refresh` a access token uloží z odpovědi. Při `401` z chráněného API může FE jednou obnovit token a původní požadavek zopakovat; pokud refresh vrátí `401`, musí uživatele odhlásit. Refresh endpoint vyžaduje hlavičku `Origin` odpovídající nakonfigurovanému originu nebo originu samotného API.

Refresh tokeny se při obnově rotují. Opětovné použití spotřebovaného tokenu zneplatní jeho tokenovou rodinu (konkrétní relaci), ne ostatní relace uživatele. Access tokeny se při revokaci blokují v Redis do konce jejich platnosti.

Cookie s `SameSite=Lax` funguje pro FE a API na stejné site (např. různé porty na localhostu). Pro cross-site FE je třeba nastavit `SameSite=None; Secure` a povolit jeho přesný origin v CORS; produkce musí používat HTTPS.

### Administrátorské funkce (Role: Admin)

Administrátoři mohou spravovat relace uživatelů:

* `GET /api/admin/users/{userId}/sessions` – Výpis všech aktivních relací uživatele.
* `DELETE /api/admin/users/{userId}/sessions/{sessionId}` – Ukončení jedné konkrétní relace (`sessionId` odpovídá ID rodiny tokenů z výpisu).
* `DELETE /api/admin/users/{userId}/sessions` – Okamžité odhlášení uživatele ze všech zařízení.

### Vývojové prostředí (Seeding)

Při lokálním spuštění se automaticky vytvoří testovací účty: `admin/admin`, `student/student` a `instructor/instructor`. Tento seed se v produkčním prostředí nespouští.

---

## 5. Důležité poznámky pro vývojáře

* **Automatické EF Core migrace:** Databázové schématu se aplikuje automaticky při startu backendového kontejneru (`dbContext.Database.MigrateAsync()`).
* **MariaDB verze:** Databáze používá příznak `MARIADB_AUTO_UPGRADE=1` pro bezproblémové aktualizace systémových tabulek databáze.
* **Rychlé řešení neprověřeného SSL v prohlížeči:** Pokud prohlížeč u API volání z frontendu hlásí chybu certifikátu, otevři v Chrome/Edge adresu `chrome://flags/#allow-insecure-localhost`, přepni na **Enabled** a restartuj prohlížeč.
* **Git:** Ujisti se, že soubory `*.pfx` a lokální `.env` soubory nepushuješ do Git repozitáře.
