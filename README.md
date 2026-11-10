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

Všechny Docker příkazy spouštěj z kořene repozitáře:

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

## 4. Důležité poznámky pro vývojáře

* **Automatické EF Core migrace:** Databázové schématu se aplikuje automaticky při startu backendového kontejneru (`dbContext.Database.MigrateAsync()`).
* **MariaDB verze:** Databáze používá příznak `MARIADB_AUTO_UPGRADE=1` pro bezproblémové aktualizace systémových tabulek databáze.
* **Rychlé řešení neprověřeného SSL v prohlížeči:** Pokud prohlížeč u API volání z frontendu hlásí chybu certifikátu, otevři v Chrome/Edge adresu `chrome://flags/#allow-insecure-localhost`, přepni na **Enabled** a restartuj prohlížeč.
* **Git:** Ujisti se, že soubory `*.pfx` a lokální `.env` soubory nepushuješ do Git repozitáře.