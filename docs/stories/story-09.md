# Historyjka: Check-in uczestnika przez skanowanie kodu QR

## Opis

**Jako** organizator lub osoba z obsługi wejścia,  
**chcę** zeskanować telefonem kod QR z biletu uczestnika,  
**aby** oznaczyć go jako obecnego i od razu wiedzieć, czy bilet nie został już użyty.

## Kryteria akceptacji

- [ ] Skanować bilety może właściciel konferencji (oraz osoby z uprawnieniem do skanowania, jeśli zostanie wprowadzone – BE-20).
- [ ] Widok skanera działa w przeglądarce urządzenia mobilnego i korzysta z aparatu.
- [ ] Po zeskanowaniu poprawnego biletu system zapisuje obecność uczestnika na konferencji wraz z czasem wejścia.
- [ ] Odpowiedź zawiera imię i nazwisko uczestnika.
- [ ] Ponowne użycie tego samego biletu jest wyraźnie sygnalizowane wraz z czasem pierwszego użycia i nie tworzy kolejnego wpisu.
- [ ] Bilet nieprawidłowy (nieistniejący lub z innej konferencji) jest odrzucany i niczego nie zapisuje.
- [ ] Check-in jest możliwy także przy wejściu na konkretne wystąpienie, co zapisuje obecność na tym wystąpieniu.
- [ ] Wynik skanowania jest prezentowany jednoznacznie: sukces, bilet już użyty, bilet nieprawidłowy.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Pierwsze wejście

- **Zakładając, że (Given)** uczestnik jest zapisany na konferencję i jeszcze nie wszedł,
- **Gdy (When)** obsługa zeskanuje jego kod QR,
- **Wtedy (Then)** system zapisuje obecność i wyświetla komunikat sukcesu z imieniem i nazwiskiem.

### Scenariusz 2: Ponowne użycie biletu

- **Zakładając, że (Given)** bilet został już zeskanowany o 9:12,
- **Gdy (When)** obsługa zeskanuje go ponownie,
- **Wtedy (Then)** system sygnalizuje, że bilet został już użyty o 9:12, i nie zapisuje kolejnego wejścia.

### Scenariusz 3: Nieprawidłowy bilet

- **Zakładając, że (Given)** kod QR nie odpowiada żadnemu zapisowi na tę konferencję,
- **Gdy (When)** obsługa go zeskanuje,
- **Wtedy (Then)** system wyświetla informację o nieprawidłowym bilecie i niczego nie zapisuje.

### Scenariusz 4: Wejście na wystąpienie

- **Zakładając, że (Given)** uczestnik jest zapisany na konferencję,
- **Gdy (When)** obsługa przy sali wybierze wystąpienie i zeskanuje jego bilet,
- **Wtedy (Then)** system zapisuje obecność uczestnika na tym wystąpieniu.

## Dodatkowe informacje

### Zależności

- Bilet z kodem QR (US-08).
- Biblioteka do odczytu kodów QR w przeglądarce (wybór na etapie implementacji frontendu, ADR-005).

### Notatki techniczne

- Nowe encje `ConferenceCheckIn` i `SessionCheckIn` (ustalenie z event stormingu) z unikalnością zapisu na poziomie bazy, aby równoczesne skany nie utworzyły dwóch wejść.
- Proponowane endpointy: `POST /api/conferences/{conferenceId}/check-ins` i `POST /api/conferences/{conferenceId}/sessions/{sessionId}/check-ins` z kodem biletu w treści żądania.
- Ponowne użycie biletu sygnalizowane osobnym kodem odpowiedzi (np. `409 Conflict` z czasem pierwszego użycia).
- Tryb offline nie jest wspierany.

### Przypadki brzegowe

- Brak internetu przy wejściu – skaner pokazuje błąd połączenia, check-in nie jest wykonywany.
- Zapis anulowany przed konferencją – bilet traktowany jako nieprawidłowy.
- Skan przy wystąpieniu bez wcześniejszego check-inu na konferencję – do ustalenia (proponowane: dozwolony, zapisuje oba wejścia).
- Dwa telefony skanujące ten sam bilet w tej samej chwili – tylko jeden check-in zostaje zapisany.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 18 – suma elementów backlogu: BE-17 (#38): 5, BE-19 (#39): 3, BE-20 (#48): 5, FE-10 (Front#10): 5
- **Status:** backend – In progress; frontend – FE-10 – Backlog
- **Sprint docelowy:** Sprint 5
