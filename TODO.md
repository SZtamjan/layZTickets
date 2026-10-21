# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Kolejność pochodzi z planu sesji z 20.10.2026; „następna sesja” oznacza kolejność pracy, nie termin.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

## Teraz

- [ ] **Zmiana statusu** — najbliższa sesja · etap 3

  Uzgodnić statusy i dozwolone przejścia, w tym ponowne otwarcie zgłoszenia, oraz reguły `UpdatedAt` i `ClosedAt`. Przejścia sprawdzać na serwerze; status i daty muszą pozostać po ponownym odczycie.

## Następne sesje

- [ ] **Drobne poprawki tekstów kategorii** — etap 3

  W szczegółach poprawić „Karegoria” na „Kategoria” i „Incorrect category” na „Nieznana kategoria”; sprawdzić wyświetlanie polskich nazw dodanych w `GetCategoryName`.

- [ ] **Edycja zgłoszenia** — kolejna sesja · etap 3

  Można zmienić tytuł, opis, kategorię i priorytet. Poprawny zapis aktualizuje datę modyfikacji, a nieprawidłowe dane są odrzucane. Zmiany pozostają po odświeżeniu.

## Pozostała weryfikacja techniczna

- [ ] **Pusta baza z pełnego zestawu migracji** — etap 2

  Sprawdzić utworzenie osobnej pustej bazy z aktualnych czterech migracji, w tym kolumny `Category`. Wcześniejsza próba obejmowała tylko dwie pierwsze migracje.

## Wykonane

- [x] **Kategorie — zapis i odczyt** — 21.10.2026 · etap 3 roadmapy

  `TicketCategory` ma jawne wartości `Other = 0`, `Hardware = 1`, `Software = 2`, `Network = 3`. Wybór jest powiązany z modelem przez `InputRadioGroup`, zapisywany jako `int` i wyświetlany w szczegółach. Migracja `AddTicketCategory` dodaje kolumnę z wartością domyślną `0`. Użytkownik potwierdził poprawne działanie po migracji. Polskie nazwy dodano przez `GetCategoryName`; drobne poprawki tekstów pozostają osobnym punktem powyżej.

- [x] **Walidacja kategorii na serwerze** — 21.10.2026 · etap 3

  `Enum.IsDefined` odrzuca wartość spoza enuma przed dostępem do bazy. Użytkownik wymusił `(TicketCategory)9999` i potwierdził komunikat błędu oraz brak nowego zgłoszenia. Testowe przypisanie zostało usunięte; agent potwierdził to w kodzie. Kompilacja walidacji i polskich nazw przeszła bez błędów i ostrzeżeń.

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
