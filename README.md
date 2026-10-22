# layZTickets

Projekt aplikacji HelpDesk do portfolio pod stanowiska związane z C#. Rozwój projektu łączy ukończenie systemu zgłoszeń z praktyczną nauką frontendu, backendu, SQL, logowania i uprawnień.

Docelowo aplikacja ma obsługiwać użytkownika zgłaszającego problem, agenta Service Desk i administratora. Obecna wersja ma logowanie i wylogowanie przez Identity, ochronę stron oraz panel tworzenia kont dostępny tylko administratorowi. Uprawnienia do konkretnych zgłoszeń i operacji pozostają do wdrożenia.

[Zasady współpracy i nauki](AGENTS.md) · [Etapy rozwoju](ROADMAP.md) · [Lista TODO](TODO.md)

## Technologie i sposób działania

| Technologia | Rola w projekcie |
| --- | --- |
| C# / .NET 8 | Język i platforma aplikacji; projekt wskazuje `net8.0` |
| ASP.NET Core | Uruchomienie serwera, konfiguracja, rejestrowanie usług i obsługa żądań |
| Blazor / komponenty Razor | Interfejs z HTML i C#; strony zgłoszeń używają trybu `InteractiveServer` |
| Entity Framework Core 8.0.27 | Odczyt i zapis obiektów C# w bazie przez provider SQL Server |
| SQL Server | Trwałe przechowywanie zgłoszeń i komentarzy |
| CSS / Bootstrap | Układ i wygląd interfejsu |

W trybie Interactive Server przeglądarka wyświetla interfejs i przekazuje interakcje do serwera. Logika komponentów C# oraz dostęp do bazy wykonują się na serwerze. Obecny projekt nie ma oddzielnego frontendowego klienta ani własnej warstwy REST API.

## Obecny stan

Stan analizy i weryfikacji: **22 października 2026 r.** Podstawowe operacje na zgłoszeniach, logowanie, wylogowanie, ochronę stron i formularz tworzenia kont użytkownik sprawdził ręcznie z SQL Server. Późniejsze rozszerzenie o imię i nazwisko potwierdzono kompilacją, 51 izolowanymi sprawdzeniami HTTP i zastosowaniem migracji z kontrolą zachowania wcześniejszych danych kont. Szczegóły opisano w sekcji „Stan weryfikacji”.

| Obszar | Stan w kodzie |
| --- | --- |
| Tworzenie zgłoszenia | Formularz, sprawdzanie wymaganych pól, jeden zapis do bazy, priorytet i tekstowo wpisywani zgłaszający oraz agent; numer `IT-{Id}` wyliczany w C# bez zer wiodących |
| Lista zgłoszeń | Odczyt 10 najnowszych rekordów i przejście do szczegółów |
| Szczegóły i komentarze | Odczyt zgłoszenia z komentarzami; zapis odpowiedzi i notatek z flagą `IsInternal` |
| Schemat bazy | Siedem migracji EF Core, w tym `AddUserNames` z kolumnami imienia i nazwiska w `AspNetUsers`. Strony nie tworzą ani nie aktualizują schematu; `TicketNumber` nie jest kolumną bazy |
| Kategorie | Cztery wartości enuma `TicketCategory`; wybór w formularzu zapisuje się jako `int`, serwer sprawdza dozwoloną wartość przez `Enum.IsDefined`, szczegóły mapują ją na polską nazwę przez `GetCategoryName`; pozostają drobne poprawki tekstów |
| Statusy i priorytety | `TicketStatus` i `TicketPriority` przechowywane jako `int`; wybór z listy w szczegółach zapisuje odpowiednie pole i `UpdatedAt`. Zamknięcie ustawia `ClosedAt`, a inny status ją czyści. Podstawowy zapis sprawdzony przez użytkownika; pełna weryfikacja dat i błędów pozostaje do wykonania |
| Pozostałe przyciski w szczegółach | „Edytuj”, „Zmień status” i „Przypisz do mnie” nadal są nieaktywne. Status zmienia się listą; dwa pierwsze przyciski przeznaczono do usunięcia. Tekstowe przypisanie można podać przy tworzeniu |
| Filtry, wyszukiwarka, paginacja | Elementy interfejsu bez zaimplementowanej logiki |
| Załączniki | Makieta interfejsu, bez przesyłania i przechowywania plików |
| Logowanie i role | Pierwszego administratora tworzy jawne polecenie `--bootstrap-admin=true`; użytkownik potwierdził `UserCount = 1`. Działają formularz `/login`, POST `/logout`, ochrona stron w `Components/Pages/Main` oraz nazwa i opis roli zalogowanej osoby w menu. Pełne uprawnienia do zgłoszeń pozostają do wdrożenia |
| Panel administratora | `/admin` wymaga roli `Admin`. Wysuwany formularz tworzy zwykłe konto przez `UserManager`, wymaga imienia i nazwiska oraz sprawdza e-mail, potwierdzenie hasła i reguły Identity. Lista kont jest planowana; `/profile` pozostaje pustą stroną ustawień |
| Nazwa osoby | `FirstName` i `LastName` przechowywane osobno, po maksymalnie 100 znaków. Menu odczytuje je z tożsamości logowania i wylicza inicjały. Powiązania zgłoszeń z kontami będą korzystać z `Id`, a nie z imienia i nazwiska |
| Dashboard | Strona główna nadal zawiera treść szablonu |
| Przykłady z szablonu | Strony Counter i Weather usunięto; serwis zapisu JSON pozostaje w projekcie |

