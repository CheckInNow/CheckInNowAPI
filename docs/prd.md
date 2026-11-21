# PRD: CheckInNow

- **Autor:** Julia Kozłowska
- **Data:** 19.11.2026
- **Status:** Wersja robocza
- **Zespół:**
  - Julia Kozłowska – backend, dokumentacja
  - Stanislav Zhuk – backend
  - Paweł Miedzieński – frontend

---

## 1. Podsumowanie

CheckInNow to system internetowy do organizacji konferencji. Organizator tworzy w nim konferencję, sale, listę prelegentów i agendę wystąpień. Uczestnik zapisuje się na konferencję, otrzymuje indywidualny bilet z kodem QR, układa własny plan wystąpień i ocenia wystąpienia, w których brał udział. Przy wejściu obsługa skanuje bilet telefonem, a system oznacza uczestnika jako obecnego i sygnalizuje ponowne użycie biletu. Organizator widzi statystyki zapisów, obecności, frekwencji i ocen, a administrator zarządza użytkownikami i przegląda logi aplikacji.

Repozytorium `CheckInNowAPI` zawiera backend (REST API udokumentowane w OpenAPI). Frontend jest osobną aplikacją (zob. [ADR-005](adr/ADR-005-frontend.md)).

Projekt realizowany jest w 2-tygodniowych sprintach od 19.10.2026, z terminem oddania **10.01.2027**.

---

## 2. Opis Problemu

### Jaki problem rozwiązujemy?

Małe i średnie konferencje (meetupy, konferencje branżowe, wydarzenia uczelniane) często obsługiwane są zestawem niepowiązanych narzędzi: formularz zapisów, arkusz kalkulacyjny z listą uczestników, agenda w PDF i ręczne odhaczanie nazwisk przy wejściu. Prowadzi to do:

- kolejek przy wejściu i błędów przy wyszukiwaniu uczestników na papierowej liście,
- braku kontroli nad limitem miejsc i nad tym, czy ktoś nie wchodzi drugi raz na ten sam bilet,
- konfliktów w agendzie (dwa wystąpienia w tej samej sali o tej samej porze),
- braku wiarygodnych danych po wydarzeniu: ile osób faktycznie przyszło, które wystąpienia były popularne i jak zostały ocenione,
- trudności uczestników w ułożeniu własnego planu, gdy wystąpienia odbywają się równolegle.

CheckInNow łączy zapisy, agendę, bilety QR, check-in i statystyki w jednym systemie.

### Dla kogo jest ten produkt?

- **Uczestnicy konferencji** – osoby, które chcą szybko zapisać się na wydarzenie, mieć bilet zawsze pod ręką w telefonie i zaplanować, na które wystąpienia pójść (persona: [Karolina](personas/persona-01.md)).
- **Organizatorzy konferencji** – osoby i firmy przygotowujące wydarzenia, które potrzebują jednego miejsca do budowy agendy, kontroli zapisów i oceny sukcesu wydarzenia (persona: [Tomasz](personas/persona-02.md)).
- **Obsługa wejścia** – osoby z zespołu organizatora, które w dniu konferencji skanują bilety na urządzeniach mobilnych (persona: [Ola](personas/persona-03.md)).
- **Administrator systemu** – osoba odpowiedzialna za konta użytkowników, nadawanie ról i diagnozowanie problemów na podstawie logów (persona: [Michał](personas/persona-04.md)).

---

## 3. Cele i Mierniki Sukcesu

### Cele projektu

1. Umożliwić organizatorowi przygotowanie kompletnej konferencji (sale, prelegenci, agenda bez kolizji) w jednym systemie.
2. Skrócić i uszczelnić wejście na konferencję dzięki biletom QR i skanowaniu na telefonie.
3. Dać uczestnikom narzędzie do zaplanowania udziału w wydarzeniu i oceny wystąpień.
4. Dostarczyć organizatorom wiarygodnych statystyk po wydarzeniu.
5. Spełnić wymagania techniczne projektu: rozdzielony frontend i backend, REST API opisane w OpenAPI, logowanie e-mailem i przez zewnętrznego dostawcę OAuth.

### Mierniki sukcesu

