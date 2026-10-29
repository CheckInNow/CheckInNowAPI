# ADR-005: Frontend

- **Status:** Zaakceptowana
- **Data:** 29.10.2026
- **Autorzy:** Julia Kozłowska, Stanislav Zhuk, Paweł Miedzieński

---

## Kontekst

Frontend CheckInNow to osobna aplikacja, która komunikuje się z backendem wyłącznie przez REST API udokumentowane w formacie OpenAPI. Interfejs obejmuje widoki dla trzech ról: uczestnika (zapisy, bilet z kodem QR, własny plan konferencji, oceny), organizatora (konferencje, sale, prelegenci, agenda, statystyki, skanowanie biletów) oraz administratora (użytkownicy, logi).

Część służąca do sprawdzania biletów musi być dostosowana do urządzeń mobilnych. Za całość interfejsu odpowiada jedna osoba, dlatego wybrane narzędzia muszą maksymalnie przyspieszać budowę widoków i ograniczać ryzyko błędów przy mapowaniu danych z API.

Decyzja obejmuje trzy obszary: framework i narzędzie budowania, język programowania oraz stylowanie i komponenty UI.

---

## Rozważane opcje

### Framework i narzędzie budowania

#### Opcja 1: React + Vite (Single Page Application)
- **Zalety:**
  - bardzo szybkie uruchamianie środowiska deweloperskiego i budowanie aplikacji,
  - aplikacja działa w całości w przeglądarce, więc komunikacja z osobnym backendem jest prosta,
  - największy ekosystem bibliotek i materiałów.
- **Wady:**
  - routing, pobieranie danych i zarządzanie stanem wymagają doboru dodatkowych bibliotek,
  - brak renderowania po stronie serwera.

#### Opcja 2: Next.js
- **Zalety:**
  - gotowy routing i renderowanie po stronie serwera,
  - rozbudowany i popularny framework oparty na React.
- **Wady:**
  - dodatkowa warstwa serwerowa, zbędna przy osobnym backendzie,
  - komplikuje obsługę sesji i CORS w komunikacji z backendem.

#### Opcja 3: Angular
- **Zalety:**
  - kompletny framework z wbudowanym routingiem, formularzami i klientem HTTP.
- **Wady:**
  - wyższy próg wejścia i więcej powtarzalnego kodu,
  - wolniejsze tworzenie widoków przy pracy jednej osoby.

### Język programowania

#### Opcja 1: TypeScript
- **Zalety:**
  - statyczne typowanie wykrywa wiele błędów już na etapie pisania kodu,
  - wymusza porządek w strukturach danych, co jest kluczowe przy mapowaniu odpowiedzi REST API,
  - ułatwia utrzymanie projektu i zwiększa czytelność kodu.
- **Wady:**
  - dodatkowy kod definicji typów,
  - typy po stronie frontendu trzeba utrzymywać zgodnie z API.

#### Opcja 2: JavaScript
- **Zalety:**
  - brak etapu kompilacji typów i szybszy start.
- **Wady:**
  - błędy w strukturach danych ujawniają się dopiero w trakcie działania aplikacji,
  - trudniejsze utrzymanie wraz z rozwojem projektu.

### Stylowanie i komponenty UI

#### Opcja 1: Tailwind CSS + shadcn/ui
- **Zalety:**
  - shadcn/ui dostarcza gotowe, nowoczesne i dostępne komponenty, których nie trzeba projektować od zera,
  - kod komponentów trafia do projektu, więc można go dowolnie modyfikować,
  - Tailwind CSS pozwala elastycznie dostosowywać style i łatwo budować responsywne widoki.
- **Wady:**
  - długie listy klas w kodzie komponentów,
  - aktualizacje komponentów shadcn/ui trzeba przenosić ręcznie.

#### Opcja 2: Material UI (MUI)
- **Zalety:**
  - bardzo duży zestaw gotowych komponentów.
- **Wady:**
  - narzucony wygląd Material Design, trudniejszy do dostosowania,
  - większy rozmiar aplikacji.

#### Opcja 3: Bootstrap
- **Zalety:**
  - dobrze znany, z gotowym systemem siatki i komponentami.
- **Wady:**
  - charakterystyczny, mniej nowoczesny wygląd,
  - słabsza integracja z komponentami React.

## Decyzja

Wybrano **React** z **Vite** jako aplikację typu Single Page Application, napisaną w **TypeScript**, ze stylowaniem w **Tailwind CSS** i komponentami **shadcn/ui**. Wersje zostaną ustalone przy inicjalizacji projektu frontendowego.

Vite zapewnia bardzo szybki czas uruchamiania środowiska deweloperskiego i budowania aplikacji. Świadomie zrezygnowano z frameworków typu Next.js, aby uniknąć zbędnego narzutu serwerowego, który skomplikowałby architekturę i komunikację z osobnym backendem (np. w kwestii obsługi sesji i CORS). TypeScript chroni przed błędami przy mapowaniu danych z REST API i minimalizuje ryzyko pomyłek przy pracy w pojedynkę na warstwie frontendowej. Tailwind CSS i shadcn/ui maksymalnie przyspieszają budowę widoków i ułatwiają tworzenie responsywnego interfejsu, w tym mobilnego skanera QR.

**Konsekwencje:**

- frontend i backend są w pełni niezależne, a komunikacja odbywa się wyłącznie przez REST API,
- backend musi zezwalać na żądania z domeny frontendu (CORS),
- typy danych po stronie frontendu muszą być utrzymywane zgodnie z dokumentacją OpenAPI backendu,
- odczyt kodów QR odbywa się w przeglądarce urządzenia mobilnego, a biblioteka do skanowania zostanie wybrana na etapie implementacji,
- brak renderowania po stronie serwera nie stanowi problemu, ponieważ aplikacja nie wymaga pozycjonowania w wyszukiwarkach.