### Ograniczenia wymagające uwagi

- Notatki „wewnętrzne” są pobierane wraz z pozostałymi komentarzami. Flaga i wygląd nie ograniczają dostępu do zespołu IT.
- Aplikacja wymaga bazy przygotowanej migracjami. Nie wykonuje ich automatycznie przy starcie ani podczas otwierania stron. Dotychczasowa baza utworzona przez `EnsureCreatedAsync` wymaga osobnego przygotowania historii migracji; zwykła aktualizacja nie rozpoznaje automatycznie istniejących tabel.
- Numer `TicketNumber` jest właściwością C# z `[NotMapped]`. Wyszukiwanie numeru w zapytaniu EF powinno wykorzystywać odpowiadające mu `Id`; właściwość wyliczana nie jest kolumną SQL.
- Pola nazwane `CreatedByUserId`, `AssignedAgentId` i `AuthorUserId` przechowują obecnie wpisywany tekst, bez powiązania z kontami.
- Obsługa błędów stron zgłoszeń nadal wyświetla treść wyjątków. Logowanie i tworzenie kont pokazują przyjazny komunikat i zapisują wyjątek w logach. Nie ma stałego projektu testów automatycznych.
- Zmiana statusu sprawdza wartość enuma, ale nie ogranicza przejść między statusami. Wybór tej samej wartości nadal aktualizuje datę; reguły i sprawdzenia pozostają w roadmapie. Błędy aktualizacji wyświetlane są przy listach, bez ukrywania szczegółów; błąd początkowego odczytu i brak rekordu mają osobny ekran.

## Struktura projektu

| Miejsce | Odpowiedzialność |
| --- | --- |
| `Program.cs` | Konfiguracja aplikacji, fabryki kontekstów EF, usług Identity, uwierzytelniania, autoryzacji i trybu renderowania |
| `Components/Pages/` | Strony, formularze i obecna logika operacji na zgłoszeniach |
| `Components/Pages/Login.razor`, `Components/Pages/Main/_Imports.razor` | Publiczne logowanie i wspólne wymaganie zalogowania dla stron w `Main`; strona błędu ma `[AllowAnonymous]` |
| `Components/Pages/Main/AdminPanel.razor` | Formularz tworzenia konta przez administratora w static SSR; POST na `/admin`, walidacja i zapis przez Identity |
| `Components/Layout/`, `Components/App.razor`, `Components/Routes.razor` | Układ, dokument HTML i routing |
| `Data/AppDbContext.cs` | Kontekst dziedziczący po `IdentityDbContext<ApplicationUser>`; model kont i ról Identity oraz zestawy zgłoszeń i komentarzy |
| `Data/ApplicationUser.cs` | Encja konta dziedzicząca po `IdentityUser`, rozszerzona o `FirstName` i `LastName` |
| `Data/ApplicationUserClaimsPrincipalFactory.cs` | Dodaje imię i nazwisko do tożsamości tworzonej przy logowaniu, zachowując standardowy login, identyfikator i role Identity |
| `DataSets/UserData.cs` | Model formularza: imię, nazwisko, e-mail, hasło i jego potwierdzenie; nie jest encją EF ani nową tabelą |
| `DataSets/` | Modele `TicketData` i `TicketComment` oraz enumy `TicketCategory`, `TicketStatus` i `TicketPriority` |
| `Migrations/` | Zmiany struktury bazy, metadane migracji i snapshot modelu, czyli jego zapis do porównania przy kolejnych zmianach |
| `dotnet-tools.json` | Manifest lokalnego narzędzia `dotnet-ef` w wersji 8.0.27 |
| `Components/Services/SaveService/`, `App_Data/` | Serwis i pliki JSON; licznik zapisuje tam swój stan, zgłoszenia korzystają z SQL Server |
| `wwwroot/` | CSS i zasoby statyczne |
| `Properties/launchSettings.json` | Lokalne profile uruchomienia i porty |

### Droga od formularza do bazy

