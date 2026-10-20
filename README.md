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

Stan analizy: **20 października 2026 r.** „Zaimplementowane” oznacza obecność ścieżki w kodzie, nie potwierdzenie działania z bazą.

| Obszar | Stan w kodzie |
| --- | --- |
| Tworzenie zgłoszenia | Formularz, sprawdzanie wymaganych pól, zapis, numer `IT-…`, priorytet i tekstowo wpisywani zgłaszający oraz agent |
| Lista zgłoszeń | Odczyt 10 najnowszych rekordów i przejście do szczegółów |
| Szczegóły i komentarze | Odczyt zgłoszenia z komentarzami; zapis odpowiedzi i notatek z flagą `IsInternal` |
| Kategorie | Wybór widoczny w formularzu, bez zapisu w modelu i bazie |
| Edycja, zmiana statusu, przypisanie do siebie | Przyciski bez obsługi tych operacji; tekstowe przypisanie można podać przy tworzeniu |
| Filtry, wyszukiwarka, paginacja | Elementy interfejsu bez zaimplementowanej logiki |
| Załączniki | Makieta interfejsu, bez przesyłania i przechowywania plików |
| Logowanie i role | Brak uwierzytelniania i autoryzacji; dane profilu w układzie są przykładowe |
| Dashboard | Strona główna nadal zawiera treść szablonu |
| Przykłady z szablonu | Pozostały strony Counter i Weather; licznik korzysta z serwisu zapisu JSON |

### Ograniczenia wymagające uwagi

- Notatki „wewnętrzne” są pobierane wraz z pozostałymi komentarzami. Flaga i wygląd nie ograniczają dostępu do zespołu IT.
- Brak migracji EF Core. Strony zgłoszeń wywołują `EnsureCreatedAsync` i powieloną metodę `EnsureTicketSchemaAsync`, która wykonuje `ALTER TABLE` i `UPDATE`. Samo otwarcie listy lub szczegółów może zmienić bazę.
- Numer zgłoszenia jest nadawany drugim wywołaniem `SaveChangesAsync`; oba zapisy nie są objęte wspólną, jawną transakcją.
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
| `Components/Services/SaveService/`, `App_Data/` | Serwis i pliki JSON; licznik zapisuje tam swój stan, zgłoszenia korzystają z SQL Server |
| `wwwroot/` | CSS i zasoby statyczne |
| `Properties/launchSettings.json` | Lokalne profile uruchomienia i porty |

### Droga od formularza do bazy

1. Użytkownik uzupełnia formularz w `Components/Pages/CreateTicket.razor`. `@bind-Value` wiąże pola z obiektem `ticketDraft`.
2. Wysłanie `EditForm` uruchamia na serwerze `CreateTicketAsync`. Metoda sprawdza tytuł, opis i zgłaszającego oraz ustawia wartości początkowe.
3. Wstrzyknięta przez DI fabryka `IDbContextFactory<AppDbContext>` tworzy kontekst do operacji na bazie. Rejestracja fabryki znajduje się w `Program.cs`.
4. Obecny kod zapewnia utworzenie bazy i wykonuje własne polecenia aktualizacji schematu. To istniejące obejście, zaplanowane do zastąpienia migracjami.
5. `Tickets.Add` dodaje obiekt do śledzenia przez EF. Pierwsze `SaveChangesAsync` zapisuje go w SQL Server i uzupełnia `Id`.
6. Kod tworzy `TicketNumber` z `Id` i zapisuje go drugim `SaveChangesAsync`, po czym wyświetla komunikat i czyści formularz.
7. Lista wykonuje osobny odczyt przez EF, a szczegóły dołączają komentarze przez `Include`. Zapis komentarza aktualizuje również `UpdatedAt` zgłoszenia.

## Uruchomienie lokalne

### Wymagania

- SDK .NET obsługujący budowanie projektu `net8.0`; do lokalnego uruchomienia potrzebny jest runtime ASP.NET Core 8 i .NET 8.
- Dostępna instancja **SQL Server** oraz konto z uprawnieniami do odczytu, zapisu i obecnych operacji tworzenia bazy/tabel oraz zmiany schematu. Samo SDK nie instaluje serwera bazy danych.
- Dostęp do NuGet przy pierwszym przywracaniu pakietów.
- PowerShell dla poniższych przykładów. Visual Studio jest opcjonalne.

