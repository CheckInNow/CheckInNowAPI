# Historyjka: Dodawanie prelegentów

## Opis

**Jako** organizator,  
**chcę** dodać do konferencji prelegentów wraz z krótkim biogramem,  
**aby** przypisywać ich do wystąpień i przedstawić ich uczestnikom.

## Kryteria akceptacji

- [x] Prelegenta może dodać tylko właściciel konferencji.
- [x] Imię i nazwisko są wymagane (maks. 100 znaków), biogram jest opcjonalny (maks. 1000 znaków).
- [x] Prelegent należy do konkretnej konferencji.
- [x] Lista prelegentów konferencji jest dostępna publicznie.
- [ ] Organizator może edytować i usuwać prelegentów (BE-29).

## Scenariusze (Given – When – Then)

### Scenariusz 1: Dodanie prelegenta

- **Zakładając, że (Given)** organizator jest właścicielem konferencji,
- **Gdy (When)** doda prelegenta z imieniem, nazwiskiem i biogramem,
- **Wtedy (Then)** prelegent pojawia się na liście prelegentów konferencji.

### Scenariusz 2: Brak wymaganych danych

- **Zakładając, że (Given)** organizator wypełnia formularz prelegenta,
- **Gdy (When)** pozostawi puste nazwisko,
- **Wtedy (Then)** system odrzuca zapis z błędem walidacji.

### Scenariusz 3: Prelegent w cudzej konferencji

- **Zakładając, że (Given)** organizator nie jest właścicielem konferencji,
- **Gdy (When)** spróbuje dodać prelegenta,
- **Wtedy (Then)** system zwraca `403 Forbidden`.

## Dodatkowe informacje

### Zależności

- Istniejąca konferencja (US-03).

### Notatki techniczne

- Endpointy: `GET/POST /api/conferences/{conferenceId}/speakers`.
- Prelegent nie musi mieć konta w systemie.

### Przypadki brzegowe

- Nieistniejąca lub usunięta konferencja – `404 Not Found`.
- Ten sam prelegent na kilku konferencjach – dodawany osobno w każdej konferencji.

## Priorytet i estymacja

- **Priorytet:** Must Have
- **Estymacja (Story Points):** 8 (wspólnie z US-04) – suma elementów backlogu: BE-09 (#23): 3, FE-05 (Front#5): 1, FE-06 (Front#6): 1, BE-29 (#55): 3
- **Status:** backend – częściowo (pozostało: BE-29); frontend – FE-05 – Backlog, FE-06 – Backlog
- **Sprint docelowy:** Sprint 6