- **Czas obsługi jednej osoby przy wejściu:** poniżej 5 sekund od zeskanowania kodu do wyświetlenia wyniku.
- **Ponowne użycie biletu:** 100% prób ponownego wejścia na ten sam bilet jest sygnalizowanych obsłudze.
- **Kolizje w agendzie:** 0 nakładających się wystąpień w tej samej sali (wymuszane przez system).
- **Przekroczenie limitu miejsc:** 0 zapisów ponad limit uczestników konferencji.
- **Wykorzystanie planu konferencji:** co najmniej 40% zapisanych uczestników dodaje do ulubionych przynajmniej jedno wystąpienie.
- **Zaangażowanie w oceny:** co najmniej 25% obecnych na wystąpieniu uczestników wystawia mu ocenę.
- **Dokumentacja API:** 100% endpointów opisanych w dokumencie OpenAPI.

---

## 4. Wymagania i Zakres

### Kluczowe historyjki użytkownika

| ID | Historyjka | Priorytet | Story Points | Status backendu | Sprint docelowy |
| -- | ---------- | --------- | ------------ | --------------- | --------------- |
| US-01 | [Rejestracja przez e-mail z potwierdzeniem adresu](stories/story-01.md) | M | 15 | Done | Sprint 3 |
| US-02 | [Logowanie przez Google](stories/story-02.md) | M | 7 | Done | Sprint 3 |
| US-03 | [Tworzenie i zarządzanie konferencją](stories/story-03.md) | M | 11 | Częściowo (BE-23) | Sprint 4 |
| US-04 | [Dodawanie sal konferencyjnych](stories/story-04.md) | M | 8 (razem z US-05) | Częściowo (BE-29) | Sprint 6 |
| US-05 | [Dodawanie prelegentów](stories/story-05.md) | M | 8 (razem z US-04) | Częściowo (BE-29) | Sprint 6 |
| US-06 | [Budowanie agendy bez nakładania się wystąpień](stories/story-06.md) | M | 13 | Done | Sprint 4 |
| US-07 | [Zapis na konferencję](stories/story-07.md) | M | 7 | Done | Sprint 4 |
| US-08 | [Bilet z kodem QR](stories/story-08.md) | M | 6 | Częściowo (BE-18) | Sprint 5 |
| US-09 | [Check-in uczestnika przez skanowanie kodu QR](stories/story-09.md) | M | 18 | In progress | Sprint 5 |
| US-10 | [Własny plan konferencji (ulubione wystąpienia)](stories/story-10.md) | M | 6 | Backlog | Sprint 5 |
| US-11 | [Ostrzeżenie o nakładających się ulubionych wystąpieniach](stories/story-11.md) | M | 3 | Backlog | Sprint 5 |
| US-12 | [Ocenianie zakończonych wystąpień](stories/story-12.md) | M | 7 | Backlog | Sprint 6 |
| US-13 | [Statystyki konferencji dla organizatora](stories/story-13.md) | M | 8 | Backlog | Sprint 6 |
| US-14 | [Zarządzanie użytkownikami przez administratora](stories/story-14.md) | M | 14 | Częściowo (BE-24) | Sprint 6 |
| US-15 | [Przeglądanie logów aplikacji przez administratora](stories/story-15.md) | M | 9 | Backlog | Sprint 6 |

Story Points historyjki to suma zaplanowanych elementów backlogu (backend i frontend). „Sprint docelowy” to sprint, w którym historyjka ma być ukończona razem z frontendem. Szczegółowy podział na elementy, priorytety (MoSCoW), estymacje i sprinty znajduje się w [backlogu](backlog.csv).

### Zakres projektu

**W zakresie (In Scope):**

- rejestracja i logowanie przez e-mail i hasło z potwierdzeniem adresu e-mail oraz resetem hasła,
- logowanie przez Google (OAuth),
- role: uczestnik (`User`), organizator (`Organizer`), administrator (`Admin`),
- zarządzanie konferencjami (nazwa, opis, daty, miejsce, limit uczestników) i filtrowanie listy konferencji na nadchodzące i zakończone,
- zarządzanie salami i prelegentami w ramach konferencji,
- agenda wystąpień (tytuł, opis, prelegent, sala, godzina rozpoczęcia i zakończenia) z blokadą nakładania się wystąpień w tej samej sali,
- zapisy na konferencję z kontrolą limitu miejsc i możliwością rezygnacji przed rozpoczęciem,
- indywidualny bilet z kodem QR,
- check-in na konferencję i na wystąpienia przez skanowanie kodu QR, z sygnalizacją ponownego użycia biletu,
- własny plan konferencji (ulubione wystąpienia) z ostrzeżeniem o nakładaniu się godzin,
- oceny zakończonych wystąpień,
- statystyki dla organizatora: liczba zapisanych, liczba obecnych, frekwencja na wystąpieniach, średnie oceny,
- zarządzanie użytkownikami (role, blokowanie kont) i podgląd logów przez administratora,
- dokumentacja API w formacie OpenAPI.

