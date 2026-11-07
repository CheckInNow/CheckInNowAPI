# Event storming

To co ustaliliśmy na hotspotach

## Konto

1. **Co jeśli użytkownik się nie zweryfikuje?** - Może widzieć konferencje i agendy bez możliwości zapisu na nie. Link do weryfikacji jest ważny 24h. Można poprosić i ponowne wysłanie linku. Jeśli przez 7 dni użytkownik się nie zweryfikuje konto jest usuwane.

## Zapisy i check-in

1. **Co jest w kodzie QR?** - W kodzie QR będzie losowo wygenerowany kod biletu służący do wejścia na konferencje i na wystąpienia (z użyciem tabel `ConferenceCheckIn` oraz `SessionCheckIn`).
2. **Brak internetu przy wejściu** - Offline nie będzie wspierane.
3. **Co w przypadku nieprawidłowego biletu?** - Skaner odrzuca wejście, pokazuje że bilet jest nieprawidłowy, niczego nie zapisuje.

## Oceny i statystyki

1. **Kto może oceniać** - Uczestnik obecny na wystąpieniu (zeskanowany i odnotowany w `SessionCheckIn`).

## Administracja

1. **Ban na konto czy IP?** - Ban na konto.
2. **Czy administrator może zabrać uprawnienia innemu administratorowi?** - Nie, bo administrator jest jeden.
