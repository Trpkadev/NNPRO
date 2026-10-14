### Zadání semestrální práce: E-learningový systém

#### Cíl:

Cílem této semestrální práce je navrhnout a implementovat e-learningový
systém, který umožní lektorům vytvářet a spravovat kurzy a studentům
procházet jejich obsah, plnit kvízy a sledovat průchod kurzem. Systém
bude rozlišovat tři uživatelské role -- správce, lektora a studenta.

#### Funkcionality aplikace:

1.  **Uživatelské role**:

    -   **Správce**: Správa uživatelských účtů a kurzů.
    -   **Lektor**: Tvorba a správa vlastních kurzů, strukturování
        obsahu a přehled zapsaných studentů.
    -   **Student**: Procházení katalogu kurzů, zápis do kurzů,
        procházení lekcí a plnění kvízů.

2.  **Struktura kurzu a obsah**:

    -   **Hierarchie obsahu**: Kurz se skládá z tematických sekcí, které
        obsahují jednotlivé lekce.
    -   **Obsah lekcí**: Formátovaný text se vloženými odkazy a
        externími videi (např. YouTube) a přílohy ke stažení (PDF atd.).
    -   **Kvízy**: Kvízy s uzavřenými otázkami, které se automaticky
        vyhodnocují ihned po odevzdání. Kvíz je součástí tematické sekce
        a slouží k ověření jejího obsahu.
    -   **Typy otázek**: Otázky se uzavřenou odpovědí -- jednoznačná
        volba, výběr více odpovědí a pravda/nepravda. Text otázky i
        jednotlivé odpovědi mohou obsahovat formátovaný text a vložený
        obrázek.
    -   **Bodové hodnocení**: Každá otázka má nastavitelnou bodovou
        hodnotu, hodnocení celého pokusu se vyjadřuje v procentech
        (0--100). Odpověď se hodnotí binárně -- všechny body otázky za
        úplnou správnost, jinak žádné.
    -   **Zpětná vazba**: Po odevzdání pokusu student okamžitě uvidí své
        hodnocení a správné odpovědi v porovnání se svými odpověďmi.
    -   **Parametry kvízu**: Lektor u každého kvízu nastavuje maximální
        počet pokusů (případně pokusy neomezuje), volitelný časový
        limit, procházející práh (minimální procento pro splnění kvízu)
        a případné náhodné pořadí otázek a variant odpovědí.

3.  **Zápis do kurzů**:

    -   **Veřejné kurzy**: Přímý zápis studenta do kurzu na jedno
        kliknutí.
    -   **Uzavřené kurzy**: Zápis pomocí přístupového klíče (hesla
        kurzu).
    -   **Odhlášení z kurzu**: Student se může z kurzu kdykoliv
        odhlásit.

4.  **Sledování průchodu kurzem**:

    -   **Dokončení lekcí**: Student si může lekci manuálně označit za
        dokončenou.
    -   **Průchod kurzem**: Výpočet celkového procenta dokončení kurzu.
    -   **Historie pokusů u kvízů**: Ukládání historie pokusů o kvízy
        včetně bodového hodnocení každého pokusu.

#### Technické požadavky:

1.  **Front-end**:

    -   **Webové rozhraní**: Implementace uživatelského rozhraní jako
        webové aplikace s využitím frameworku Vue 3 (Vite, Pinia).
    -   **Přizpůsobení rozhraní**: Responzivní design umožňující použití
        aplikace na desktopu i mobilních zařízeních.

2.  **Back-end**:

    -   **Serverová aplikace**: Implementace serverové části v jazyce C#
        s využitím frameworku ASP.NET Core; komunikace s front-endem
        prostřednictvím REST API.
    -   **Práce s daty**: Využití objektově-relačního mapování (Entity
        Framework) pro práci s daty.

3.  **Databáze**:

    -   **Relační databáze**: Ukládání dat o uživateli, kurzech, lekcích
        a pokusech u kvízů v relační databázi MariaDB.

4.  **Autentizace a autorizace**:

    -   **Přihlašování uživatelů**: Implementace autentizace uživatelů
        pomocí REST API s využitím JWT tokenů včetně bezpečného ukládání
        hesel (hashování).
    -   **Oprávnění podle rolí**: Autorizace přístupu k funkcím systému
        na základě uživatelské role (správce, lektor, student).

5.  **Mobilní přístup**:

    -   **PWA / Capacitor**: Zajištění mobilního přístupu
        prostřednictvím progresivní webové aplikace (PWA) s využitím
        responzivního webového rozhraní (např. pomocí Capacitor).

6.  **Testování a nasazení**:

    -   **Testy**: Implementace unit testů a integračních testů pro obě
        části aplikace.
    -   **Nasazení**: Nasazení aplikace pomocí Docker Compose s podporou
        bezpečných protokolů (HTTPS).

#### Očekávaný výstup:

-   Funkční e-learningový systém umožňující lektorům vytvářet a
    spravovat kurzy a studentům procházet lekce, plnit kvízy a sledovat
    svůj průchod kurzem.
-   Podpora veřejného i uzavřeného zápisu do kurzů (přímo nebo pomocí
    přístupového klíče) včetně možnosti odhlášení z kurzu.
-   Sledování dokončení lekcí, celkového procenta dokončení kurzu a
    historie pokusů u kvízů s bodovým hodnocením.
-   Dokumentace projektu včetně uživatelské příručky a případné
    technické dokumentace.

#### Hodnotící kritéria:

1.  Splnění všech požadovaných funkcionalit (správa kurzů, zápis
    studentů, obsah lekcí, kvízy, sledování průchodu).
2.  Kvalita kódu a dodržení zásad dobré praxe v oblasti vývoje softwaru.
3.  Uživatelská přívětivost rozhraní.
4.  Správné rozlišení uživatelských rolí a oprávnění (správce, lektor,
    student).
5.  Funkční mobilní přístup prostřednictvím PWA.
6.  Kvalita dokumentace projektu, včetně uživatelské příručky a
    technické dokumentace.

#### Doporučené deadline:

-   **První fáze**: Návrh aplikace a databázového schématu (2 týdny).
-   **Druhá fáze**: Implementace základních funkcionalit (5 týdnů).
-   **Třetí fáze**: Testování, ladění a nasazení (2 týdny).
