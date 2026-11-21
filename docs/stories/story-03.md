# Historyjka: Tworzenie i zarządzanie konferencją

## Opis

**Jako** organizator,  
**chcę** utworzyć konferencję z nazwą, opisem, datami, miejscem i limitem uczestników,  
**aby** udostępnić ją uczestnikom i rozpocząć zapisy.

## Kryteria akceptacji

- [x] Konferencję może utworzyć tylko użytkownik z rolą `Organizer`, który zostaje jej właścicielem.
- [x] Nazwa (maks. 200 znaków), miejsce (maks. 200 znaków), daty i limit uczestników są wymagane, opis jest opcjonalny (maks. 1000 znaków).
- [x] Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.
- [x] Limit uczestników musi być większy od zera.
- [x] Tylko właściciel może edytować i usuwać konferencję.
- [x] Usunięcie konferencji jest miękkie – dane pozostają w bazie, ale konferencja przestaje być widoczna.
- [x] Lista i szczegóły konferencji są dostępne publicznie, bez logowania.
- [x] Organizator widzi listę swoich konferencji.
- [ ] Listę konferencji można filtrować na nadchodzące i zakończone (BE-23, issue #37).

## Scenariusze (Given – When – Then)

### Scenariusz 1: Utworzenie konferencji

- **Zakładając, że (Given)** użytkownik ma rolę `Organizer`,
- **Gdy (When)** wypełni poprawnie formularz konferencji i go wyśle,
- **Wtedy (Then)** konferencja zostaje utworzona, a użytkownik zostaje jej właścicielem.

### Scenariusz 2: Błędne daty

- **Zakładając, że (Given)** organizator wypełnia formularz konferencji,
- **Gdy (When)** poda datę zakończenia wcześniejszą niż data rozpoczęcia,
- **Wtedy (Then)** system odrzuca zapis z błędem walidacji.

### Scenariusz 3: Edycja cudzej konferencji

- **Zakładając, że (Given)** organizator nie jest właścicielem konferencji,
- **Gdy (When)** spróbuje ją edytować lub usunąć,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

### Scenariusz 4: Użytkownik bez roli organizatora

- **Zakładając, że (Given)** użytkownik ma tylko rolę `User`,
- **Gdy (When)** spróbuje utworzyć konferencję,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Nadanie roli `Organizer` przez administratora (US-14).

### Notatki techniczne

- Endpointy: `GET/POST /api/conferences`, `GET/PATCH/DELETE /api/conferences/{id}`, `GET /api/conferences/my`.
- Edycja jest częściowa (`PATCH`) – zmieniane są tylko przesłane pola.
- Listy są stronicowane (`PagedResult`).

### Przypadki brzegowe

- Konferencja miękko usunięta – `404 Not Found` przy pobieraniu szczegółów.
- Zmniejszenie limitu poniżej liczby zapisanych osób – do ustalenia (obecnie dozwolone, nowe zapisy są blokowane).

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 11 – suma elementów backlogu: BE-08 (#14): 5, BE-13 (#22): 1, BE-23 (#37): 2, FE-04 (Front#4): 3
- **Status:** backend – częściowo (pozostało: BE-23); frontend – FE-04 – Backlog
- **Sprint docelowy:** Sprint 4