1. Użytkownik uzupełnia formularz w `Components/Pages/Main/CreateTicket.razor`. `@bind-Value` wiąże pola z obiektem `ticketDraft`.
2. Wysłanie `EditForm` uruchamia na serwerze `CreateTicketAsync`. Metoda sprawdza tytuł, opis i zgłaszającego oraz ustawia wartości początkowe.
3. Wstrzyknięta przez DI fabryka `IDbContextFactory<AppDbContext>` tworzy kontekst do operacji na bazie. Rejestracja fabryki znajduje się w `Program.cs`.
4. Kontekst korzysta ze struktury przygotowanej wcześniej migracjami. Komponent nie tworzy tabel ani nie wykonuje ręcznego SQL poprawiającego schemat lub numery istniejących zgłoszeń.
5. `Tickets.Add` dodaje obiekt do śledzenia przez EF. Jedyne `SaveChangesAsync` zapisuje go w SQL Server i uzupełnia `Id`.
6. Właściwość `TicketNumber` zwraca `IT-{Id}`, np. `IT-7`. Numer jest wyliczany przy odczycie właściwości, bez zapisu w bazie. Komponent wyświetla komunikat i czyści formularz. Nie ma już osobnego zapisu numeru wymagającego wspólnej transakcji z utworzeniem zgłoszenia.
7. Lista wykonuje osobny odczyt przez EF, a szczegóły dołączają komentarze przez `Include`. Zapis komentarza aktualizuje również `UpdatedAt` zgłoszenia.
8. W szczegółach `@bind-Value:get` / `@bind-Value:set` łączy listy z metodami `ChangeStatus` i `ChangePriority`. Nowy kontekst pobiera śledzony rekord, zmienia odpowiednie pole i daty, a `SaveChangesAsync` zapisuje zmiany. Dopiero po sukcesie aktualizowany jest obiekt używany przez widok. `isUpdating` blokuje kolejne wywołania i obie listy na czas zapisu.

## Uruchomienie lokalne

### Wymagania

- SDK .NET obsługujący budowanie projektu `net8.0`; do lokalnego uruchomienia potrzebny jest runtime ASP.NET Core 8 i .NET 8.
- Dostępna instancja **SQL Server** oraz konto z uprawnieniami do odczytu i zapisu danych. Wykonanie migracji wymaga dodatkowo uprawnień do zmian struktury, a utworzenie nowej bazy — do jej tworzenia. Samo SDK nie instaluje serwera bazy danych.
- Dostęp do NuGet przy pierwszym przywracaniu pakietów i narzędzia `dotnet-ef`.
- PowerShell dla poniższych przykładów. Visual Studio jest opcjonalne.

W analizowanym środowisku kompilacja powiodła się z SDK `10.0.400`; runtime ASP.NET Core 8 i .NET 8 są zainstalowane. Repozytorium nie przypina SDK plikiem `global.json`.

### Konfiguracja bazy

Aplikacja odczytuje `ConnectionStrings:DefaultConnection`. Repozytorium zawiera lokalne ustawienie w `appsettings.json`, wskazujące bazę roboczą `layZTicketsDb`; nazwę instancji i sposób uwierzytelniania dostosuj do swojego komputera. Wskaż świadomie bazę docelową przed wykonaniem migracji i uruchomieniem aplikacji.

W uporządkowanym środowisku lokalnym pozostały dwie bazy:

| Baza | Przeznaczenie |
| --- | --- |
| `layZTicketsDb` | Aktualna baza robocza aplikacji; odczyt i zapis, wszystkie trzy migracje |
| `layZTicketsDb_backup` | Odtworzona kopia stanu z chwili porządkowania nazw; ustawiona jako tylko do odczytu, z własnymi plikami danych i logu |

Obie bazy miały w chwili utworzenia kopii 7 zgłoszeń i 10 komentarzy. Kopia nie synchronizuje się automatycznie z bazą roboczą i nie jest archiwum kolejnych wersji rekordów. Przed kolejnymi zmianami wymagającymi zabezpieczenia danych należy wykonać świeży backup; dotychczasowe pliki `.bak` zachowano w folderze backupów SQL Server. Kopia na tym samym serwerze umożliwia ćwiczenie odtwarzania, ale nie zastępuje przechowywania plików backupu poza tym serwerem.

Tabela `__EFMigrationsHistory` znajduje się wewnątrz każdej bazy i opisuje zastosowane migracje. Nie przechowuje dawnych wersji zgłoszeń; nie potrzebuje osobnej bazy `_MigrationHistory`. Aktualna baza zawiera dane aplikacji i swoją historię migracji, a kopia odtwarza oba te elementy ze stanu backupu.

W PowerShell możesz nadpisać połączenie na czas bieżącej sesji bez edytowania plików repozytorium:

```powershell
$developmentConnection = 'Server=YOUR_SQL_INSTANCE;Database=layZTicketsDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
$env:ConnectionStrings__DefaultConnection = $developmentConnection
```

Zastąp `YOUR_SQL_INSTANCE` nazwą swojej dostępnej instancji. Przykład używa uwierzytelniania Windows i lokalnego zaufania do certyfikatu SQL Server; jest konfiguracją do prób deweloperskich. Nie zawiera hasła i nie jest gotową konfiguracją wdrożeniową.

