# ADR-006: Testowanie

- **Status:** Zaakceptowana
- **Data:** 29.10.2026
- **Autorzy:** Julia Kozłowska, Stanislav Zhuk, Paweł Miedzieński

---

## Kontekst

Aplikacja zawiera reguły biznesowe, których błędy są od razu widoczne dla użytkownika: nakładające się wystąpienia, limit uczestników, jednorazowe użycie biletu, ostrzeżenia o kolizjach w planie i ocenianie wyłącznie zakończonych wystąpień.

Na obecnym etapie zakłada się wyłącznie testy jednostkowe. Wymagane są narzędzia, które pozwalają testować logikę bez uruchamiania bazy danych i serwera, zastępować zależności atrapami i pisać czytelne asercje. Testy muszą działać lokalnie i w CI.

---

## Rozważane opcje

### Opcja 1: xUnit v3
- **Zalety:**
  - najpopularniejszy framework testowy w nowoczesnym .NET,
  - każdy test otrzymuje nową instancję klasy, co ogranicza zależności między testami,
  - szybkie uruchamianie na nowej platformie testowej Microsoftu.
- **Wady:**
  - część materiałów w sieci dotyczy jeszcze starszej wersji 2.

### Opcja 2: NUnit
- **Zalety:**
  - dojrzały framework z wieloma atrybutami.
- **Wady:**
  - domyślnie współdzielona instancja klasy testowej sprzyja zależnościom między testami,
  - rzadziej wybierany w nowych projektach.

### Opcja 3: MSTest
- **Zalety:**
  - rozwijany przez Microsoft i dobrze zintegrowany z Visual Studio.
- **Wady:**
  - mniejsza społeczność i mniej przykładów.

## Decyzja

Wybrano **xUnit v3** (pakiet `xunit.v3` 4.0.2), uzupełniony o:

- **NSubstitute 6.2.0** – tworzenie atrap zależności,
- **FluentAssertions 8.11.0** – czytelne asercje,
- **EF Core InMemory 10.0.13** – bazę w pamięci do testowania logiki korzystającej z danych,
- **NetArchTest 1.3.2** – testy pilnujące zależności między warstwami.

Testy uruchamiane są automatycznie w GitHub Actions przy każdej zmianie w gałęzi `main` i w pull requestach.

Wybrany zestaw jest popularny, dobrze udokumentowany i pozwala pisać krótkie, czytelne testy. Baza w pamięci umożliwia testowanie logiki bez infrastruktury, co odpowiada założeniu o wyłącznie jednostkowym testowaniu.

**Konsekwencje:**

- testy są szybkie i nie wymagają Dockera,
- testy jednostkowe nie obejmują zachowań samej bazy PostgreSQL (ograniczeń, transakcji, równoczesnych operacji),
- reguły biznesowe muszą być zaimplementowane w logice aplikacji, aby można je było przetestować,
- FluentAssertions od wersji 8 jest darmowe wyłącznie w projektach niekomercyjnych.
