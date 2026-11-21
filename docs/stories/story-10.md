# Historyjka: Własny plan konferencji (ulubione wystąpienia)

## Opis

**Jako** uczestnik zapisany na konferencję,  
**chcę** dodawać wybrane wystąpienia do ulubionych,  
**aby** mieć własny plan konferencji i szybko sprawdzić, gdzie mam być o danej godzinie.

## Kryteria akceptacji

- [ ] Do ulubionych może dodawać wystąpienia tylko zalogowany użytkownik zapisany na daną konferencję.
- [ ] Uczestnik może dodać wystąpienie do ulubionych i je z nich usunąć.
- [ ] To samo wystąpienie nie może zostać dodane do ulubionych dwukrotnie.
- [ ] Uczestnik widzi swój plan: listę ulubionych wystąpień posortowaną według godziny rozpoczęcia, z salą i prelegentem.
- [ ] W agendzie wystąpienia z ulubionych są oznaczone.
- [ ] Plan jest prywatny – nie widzą go inni użytkownicy.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Dodanie do ulubionych

- **Zakładając, że (Given)** uczestnik jest zapisany na konferencję,
- **Gdy (When)** doda wystąpienie do ulubionych,
- **Wtedy (Then)** wystąpienie pojawia się w jego planie.

### Scenariusz 2: Usunięcie z ulubionych

- **Zakładając, że (Given)** wystąpienie jest w planie uczestnika,
- **Gdy (When)** uczestnik usunie je z ulubionych,
- **Wtedy (Then)** wystąpienie znika z planu.

### Scenariusz 3: Użytkownik niezapisany

- **Zakładając, że (Given)** użytkownik nie jest zapisany na konferencję,
- **Gdy (When)** spróbuje dodać wystąpienie do ulubionych,
- **Wtedy (Then)** system zwraca `403 Forbidden` z informacją, że najpierw trzeba się zapisać.

## Dodatkowe informacje

### Zależności

- Agenda (US-06) i zapis na konferencję (US-07).

### Notatki techniczne

- Nowa encja `FavoriteSession` (użytkownik, wystąpienie) z unikalnym kluczem złożonym.
- Proponowane endpointy: `GET /api/conferences/{conferenceId}/favorites`, `POST/DELETE /api/conferences/{conferenceId}/sessions/{sessionId}/favorite`.
- Dane ulubionych mogą posłużyć w statystykach jako miara zainteresowania wystąpieniem.

### Przypadki brzegowe

- Organizator usuwa wystąpienie z ulubionych uczestników – wystąpienie znika z planu.
- Uczestnik rezygnuje z zapisu – jego ulubione dla tej konferencji są usuwane.
- Ponowne dodanie tego samego wystąpienia – operacja idempotentna, bez błędu.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 6 – suma elementów backlogu: BE-21 (#41): 3, FE-11 (Front#11): 3
- **Status:** backend – Backlog; frontend – FE-11 – Backlog
- **Sprint docelowy:** Sprint 5
