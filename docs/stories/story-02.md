# Historyjka: Logowanie przez Google

## Opis

**Jako** użytkownik posiadający konto Google,  
**chcę** zalogować się do systemu jednym kliknięciem przez Google,  
**aby** nie musieć zakładać i pamiętać osobnego hasła.

## Kryteria akceptacji

- [x] Frontend przekazuje do API token ID otrzymany od Google.
- [x] Backend weryfikuje token po stronie serwera i odrzuca nieprawidłowe tokeny.
- [x] Logowanie jest możliwe tylko dla adresów e-mail zweryfikowanych przez Google.
- [x] Jeśli konto o danym adresie nie istnieje, system tworzy je z rolą `User` i potwierdzonym adresem e-mail.
- [x] Jeśli konto istnieje i ma potwierdzony adres, logowanie Google zostaje do niego dołączone.
- [x] Zablokowane konto nie może zalogować się przez Google.
- [x] Po udanym logowaniu użytkownik otrzymuje token JWT oraz refresh token.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Pierwsze logowanie przez Google

- **Zakładając, że (Given)** użytkownik nie ma konta w systemie,
- **Gdy (When)** zaloguje się przez Google,
- **Wtedy (Then)** system tworzy nowe konto i zwraca tokeny dostępowe.

### Scenariusz 2: Połączenie z istniejącym kontem

- **Zakładając, że (Given)** użytkownik ma potwierdzone konto e-mail z tym samym adresem co konto Google,
- **Gdy (When)** zaloguje się przez Google,
- **Wtedy (Then)** logowanie Google zostaje dołączone do istniejącego konta i użytkownik jest zalogowany.

### Scenariusz 3: Istniejące konto z niepotwierdzonym adresem

- **Zakładając, że (Given)** istnieje konto z tym adresem, ale adres nie został potwierdzony,
- **Gdy (When)** użytkownik zaloguje się przez Google,
- **Wtedy (Then)** system odmawia logowania z informacją o niepotwierdzonym adresie e-mail.

### Scenariusz 4: Zablokowane konto

- **Zakładając, że (Given)** konto użytkownika zostało zablokowane przez administratora,
- **Gdy (When)** użytkownik zaloguje się przez Google,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Projekt w Google Cloud z identyfikatorem klienta OAuth (`Authentication__Google__ClientId`).
- Biblioteka logowania Google po stronie frontendu.

### Notatki techniczne

- Endpoint: `POST /api/identity/google`.
- Weryfikacja tokenu biblioteką `Google.Apis.Auth`.

### Przypadki brzegowe

- Wygasły lub sfałszowany token Google – błąd `OAuth.InvalidToken`.
- Adres niezweryfikowany przez Google – błąd `OAuth.GoogleEmailNotVerified`.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 7 – suma elementów backlogu: BE-04 (#8): 5, FE-03 (Front#3): 2
- **Status:** backend – Done; frontend – FE-03 – Todo
- **Sprint docelowy:** Sprint 3
