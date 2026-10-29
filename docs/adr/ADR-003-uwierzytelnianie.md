# ADR-003: Uwierzytelnianie i autoryzacja

- **Status:** Zaakceptowana
- **Data:** 29.10.2026
- **Autorzy:** Julia Kozłowska, Stanislav Zhuk, Paweł Miedzieński

---

## Kontekst

Użytkownik musi mieć możliwość rejestracji na dwa sposoby: przez e-mail i hasło oraz przez zewnętrznego dostawcę OAuth. System przewiduje trzy role: uczestnik, organizator i administrator.

Frontend i backend to osobne aplikacje, a bilety skanowane są na urządzeniach mobilnych, dlatego uwierzytelnianie musi działać bez sesji po stronie serwera. Rejestracja przez e-mail wymaga również wysyłania wiadomości, np. potwierdzenia adresu i resetu hasła.

---

## Rozważane opcje

### Opcja 1: ASP.NET Core Identity + tokeny JWT z refresh tokenami
- **Zalety:**
  - gotowe mechanizmy kont, haseł, ról i tokenów do potwierdzania e-maila,
  - dane użytkowników przechowywane są we własnej bazie, razem z biletami i ocenami,
  - JWT dobrze współpracuje z osobnym frontendem i urządzeniami mobilnymi,
  - łatwe dodanie logowania przez zewnętrznego dostawcę.
- **Wady:**
  - wydawanie i odświeżanie tokenów wymaga własnej implementacji,
  - wydanego tokenu nie można unieważnić przed jego wygaśnięciem.

### Opcja 2: Zewnętrzna usługa tożsamości (Auth0, Microsoft Entra ID, Keycloak)
- **Zalety:**
  - gotowe logowanie społecznościowe i uwierzytelnianie wieloskładnikowe,
  - mniej kodu związanego z bezpieczeństwem.
- **Wady:**
  - zależność od zewnętrznej usługi i możliwe koszty,
  - dane użytkowników znajdują się poza bazą aplikacji, co utrudnia łączenie ich z pozostałymi danymi.

### Opcja 3: Sesje oparte na ciasteczkach
- **Zalety:**
  - prosta konfiguracja i łatwe wylogowanie użytkownika.
- **Wady:**
  - gorzej sprawdzają się, gdy frontend i backend działają na różnych domenach,
  - mniej wygodne dla aplikacji mobilnych.

## Decyzja

Wybrano **ASP.NET Core Identity 10.0.13** z tokenami **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.13) i **refresh tokenami**. Jako zewnętrznego dostawcę OAuth wybrano **Google**. E-maile wysyłane są przez **Mailgun**, a podczas pracy lokalnej przechwytuje je **Mailpit**. Wersje integracji z Google, Mailgun i Mailpit zostaną ustalone przy ich wdrożeniu.

Identity zapewnia gotowe i bezpieczne mechanizmy kont oraz ról, a dane użytkowników pozostają w bazie aplikacji. JWT pasuje do osobnego frontendu i urządzeń do check-inu. Google wybrano ze względu na największą liczbę użytkowników i oficjalne wsparcie logowania przez Google w .NET.

**Konsekwencje:**

- zespół odpowiada za bezpieczne wydawanie, odświeżanie i unieważnianie tokenów,
- krótki czas życia tokenu ogranicza skutki jego wycieku, a refresh token eliminuje potrzebę ponownego logowania,
- konieczna jest konfiguracja i bezpieczne przechowywanie sekretów (klucz JWT, dane Google OAuth, klucz Mailgun),
- wysyłka e-maili w środowisku produkcyjnym zależy od zewnętrznej usługi.
