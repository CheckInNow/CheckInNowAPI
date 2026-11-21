# Historyjka: Ocenianie zakończonych wystąpień

## Opis

**Jako** uczestnik, który był na wystąpieniu,  
**chcę** ocenić to wystąpienie po jego zakończeniu,  
**aby** przekazać organizatorowi i prelegentowi informację zwrotną.

## Kryteria akceptacji

- [ ] Ocenić można tylko wystąpienie, które już się zakończyło.
- [ ] Ocenić może tylko uczestnik obecny na wystąpieniu (zarejestrowany check-in na wystąpienie – ustalenie z event stormingu).
- [ ] Ocena jest liczbą całkowitą w skali 1–5 (skala do potwierdzenia).
- [ ] Uczestnik może wystawić jedną ocenę danemu wystąpieniu i może ją zmienić.
- [ ] Uczestnik widzi swoją ocenę przy wystąpieniu.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Wystawienie oceny

- **Zakładając, że (Given)** uczestnik był na wystąpieniu, które już się zakończyło,
- **Gdy (When)** wystawi ocenę 4,
- **Wtedy (Then)** ocena zostaje zapisana.

### Scenariusz 2: Wystąpienie jeszcze trwa

- **Zakładając, że (Given)** wystąpienie jeszcze się nie zakończyło,
- **Gdy (When)** uczestnik spróbuje je ocenić,
- **Wtedy (Then)** system odrzuca ocenę z informacją, że wystąpienie się nie zakończyło.

### Scenariusz 3: Uczestnik nieobecny

- **Zakładając, że (Given)** uczestnik nie ma check-inu na wystąpienie,
- **Gdy (When)** spróbuje je ocenić,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

### Scenariusz 4: Zmiana oceny

- **Zakładając, że (Given)** uczestnik ocenił już wystąpienie na 3,
- **Gdy (When)** wystawi ocenę 5,
- **Wtedy (Then)** poprzednia ocena zostaje zastąpiona nową.

## Dodatkowe informacje

### Zależności

- Check-in na wystąpienie (US-09).

### Notatki techniczne

- Nowa encja `SessionRating` (użytkownik, wystąpienie, ocena, data) z unikalnym kluczem złożonym.
- Proponowany endpoint: `PUT /api/conferences/{conferenceId}/sessions/{sessionId}/rating`.
- Walidacja zakresu oceny w FluentValidation i ograniczeniem `CHECK` w bazie.

### Przypadki brzegowe

- Ocena spoza skali – błąd walidacji `400 Bad Request`.
- Wystąpienie usunięte po wystawieniu ocen – oceny pozostają w bazie, ale nie są uwzględniane w statystykach.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 7 – suma elementów backlogu: BE-26 (#51): 5, FE-13 (Front#13): 2
- **Status:** backend – Backlog; frontend – FE-13 – Backlog
- **Sprint docelowy:** Sprint 6
