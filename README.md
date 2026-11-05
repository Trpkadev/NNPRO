# NNPRO Backend

## Spuštění pomocí Docker Compose

Compose spouštěj z kořene repozitáře:

```bash
docker compose up -d --build
```

Aktivní build target se nastavuje v `docker-compose.yaml`. Pro development:

```yaml
target: development
#target: production
```

Pro production prohoď komentáře:

```yaml
#target: development
target: production
```

Po změně targetu vždy image znovu sestav:

```bash
docker compose up -d --build
```

Compose zároveň spouští MariaDB na portu `3306` a backend na:

- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8081`
- OpenAPI JSON (pouze development): `https://localhost:8081/openapi/v1.json`
- Scalar dokumentace (pouze development): `https://localhost:8081/scalar`
- Health check: `https://localhost:8081/health`

HTTP požadavky jsou přesměrovány na HTTPS. HTTPS používá lokálně vygenerovaný
self-signed certifikát, proto může prohlížeč zobrazit bezpečnostní varování.