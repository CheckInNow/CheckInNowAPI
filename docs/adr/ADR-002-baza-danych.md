# ADR-002: Baza danych

- **Status:** Zaakceptowana
- **Data:** 29.10.2026
- **Autorzy:** Julia Kozłowska, Stanislav Zhuk, Paweł Miedzieński

---

## Kontekst

Dane w CheckInNow są silnie powiązane: konferencje mają sale, prelegentów i wystąpienia, a użytkownicy mają zapisy, bilety, ulubione wystąpienia i oceny. Baza danych musi:

- gwarantować spójność danych, np. przy limicie uczestników i jednorazowym użyciu biletu,
- sprawnie obsługiwać zapytania o przedziały czasu, np. przy sprawdzaniu nakładania się wystąpień,
- umożliwiać obliczanie statystyk dla organizatora (zapisy, obecność, frekwencja, średnie oceny),
- przechowywać użytkowników i ich role.

---

## Rozważane opcje

### Opcja 1: PostgreSQL
- **Zalety:**
  - darmowa, dojrzała relacyjna baza open source,
  - pełna obsługa transakcji i ograniczeń integralności,
  - wbudowane typy zakresów czasu, pozwalające zablokować nakładanie się wystąpień na poziomie bazy,
  - łatwe uruchomienie w kontenerze i dostępność jako usługa w chmurze.
- **Wady:**
  - najbardziej zaawansowane funkcje wymagają ręcznie pisanego SQL.

### Opcja 2: Microsoft SQL Server
- **Zalety:**
  - bardzo dobra integracja z ekosystemem .NET,
  - rozbudowane narzędzia administracyjne.
- **Wady:**
  - płatna licencja w wersjach produkcyjnych,
  - cięższy kontener i słabsze wsparcie poza Windowsem.

### Opcja 3: MongoDB
- **Zalety:**
  - elastyczny schemat danych.
- **Wady:**
  - dane aplikacji są relacyjne i wymagają łączenia oraz agregowania,
  - słabsze gwarancje spójności między kolekcjami.

## Decyzja

Wybrano **PostgreSQL 18**. Dostęp do danych zapewnia **Entity Framework Core 10.0.13** z providerem **Npgsql 10.0.3**, a zmiany schematu wprowadzane są przez migracje. Użytkownicy i role przechowywani są w tej samej bazie co pozostałe dane. Często odczytywane dane (np. agenda) trafiają do cache'u **HybridCache 10.11.0**, który łączy pamięć aplikacji z **Redisem 8.2**.

Model relacyjny odpowiada domenie aplikacji, a PostgreSQL zapewnia spójność danych i wygodną pracę z przedziałami czasu. Baza jest darmowa i łatwo ją uruchomić zarówno lokalnie, jak i w chmurze.

**Konsekwencje:**

- reguły kluczowe dla spójności (limit miejsc, jednorazowy bilet, brak nakładania się wystąpień) można zabezpieczyć również na poziomie bazy,
- schemat bazy jest wersjonowany razem z kodem,
- Redis to dodatkowy element infrastruktury do utrzymania,
- cache'owanie wymaga określenia zasad odświeżania danych po zmianach.
