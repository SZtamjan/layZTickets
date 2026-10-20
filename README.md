# layZTickets

Projekt aplikacji HelpDesk do portfolio pod stanowiska związane z C#. Rozwój projektu łączy ukończenie systemu zgłoszeń z praktyczną nauką frontendu, backendu, SQL, logowania i uprawnień.

Docelowo aplikacja ma obsługiwać użytkownika zgłaszającego problem, agenta Service Desk i administratora. Obecna wersja jest prototypem bez logowania i kontroli dostępu.

[Zasady współpracy i nauki](AGENTS.md) · [Etapy rozwoju](ROADMAP.md)

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

Stan analizy i weryfikacji: **20 października 2026 r.** Poniższa tabela opisuje implementację w kodzie. Podstawowy scenariusz tworzenia, odczytu i komentowania został również sprawdzony ręcznie przez użytkownika w jego lokalnym środowisku; zakres prób opisano w sekcji „Stan weryfikacji”.

| Obszar | Stan w kodzie |
| --- | --- |
| Tworzenie zgłoszenia | Formularz, sprawdzanie wymaganych pól, jeden zapis do bazy, priorytet i tekstowo wpisywani zgłaszający oraz agent; numer `IT-{Id}` wyliczany w C# bez zer wiodących |
| Lista zgłoszeń | Odczyt 10 najnowszych rekordów i przejście do szczegółów |
| Szczegóły i komentarze | Odczyt zgłoszenia z komentarzami; zapis odpowiedzi i notatek z flagą `IsInternal` |
| Schemat bazy | Trzy migracje EF Core; strony nie tworzą ani nie aktualizują schematu; `TicketNumber` nie jest kolumną bazy |
| Kategorie | Wybór widoczny w formularzu, bez zapisu w modelu i bazie |
| Edycja, zmiana statusu, przypisanie do siebie | Przyciski bez obsługi tych operacji; tekstowe przypisanie można podać przy tworzeniu |
| Filtry, wyszukiwarka, paginacja | Elementy interfejsu bez zaimplementowanej logiki |
| Załączniki | Makieta interfejsu, bez przesyłania i przechowywania plików |
| Logowanie i role | Brak uwierzytelniania i autoryzacji; dane profilu w układzie są przykładowe |
| Dashboard | Strona główna nadal zawiera treść szablonu |
| Przykłady z szablonu | Pozostały strony Counter i Weather; licznik korzysta z serwisu zapisu JSON |

### Ograniczenia wymagające uwagi

- Notatki „wewnętrzne” są pobierane wraz z pozostałymi komentarzami. Flaga i wygląd nie ograniczają dostępu do zespołu IT.
- Aplikacja wymaga bazy przygotowanej migracjami. Nie wykonuje ich automatycznie przy starcie ani podczas otwierania stron. Dotychczasowa baza utworzona przez `EnsureCreatedAsync` wymaga osobnego przygotowania historii migracji; zwykła aktualizacja nie rozpoznaje automatycznie istniejących tabel.
- Numer `TicketNumber` jest właściwością C# z `[NotMapped]`. Wyszukiwanie numeru w zapytaniu EF powinno wykorzystywać odpowiadające mu `Id`; właściwość wyliczana nie jest kolumną SQL.
- Pola nazwane `CreatedByUserId`, `AssignedAgentId` i `AuthorUserId` przechowują obecnie wpisywany tekst, bez powiązania z kontami.
- Obsługa błędów stron wyświetla treść wyjątków. Nie ma projektu testów automatycznych.

## Struktura projektu

| Miejsce | Odpowiedzialność |
| --- | --- |
| `Program.cs` | Konfiguracja aplikacji, usług, fabryki kontekstów EF i trybu renderowania |
| `Components/Pages/` | Strony, formularze i obecna logika operacji na zgłoszeniach |
| `Components/Layout/`, `Components/App.razor`, `Components/Routes.razor` | Układ, dokument HTML i routing |
| `Data/AppDbContext.cs` | Zestawy encji i konfiguracja modelu EF |
| `DataSets/` | Modele `Ticket` i `TicketComment` |
| `Migrations/` | Zmiany struktury bazy, metadane migracji i snapshot modelu, czyli jego zapis do porównania przy kolejnych zmianach |
| `dotnet-tools.json` | Manifest lokalnego narzędzia `dotnet-ef` w wersji 8.0.27 |
| `Components/Services/SaveService/`, `App_Data/` | Serwis i pliki JSON; licznik zapisuje tam swój stan, zgłoszenia korzystają z SQL Server |
| `wwwroot/` | CSS i zasoby statyczne |
| `Properties/launchSettings.json` | Lokalne profile uruchomienia i porty |

### Droga od formularza do bazy

