# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Stan na 22.10.2026: użytkownik potwierdził próby tworzenia kont z SQL Server i w przeglądarce. Dodano wymagane imię i nazwisko nowych kont, zastosowano siódmą migrację i uzupełniono dotychczasowe konta. Menu wyświetla pełną nazwę po ponownym zalogowaniu. Rozszerzenie przeszło kompilację, weryfikację bazy i 51 izolowanych sprawdzeń HTTP.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

Aktualny zakres: bez edycji treści wysłanego zgłoszenia, komentarzy i notatek. Status i priorytet zmieniamy osobnymi listami rozwijanymi, a wybór od razu uruchamia zapis w bazie przez kod serwera. Po dodaniu kont „Przypisz do mnie” zapisze zalogowaną osobę po jednym kliknięciu.

## Teraz

- [ ] **Lista kont w panelu administratora** — etap 4a

  Wyświetlić imię, nazwisko i login utworzonych kont w tym samym panelu, zgodnie z ustalonym układem. Nie wyświetlać haseł ani ich hashy. Rozbudowa pustego „Mojego profilu”, w tym zmiana loginu i hasła, pozostaje na później.

- [ ] **Własny ekran odmowy dostępu** — etap 4a

  Odmowa wejścia zwykłego użytkownika na `/admin` działa, ale prowadzi na domyślny adres Identity. Przygotować własną stronę i ustawić `AccessDeniedPath`. Cały etap 4 pozostaje w toku.

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

  Sprawdzić utworzenie osobnej pustej bazy z aktualnych siedmiu migracji, w tym kategorii, liczbowych statusów i priorytetów oraz tabel Identity z imieniem i nazwiskiem. Wcześniejsza próba obejmowała tylko dwie pierwsze migracje.

## Wykonane

- [x] **Tworzenie kolejnych kont przez administratora — próba z SQL Server** — 22.10.2026 · etap 4a

  Użytkownik potwierdził sprawdzenie scenariuszy w lokalnej aplikacji z SQL Server i poprawne działanie. Potwierdzenie dotyczy formularza sprzed rozszerzenia o imię i nazwisko. Wcześniejsze izolowane próby HTTP obejmowały 40 sprawdzeń walidacji, zapisu, logowania, duplikatu i granic dostępu. Dane logowania ustawia administrator, bez wysyłania zaproszeń i potwierdzania e-maila. Ten punkt jest wykonany; lista kont i ekran odmowy dostępu pozostają osobnymi zadaniami.

- [x] **Imię i nazwisko konta — formularz, baza i menu** — 22.10.2026 · etap 4a

  Dodano `FirstName` i `LastName` do `ApplicationUser` oraz `UserData`, wymagane pola formularza i limit po 100 znaków. `AddUserNames` zastosowano do `layZTicketsDb`: 7 migracji, nadal 2 konta, 10 zgłoszeń i 13 komentarzy. Uzgodnione dane dotychczasowych kont to Admin Admin i Jan Kowalski. Skrót wszystkich wcześniejszych pól użytkowników pozostał zgodny. Własna fabryka tożsamości dodaje imię i nazwisko do cookie przy logowaniu; menu pokazuje pełną nazwę i inicjały, a identyfikator i login pozostają bez zmian. Bootstrap nowego administratora domyślnie nadaje Admin Admin, z opcją konfiguracji. Build bez błędów i ostrzeżeń, model zgodny ze snapshotem, 51 izolowanych sprawdzeń HTTP, w tym wymagane i długie nazwy, dwa imiona, polskie znaki i różne konta o identycznej nazwie. Nowe pola w lokalnym UI można sprawdzić po ponownym zalogowaniu; agent nie wykonywał takiej interaktywnej próby.

- [x] **Pierwszy administrator, logowanie, wylogowanie i ochrona stron** — 22.10.2026 · etap 4a

  `AdminAccountSetup` uruchamia się tylko po jawnym `--bootstrap-admin=true`, z danymi spoza repozytorium. Użytkownik potwierdził utworzenie konta i `UserCount = 1`, następnie logowanie, wylogowanie i przekierowania po bezpośrednim wejściu na chronione adresy. Menu pokazuje nazwę i opis roli z tożsamości. Strony w `Main` wymagają zalogowania, `/admin` dodatkowo roli `Admin`; `/login` i strona błędu pozostają publiczne. Pozostała weryfikacja nowego formularza i uprawnień jest opisana powyżej; nie oznaczamy całego etapu 4 jako ukończonego.

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
