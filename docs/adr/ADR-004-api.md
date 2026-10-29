# ADR-004: REST API

- **Status:** Zaakceptowana
- **Data:** 29.10.2026
- **Autorzy:** Julia Kozłowska, Stanislav Zhuk, Paweł Miedzieński

---

## Kontekst

Frontend komunikuje się z backendem wyłącznie przez REST API, które musi być udokumentowane w formacie OpenAPI. API obejmuje wiele obszarów: konferencje, sale, prelegentów, agendę, zapisy, bilety, check-in, plan uczestnika, oceny, statystyki i administrację.

Wymagany jest jeden, powtarzalny sposób tworzenia endpointów, walidacja danych wejściowych w jednym miejscu oraz spójny format błędów. Pozwala to frontendowi jednoznacznie sygnalizować np. nakładające się wystąpienia lub ponowne użycie biletu.

---

## Rozważane opcje

### Opcja 1: Minimal APIs
- **Zalety:**
  - mało kodu, łatwe grupowanie endpointów według funkcji,
  - typowane odpowiedzi automatycznie trafiają do dokumentacji OpenAPI,
  - dobra wydajność i kierunek rozwoju zalecany przez Microsoft.
- **Wady:**
  - mniej gotowych konwencji niż w kontrolerach, część mechanizmów wymaga ręcznej konfiguracji.

### Opcja 2: Kontrolery MVC
- **Zalety:**
  - dojrzałe i powszechnie znane rozwiązanie z wieloma gotowymi mechanizmami.
- **Wady:**
  - więcej powtarzalnego kodu,
  - kontrolery łatwo się rozrastają i zaczynają zawierać logikę biznesową.

### Opcja 3: GraphQL
- **Zalety:**
  - klient pobiera dokładnie te dane, których potrzebuje.
- **Wady:**
  - wymagania projektu zakładają REST i OpenAPI,
  - większa złożoność autoryzacji i cache'owania.

## Decyzja

Wybrano **Minimal APIs** (ASP.NET Core 10). Każdy przypadek użycia to osobna komenda lub zapytanie obsługiwane przez **MediatR 14.2.0** (wzorzec CQRS). Endpoint jedynie przekazuje żądanie dalej, a logika biznesowa znajduje się w warstwie aplikacji. Dane wejściowe są sprawdzane przez **FluentValidation 12.2.0** przed wykonaniem logiki. Wyniki operacji zwracane są we wzorcu **Result**, a błędy w standardowym formacie **ProblemDetails**. Dokumentacja API generowana jest przez wbudowane w .NET **OpenAPI** (`Microsoft.AspNetCore.OpenApi` 10.0.13) i udostępniana w **Scalarze 2.18.0**.

Ten zestaw zapewnia proste endpointy i jeden wzorzec dla wszystkich funkcji. Walidacja odbywa się w jednym miejscu, a frontend otrzymuje przewidywalne odpowiedzi i błędy. Wbudowane OpenAPI spełnia wymaganie dokumentacji bez dodatkowych narzędzi.

**Konsekwencje:**

- każdy przypadek użycia składa się z kilku małych plików (komenda, obsługa, walidator, endpoint),
- sytuacje wymagające rozpoznania po stronie frontendu (np. bilet już użyty) otrzymują własne kody błędów,
- dokumentacja API powstaje razem z kodem, co umożliwia równoległą pracę nad frontendem,
- MediatR i FluentValidation to dodatkowe zależności, których licencje i aktualizacje wymagają śledzenia.
