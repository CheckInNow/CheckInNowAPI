# Historyjka: Ostrzeżenie o nakładających się ulubionych wystąpieniach

## Opis

**Jako** uczestnik układający swój plan konferencji,  
**chcę** zostać ostrzeżony, gdy wybrane przeze mnie wystąpienia nakładają się godzinowo,  
**aby** świadomie zdecydować, na które z nich pójść.

## Kryteria akceptacji

- [ ] Przy dodawaniu wystąpienia do ulubionych system sprawdza, czy nakłada się ono z innymi ulubionymi wystąpieniami uczestnika.
- [ ] Nakładanie się nie blokuje dodania – wystąpienie zostaje dodane, a uczestnik otrzymuje ostrzeżenie.
- [ ] Ostrzeżenie zawiera listę kolidujących wystąpień (tytuł, sala, godziny).
- [ ] Plan konferencji oznacza wszystkie nakładające się wystąpienia.
- [ ] Wystąpienia następujące bezpośrednio po sobie (koniec jednego = początek drugiego) nie są traktowane jako kolizja.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Dodanie kolidującego wystąpienia

- **Zakładając, że (Given)** w planie uczestnika jest wystąpienie 10:00–11:00,
- **Gdy (When)** doda do ulubionych wystąpienie 10:30–11:30 w innej sali,
- **Wtedy (Then)** wystąpienie zostaje dodane, a uczestnik widzi ostrzeżenie o kolizji z pierwszym wystąpieniem.

### Scenariusz 2: Brak kolizji

- **Zakładając, że (Given)** w planie uczestnika jest wystąpienie 10:00–11:00,
- **Gdy (When)** doda do ulubionych wystąpienie 11:00–12:00,
- **Wtedy (Then)** wystąpienie zostaje dodane bez ostrzeżenia.

### Scenariusz 3: Rozwiązanie kolizji

- **Zakładając, że (Given)** dwa wystąpienia w planie są oznaczone jako kolidujące,
- **Gdy (When)** uczestnik usunie jedno z nich z ulubionych,
- **Wtedy (Then)** oznaczenie kolizji znika z pozostałego wystąpienia.

## Dodatkowe informacje

### Zależności

- Własny plan konferencji (US-10).

### Notatki techniczne

- Odpowiedź na dodanie do ulubionych oraz lista planu zawierają informację o kolidujących wystąpieniach (np. pole `overlappingSessionIds`).
- Warunek nakładania się taki sam jak w agendzie: `start1 < end2 AND end1 > start2`.

### Przypadki brzegowe

- Organizator zmienia godziny wystąpienia z planu uczestnika – kolizje są wyliczane na bieżąco przy pobieraniu planu.
- Więcej niż dwa wystąpienia nakładające się naraz – wszystkie są oznaczone.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 3 – suma elementów backlogu: BE-22 (#59): 2, FE-12 (Front#12): 1
- **Status:** backend – Backlog; frontend – FE-12 – Backlog
- **Sprint docelowy:** Sprint 5
