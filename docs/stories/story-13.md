# Historyjka: Statystyki konferencji dla organizatora

## Opis

**Jako** organizator,  
**chcę** zobaczyć statystyki mojej konferencji,  
**aby** ocenić frekwencję i odbiór wystąpień oraz przygotować raport dla sponsorów.

## Kryteria akceptacji

- [ ] Statystyki widzi tylko właściciel konferencji.
- [ ] Statystyki zawierają liczbę zapisanych osób.
- [ ] Statystyki zawierają liczbę osób, które faktycznie pojawiły się na konferencji, oraz procent obecnych względem zapisanych.
- [ ] Dla każdego wystąpienia widoczna jest frekwencja (liczba osób z check-inem na wystąpienie).
- [ ] Dla każdego wystąpienia widoczna jest średnia ocena i liczba ocen.
- [ ] Wystąpienia bez ocen są oznaczone jako „brak ocen”, a nie średnią 0.
- [ ] Statystyki są dostępne w trakcie i po zakończeniu konferencji.

## Scenariusze (Given – When – Then)

### Scenariusz 1: Podgląd statystyk po konferencji

- **Zakładając, że (Given)** na konferencję zapisało się 200 osób, a 170 przeszło check-in,
- **Gdy (When)** organizator otworzy statystyki,
- **Wtedy (Then)** widzi 200 zapisanych, 170 obecnych (85%) oraz frekwencję i średnie oceny każdego wystąpienia.

### Scenariusz 2: Wystąpienie bez ocen

- **Zakładając, że (Given)** żaden uczestnik nie ocenił wystąpienia,
- **Gdy (When)** organizator otworzy statystyki,
- **Wtedy (Then)** przy wystąpieniu widnieje informacja „brak ocen”.

### Scenariusz 3: Cudza konferencja

- **Zakładając, że (Given)** organizator nie jest właścicielem konferencji,
- **Gdy (When)** spróbuje otworzyć jej statystyki,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Zapisy (US-07), check-in (US-09), oceny (US-12).

### Notatki techniczne

- Proponowany endpoint: `GET /api/conferences/{conferenceId}/statistics`.
- Agregacje wykonywane w jednym zapytaniu SQL (`COUNT`, `AVG`) zamiast w pamięci aplikacji.
- W trakcie konferencji wynik może być krótko cache'owany (HybridCache).

### Przypadki brzegowe

- Konferencja bez zapisów – wszystkie wartości równe 0, procent obecności bez dzielenia przez zero.
- Usunięte wystąpienia – nie są uwzględniane w statystykach.
- Rezygnacje z zapisu – nie są liczone jako zapisani (zapis jest usuwany).

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 8 – suma elementów backlogu: BE-27 (#52): 5, FE-14 (Front#14): 3
- **Status:** backend – Backlog; frontend – FE-14 – Backlog
- **Sprint docelowy:** Sprint 6
