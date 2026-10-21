# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Kolejność pochodzi z planu sesji z 20.10.2026; „następna sesja” oznacza kolejność pracy, nie termin.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

Aktualny zakres: bez edycji treści wysłanego zgłoszenia, komentarzy i notatek. Status i priorytet zmieniamy osobnymi listami rozwijanymi, a wybór od razu uruchamia zapis w bazie przez kod serwera. Po dodaniu kont „Przypisz do mnie” zapisze zalogowaną osobę po jednym kliknięciu.

## Teraz

- [ ] **Status — lista rozwijana z zapisem po wyborze** — najbliższa sesja · etap 3

  Uzgodnić statusy i dozwolone przejścia, w tym ponowne otwarcie zgłoszenia, oraz reguły `UpdatedAt` i `ClosedAt`. Zastąpić przycisk „Zmień status” listą rozwijaną. Wybór zapisuje status i wymagane daty bez dodatkowego przycisku zapisu. Serwer sprawdza przejście; wynik pozostaje po ponownym odczycie. Pokazać trwający zapis i błąd oraz obsłużyć kolejne wybory podczas zapisu.

## Następne sesje

- [ ] **Priorytet — lista rozwijana z zapisem po wyborze** — kolejna sesja · etap 3

  W szczegółach wybór priorytetu od razu zapisuje go w bazie i aktualizuje `UpdatedAt`, bez osobnego zatwierdzania. Serwer sprawdza dozwoloną wartość; zmiana nie nadpisuje statusu ani pozostałych pól. Wynik pozostaje po odświeżeniu, a interfejs obsługuje trwający zapis, błąd i kolejne wybory.

- [ ] **Drobne poprawki tekstów kategorii** — etap 3

  W szczegółach poprawić „Karegoria” na „Kategoria” i „Incorrect category” na „Nieznana kategoria”; sprawdzić wyświetlanie polskich nazw dodanych w `GetCategoryName`.

- [ ] **Usunięcie przycisku „Edytuj”** — etap 3

  Usunąć nieaktywny przycisk ze szczegółów zgodnie z zakresem bez edycji treści zgłoszenia, komentarzy i notatek. Zmiany statusu i priorytetu mają własne listy rozwijane.

## Pozostała weryfikacja techniczna

- [ ] **Pusta baza z pełnego zestawu migracji** — etap 2

  Sprawdzić utworzenie osobnej pustej bazy z aktualnych pięciu migracji, w tym kolumny `Category` i liczbowych kolumn `Status` oraz `Priority`. Wcześniejsza próba obejmowała tylko dwie pierwsze migracje.

## Wykonane

- [x] **Enumy statusów i priorytetów — migracja bazy** — 21.10.2026 · etap 3

  Model korzysta z `TicketStatus` i `TicketPriority`, a kolumny `Status` i `Priority` mają typ `int`. Agent zastosował `ConvertTicketStatusAndPriorityToEnums` do `layZTicketsDb`, zachowując 9 zgłoszeń i 12 komentarzy; dawne `Critical` mapuje się na `Highest = 3`. Zweryfikowano dane skrótami SHA-256, historię 5 migracji, kompilację i zgodność modelu ze snapshotem. Opcje priorytetu formularza tworzenia dostosowano do enuma. Próba interaktywna formularza po migracji pozostaje do wykonania przy dalszej pracy. Same zmiany statusu i priorytetu po wyborze z listy nie są jeszcze zaimplementowane.

- [x] **Kategorie — zapis i odczyt** — 21.10.2026 · etap 3 roadmapy

  `TicketCategory` ma jawne wartości `Other = 0`, `Hardware = 1`, `Software = 2`, `Network = 3`. Wybór jest powiązany z modelem przez `InputRadioGroup`, zapisywany jako `int` i wyświetlany w szczegółach. Migracja `AddTicketCategory` dodaje kolumnę z wartością domyślną `0`. Użytkownik potwierdził poprawne działanie po migracji. Polskie nazwy dodano przez `GetCategoryName`; drobne poprawki tekstów pozostają osobnym punktem powyżej.

- [x] **Walidacja kategorii na serwerze** — 21.10.2026 · etap 3

  `Enum.IsDefined` odrzuca wartość spoza enuma przed dostępem do bazy. Użytkownik wymusił `(TicketCategory)9999` i potwierdził komunikat błędu oraz brak nowego zgłoszenia. Testowe przypisanie zostało usunięte; agent potwierdził to w kodzie. Kompilacja walidacji i polskich nazw przeszła bez błędów i ostrzeżeń.

## Większe bloki

- [ ] **Identity: konta i logowanie** — następny większy blok · etap 4

  Działają logowanie i wylogowanie. Autor zgłoszenia i komentarza wynika z zalogowanego konta. Przed wdrożeniem ustalamy sposób tworzenia kont i powiązania dotychczasowych danych z kontami.

- [ ] **Uprawnienia i przypisywanie** — kolejny większy blok · etap 4

  Użytkownik ma dostęp do własnych zgłoszeń, a agent i administrator działają zgodnie z uzgodnionymi uprawnieniami. „Przypisz do mnie” od razu zapisuje przypisanie do osoby zalogowanej, której identyfikator serwer odczytuje z konta. Uzgodnić zasady przejęcia już przypisanego zgłoszenia i daty aktualizacji. Przypisanie pozostaje po odświeżeniu, a błędy są widoczne. Serwer sprawdza także uprawnienia do zmian statusu i priorytetu oraz chroni cudze zgłoszenia i notatki wewnętrzne przy wejściu bezpośrednio na adres szczegółów.

- [ ] **Wyszukiwanie, filtry i paginacja** — potem · etap 5

  Wyszukiwanie i filtry współpracują z podziałem wyników na strony. Można dotrzeć do zgłoszeń poza pierwszą dziesiątką, z zachowaniem uprawnień.

- [ ] **Załączniki i dashboard** — następnie · etap 5

  Można dodać i pobrać plik zgodnie z ustalonymi limitami i kontrolą dostępu. Dashboard pokazuje rzeczywiste statystyki zgodne z danymi dostępnymi dla zalogowanej osoby.

- [ ] **Przygotowanie demonstracji** — na końcu · etap 6

  Interfejs jest uporządkowany, błędy można diagnozować, a dokumentacja opisuje aktualne uruchomienie i ograniczenia. Gotowy i sprawdzony scenariusz prezentacji prowadzi od utworzenia zgłoszenia przez obsługę do zamknięcia w odpowiednich rolach.

## Po zakończeniu punktu

Odhacz go tutaj, zaktualizuj status właściwego etapu w roadmapie i — gdy zmieniają się funkcje lub uruchomienie — README. Ukończenie pojedynczego punktu nie oznacza automatycznie ukończenia całego etapu roadmapy.