W PowerShell nazwa instancji zawiera pojedynczy ukośnik, np. `'HOST\INSTANCE'`. W JSON ten sam znak wymaga zapisu `"HOST\\INSTANCE"`. Skopiowanie dwóch ukośników z JSON do PowerShell może spowodować błąd połączenia `Instance failure`.

Zmienna środowiskowa jest przekazywana procesom uruchomionym z tej konsoli. Aplikację uruchom z tej samej sesji; uruchomienie jej niezależnie z IDE może korzystać z innej konfiguracji.

Po dawnych testach zmienna może jeszcze wskazywać nieistniejącą już bazę `_MigrationTest` lub `_FreshTest`. Aby w bieżącej konsoli wrócić do domyślnego ustawienia z `appsettings.json`:

```powershell
Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
```

Zmienne pomocnicze, takie jak `$migrationConnection` i `$freshConnection`, również mogą zawierać stare nazwy; nie należy używać ich do kolejnych aktualizacji bez sprawdzenia celu.

### Pakiety, narzędzia i kompilacja

Wykonuj polecenia z katalogu głównego repozytorium. Połączenie ustaw w tej samej sesji PowerShell, w której uruchamiasz aplikację.

```powershell
dotnet restore layZTickets.sln
dotnet tool restore
dotnet ef --version
dotnet build layZTickets.sln --no-restore
```

`dotnet tool restore` odtwarza narzędzie zapisane w istniejącym `dotnet-tools.json`; nie trzeba ponownie tworzyć manifestu. Oczekiwana wersja `dotnet-ef` to `8.0.27`.

### Migracje i nowa baza

