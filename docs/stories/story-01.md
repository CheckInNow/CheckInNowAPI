# Historyjka: Rejestracja przez e-mail z potwierdzeniem adresu

## Opis

**Jako** nowy użytkownik,  
**chcę** założyć konto za pomocą adresu e-mail i hasła oraz potwierdzić swój adres,  
**aby** móc zapisywać się na konferencje i korzystać z systemu.

## Kryteria akceptacji

- [x] Formularz rejestracji zawiera imię, nazwisko, e-mail, hasło i powtórzenie hasła.
- [x] Imię i nazwisko są wymagane (maks. 100 znaków), e-mail musi mieć poprawny format, hasło ma co najmniej 6 znaków, a oba hasła muszą być identyczne.
- [x] Nie można założyć drugiego konta na ten sam adres e-mail.
- [x] Nowe konto otrzymuje rolę `User`.
- [x] Po rejestracji system wysyła e-mail z linkiem potwierdzającym ważnym 24 godziny.
- [x] Użytkownik może poprosić o ponowne wysłanie linku (maks. 3 żądania na 15 minut z jednego adresu IP).
- [x] Konto niepotwierdzone w ciągu 7 dni jest automatycznie usuwane.
- [x] Użytkownik loguje się e-mailem i hasłem i otrzymuje token JWT oraz refresh token.
- [x] Po 5 nieudanych próbach logowania konto jest blokowane na 15 minut.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Pomyślna rejestracja

- **Zakładając, że (Given)** użytkownik nie ma konta w systemie,
- **Gdy (When)** wypełni poprawnie formularz rejestracji i go wyśle,
- **Wtedy (Then)** konto zostaje utworzone, a na podany adres trafia e-mail z linkiem potwierdzającym.

### Scenariusz 2: Potwierdzenie adresu e-mail

- **Zakładając, że (Given)** użytkownik otrzymał e-mail z linkiem potwierdzającym,
- **Gdy (When)** kliknie link w ciągu 24 godzin,
- **Wtedy (Then)** adres e-mail zostaje oznaczony jako potwierdzony i użytkownik może zapisywać się na konferencje.

### Scenariusz 3: Wygasły link

- **Zakładając, że (Given)** od wysłania linku minęło ponad 24 godziny,
- **Gdy (When)** użytkownik kliknie link,
- **Wtedy (Then)** system informuje, że link jest nieprawidłowy, i pozwala wysłać nowy.

### Scenariusz 4: Zbyt wiele nieudanych logowań

- **Zakładając, że (Given)** użytkownik 5 razy podał błędne hasło,
- **Gdy (When)** spróbuje zalogować się ponownie,
- **Wtedy (Then)** system odmawia logowania z informacją o zablokowanym koncie przez 15 minut.

## Dodatkowe informacje

### Zależności

- Usługa wysyłki e-maili (Mailgun w produkcji, Mailpit lokalnie).
- Adres frontendu do budowania linku potwierdzającego.

### Notatki techniczne

- Endpointy: `POST /api/identity/register`, `/login`, `/refresh`, `/confirm-email`, `/resend-verification`.
- ASP.NET Core Identity z własnym dostawcą tokenów potwierdzających (ważność 24 h).
- Usuwanie niepotwierdzonych kont realizuje usługa działająca w tle (`UnconfirmedUserCleanupService`).
- Token dostępowy JWT ważny 60 minut, refresh token 7 dni.

### Przypadki brzegowe

- Ponowne wysłanie linku dla nieistniejącego adresu – odpowiedź identyczna jak dla istniejącego, aby nie ujawniać kont.
- Przekroczony limit ponownych wysyłek – odpowiedź `429 Too Many Requests`.
- Niepotwierdzony użytkownik – może się zalogować i przeglądać konferencje, ale nie może się na nie zapisać.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 15 – suma elementów backlogu: BE-05 (#11): 5, BE-06 (#12): 5, BE-12 (#16): 2, FE-02 (Front#2): 3
- **Status:** backend – Done; frontend – FE-02 – Todo
- **Sprint docelowy:** Sprint 3
