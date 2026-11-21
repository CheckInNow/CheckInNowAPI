# Historyjka: Zarządzanie użytkownikami przez administratora

## Opis

**Jako** administrator,  
**chcę** przeglądać konta użytkowników, nadawać i odbierać rolę organizatora oraz blokować konta,  
**aby** kontrolować, kto może organizować konferencje, i reagować na nadużycia.

## Kryteria akceptacji

- [x] Funkcje są dostępne tylko dla roli `Admin`.
- [x] Administrator widzi stronicowaną listę użytkowników z możliwością filtrowania po e-mailu lub imieniu i nazwisku.
- [x] Administrator widzi szczegóły użytkownika wraz z jego rolami.
- [x] Administrator może nadać i odebrać rolę `Organizer`; rola `User` zostaje zachowana.
- [x] Administrator może zablokować konto (trwała blokada logowania) bez usuwania danych użytkownika.
- [x] Administrator może odblokować konto.
- [x] Można zablokować konto tymczasowo zablokowane po nieudanych logowaniach.
- [x] Administrator nie może zablokować samego siebie ani zmienić własnych ról.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Nadanie roli organizatora

- **Zakładając, że (Given)** użytkownik ma tylko rolę `User`,
- **Gdy (When)** administrator nada mu rolę `Organizer`,
- **Wtedy (Then)** użytkownik może tworzyć konferencje.

### Scenariusz 2: Blokada konta

- **Zakładając, że (Given)** użytkownik narusza zasady serwisu,
- **Gdy (When)** administrator zablokuje jego konto,
- **Wtedy (Then)** użytkownik nie może się zalogować ani odświeżyć tokenu, a jego dane pozostają w systemie.

### Scenariusz 3: Ponowna blokada

- **Zakładając, że (Given)** konto jest już zablokowane przez administratora,
- **Gdy (When)** administrator spróbuje zablokować je ponownie,
- **Wtedy (Then)** system zwraca `409 Conflict`.

### Scenariusz 4: Zmiana własnych uprawnień

- **Zakładając, że (Given)** administrator jest zalogowany,
- **Gdy (When)** spróbuje zablokować siebie lub zmienić swoje role,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Konto administratora – obecnie tworzone tylko przez seeder w środowisku deweloperskim; sposób utworzenia konta w produkcji do ustalenia.

### Notatki techniczne

- Endpointy: `GET /api/admin/users`, `GET /api/admin/users/{id}`, `POST/DELETE /api/admin/users/{id}/roles/organizer`, `POST /api/admin/users/{id}/ban`, `POST /api/admin/users/{id}/unban`.
- Blokada realizowana mechanizmem lockout z ASP.NET Core Identity.
- Blokada dotyczy konta, nie adresu IP (ustalenie z event stormingu).

### Przypadki brzegowe

- Odebranie roli `Organizer` użytkownikowi bez tej roli – `409 Conflict`.
- Odebranie roli organizatorowi z aktywnymi konferencjami – konferencje pozostają, ale nie może nimi zarządzać do czasu ponownego nadania roli.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 14 – suma elementów backlogu: BE-07 (#13): 2, BE-10 (#19): 5, BE-16 (#31): 2, BE-24 (#50): 2, FE-15 (Front#15): 3
- **Status:** backend – częściowo (pozostało: BE-24); frontend – FE-15 – Backlog
- **Sprint docelowy:** Sprint 6
