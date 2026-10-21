# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Stan na zakończenie sesji 21.10.2026: model i tabele Identity oraz konfiguracja usług są gotowe, użytkownik potwierdził uruchomienie aplikacji. Kolejna sesja planowana na 22.10.2026; rozpoczynamy od pierwszego administratora i logowania.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

Aktualny zakres: bez edycji treści wysłanego zgłoszenia, komentarzy i notatek. Status i priorytet zmieniamy osobnymi listami rozwijanymi, a wybór od razu uruchamia zapis w bazie przez kod serwera. Po dodaniu kont „Przypisz do mnie” zapisze zalogowaną osobę po jednym kliknięciu.

## Teraz

- [ ] **Identity: pierwsze konto, logowanie i wylogowanie** — etap 4a roadmapy

  Ustalono: wszystkie konta tworzy administrator; bez publicznej rejestracji. Ustalić dane konta, sposób pierwszego ustawienia hasła i potwierdzanie e-maila. Dodać Identity do obecnego .NET 8 i SQL Server oraz migrację. Przygotować jednorazowe lokalne utworzenie pierwszego administratora z danymi dostępowymi spoza repozytorium, następnie logowanie, wylogowanie i rzeczywistą tożsamość w układzie. Pierwszy efekt: administrator może się zalogować, odświeżyć stronę i wylogować.

  Na kolejną sesję: ustalić dane logowania pierwszego administratora i sposób ich podania poza repozytorium, np. przez User Secrets. Przygotować jednorazowe, jawnie uruchamiane tworzenie roli administratora i konta przez usługi Identity; bez publicznej rejestracji, stałego hasła w kodzie i automatycznego tworzenia konta przy każdym starcie. Następnie dodać logowanie i wylogowanie zgodne z Blazor Interactive Server oraz wyświetlić rzeczywistą tożsamość w układzie. Model, migracja i konfiguracja usług są już gotowe; odczyt zgłoszeń i zachowanie danych po migracji pozostają do potwierdzenia.

- [ ] **Tworzenie kolejnych kont przez administratora** — etap 4a

  Dodać formularz tworzenia kont oraz rolę administratora i ochronę operacji na serwerze. Sprawdzić utworzenie użytkownika i jego logowanie, błędne hasło, duplikat konta oraz zakaz tworzenia kont dla osoby niezalogowanej i zwykłego użytkownika, również przy bezpośrednim wywołaniu operacji. Nie udostępniać publicznej rejestracji. Pełne uprawnienia do zgłoszeń pozostają osobnym zadaniem.

## Następne sesje

- [ ] **Autor zgłoszenia i komentarza z zalogowanego konta** — etap 4b

  Przed zmianą modelu ustalić los dotychczasowych tekstowych autorów i przypisań. Nowe zgłoszenie i komentarz otrzymują identyfikator osoby zalogowanej na serwerze; nie wpisujemy autora dowolnym tekstem. Sprawdzić zachowanie osoby niezalogowanej.

## Domknięcie etapu 3

- [ ] **Reguły i weryfikacja statusów oraz priorytetów**

  Uzgodnić dozwolone przejścia; obecnie każdy status z enuma jest akceptowany. Ustalić i obsłużyć wybór tej samej wartości, aby nie zmieniać bez potrzeby dat. Powtórzyć próby po odświeżeniu: zamknięcie ustawia `ClosedAt`, ponowne otwarcie ją czyści, status i priorytet pozostają zapisane. Sprawdzić niepoprawne enumy, błąd zapisu, widoczność szczegółów i udane ponowienie; przygotować odpowiednie testy uzgodnionych reguł. Podstawowy zapis użytkownik już sprawdził; nie deklarujemy pełnej weryfikacji ostatniej wersji.

- [ ] **Drobne poprawki tekstów kategorii** — etap 3

  W szczegółach poprawić „Karegoria” na „Kategoria” i „Incorrect category” na „Nieznana kategoria”; sprawdzić wyświetlanie polskich nazw dodanych w `GetCategoryName`.

- [ ] **Porządki w szczegółach** — etap 3

  Usunąć nieaktywne przyciski „Edytuj” i „Zmień status”. Status ma już własną listę rozwijaną; nadać jej `id="ticket-status"`, aby nie powtarzała identyfikatora priorytetu. Poprawić komunikaty w `catch`, które przy aktualizacji nadal mówią o pobieraniu zgłoszenia. Przeniesienie szczegółów wyjątków do logów pozostaje w etapie 6.

## Pozostała weryfikacja techniczna

- [ ] **Pusta baza z pełnego zestawu migracji** — etap 2

  Sprawdzić utworzenie osobnej pustej bazy z aktualnych sześciu migracji, w tym kolumny `Category`, liczbowych kolumn `Status` i `Priority` oraz tabel Identity. Wcześniejsza próba obejmowała tylko dwie pierwsze migracje.

## Wykonane