**Opcjonalne rozszerzenia (Could):**

Elementy zaplanowane na Sprint 6 i realizowane tylko przy wolnej pojemności; nie są powiązane z historyjkami i nie wliczają się do ich Story Points.

- centralizacja soft delete (BE-30),
- szablony wiadomości e-mail w plikach HTML w wielu językach (BE-32).

**Poza zakresem (Out of Scope):**

- płatności i bilety płatne,
- check-in w trybie offline (ustalenie z [event stormingu](event-storming/event-storming.md)),
- lista oczekujących na zwolnione miejsca,
- powiadomienia push i SMS,
- wielu administratorów systemu (administrator jest jeden),
- blokowanie użytkowników po adresie IP (blokada dotyczy konta).

---

## 5. Doświadczenie Użytkownika (UX) i Projekt Interfejsu (UI)

- Interfejs dzieli się na trzy obszary odpowiadające rolom: **uczestnik** (lista konferencji, zapisy, bilet QR, mój plan, oceny), **organizator** (moje konferencje, sale, prelegenci, agenda, skanowanie biletów, statystyki) i **administrator** (użytkownicy, logi).
- Bilet z kodem QR musi być czytelny na ekranie telefonu, z dużym kodem i nazwą konferencji. Generowanie i odczyt kodów QR odbywa się w całości po stronie frontendu: obraz QR powstaje z kodu biletu zwracanego przez API.
- Widok skanera biletów jest projektowany w pierwszej kolejności pod urządzenia mobilne: podgląd z kamery na cały ekran, duży i jednoznaczny wynik skanowania w kolorach (sukces / bilet już użyty / bilet nieprawidłowy) oraz imię i nazwisko uczestnika. Po wyświetleniu wyniku skaner jest od razu gotowy na kolejny bilet.
- Agenda prezentowana jest jako siatka czasowa z podziałem na sale. Wystąpienia dodane do ulubionych są wyróżnione, a nakładające się ulubione wystąpienia oznaczone ostrzeżeniem.
- Komunikaty błędów API zwracane są w formacie ProblemDetails i tłumaczone na czytelne komunikaty w interfejsie.
- Interfejs jest responsywny (Tailwind CSS, shadcn/ui), szczegóły w [ADR-005](adr/ADR-005-frontend.md).

---

## 6. Kwestie Techniczne

- **Backend:** .NET 10, ASP.NET Core Minimal APIs, Clean Architecture (Domain, Application, Infrastructure, Api), CQRS z MediatR, walidacja FluentValidation, wzorzec Result i ProblemDetails – [ADR-001](adr/ADR-001-architektura.md), [ADR-004](adr/ADR-004-api.md).
- **Baza danych:** PostgreSQL 18 z EF Core. Nakładanie się wystąpień w sali dodatkowo blokowane ograniczeniem `EXCLUDE USING gist` na przedziałach czasu – [ADR-002](adr/ADR-002-baza-danych.md).
- **Cache:** Redis jako L2 dla HybridCache.
- **Uwierzytelnianie:** ASP.NET Core Identity, JWT z refresh tokenem, logowanie przez Google, blokada konta po 5 nieudanych próbach logowania na 15 minut – [ADR-003](adr/ADR-003-uwierzytelnianie.md).
- **E-mail:** Mailgun (produkcja), Mailpit (lokalnie). Link potwierdzający jest ważny 24 godziny, niepotwierdzone konta są usuwane po 7 dniach.
- **Dokumentacja API:** OpenAPI (`Microsoft.AspNetCore.OpenApi`) z interfejsem Scalar.
- **Kody QR:** bilet zawiera losowy kod (`TicketCode`), który frontend zamienia na obraz QR. Odczyt kodu odbywa się w przeglądarce urządzenia mobilnego, a backend weryfikuje kod i zapisuje obecność (tabele `ConferenceCheckIn` i `SessionCheckIn` z event stormingu).
- **Logowanie i monitoring:** Serilog i OpenTelemetry. Do realizacji podglądu logów przez administratora potrzebny jest trwały magazyn logów (zob. otwarte pytania).
- **Frontend:** React 19, Vite, TypeScript, Tailwind CSS, shadcn/ui – [ADR-005](adr/ADR-005-frontend.md).
- **Testy:** xUnit v3, FluentAssertions, NSubstitute, EF Core InMemory, NetArchTest – [ADR-006](adr/ADR-006-testowanie.md).
- **Uruchomienie i wdrożenie:** .NET Aspire lub Docker Compose lokalnie, GitHub Actions (build i testy, wdrożenie), Microsoft Azure z Cloudflare.

