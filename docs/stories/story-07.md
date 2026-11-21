# Historyjka: Zapis na konferencję

## Opis

**Jako** zalogowany uczestnik,  
**chcę** zapisać się na wybraną konferencję,  
**aby** zarezerwować miejsce i otrzymać bilet.

## Kryteria akceptacji

- [x] Zapisać się może tylko zalogowany użytkownik z potwierdzonym adresem e-mail.
- [x] Zapis jest możliwy tylko przed rozpoczęciem konferencji.
- [x] Liczba zapisów nie może przekroczyć limitu uczestników konferencji.
- [x] Użytkownik może zapisać się na daną konferencję tylko raz.
- [x] Uczestnik może zrezygnować z zapisu przed rozpoczęciem konferencji.
- [x] Właściciel konferencji widzi stronicowaną listę zapisanych osób.
- [x] Szczegóły zapisu widzi tylko zapisany użytkownik i właściciel konferencji.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Pomyślny zapis

- **Zakładając, że (Given)** użytkownik ma potwierdzony e-mail, a konferencja ma wolne miejsca i jeszcze się nie rozpoczęła,
- **Gdy (When)** kliknie „Zapisz się”,
- **Wtedy (Then)** system tworzy zapis i zwraca `201 Created`.

### Scenariusz 2: Brak wolnych miejsc

- **Zakładając, że (Given)** liczba zapisów osiągnęła limit,
- **Gdy (When)** kolejny użytkownik spróbuje się zapisać,
- **Wtedy (Then)** system zwraca `409 Conflict` z błędem `Registration.ParticipantLimitReached`.

### Scenariusz 3: Niepotwierdzony e-mail

- **Zakładając, że (Given)** użytkownik nie potwierdził adresu e-mail,
- **Gdy (When)** spróbuje się zapisać,
- **Wtedy (Then)** system zwraca `403 Forbidden` z prośbą o potwierdzenie adresu.

### Scenariusz 4: Rezygnacja po rozpoczęciu

- **Zakładając, że (Given)** konferencja już się rozpoczęła,
- **Gdy (When)** uczestnik spróbuje zrezygnować z zapisu,
- **Wtedy (Then)** system zwraca `409 Conflict`.

## Dodatkowe informacje

### Zależności

- Rejestracja i potwierdzenie e-maila (US-01).
- Istniejąca konferencja (US-03).

### Notatki techniczne

- Endpointy: `GET/POST /api/conferences/{conferenceId}/registrations`, `GET/DELETE /api/conferences/{conferenceId}/registrations/{registrationId}`.
- Rezygnacja usuwa zapis trwale i zwalnia miejsce.

### Przypadki brzegowe

- Ponowny zapis na tę samą konferencję – `409 Conflict` z błędem `Registration.AlreadyRegistered`.
- Zapis na usuniętą konferencję – `404 Not Found`.
- Brak listy „moje zapisy” – uczestnik zna identyfikator zapisu tylko z odpowiedzi na zapis (BE-18, issue #36).

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 7 – suma elementów backlogu: BE-14 (#32): 5, FE-08 (Front#8): 2
- **Status:** backend – Done; frontend – FE-08 – Backlog
- **Sprint docelowy:** Sprint 4