- [x] **Identity — pakiet i model EF** — etap 4a

  Dodano pakiet Identity 8.0.27 i `ApplicationUser : IdentityUser`. `AppDbContext` dziedziczy po `IdentityDbContext<ApplicationUser>` i wywołuje bazową konfigurację modelu przed konfiguracją komentarzy. Kompilacja bez błędów i ostrzeżeń. Kolejne kroki migracji i konfiguracji usług opisano osobno poniżej.

- [x] **Identity — przygotowanie migracji** — etap 4a

  Wygenerowano `20261021150554_AddIdentitySchema`. Przejrzano `Up()`, `Down()` i SQL: siedem tabel Identity z relacjami i indeksami, bez zmian tabel zgłoszeń. Kompilacja bez błędów i ostrzeżeń, model zgodny ze snapshotem. Podczas tego kroku nie zmieniano bazy; późniejsze zastosowanie opisano poniżej.

- [x] **Identity — zastosowanie migracji i sprawdzenie struktury** — etap 4a

  Użytkownik zastosował `AddIdentitySchema` i potwierdził sześć migracji bez `Pending`, siedem tabel `AspNet...` oraz `UserCount = 0`. Potwierdzenie pochodzi od użytkownika; agent nie wykonywał niezależnego odczytu bazy. Próba odczytu listy i szczegółów zgłoszeń oraz porównanie liczby zgłoszeń i komentarzy pozostają do potwierdzenia.

- [x] **Identity — konfiguracja usług i uwierzytelniania** — 21.10.2026 · etap 4a

  W `Program.cs` dodano `AddIdentity<ApplicationUser, IdentityRole>()`, `AddEntityFrameworkStores<AppDbContext>()`, `AddAuthorization()` i `AddCascadingAuthenticationState()`. `UseAuthentication()` oraz `UseAuthorization()` znajdują się przed `UseAntiforgery()`. Użytkownik potwierdził poprawne uruchomienie aplikacji; agent sprawdził konfigurację w kodzie. Samych kont, logowania, wylogowania ani uprawnień do zgłoszeń jeszcze nie zaimplementowano. Koniec pracy na 21.10.2026 — kontynuacja od pierwszego administratora.

- [x] **Status i priorytet — podstawowy zapis po wyborze z listy** — 21.10.2026 · etap 3

  `ChangeStatus` i `ChangePriority` walidują enum i aktualizują odpowiednie pole oraz `UpdatedAt` jednym zapisem. Widok jest aktualizowany po sukcesie; `isUpdating` blokuje obie listy i nakładające się wywołania. `updateErrorMessage` jest wyświetlane przy listach i czyszczone przed nową próbą; błąd odczytu i brak rekordu mają osobny ekran. W commicie `da2245f` zamknięcie ustawia `ClosedAt`, a inny status ją czyści. Użytkownik potwierdził podstawowe działanie, agent kod i kompilację bez błędów oraz ostrzeżeń. Daty i scenariusze błędów ostatniej wersji wymagają osobnych prób opisanych powyżej; cały etap 3 pozostaje w toku.

- [x] **Enumy statusów i priorytetów — migracja bazy** — 21.10.2026 · etap 3

  Model korzysta z `TicketStatus` i `TicketPriority`, a kolumny `Status` i `Priority` mają typ `int`. Agent zastosował `ConvertTicketStatusAndPriorityToEnums` do `layZTicketsDb`, zachowując 9 zgłoszeń i 12 komentarzy; dawne `Critical` mapuje się na `Highest = 3`. Zweryfikowano dane skrótami SHA-256, historię 5 migracji, kompilację i zgodność modelu ze snapshotem. Opcje priorytetu formularza tworzenia dostosowano do enuma. Agent nie wykonywał interaktywnej próby tworzenia po migracji. Późniejszy podstawowy zapis zmian statusu i priorytetu opisano osobno powyżej.

- [x] **Kategorie — zapis i odczyt** — 21.10.2026 · etap 3 roadmapy

  `TicketCategory` ma jawne wartości `Other = 0`, `Hardware = 1`, `Software = 2`, `Network = 3`. Wybór jest powiązany z modelem przez `InputRadioGroup`, zapisywany jako `int` i wyświetlany w szczegółach. Migracja `AddTicketCategory` dodaje kolumnę z wartością domyślną `0`. Użytkownik potwierdził poprawne działanie po migracji. Polskie nazwy dodano przez `GetCategoryName`; drobne poprawki tekstów pozostają osobnym punktem powyżej.

- [x] **Walidacja kategorii na serwerze** — 21.10.2026 · etap 3

  `Enum.IsDefined` odrzuca wartość spoza enuma przed dostępem do bazy. Użytkownik wymusił `(TicketCategory)9999` i potwierdził komunikat błędu oraz brak nowego zgłoszenia. Testowe przypisanie zostało usunięte; agent potwierdził to w kodzie. Kompilacja walidacji i polskich nazw przeszła bez błędów i ostrzeżeń.

## Większe bloki

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