| Migracja | Zmiana |
| --- | --- |
| `20261020162759_InitialSchema` | Tabele `Tickets` i `TicketComments`, klucze główne z `IDENTITY`, relacja z kaskadowym usuwaniem komentarzy, ich indeks `TicketId` i wartości domyślne |
| `20261020163451_AddUniqueTicketNumberIndex` | Unikalny indeks `IX_Tickets_TicketNumber` obejmujący niepuste numery |
| `20261020184200_RemoveStoredTicketNumber` | Usunięcie indeksu i kolumny `TicketNumber`; aktualny numer jest wyliczany z `Id` w C# |
| `20261021100725_AddTicketCategory` | Kolumna `Tickets.Category` typu `int`, wymagana, z wartością domyślną `0` (`Other`) dla istniejących zgłoszeń |
| `20261021122842_ConvertTicketStatusAndPriorityToEnums` | Konwersja `Status` i `Priority` z tekstu na `int` zgodnie z enumami; dawne `Critical` odpowiada `Highest = 3` |
| `20261021150554_AddIdentitySchema` | Siedem tabel Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`) wraz z relacjami i indeksami; bez zmian tabel zgłoszeń. Zastosowanie potwierdzone przez użytkownika |
| `20261022173719_AddUserNames` | `AspNetUsers.FirstName` i `LastName`, obie `nvarchar(100)` i wymagane. Uzgodnione dane demonstracyjne istniejących kont: Admin Admin dla administratora, Jan Kowalski dla zwykłego konta. Zastosowana przez agenta do bazy roboczej |

Migracje opisują kolejne decyzje projektowe: pierwsza tworzyła kolumnę numeru, druga dodała indeks, a trzecia usuwa obie struktury. Po zastosowaniu wszystkich trzech baza nie przechowuje numeru zgłoszenia. `Down()` trzeciej migracji przywraca kolumnę z pustymi wartościami i indeks, ale nie odtwarza historycznej zawartości usuniętej kolumny; przed jej usunięciem wykonano i zweryfikowano backup kopii bazy.

Czwarta migracja dodaje kategorię: `Other = 0` (Inne), `Hardware = 1` (Sprzęt), `Software = 2` (Oprogramowanie), `Network = 3` (Sieć). W modelu C# jest to enum, w SQL Server liczba całkowita. `InputRadioGroup` aktualizuje `ticketDraft.Category`, a zapis zgłoszenia obejmuje tę właściwość bez osobnego przypisania. Znaczenie zapisanych wartości liczbowych enuma powinno pozostać stałe.

Piąta migracja mapuje tekstowe statusy na `New = 0`, `InProgress = 1`, `Pending = 2`, `Closed = 3`, a priorytety na `Low = 0`, `Normal = 1`, `High = 2`, `Highest = 3`. Najpierw zamienia nazwy na tekstowe liczby, a następnie zmienia typ obu kolumn na `int` w transakcji migracji. Dawne `Critical` oraz tekstowe `Highest` oznaczają wartość `3`; nierozpoznana wartość przerywa migrację. `Down()` przywraca tekstowe nazwy, używając dawnego `Critical` dla najwyższego priorytetu. Formularz tworzenia korzysta z nazw obecnego enuma, w tym `Highest` zamiast `Critical`.

Szósta migracja tworzy strukturę Identity. Samo wygenerowanie plików i skryptu SQL nie wykonuje zmian w bazie. Użytkownik zastosował ją i potwierdził sześć migracji bez `Pending`, siedem tabel `AspNet...` i `UserCount = 0`. Migracja nie tworzy administratora, nie uruchamia logowania ani nie wiąże dotychczasowych tekstowych autorów zgłoszeń z kontami. `Down()` usuwa siedem tabel Identity wraz z ich ewentualną zawartością, bez usuwania tabel zgłoszeń. Przed stosowaniem migracji w innym środowisku trzeba sprawdzić docelowe połączenie i stan bazy.

Siódma migracja dodaje imię i nazwisko, a istniejące konta uzupełnia uzgodnionymi danymi demonstracyjnymi na podstawie roli `Admin`. Nie zmienia identyfikatorów, loginów, haseł ani przypisań do ról. `Down()` usuwa obie nowe kolumny wraz z ich zawartością. Nowe konta dostają dane z formularza, nie wartości demonstracyjne. W bazie roboczej potwierdzono 7 migracji, 2 konta, 10 zgłoszeń i 13 komentarzy oraz zgodność skrótu wszystkich dotychczasowych pól użytkowników sprzed i po migracji.

Dla nowej bazy, po ustawieniu przykładowego połączenia powyżej:

```powershell
dotnet ef database update --context AppDbContext --connection $developmentConnection
```

Polecenie tworzy bazę i tabele oraz zapisuje wykonane migracje w `dbo.__EFMigrationsHistory`. Nowa baza nie zawiera przykładowych zgłoszeń ani komentarzy. Uruchomienie aplikacji samo nie przygotuje schematu.

`dotnet ef migrations add NazwaZmiany --context AppDbContext` generuje pliki migracji na podstawie różnicy między modelem a snapshotem. Nie aktualizuje bazy; przed zastosowaniem trzeba przejrzeć wygenerowane operacje. Przy zwykłym uruchomieniu projektu korzysta się z już istniejących migracji.

### Istniejąca baza z poprzedniej wersji

Dotyczy baz utworzonych przez `EnsureCreatedAsync` i ręczny SQL, bez historii migracji. Nie stosuj do nich pierwszej migracji wprost: próba ponownego utworzenia istniejących tabel zakończy się błędem. Sprawdzona procedura przejścia obejmowała:

1. Pełną kopię zapasową `.bak` i odtworzenie pod nową nazwą `layZTicketsDb_MigrationTest`, z osobnymi plikami danych i logu.
2. Porównanie danych oraz definicji tabel z `InitialSchema`: kolumn, typów, wymagalności, `IDENTITY`, kluczy, wartości domyślnych, kaskady i indeksów. Sprawdzono też brak powtarzających się niepustych numerów zgłoszeń.
3. Po potwierdzeniu zgodności — utworzenie tabeli `__EFMigrationsHistory` i wpisanie `20261020162759_InitialSchema` z `ProductVersion = '8.0.27'`, bez wykonywania operacji tworzenia istniejących tabel.
4. Zastosowanie `AddUniqueTicketNumberIndex` narzędziem EF z jawnym `--connection` do odtworzonej kopii.
5. Sprawdzenie historii migracji, liczby rekordów i działania aplikacji na kopii.

Po późniejszej decyzji o wyliczaniu numeru wykonano świeży backup kopii i zastosowano `RemoveStoredTicketNumber`. Potwierdzono zachowanie wszystkich pozostałych danych zgłoszeń i komentarzy. Dla bazy mającej już dwie pierwsze migracje zwykłe `database update` stosuje brakujące migracje: usunięcie przechowywanego numeru, dodanie kategorii oraz konwersję statusu i priorytetu na liczby.

Definicję tabeli historii i właściwy wpis można odczytać ze skryptu wygenerowanego poleceniem:

```powershell
dotnet ef migrations script 0 InitialSchema --context AppDbContext
```

Generowanie tego skryptu nie wykonuje go w bazie. Przy rejestracji istniejącej struktury wykorzystuje się wyłącznie utworzenie historii i wpis pierwszej migracji, po sprawdzeniu bazy docelowej. Pełny skrypt zawiera również `CREATE TABLE` dla zgłoszeń i komentarzy. Ręczny wpis do historii nie weryfikuje ani nie naprawia struktury; jest specjalnym krokiem przejścia, a nie sposobem pomijania brakujących zmian. Pierwsza migracja pozostaje kompletna, aby nadal umożliwiała utworzenie nowej bazy.

Przejście wykonano na odtworzonej kopii. Następnie aktualną bazę `_MigrationTest` przemianowano na `layZTicketsDb`, a z jej backupu odtworzono `layZTicketsDb_backup` jako tylko do odczytu. Wszystkie dane zgłoszeń i komentarzy ze starej bazy miały odpowiedniki w aktualnej; starą bazę usunięto po backupie, porównaniu danych i sprawdzeniu aplikacji. Dawne nazwy baz w opisie prób dokumentują przebieg testów, nie są aktualnymi celami połączeń.

### Start aplikacji

Profil HTTPS korzysta z lokalnego certyfikatu deweloperskiego. Jeśli nie jest jeszcze zaufany, skonfiguruj go:

```powershell
dotnet dev-certs https --trust
```

Po skonfigurowaniu bazy i certyfikatu:

```powershell
dotnet run --project layZTickets.csproj --launch-profile https
```

Profil `https` określa adresy `https://localhost:7261` i `http://localhost:5269`; aplikacja ma włączone przekierowanie HTTPS. Otwórz [logowanie](https://localhost:7261/login). Po zalogowaniu dostępne są lista zgłoszeń i formularz tworzenia. Strona `/` jest nadal stroną szablonową. Zatrzymanie aplikacji: `Ctrl+C`.

