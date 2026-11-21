# Historyjka: Dodawanie sal konferencyjnych

## Opis

**Jako** organizator,  
**chcę** dodać do konferencji sale, w których odbywają się wystąpienia,  
**aby** móc przypisywać do nich wystąpienia w agendzie.

## Kryteria akceptacji

- [x] Salę może dodać tylko właściciel konferencji.
- [x] Nazwa sali jest wymagana (maks. 200 znaków) i unikalna w ramach konferencji.
- [x] Pojemność sali jest opcjonalna, a jeśli podana – większa od zera.
- [x] Lista sal konferencji jest dostępna publicznie.
- [ ] Organizator może edytować i usuwać sale (BE-29).

## Scenariusze (Given – When – Then)

### Scenariusz 1: Dodanie sali

- **Zakładając, że (Given)** organizator jest właścicielem konferencji,
- **Gdy (When)** doda salę „Sala A” o pojemności 120 osób,
- **Wtedy (Then)** sala pojawia się na liście sal konferencji.

### Scenariusz 2: Powtórzona nazwa

- **Zakładając, że (Given)** w konferencji istnieje już „Sala A”,
- **Gdy (When)** organizator doda drugą salę o tej samej nazwie,
- **Wtedy (Then)** system zwraca `409 Conflict`.

### Scenariusz 3: Sala w cudzej konferencji

- **Zakładając, że (Given)** organizator nie jest właścicielem konferencji,
- **Gdy (When)** spróbuje dodać salę,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Istniejąca konferencja (US-03).

### Notatki techniczne

- Endpointy: `GET/POST /api/conferences/{conferenceId}/rooms`.

### Przypadki brzegowe

- Nieistniejąca lub usunięta konferencja – `404 Not Found`.
- Usunięcie sali, do której przypisane są wystąpienia – do ustalenia przy realizacji BE-29.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 8 (wspólnie z US-05) – suma elementów backlogu: BE-09 (#23): 3, FE-05 (Front#5): 1, FE-06 (Front#6): 1, BE-29 (#55): 3
- **Status:** backend – częściowo (pozostało: BE-29); frontend – FE-05 – Backlog, FE-06 – Backlog
- **Sprint docelowy:** Sprint 6
