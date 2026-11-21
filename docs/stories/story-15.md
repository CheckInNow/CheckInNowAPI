# Historyjka: Przeglądanie logów aplikacji przez administratora

## Opis

**Jako** administrator,  
**chcę** przeglądać logi aplikacji w panelu administracyjnym,  
**aby** szybko diagnozować błędy zgłaszane przez użytkowników bez logowania się na serwer.

## Kryteria akceptacji

- [ ] Logi widzi tylko użytkownik z rolą `Admin`.
- [ ] Logi są zapisywane trwale, a nie tylko na konsolę.
- [ ] Lista logów jest stronicowana i posortowana od najnowszych.
- [ ] Logi można filtrować po poziomie (np. Information, Warning, Error), zakresie dat i treści komunikatu.
- [ ] Każdy wpis zawiera czas, poziom, komunikat oraz – jeśli wystąpił – szczegóły wyjątku.
- [ ] Logi nie zawierają haseł, tokenów ani innych danych wrażliwych.
- [ ] Logi starsze niż ustalony okres retencji są automatycznie usuwane.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Przegląd ostatnich błędów

- **Zakładając, że (Given)** w ostatniej godzinie wystąpiły błędy aplikacji,
- **Gdy (When)** administrator przefiltruje logi po poziomie „Error” i ostatniej godzinie,
- **Wtedy (Then)** widzi listę błędów od najnowszego wraz ze szczegółami wyjątków.

### Scenariusz 2: Wyszukiwanie po treści

- **Zakładając, że (Given)** organizator zgłosił problem z dodaniem wystąpienia,
- **Gdy (When)** administrator wyszuka logi zawierające „Session”,
- **Wtedy (Then)** widzi tylko wpisy z pasującym komunikatem.

### Scenariusz 3: Brak uprawnień

- **Zakładając, że (Given)** użytkownik nie ma roli `Admin`,
- **Gdy (When)** spróbuje pobrać logi,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Decyzja o magazynie logów opisana w nowym ADR (DOC-06).

### Notatki techniczne

- Obecnie Serilog zapisuje logi tylko na konsolę, a OpenTelemetry wysyła je do dashboardu Aspire.
- Proponowane rozwiązanie: dodatkowy sink Serilog zapisujący do tabeli w PostgreSQL oraz endpoint `GET /api/admin/logs` z filtrami i paginacją.
- Zmiana konfiguracji Serilog wymaga zgody zespołu.

### Przypadki brzegowe

- Bardzo duża liczba wpisów – obowiązkowa paginacja i indeks po czasie.
- Niedostępność magazynu logów – aplikacja działa dalej, logi trafiają na konsolę.
- Pętla logowania (odczyt logów generuje logi) – zapytania o logi nie są logowane na poziomie Information.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 9 – suma elementów backlogu: DOC-06 (#53): 2, BE-28 (#54): 5, FE-16 (Front#16): 2
- **Status:** backend – Backlog; frontend – FE-16 – Backlog
- **Sprint docelowy:** Sprint 6
