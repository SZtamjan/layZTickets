# TODO — layZTickets

Krótka lista do odhaczania zamiast osobnego narzędzia do zadań. Stan na 22.10.2026: pierwszy administrator, logowanie, wylogowanie, ochrona stron i tożsamość w menu są zaimplementowane. Formularz tworzenia kont przez administratora jest gotowy w kodzie i przeszedł izolowane próby HTTP; pozostaje sprawdzenie z SQL Server i w przeglądarce.

Pracujemy nad jednym punktem naraz. Zaznacz `[x]` dopiero po sprawdzeniu opisanego efektu. Szczegółowe kryteria etapów i ich statusy są w [ROADMAP.md](ROADMAP.md).

Aktualny zakres: bez edycji treści wysłanego zgłoszenia, komentarzy i notatek. Status i priorytet zmieniamy osobnymi listami rozwijanymi, a wybór od razu uruchamia zapis w bazie przez kod serwera. Po dodaniu kont „Przypisz do mnie” zapisze zalogowaną osobę po jednym kliknięciu.

## Teraz

- [ ] **Tworzenie kolejnych kont przez administratora — próba z SQL Server** — etap 4a

  Formularz w wysuwanym panelu `/admin` jest zaimplementowany: `UserData`, walidacja, `UserManager.CreateAsync`, polskie komunikaty, ochrona roli `Admin`, czyszczenie haseł i przekierowanie po sukcesie. Nowe konto jest zwykłym kontem, a sesja administratora pozostaje bez zmian. Ustalono e-mail jako login, ręczne ustawienie hasła przez administratora i brak wysyłania zaproszeń oraz potwierdzania e-maila w tej wersji. Nie ma publicznej rejestracji.

  Build: 0 błędów i ostrzeżeń. W izolowanych próbach HTTP skompilowanych stron z Identity i magazynem w pamięci przeszło 40 sprawdzeń, w tym tworzenie i logowanie, duplikat, błędne dane, awaria magazynu, brak wycieku hasła w HTML, token formularza i zakaz odczytu/zapisu dla osoby niezalogowanej oraz zwykłego użytkownika. Teraz sprawdzić te scenariusze w lokalnej aplikacji z SQL Server i ponowne otwieranie panelu po błędzie w przeglądarce. Dodać własny ekran odmowy dostępu zamiast domyślnego adresu Identity. Etap 4a pozostaje w toku.

## Następne sesje

- [ ] **Lista kont w panelu administratora** — etap 4a

  Wyświetlić utworzone konta w tym samym panelu, zgodnie z ustalonym układem. Nie wyświetlać haseł ani ich hashy. Rozbudowa pustego „Mojego profilu”, w tym zmiana loginu i hasła, pozostaje na później.

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
