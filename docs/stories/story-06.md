# Historyjka: Budowanie agendy bez nakładania się wystąpień

## Opis

**Jako** organizator,  
**chcę** dodawać do agendy wystąpienia z tytułem, opisem, prelegentem, salą i godzinami,  
**aby** uczestnicy wiedzieli, co, gdzie i kiedy się odbywa, a w jednej sali nie wypadły dwa wystąpienia naraz.

## Kryteria akceptacji

- [x] Wystąpienie może dodać, edytować i usunąć tylko właściciel konferencji.
- [x] Tytuł jest wymagany (maks. 200 znaków), opis jest opcjonalny (maks. 1000 znaków).
- [x] Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.
- [x] Wystąpienie musi mieścić się w czasie trwania konferencji.
- [x] Sala i prelegent muszą należeć do tej samej konferencji.
- [x] Wystąpienia w tej samej sali nie mogą się nakładać; wystąpienia następujące bezpośrednio po sobie są dozwolone.
- [x] Prelegent nie może prowadzić dwóch wystąpień w tym samym czasie.
- [x] Usunięcie wystąpienia jest miękkie.
- [x] Agenda jest dostępna publicznie.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Dodanie wystąpienia

- **Zakładając, że (Given)** konferencja ma salę i prelegenta,
- **Gdy (When)** organizator doda wystąpienie 10:00–11:00 w tej sali,
- **Wtedy (Then)** wystąpienie pojawia się w agendzie.

### Scenariusz 2: Nakładanie się w sali

- **Zakładając, że (Given)** w „Sali A” jest wystąpienie 10:00–11:00,
- **Gdy (When)** organizator doda w „Sali A” wystąpienie 10:30–11:30,
- **Wtedy (Then)** system odrzuca zapis z błędem `Session.RoomOverlapping`.

### Scenariusz 3: Wystąpienia następujące po sobie

- **Zakładając, że (Given)** w „Sali A” jest wystąpienie 10:00–11:00,
- **Gdy (When)** organizator doda w „Sali A” wystąpienie 11:00–12:00,
- **Wtedy (Then)** wystąpienie zostaje zapisane.

### Scenariusz 4: Wystąpienie poza czasem konferencji

- **Zakładając, że (Given)** konferencja trwa od 9:00 do 18:00,
- **Gdy (When)** organizator doda wystąpienie 17:30–18:30,
- **Wtedy (Then)** system odrzuca zapis z błędem `Session.InvalidTime`.

## Dodatkowe informacje

### Zależności

- Sale (US-04) i prelegenci (US-05).

### Notatki techniczne

- Endpointy: `GET/POST /api/conferences/{conferenceId}/sessions`, `GET/PATCH/DELETE /api/conferences/{conferenceId}/sessions/{sessionId}`.
- Nakładanie się sprawdzane jest w warstwie aplikacji i dodatkowo blokowane w bazie ograniczeniem `EXCLUDE USING gist` na przedziale `tstzrange(StartTime, EndTime)` dla aktywnych wystąpień.

### Przypadki brzegowe

- Dwa równoczesne żądania dodania kolidujących wystąpień – drugie zostaje odrzucone przez ograniczenie w bazie.
- Edycja wystąpienia bez zmiany godzin – nie koliduje samo ze sobą.
- Miękko usunięte wystąpienie – nie blokuje sali.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 13 – suma elementów backlogu: BE-11 (#26): 8, FE-07 (Front#7): 5
- **Status:** backend – Done; frontend – FE-07 – Backlog
- **Sprint docelowy:** Sprint 4
