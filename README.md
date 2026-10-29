# layZTickets

System HelpDesk napisany w C#. Użytkownik zgłasza problem, a zespół obsługi przypisuje go do agenta, prowadzi rozmowę i zmienia status.

## Funkcje

- Zgłoszenia z kategorią, priorytetem i statusem, wyszukiwanie oraz filtrowanie listy.
- Odpowiedzi, notatki wewnętrzne i załączniki do 5 MB.
- Konta użytkowników, agentów i administratorów. Użytkownik widzi własne zgłoszenia; agenci obsługują przypisane im kategorie.
- Panel zarządzania kontami i dashboard ze statystykami.

## Technologie

.NET 8, ASP.NET Core, Blazor Interactive Server, Entity Framework Core, SQL Server i ASP.NET Core Identity. Interfejs korzysta z CSS i Bootstrapa. Kod C# komponentów wykonuje się na serwerze.

Uprawnienia są sprawdzane przy odczycie i zapisie danych. Operacje na kontach korzystają z transakcji, struktura bazy jest wersjonowana migracjami, a szczegóły błędów trafiają do logów. Logowanie ma limit prób i czasową blokadę konta.

## Uruchomienie

Potrzebne są SDK .NET obsługujące `net8.0`, runtime ASP.NET Core 8 i SQL Server. Polecenia wykonuj w katalogu projektu. Poniższe wartości zastąp własnymi; konfiguracja User Secrets pozostaje poza repozytorium.

```powershell
dotnet restore
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SQL_INSTANCE;Database=layZTicketsDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.test"
dotnet user-secrets set "BootstrapAdmin:Password" "YOUR_ADMIN_PASSWORD"
dotnet run --launch-profile https -- --bootstrap-admin=true
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Hasło nowego konta musi mieć minimum 12 znaków, małą i wielką literę, cyfrę oraz znak specjalny. Pierwsze uruchomienie migracji przygotuje nową bazę. Dla istniejącej bazy najpierw sprawdź jej stan i wykonaj kopię zapasową.

Aplikacja działa pod adresem https://localhost:7261. Administrator tworzy pozostałe konta w panelu. Po utworzeniu zwykłego użytkownika i agentów można dodać przykładowe zgłoszenia:

```powershell
dotnet run --launch-profile https -- --seed-demo-tickets=true
```

Ponowienie tego polecenia nie nadpisuje istniejących przykładów. Po utworzeniu administratora można usunąć jego hasło konfiguracyjne przez `dotnet user-secrets remove "BootstrapAdmin:Password"`.

## Wdrożenie

Pakiet aplikacji buduje `dotnet publish -c Release`. Na serwerze ustaw `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection` oraz `AllowedHosts` dla swojej domeny. Migracje wykonuj osobno, przed uruchomieniem nowej wersji.

Publiczne wdrożenie wymaga HTTPS, trwałego magazynu kluczy Data Protection, zbierania logów i kopii bazy. Przy reverse proxy trzeba skonfigurować zaufane proxy i przekazywanie adresu klienta oraz protokołu — limit logowania korzysta z adresu IP. Te ustawienia zależą od wybranego hostingu.

Załączniki są przechowywane w SQL, bez skanowania antywirusowego. Projekt nie ma historii wszystkich zmian zgłoszenia ani obsługi konfliktów jednoczesnej edycji. Przed publicznym wdrożeniem należy też zaplanować aktualizację platformy: [wsparcie .NET 8 kończy się 10 listopada 2026](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