### Pierwszy administrator i kolejne konta

W środowisku Development podaj `BootstrapAdmin:Email` i `BootstrapAdmin:Password` przez User Secrets, poza repozytorium. Następnie uruchom jednorazowo:

```powershell
dotnet run --project layZTickets.csproj --launch-profile https -- --bootstrap-admin=true
```

Polecenie tworzy brakującą rolę `Admin`, konto i przypisanie do roli, po czym kończy proces. Nie zmienia hasła już istniejącego konta. Zwykłe uruchomienie aplikacji nie odtwarza administratora. Hasło musi spełniać aktualne reguły Identity; obecne ustawienia wymagają minimum 6 znaków, małej i wielkiej litery, cyfry oraz znaku specjalnego. Po konfiguracji można usunąć `BootstrapAdmin:Password` z User Secrets.

Po zalogowaniu administrator otwiera **Panel administratora → Utwórz użytkownika**. Podaje imię i nazwisko (każde wymagane, do 100 znaków), e-mail będący loginem oraz hasło i jego potwierdzenie. Dozwolone są polskie znaki, dwa imiona i nazwiska z łącznikiem. W tej wersji nie wysyłamy wiadomości, zaproszeń ani potwierdzeń e-mail. Nowe konto nie otrzymuje roli `Admin`; nie następuje też przełączenie sesji administratora na utworzone konto.

`UserData` zbiera dane POST, a `UserManager.CreateAsync` weryfikuje reguły Identity, tworzy hash hasła i zapisuje `ApplicationUser`, w tym imię i nazwisko, przez istniejący magazyn EF do `AspNetUsers`. Struktura bazy wymaga migracji `AddUserNames`. Pola hasła są czyszczone po każdej obsłużonej próbie. Po sukcesie następuje przekierowanie na GET, aby odświeżenie nie ponawiało zapisu. Po błędzie krótki skrypt przeglądarkowy ponownie otwiera panel z komunikatem; pozostałe dane pozostają w formularzu, hasła trzeba wpisać ponownie.

Przy tworzeniu pierwszego administratora domyślne imię i nazwisko to Admin Admin. Opcjonalne klucze `BootstrapAdmin:FirstName` i `BootstrapAdmin:LastName` pozwalają podać inne dane nowego administratora. Ponowne wykonanie bootstrapu nie nadpisuje danych już istniejącego konta.

`ApplicationUserClaimsPrincipalFactory` dodaje `ClaimTypes.GivenName` i `ClaimTypes.Surname` do tożsamości zapisywanej w cookie. Menu wyświetla je zamiast e-maila, a login nadal pozostaje adresem e-mail. **Po aktualizacji wyloguj się i zaloguj ponownie**, aby odtworzyć cookie z nowymi danymi. Starsze cookie zachowuje dotychczasowy widok z adresem e-mail. Zmiana danych profilu i automatyczne odświeżanie tych informacji będą osobnym zadaniem.

### Stan weryfikacji