---

## 7. Otwarte Pytania i Ryzyka

- **Kto może skanować bilety?** Obecnie operacje na konferencji może wykonywać tylko jej właściciel (organizator). Obsługa wejścia to często kilka osób.
  - *Ryzyko:* współdzielenie jednego konta organizatora przez kilka osób.
  - *Mitygacja:* przypisywanie do konferencji dodatkowych osób z uprawnieniem tylko do skanowania biletów (BE-20).
- **Gdzie przechowywać logi dla administratora?** Serilog zapisuje obecnie logi tylko na konsolę.
  - *Ryzyko:* brak danych do wyświetlenia w panelu administratora, rozrost tabeli logów w bazie.
  - *Mitygacja:* zapis logów do tabeli w PostgreSQL z okresem retencji lub odczyt z zewnętrznego magazynu; decyzja do opisania w ADR-007 (DOC-06).
- **Jaka jest skala ocen i czy ocena może zawierać komentarz?** Roboczo przyjęto skalę 1–5 bez komentarza.
  - *Ryzyko:* zmiana skali po zebraniu ocen utrudni porównywanie statystyk.
  - *Mitygacja:* ustalić skalę przed implementacją US-12.
- **Czy wejście na wystąpienie wymaga osobnego skanowania?** Event storming zakłada tabelę `SessionCheckIn`, a frekwencja na wystąpieniach i prawo do oceny zależą od skanowania przy sali.
  - *Ryzyko:* kolejki przy salach i dodatkowa obsługa przy każdym wystąpieniu.
  - *Mitygacja:* skanowanie przy sali tym samym widokiem skanera, z wyborem wystąpienia przez obsługę.
- **Jak uczestnik znajduje swój bilet?** API zwraca identyfikator zapisu tylko w odpowiedzi na zapis, brakuje endpointu z listą zapisów bieżącego użytkownika.
  - *Ryzyko:* uczestnik nie odnajdzie biletu po ponownym zalogowaniu.
  - *Mitygacja:* endpoint „moje zapisy” ujęty w backlogu (BE-18, issue #36).
- **Słaby zasięg w miejscu konferencji.** Check-in offline jest poza zakresem.
  - *Ryzyko:* zatrzymanie wejścia przy awarii sieci.
  - *Mitygacja:* lekkie odpowiedzi API i zalecenie organizatorom zapewnienia Wi-Fi dla obsługi wejścia.
- **Brak resetu hasła.** ADR-003 zakłada e-maile z resetem hasła, ale funkcja nie jest zaimplementowana.
  - *Ryzyko:* użytkownik bez dostępu do hasła traci dostęp do biletu.
  - *Mitygacja:* reset hasła ujęty w backlogu (BE-25).
- **Brak konta administratora w produkcji.** Konto administratora tworzy obecnie tylko seeder w środowisku deweloperskim.
  - *Ryzyko:* funkcje administracyjne są niedostępne po wdrożeniu.
  - *Mitygacja:* utworzenie konta administratora z konfiguracji przy pierwszym uruchomieniu (BE-24).
- **Harmonogram frontendu.** Cały frontend (43 SP) realizuje jedna osoba, a jej velocity nie jest jeszcze znane.
  - *Ryzyko:* niedokończone widoki w dniu oddania projektu.
  - *Mitygacja:* weryfikacja velocity po Sprincie 3 i ewentualne przesunięcie części widoków do zespołu backendowego.