1. Użytkownik uzupełnia formularz w `Components/Pages/CreateTicket.razor`. `@bind-Value` wiąże pola z obiektem `ticketDraft`.
2. Wysłanie `EditForm` uruchamia na serwerze `CreateTicketAsync`. Metoda sprawdza tytuł, opis i zgłaszającego oraz ustawia wartości początkowe.
3. Wstrzyknięta przez DI fabryka `IDbContextFactory<AppDbContext>` tworzy kontekst do operacji na bazie. Rejestracja fabryki znajduje się w `Program.cs`.
4. Kontekst korzysta ze struktury przygotowanej wcześniej migracjami. Komponent nie tworzy tabel ani nie wykonuje ręcznego SQL poprawiającego schemat lub numery istniejących zgłoszeń.
5. `Tickets.Add` dodaje obiekt do śledzenia przez EF. Jedyne `SaveChangesAsync` zapisuje go w SQL Server i uzupełnia `Id`.
6. Właściwość `TicketNumber` zwraca `IT-{Id}`, np. `IT-7`. Numer jest wyliczany przy odczycie właściwości, bez zapisu w bazie. Komponent wyświetla komunikat i czyści formularz. Nie ma już osobnego zapisu numeru wymagającego wspólnej transakcji z utworzeniem zgłoszenia.
7. Lista wykonuje osobny odczyt przez EF, a szczegóły dołączają komentarze przez `Include`. Zapis komentarza aktualizuje również `UpdatedAt` zgłoszenia.

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

Migracje opisują kolejne decyzje projektowe: pierwsza tworzyła kolumnę numeru, druga dodała indeks, a trzecia usuwa obie struktury. Po zastosowaniu wszystkich trzech baza nie przechowuje numeru zgłoszenia. `Down()` trzeciej migracji przywraca kolumnę z pustymi wartościami i indeks, ale nie odtwarza historycznej zawartości usuniętej kolumny; przed jej usunięciem wykonano i zweryfikowano backup kopii bazy.

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

Po późniejszej decyzji o wyliczaniu numeru wykonano świeży backup kopii i zastosowano `RemoveStoredTicketNumber`. Potwierdzono zachowanie wszystkich pozostałych danych zgłoszeń i komentarzy. Dla bazy mającej już dwie pierwsze migracje zwykłe `database update` stosuje trzecią; przy przejściu starszej bazy na aktualny stan należy uwzględnić również tę migrację.

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

Profil `https` określa adresy `https://localhost:7261` i `http://localhost:5269`; aplikacja ma włączone przekierowanie HTTPS. Otwórz [listę zgłoszeń](https://localhost:7261/DisplayTickets) lub [formularz tworzenia](https://localhost:7261/CreateTicket). Strona `/` jest nadal stroną szablonową. Zatrzymanie aplikacji: `Ctrl+C`.

### Stan weryfikacji

| Sprawdzenie | Wynik na 20.10.2026 |
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
| Zapis po uproszczeniu numeracji | Kod i kompilacja potwierdzają pojedyncze `SaveChangesAsync`; nie ma osobnego zapisu numeru. Nowe tworzenie i komentowanie przez interaktywny formularz po trzeciej migracji wymaga ponownej próby |
| Numeracja dla `Id` powyżej 9999 | Agent sprawdził skompilowaną właściwość C# dla 1, 9999, 10000 i `Int32.MaxValue`; nie wstawiał takich identyfikatorów do SQL Server |
| Pusta baza z aktualnymi trzema migracjami | Do ponownego sprawdzenia; dotychczasowy test pustej bazy obejmował dwie pierwsze migracje |
| Testy automatyczne | Brak projektu testowego |

Przed zmianami użytkownik potwierdził tworzenie zgłoszeń, ich obecność na liście, szczegóły, odpowiedzi, notatki i trwałość po odświeżeniu. Po usunięciu kodu poprawiającego schemat powtórzył główne operacje na kopii po migracjach, potwierdzając poprawny odczyt, zapis i aktualizację daty. Próba pustej bazy potwierdza przygotowanie struktury, nie obejmowała jeszcze interfejsu tej bazy.

Wyniki wcześniejszych prób interaktywnych pochodzą od użytkownika. Przy trzeciej migracji agent wykonał backup kopii z sumami kontrolnymi, sprawdził go przez `RESTORE VERIFYONLY`, zastosował migrację i porównał pozostałe dane przed i po. Sprawdził też kod, SQL, kompilację, zgodność modelu ze snapshotem oraz odczyt listy i szczegółów przez HTTP. Te odczyty sprawdzały HTML generowany na serwerze; nie obejmowały klikania ani wysłania formularza w przeglądarce. Nie potwierdzono odtworzenia całego środowiska na czystym komputerze. Notatka wewnętrzna nadal nie ma ochrony uprawnieniami.

Ten sam scenariusz — utworzenie zgłoszenia, lista, szczegóły, odpowiedź, notatka, odświeżenie oraz walidacja i nieistniejący identyfikator — stanowi punkt odniesienia do ponownego sprawdzenia po zmianach obsługi bazy.

Problemy diagnozuj od komunikatu i kontekstu: SDK/pakiety przy budowaniu, certyfikat i port przy uruchamianiu, a instancja SQL Server, uwierzytelnianie, uprawnienia, zastosowane migracje i stan schematu przy otwieraniu stron zgłoszeń. Przy błędzie `Instance failure` sprawdź również zapis nazwy instancji w danym formacie konfiguracji. Nie usuwaj istniejącej bazy jako pierwszego kroku naprawy.