| Sprawdzenie | Wynik na 22.10.2026; wcześniejsze próby zachowują opis swojego zakresu |
| --- | --- |
| `dotnet build layZTickets.sln --no-restore -v minimal` | Sukces, 0 błędów, 0 ostrzeżeń; użyto już dostępnych pakietów |
| Przywrócenie pakietów w nowym środowisku | Niezweryfikowane |
| Lokalne uruchomienie aplikacji | Potwierdzone przez użytkownika podczas ręcznych prób |
| Konfiguracja certyfikatu od zera | Niezweryfikowana niezależnie |
| Usunięcie SQL poprawiającego schemat ze stron | Agent potwierdził brak `EnsureCreatedAsync`, `EnsureTicketSchemaAsync` i ręcznego SQL w stronach zgłoszeń oraz poprawną kompilację |
| Kopia zapasowa i odtworzenie | Użytkownik potwierdził 6 zgłoszeń i 8 komentarzy w oryginale oraz odtworzonej kopii |
| Przejście istniejącej kopii na migracje | Użytkownik potwierdził oba wpisy w historii oraz nadal 6 zgłoszeń i 8 komentarzy po dodaniu indeksu |
| Interfejs po zmianach obsługi bazy | Użytkownik potwierdził odczyt istniejących zgłoszeń i komentarzy, utworzenie nowego zgłoszenia, zapis odpowiedzi i notatki oraz daty aktualizacji |
| Nowa baza utworzona wyłącznie migracjami | Użytkownik potwierdził 2 wpisy historii, 0 zgłoszeń i 0 komentarzy w `layZTicketsDb_FreshTest` |
| Usunięcie przechowywanego numeru | Agent zastosował trzecią migrację do `layZTicketsDb_MigrationTest`, potwierdził brak kolumny i indeksu oraz 3 wpisy historii; nadal 7 zgłoszeń i 10 komentarzy |
| Zachowanie danych podczas trzeciej migracji | Agent porównał skróty SHA-256 uporządkowanych danych przed i po: wszystkie pozostałe pola zgłoszeń oraz wszystkie pola komentarzy zgodne; usunięte numery celowo wyłączone z porównania |
| Odczyt aplikacji po trzeciej migracji | Agent sprawdził odpowiedzi HTTP 200 i HTML listy 7 zgłoszeń oraz szczegółów jednego zgłoszenia; numery mają format `IT-1` do `IT-7` |
| Baza robocza i kopia po uporządkowaniu nazw | Agent potwierdził `layZTicketsDb` do zapisu i `layZTicketsDb_backup` tylko do odczytu, zgodność danych i historii migracji oraz listę i szczegóły HTTP 200 z domyślnej konfiguracji |
| Walidacja wymaganych pól | Użytkownik potwierdził wymaganie tytułu, opisu i zgłaszającego; pusty komentarz sprawdzony w ramach próby komentowania |
| Nieistniejący identyfikator | Użytkownik potwierdził ekran „Nie znaleziono ticketu” dla `/DisplayTicket/777` |
| Zapis po uproszczeniu numeracji | Kod i kompilacja potwierdzają pojedyncze `SaveChangesAsync`; użytkownik potwierdził tworzenie i komentowanie po trzeciej migracji wynikiem 8 zgłoszeń i 12 komentarzy |
| Numeracja dla `Id` powyżej 9999 | Agent sprawdził skompilowaną właściwość C# dla 1, 9999, 10000 i `Int32.MaxValue`; nie wstawiał takich identyfikatorów do SQL Server |
| Kategorie po czwartej migracji | Użytkownik potwierdził poprawne działanie kategorii po migracji. Agent sprawdził binding, model, migrację i kompilację; nie wykonywał własnej próby z bazą ani interfejsem. Polskie nazwy dodano przez `GetCategoryName`; pozostały drobne poprawki tekstów i sprawdzenie ich wyświetlania |
| Walidacja kategorii | Użytkownik wymusił `(TicketCategory)9999` przed warunkiem i potwierdził komunikat błędu oraz brak nowego zgłoszenia. Agent potwierdził `Enum.IsDefined` przed dostępem do bazy, poprawną kompilację i usunięcie testowego przypisania |
| Statusy i priorytety po piątej migracji | Agent zastosował migrację do `layZTicketsDb`: obie kolumny mają typ `int`, historia zawiera 5 migracji. Zachowano 9 zgłoszeń i 12 komentarzy; skróty SHA-256 wszystkich pól zgłoszeń po uwzględnieniu mapowania oraz wszystkich pól komentarzy są zgodne. Statusy: 9 × `New`; priorytety: 6 × `Normal`, 3 × `Highest` (dawne `Critical`). Kompilacja bez błędów i ostrzeżeń, model zgodny ze snapshotem. Formularza po tej migracji nie sprawdzano interaktywnie; cofania migracji nie wykonywano |
| Zmiana statusu i priorytetu w szczegółach | Użytkownik potwierdził podstawowe działanie podczas ręcznych prób. Agent sprawdził kod po commicie `da2245f` (`Improved error handling`) i kompilację: 0 błędów, 0 ostrzeżeń. Kod zapisuje pola i daty, blokuje nakładające się wywołania, rozdziela błędy odczytu od aktualizacji i czyści `ClosedAt` przy ponownym otwarciu. Agent nie wykonywał własnych prób z bazą ani interfejsem tej wersji. Trwałość ostatnich zmian dat, błędy zapisu i ponowienie wymagają dalszych prób |
| Przygotowanie migracji Identity | Agent wygenerował `20261021150554_AddIdentitySchema`, przejrzał `Up()` i `Down()` oraz wygenerowany SQL: wyłącznie siedem tabel Identity z relacjami i indeksami, bez zmian `Tickets` i `TicketComments`. Kompilacja: 0 błędów i ostrzeżeń; model zgodny ze snapshotem. Nie łączono się z bazą ani nie stosowano tej migracji |
| Zastosowanie migracji Identity | Użytkownik zastosował migrację i potwierdził sześć migracji bez `Pending`, siedem tabel `AspNet...` i `UserCount = 0`. Agent nie wykonywał niezależnego sprawdzenia bazy. Odczyt listy i szczegółów zgłoszeń po tej migracji oraz porównanie liczby zgłoszeń i komentarzy pozostają do potwierdzenia |
| Konfiguracja usług Identity | Agent sprawdził w `Program.cs` rejestrację kont, ról, magazynów EF, autoryzacji i stanu uwierzytelnienia dla Blazora oraz kolejność `UseAuthentication`, `UseAuthorization` i `UseAntiforgery`. Użytkownik potwierdził poprawne uruchomienie aplikacji. Nie wykonywano jeszcze prób tworzenia kont, logowania, wylogowania ani egzekwowania uprawnień do zgłoszeń |
| Konta i sesja — 22.10.2026 | Użytkownik potwierdził utworzenie administratora (`UserCount = 1`), działanie logowania, wylogowania oraz brak dostępu do chronionych stron po wylogowaniu także przez bezpośredni adres. Menu korzysta z rzeczywistej nazwy i roli konta |
| Tworzenie kont w panelu administratora — 22.10.2026 | Build: 0 błędów i ostrzeżeń. Izolowane próby HTTP skompilowanych stron z rzeczywistym Identity i testowym magazynem w pamięci: 40 sprawdzeń, w tym wymagane pola, e-mail, różne hasła, polityka hasła, duplikat bez rozróżniania wielkości liter, hash, logowanie nowego konta, zachowanie sesji administratora, brak nadania roli z dodatkowych pól POST, brak hasła w HTML, brak ponownego zapisu po odświeżeniu, obsługa błędu magazynu, odrzucenie POST bez tokena oraz odczytu i zapisu osoby niezalogowanej i zwykłego użytkownika. Nie korzystano z SQL Server ani istniejących sekretów |
| Ręczna próba formularza z SQL Server | Użytkownik potwierdził sprawdzenie scenariuszy i poprawne działanie formularza sprzed rozszerzenia o imię i nazwisko. Punkt TODO oznaczono jako wykonany. Własny ekran odmowy dostępu nadal pozostaje do przygotowania; obecny zakaz dostępu kieruje na domyślny adres Identity |
| Imię i nazwisko — baza | Agent zastosował `AddUserNames` do `layZTicketsDb` i potwierdził typy oraz wymaganie nowych kolumn, dane Admin Admin i Jan Kowalski, 2 konta, 10 zgłoszeń i 13 komentarzy. Skrót wszystkich wcześniejszych pól użytkowników jest zgodny. Model zgodny ze snapshotem, build bez błędów i ostrzeżeń |
| Imię i nazwisko — zachowanie | 51 izolowanych sprawdzeń HTTP z magazynem w pamięci: poprzednie reguły tworzenia kont oraz wymagane nazwy, limit 100 znaków, przycinanie spacji, dwa imiona, polskie znaki i nazwiska z łącznikiem, poprawna nazwa i inicjały po logowaniu oraz różne identyfikatory kont o identycznych nazwach. Agent nie wykonywał interaktywnego logowania do lokalnej aplikacji po tej zmianie |
| Pusta baza z aktualnymi siedmioma migracjami | Do sprawdzenia; dotychczasowy test pustej bazy obejmował dwie pierwsze migracje |
| Testy automatyczne | Brak projektu testowego |