W analizowanym środowisku kompilacja powiodła się z SDK `10.0.400`; runtime ASP.NET Core 8 i .NET 8 są zainstalowane. Repozytorium nie przypina SDK plikiem `global.json`.

### Konfiguracja bazy

Aplikacja odczytuje `ConnectionStrings:DefaultConnection`. Repozytorium zawiera lokalne ustawienie w `appsettings.json`; nie zakładaj, że pasuje do Twojego komputera. Użyj osobnej bazy deweloperskiej, ponieważ obecne strony mogą modyfikować jej schemat i dane także przy odczycie.

W PowerShell możesz nadpisać połączenie na czas bieżącej sesji bez edytowania plików repozytorium:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=YOUR_SQL_INSTANCE;Database=layZTickets_Dev;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
```

Zastąp `YOUR_SQL_INSTANCE` nazwą swojej dostępnej instancji. Przykład używa uwierzytelniania Windows i lokalnego zaufania do certyfikatu SQL Server; jest konfiguracją do prób deweloperskich. Nie zawiera hasła i nie jest gotową konfiguracją wdrożeniową.

Nie uruchamiaj obecnie `dotnet ef database update` jako kroku instalacji: projekt nie zawiera migracji. `EnsureCreatedAsync` nie zastępuje historii migracji ani pełnej aktualizacji dowolnej istniejącej bazy.

### Budowanie i start

Wykonuj polecenia z katalogu głównego repozytorium. Połączenie ustaw w tej samej sesji PowerShell, w której uruchamiasz aplikację.

```powershell
dotnet restore layZTickets.sln
dotnet build layZTickets.sln --no-restore
```

Profil HTTPS korzysta z lokalnego certyfikatu deweloperskiego. Jeśli nie jest jeszcze zaufany, skonfiguruj go:

```powershell
dotnet dev-certs https --trust
```

Po skonfigurowaniu bazy i certyfikatu:

```powershell
dotnet run --project layZTickets.csproj --launch-profile https
```

Profil `https` określa adresy `https://localhost:7261` i `http://localhost:5269`; aplikacja ma włączone przekierowanie HTTPS. Otwórz [listę zgłoszeń](https://localhost:7261/DisplayTickets) lub [formularz tworzenia](https://localhost:7261/CreateTicket). Strona `/` jest nadal stroną szablonową. Zatrzymanie aplikacji: `Ctrl+C`.

### Stan weryfikacji i pierwsza próba

| Sprawdzenie | Wynik na 20.10.2026 |
| --- | --- |
| `dotnet build layZTickets.sln --no-restore -v minimal` | Sukces, 0 błędów, 0 ostrzeżeń; użyto już dostępnych pakietów |
| Przywrócenie pakietów w nowym środowisku | Niezweryfikowane |
| Konfiguracja certyfikatu i uruchomienie profilu HTTPS | Niezweryfikowane |
| Połączenie z SQL Server i operacje na danych | Niezweryfikowane |
| Interakcje w przeglądarce | Niezweryfikowane |
| Testy automatyczne | Brak projektu testowego |

Pierwsza próba na osobnej bazie deweloperskiej powinna objąć utworzenie zgłoszenia, odnalezienie go na liście, otwarcie szczegółów oraz dodanie odpowiedzi i notatki. Po odświeżeniu sprawdź trwałość danych. Sprawdź też pusty formularz i adres szczegółów z nieistniejącym identyfikatorem. Notatka wewnętrzna na tym etapie nadal nie ma ochrony uprawnieniami.

Problemy diagnozuj od komunikatu i kontekstu: SDK/pakiety przy budowaniu, certyfikat i port przy uruchamianiu, a instancja SQL Server, uwierzytelnianie, uprawnienia i stan schematu przy otwieraniu stron zgłoszeń. Nie usuwaj istniejącej bazy jako pierwszego kroku naprawy.
