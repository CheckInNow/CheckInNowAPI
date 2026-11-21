# Historyjka: Bilet z kodem QR

## Opis

**Jako** uczestnik zapisany na konferencję,  
**chcę** otrzymać indywidualny bilet z kodem QR,  
**aby** okazać go przy wejściu na konferencję i na wystąpienia.

## Kryteria akceptacji

- [x] Każdy zapis otrzymuje unikalny, losowo wygenerowany kod biletu.
- [x] Bilet zawiera nazwę konferencji i kod biletu.
- [x] Bilet może pobrać tylko użytkownik, którego dotyczy zapis.
- [x] Bilet usuniętej konferencji nie jest dostępny.
- [ ] Frontend wyświetla kod biletu jako czytelny kod QR dostosowany do ekranu telefonu.
- [ ] Uczestnik widzi listę swoich zapisów i może z niej otworzyć bilet (BE-18, issue #36).

## Scenariusze (Given – When – Then)

### Scenariusz 1: Wyświetlenie biletu

- **Zakładając, że (Given)** użytkownik jest zapisany na konferencję,
- **Gdy (When)** otworzy swój bilet,
- **Wtedy (Then)** widzi nazwę konferencji i kod QR.

### Scenariusz 2: Cudzy bilet

- **Zakładając, że (Given)** użytkownik zna identyfikator cudzego zapisu,
- **Gdy (When)** spróbuje pobrać bilet,
- **Wtedy (Then)** system zwraca `404 Not Found`.

### Scenariusz 3: Konferencja usunięta

- **Zakładając, że (Given)** organizator usunął konferencję,
- **Gdy (When)** uczestnik spróbuje pobrać bilet,
- **Wtedy (Then)** system zwraca `404 Not Found`.

## Dodatkowe informacje

### Zależności

- Zapis na konferencję (US-07).
- Biblioteka do generowania kodów QR po stronie frontendu.

### Notatki techniczne

- Endpoint: `GET /api/conferences/{conferenceId}/registrations/{registrationId}/ticket`.
- Kod biletu (`TicketCode`) to losowy GUID, który nie ujawnia danych uczestnika.
- Kod QR zawiera wyłącznie kod biletu (ustalenie z event stormingu).
- Generowanie obrazu QR i jego odczyt są realizowane w całości po stronie frontendu (FE-09, FE-10); backend zwraca i weryfikuje wyłącznie kod biletu.

### Przypadki brzegowe

- Rezygnacja z zapisu – bilet przestaje istnieć i nie przejdzie check-inu.
- Zrzut ekranu biletu przekazany innej osobie – pierwsze wejście zostanie przyjęte, każde kolejne zasygnalizowane jako ponowne użycie (US-09).

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 6 – suma elementów backlogu: BE-15 (#33): 2, BE-18 (#36): 2, FE-09 (Front#9): 2
- **Status:** backend – częściowo (pozostało: BE-18); frontend – FE-09 – Backlog
- **Sprint docelowy:** Sprint 5
