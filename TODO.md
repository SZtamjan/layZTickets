# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Kolejność pochodzi z planu sesji z 20.10.2026; „następna sesja” oznacza kolejność pracy, nie termin.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

## Teraz

- [ ] **Kategorie** — dziś / najbliższa sesja · etap 3 roadmapy

  Wybrana kategoria zapisuje się w bazie i jest widoczna w szczegółach zgłoszenia również po odświeżeniu. Przed zmianą schematu ustalamy sposób zachowania istniejących danych.

## Następne sesje

- [ ] **Zmiana statusu** — następna sesja · etap 3

  Uzgodnione przejścia statusów są sprawdzane na serwerze. Status i data zamknięcia zapisują się zgodnie z ustalonymi regułami; wynik pozostaje po ponownym odczycie.

- [ ] **Edycja zgłoszenia** — kolejna sesja · etap 3

  Można zmienić tytuł, opis, kategorię i priorytet. Poprawny zapis aktualizuje datę modyfikacji, a nieprawidłowe dane są odrzucane. Zmiany pozostają po odświeżeniu.

## Większe bloki

- [ ] **Identity: konta i logowanie** — następny większy blok · etap 4

  Działają logowanie i wylogowanie. Autor zgłoszenia i komentarza wynika z zalogowanego konta. Przed wdrożeniem ustalamy sposób tworzenia kont i powiązania dotychczasowych danych z kontami.

- [ ] **Uprawnienia i przypisywanie** — kolejny większy blok · etap 4

  Użytkownik ma dostęp do własnych zgłoszeń, a agent działa zgodnie z uzgodnionymi uprawnieniami. Przypisanie zgłoszenia zapisuje się. Serwer chroni cudze zgłoszenia i treść notatek wewnętrznych również przy wejściu bezpośrednio na adres szczegółów.

- [ ] **Wyszukiwanie, filtry i paginacja** — potem · etap 5

  Wyszukiwanie i filtry współpracują z podziałem wyników na strony. Można dotrzeć do zgłoszeń poza pierwszą dziesiątką, z zachowaniem uprawnień.

- [ ] **Załączniki i dashboard** — następnie · etap 5

  Można dodać i pobrać plik zgodnie z ustalonymi limitami i kontrolą dostępu. Dashboard pokazuje rzeczywiste statystyki zgodne z danymi dostępnymi dla zalogowanej osoby.

- [ ] **Przygotowanie demonstracji** — na końcu · etap 6

  Interfejs jest uporządkowany, błędy można diagnozować, a dokumentacja opisuje aktualne uruchomienie i ograniczenia. Gotowy i sprawdzony scenariusz prezentacji prowadzi od utworzenia zgłoszenia przez obsługę do zamknięcia w odpowiednich rolach.

## Po zakończeniu punktu

Odhacz go tutaj, zaktualizuj status właściwego etapu w roadmapie i — gdy zmieniają się funkcje lub uruchomienie — README. Ukończenie pojedynczego punktu nie oznacza automatycznie ukończenia całego etapu roadmapy.