Przed zmianami użytkownik potwierdził tworzenie zgłoszeń, ich obecność na liście, szczegóły, odpowiedzi, notatki i trwałość po odświeżeniu. Po usunięciu kodu poprawiającego schemat powtórzył główne operacje na kopii po migracjach, potwierdzając poprawny odczyt, zapis i aktualizację daty. Próba pustej bazy potwierdza przygotowanie struktury, nie obejmowała jeszcze interfejsu tej bazy.

Wyniki wcześniejszych prób interaktywnych pochodzą od użytkownika. Przy trzeciej migracji agent wykonał backup kopii z sumami kontrolnymi, sprawdził go przez `RESTORE VERIFYONLY`, zastosował migrację i porównał pozostałe dane przed i po. Sprawdził też kod, SQL, kompilację, zgodność modelu ze snapshotem oraz odczyt listy i szczegółów przez HTTP. Te odczyty sprawdzały HTML generowany na serwerze; nie obejmowały klikania ani wysłania formularza w przeglądarce. Nie potwierdzono odtworzenia całego środowiska na czystym komputerze. Notatka wewnętrzna nadal nie ma ochrony uprawnieniami.

Ten sam scenariusz — utworzenie zgłoszenia, lista, szczegóły, odpowiedź, notatka, odświeżenie oraz walidacja i nieistniejący identyfikator — stanowi punkt odniesienia do ponownego sprawdzenia po zmianach obsługi bazy.

Problemy diagnozuj od komunikatu i kontekstu: SDK/pakiety przy budowaniu, certyfikat i port przy uruchamianiu, a instancja SQL Server, uwierzytelnianie, uprawnienia, zastosowane migracje i stan schematu przy otwieraniu stron zgłoszeń. Przy błędzie `Instance failure` sprawdź również zapis nazwy instancji w danym formacie konfiguracji. Nie usuwaj istniejącej bazy jako pierwszego kroku naprawy.
