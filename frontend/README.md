# Webový a mobilní frontend aplikace E-learning založený na Vue.js

---

## Tech Stack

- **Jádro:** [Vue 3](https://vuejs.org/) (Composition API, `<script setup>`), [TypeScript](https://www.typescriptlang.org/)
- **Build nástroj & Cesty:** [Vite](https://vite.dev/), `@/` aliasy
- **Routování:** [Vue Router](https://router.vuejs.org/) (lazy-loaded routes, typovaná `meta`)
- **Stav aplikace:**
    - **Server State:** [TanStack Vue Query](https://tanstack.com/query/latest) (cachování, synchronizace API dat)
    - **Client State:** [Pinia](https://pinia.vuejs.org/) (Setup stores, správa UI stavu a auth)
- **UI & Stylování:** [OpenVue](https://openvue.org/) (komunitní MIT fork PrimeVue 4), [Tailwind CSS v4](https://tailwindcss.com/) (`tailwindcss-primeui`), [@openvue/openicons](https://openvue.org/) (`oi oi-*` ikony)
- **Formuláře & Validace:** [VeeValidate](https://vee-validate.logaretm.com/) + [Zod](https://zod.dev/) (přes Standard Schema)
- **HTTP & Auth:** [Axios](https://axios-http.com/) (interceptory pro JWT a 401), `jwt-decode`
- **Lokalizace:** [Vue I18n](https://vue-i18n.intlify.dev/)
- **Utilities:** [VueUse](https://vueuse.org/)
- **Mobilní platforma:** [Capacitor](https://capacitorjs.com/) (iOS & Android)
- **Testování:** [Vitest](https://vitest.dev/)

---

## Struktura projektu

```text
src/
├── assets/           # Globální styly (Tailwind v4, custom varianty)
├── components/       # Vlastní Vue komponenty (auto-importované)
├── i18n/             # Jazykové mutace (cs, en)
├── router/           # Konfigurace routeru (index.ts + routes.ts)
├── services/         # API klient (Axios instance, interceptory)
├── stores/           # Pinia stores (setup syntax)
├── views/            # Stránky (HomeView, NotFoundView atd.)
├── App.vue           # Hlavní komponenta (Toast, ConfirmDialog, RouterView)
├── main.ts           # Vstupní bod, registrace pluginů
├── auto-imports.d.ts # Automaticky generované typy pro auto-importy (neupravovat ručně)
└── components.d.ts   # Automaticky generované typy pro OpenVue/custom komponenty
```

---

## Požadavky pro vývoj

- **Node.js:** v24+
- **NPM**
- **VS Code nebo WebStorm** (doporučeno)
- **Pro vývoj mobilních aplikací:**
    - **Android:** Android Studio + JDK
    - **iOS (pouze macOS):** Xcode + CocoaPods

---

## Instalace a spuštění projektu

1. **Klonování repozitáře a instalace závislostí:**
   ```bash
   git clone <url-repozitari>
   cd <nazev-projektu>
   npm install
   ```

2. **Konfigurace prostředí:**
   Zkopírujte vzorový soubor s proměnnými prostředí (pokud existuje) nebo vytvořte `.env`:
   ```bash
   cp .env.example .env
   ```

3. **Spuštění lokálního vývojového serveru (Web):**
   ```bash
   npm run dev
   ```
   Aplikace poběží na `http://localhost:5173`.

---

## Vývoj pro mobilní zařízení (Capacitor)

Pro propojení webové aplikace s nativními platformami iOS a Android slouží nástroj Capacitor.

1. **Build webové aplikace:**
   Před každou synchronizací s mobilem je nutné zbuildovat aktuální kód:
   ```bash
   npm run build
   ```

2. **Synchronizace změn do nativních projektů:**
   ```bash
   npx cap sync
   ```

3. **Otevření v IDE pro danou platformu:**
    - **Android (otevře Android Studio):**
      ```bash
      npx cap open android
      ```
    - **iOS (otevře Xcode - pouze na macOS):**
      ```bash
      npx cap open ios
      ```

---

## Testování a kvalita kódu

- **Spuštění jednotkových testů (Vitest):**
  ```bash
  npm run test:unit
  ```
- **Kontrola typů (TypeScript):**
  ```bash
  npm run type-check
  ```
- **Lintování kódu (ESLint):**
  ```bash
  npm run lint
  ```

---

## Užitečné tipy pro vývoj

- **Auto-importy:** Funkce z Vue, Vue Routeru, Pinie, VueUse, Vue I18n, VeeValidate a komponenty z OpenVue se **importují automaticky**. Nemusíte psát `import { ref } from 'vue'` ani `import { Button } from 'openvue'`.
- **Ikony:** V projektu se používá balíček `@openvue/openicons`. Ikony se vkládají s prefixem `oi`, např. `<i class="oi oi-home" />` nebo v tlačítkách `icon="oi oi-sun"`.
- **Server vs. Client State:** Pro data z API používejte výhradně **TanStack Query**. Pinia slouží pouze pro klientský stav (auth token, UI preference).