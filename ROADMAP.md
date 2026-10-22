# Rozwój i nauka layZTickets

Celem jest ukończony projekt HelpDesk do portfolio oraz zrozumienie jego działania od UI po bazę danych. Domyślnie użytkownik pisze kod, a agent prowadzi zgodnie z [AGENTS.md](AGENTS.md). Opis obecnej aplikacji i uruchomienia znajduje się w [README.md](README.md).

## Jak korzystać z planu

- Bieżąca kolejność sesji i krótka lista do odhaczania: [TODO.md](TODO.md).
- Pracujemy nad jednym małym zadaniem naraz. Każdy etap dzielimy na takie zadania przed jego implementacją.
- Statusy: **do rozpoczęcia**, **w toku**, **wymaga weryfikacji**, **ukończony**. Obecność kodu nie oznacza ukończenia etapu.
- Status aktualizujemy po faktycznej pracy i sprawdzeniach. Ukończenie wymaga spełnienia kryteriów funkcjonalnych i omówienia użytych mechanizmów z użytkownikiem, bez obowiązkowego egzaminu.
- Szczegółowe wymagania funkcji ustalamy przed ich wdrażaniem. Ta roadmapa nie rozstrzyga jeszcze macierzy uprawnień, reguł przejść statusów ani polityki załączników.
- Zakres obsługi zgłoszeń: treść wysłanego zgłoszenia, komentarzy i notatek pozostaje bez edycji. Status i priorytet są zmieniane osobnymi listami rozwijanymi w szczegółach, z zapisem po wyborze, bez osobnego przycisku zapisu. Po wdrożeniu kont „Przypisz do mnie” od razu zapisze przypisanie do zalogowanej osoby; przypisywanie jest nadal planowaną funkcją.
- Stan na 22.10.2026. Etap 1 jest ukończony w zakresie istniejącego środowiska. W etapie 2 pozostała próba pustej bazy z siedmiu migracji, a w etapie 3 reguły, sprawdzenia i porządki opisane poniżej. W etapie 4a użytkownik potwierdził tworzenie kont z SQL Server i w przeglądarce. Dodano imię i nazwisko oraz listę kont w panelu administratora; wykonano migrację nazw, kompilację, izolowane próby HTTP i odczyt listy z SQL. Pozostaje własny ekran odmowy dostępu. Odtworzenie całego środowiska na czystym komputerze pozostaje do weryfikacji.

## 1. Poznanie i uruchomienie istniejącej aplikacji

**Status:** ukończony — 20.10.2026; kompilacja potwierdzona przez agenta, lokalne próby funkcjonalne przez użytkownika.

**Efekt:** lokalnie uruchomiony prototyp i wspólnie prześledzona droga tworzenia, odczytu oraz komentowania zgłoszenia.

**Nauka:** komponent Razor i routing, binding, zdarzenia formularza, wykonanie kodu na serwerze, cykl życia komponentów, DI i fabryka kontekstu, async/await.

**Praca:** zacząć od `CreateTicketAsync`, połączyć formularz z zapisem EF, następnie przeanalizować listę i szczegóły. Skonfigurować osobną bazę deweloperską i sprawdzić istniejące ścieżki przed refaktorem.

**Wynik:** omówiono przepływ od formularza przez komponent serwerowy i EF do SQL Server, fabrykę kontekstów, śledzenie zmian oraz zapis i odczyt. Użytkownik potwierdził tworzenie zgłoszenia, listę, szczegóły, dodawanie odpowiedzi i notatek oraz trwałość danych po odświeżeniu. Sprawdził wymagane pola i ekran braku zgłoszenia dla `/DisplayTicket/777`. Szczegółowy zakres i ograniczenia weryfikacji znajdują się w README. Późniejsze przygotowanie kopii istniejącej bazy i nowej bazy migracjami opisano w etapie 2; odtworzenie całego środowiska od zera pozostaje do sprawdzenia.

**Kryteria ukończenia:**

- Aplikacja buduje się i uruchamia według README; działające środowisko i wynik prób są opisane.
- Utworzenie zgłoszenia, odczyt szczegółów i dodanie komentarza zachowują dane po odświeżeniu.
- Sprawdzone są wymagane pola i nieistniejący identyfikator; napotkane błędy są zapisane jako konkretne zadania.
- Omówiono, co wykonuje przeglądarka, komponent na serwerze, EF i SQL Server.

## 2. Uporządkowanie danych i zapisu

**Status:** w toku — migracje, usunięcie ręcznego SQL i uproszczenie numeracji wykonane; końcowe próby aktualnej wersji wymagają weryfikacji.

**Efekt:** kontrolowana ewolucja schematu i czytelna obsługa danych bez powielanego SQL w stronach.

**Nauka:** model i relacje EF, LINQ, śledzenie encji, czas życia i zwalnianie kontekstu, migracje, indeksy oraz transakcje.

**Praca:** ustalić stan istniejącej bazy i przygotować kopię danych; przejść z `EnsureCreatedAsync` i ręcznego SQL do migracji. Wydzielić powieloną logikę w zakresie uzasadnionym projektem. Zgodnie z ustaloną decyzją numer jest wyliczany jako `IT-{Id}` bez zer wiodących i nie jest przechowywany w bazie; tworzenie zgłoszenia wymaga jednego zapisu, więc odpada potrzeba jawnej transakcji łączącej zapis rekordu z nadaniem numeru.

**Wykonano:** porównano udostępniony przez użytkownika schemat z modelem, uwzględniono domyślny pusty numer i przygotowano `InitialSchema` oraz `AddUniqueTicketNumberIndex`. Użytkownik wykonał backup i odtworzenie, zarejestrował punkt startowy na kopii, zastosował migrację indeksu i potwierdził zachowanie liczby rekordów (6 zgłoszeń, 8 komentarzy). Usunął `EnsureCreatedAsync` i powielone metody z SQL z trzech komponentów; agent sprawdził kod i kompilację. Użytkownik potwierdził działanie odczytu, tworzenia i komentowania oraz daty aktualizacji po zmianach. Potwierdził też utworzenie nowej bazy z obu migracji: 2 wpisy historii, 0 zgłoszeń i 0 komentarzy. Procedura i zakres weryfikacji są opisane w README.

**Uproszczenie numeracji:** użytkownik zmienił `TicketNumber` na właściwość `[NotMapped]` zwracającą `IT-{Id}`, usunął drugi zapis i powielone formatowanie. Agent zweryfikował zmiany, wygenerował `RemoveStoredTicketNumber` i zastosował ją do odtworzonej kopii po świeżym backupie i `RESTORE VERIFYONLY`. Potwierdził usunięcie kolumny oraz indeksu, 3 wpisy historii i zachowanie 7 zgłoszeń oraz 10 komentarzy. Porównanie SHA-256 objęło wszystkie pozostałe pola zgłoszeń i wszystkie pola komentarzy. Sprawdzono kompilację, brak zmian modelu poza migracją, właściwość dla `Id` 1, 9999, 10000 i `Int32.MaxValue` oraz odczyt listy i szczegółów przez HTTP z poprawnym formatem numerów. Historyczne zapisane numery zostały celowo usunięte; ich zawartość pozostaje w backupie.

**Bazy robocza i zapasowa:** po zweryfikowanych backupach obu wersji i porównaniu danych aktualna baza otrzymała nazwę `layZTicketsDb`. Odtworzono `layZTicketsDb_backup` jako kopię tylko do odczytu. Obie mają 7 zgłoszeń, 10 komentarzy i trzy migracje w chwili porządkowania nazw; kopia nie odświeża się automatycznie. Starą bazę usunięto po potwierdzeniu zabezpieczenia i zachowania jej danych. Agent potwierdził odczyt aplikacji z domyślnej konfiguracji. Nazwy `_MigrationTest` i `_FreshTest` w poprzednich wynikach są historycznymi nazwami testów.

**Dalsza weryfikacja:** po trzeciej migracji użytkownik potwierdził tworzenie i komentowanie wynikiem 8 zgłoszeń i 12 komentarzy. 21.10.2026 wygenerował `AddTicketCategory`; agent sprawdził, że dodaje wyłącznie kolumnę `Category` typu `int` z wartością domyślną `0`, bez odtwarzania tabel po zmianie klasy `Ticket` na `TicketData`. Kompilacja przeszła bez błędów i ostrzeżeń, a użytkownik potwierdził poprawne działanie kategorii po migracji.

**Pozostało:** utworzenie pustej bazy z aktualnymi siedmioma migracjami. Logika operacji nadal znajduje się w komponentach — zakres ewentualnego wydzielenia wymaga osobnego małego zadania. Próby dotyczą lokalnego środowiska.

**Kryteria ukończenia:**

- Udokumentowano i sprawdzono utworzenie pustej bazy oraz przejście na migracje na kopii istniejących danych, bez ich utraty.
- Wejście na listę lub szczegóły nie tworzy ani nie zmienia schematu i nie uzupełnia danych.
- Tworzenie, odczyt i komentowanie nadal działają po zmianach.
- Numer ma jeden format `IT-{Id}` dla nowych i dotychczasowych rekordów, również powyżej 9999; nie ma osobnej kolumny ani drugiego zapisu numeru. Sprawdzono utworzenie zgłoszenia jednym zapisem i wyliczenie numeru po otrzymaniu `Id`.
- Omówiono granice odpowiedzialności komponentu, logiki operacji i kontekstu EF.

## 3. Pełniejsza obsługa zgłoszeń

**Status:** w toku — kategoria oraz zmiana statusu i priorytetu są zaimplementowane. Podstawowe działanie użytkownik sprawdził ręcznie; ostatnią zmianę dat i komunikatów agent potwierdził w kodzie i kompilacją. Pozostały reguły przejść, pełna weryfikacja oraz drobne porządki. Rozpoczęcie etapu 4 nie oznacza ukończenia tego etapu.

**Efekt:** zapisywana kategoria przy tworzeniu oraz niezależna zmiana statusu i priorytetu w szczegółach zgłoszenia, z zapisem do bazy od razu po wyborze z listy rozwijanej.

**Nauka:** binding i zdarzenia zmiany wartości, asynchroniczny zapis pojedynczego pola, walidacja, reguły biznesowe, wartości słownikowe, obsługa błędów zapisu i znaczenie dat aktualizacji/zamknięcia.

**Praca:** ustalić wymagane pola, dozwolone wartości i przejścia statusów; zapisywać kategorię przy tworzeniu. W szczegółach dodać osobne listy rozwijane statusu i priorytetu. Każdy wybór uruchamia operację na serwerze, która sprawdza reguły i zapisuje odpowiednie pole wraz z wymaganymi datami. Nie ma osobnego przycisku „Zapisz” ani formularza edycji treści zgłoszenia, komentarzy lub notatek. Usunąć nieaktywny przycisk „Edytuj”, a przycisk „Zmień status” zastąpić listą. To nadal etap lokalnego prototypu przed dodaniem kontroli dostępu.

**Kategorie — wykonano:** dodano enum `TicketCategory` z jawnymi wartościami `Other = 0`, `Hardware = 1`, `Software = 2`, `Network = 3` i właściwość `TicketData.Category`. `InputRadioGroup` wiąże wybór z modelem, a dotychczasowe `SaveChangesAsync` zapisuje kategorię jako `int`. Migracja `20261021100725_AddTicketCategory` dodaje kolumnę do `Tickets` i nadaje istniejącym rekordom `0` (`Other`). Szczegóły wyświetlają wartość enuma. Użytkownik potwierdził poprawne działanie po migracji; agent zweryfikował kod i kompilację, bez własnej próby interaktywnej ani odczytu bazy po tej migracji. Omówiono binding, mapowanie enuma i różnicę między generowaniem a zastosowaniem migracji.

**Walidacja i nazwy kategorii:** dodano `Enum.IsDefined` z komunikatem i przerwaniem operacji przed dostępem do bazy oraz `GetCategoryName` mapujące cztery wartości na polskie nazwy. Agent potwierdził kompilację bez błędów i ostrzeżeń. Użytkownik wymusił `(TicketCategory)9999`, potwierdził komunikat i brak nowego zgłoszenia. Testowe przypisanie usunięto. Pozostają poprawki „Karegoria” na „Kategoria”, „Incorrect category” na „Nieznana kategoria” i sprawdzenie wyświetlania polskich nazw.

**Enumy statusu i priorytetu — wykonano:** użytkownik dodał `TicketStatus` (`New = 0`, `InProgress = 1`, `Pending = 2`, `Closed = 3`) i `TicketPriority` (`Low = 0`, `Normal = 1`, `High = 2`, `Highest = 3`) oraz dostosował model i referencje. Agent wygenerował `20261021122842_ConvertTicketStatusAndPriorityToEnums`, dodał jawne mapowanie istniejących nazw, w tym `Critical` na `Highest = 3`, i zastosował migrację do `layZTicketsDb`. Poprawił też opcje priorytetu w formularzu tworzenia, aby korzystały z obecnego enuma. Potwierdzono typ `int` obu kolumn, 5 migracji, zachowanie 9 zgłoszeń i 12 komentarzy oraz zgodność skrótów SHA-256 wszystkich pól po uwzględnieniu konwersji. Kompilacja przeszła bez błędów i ostrzeżeń; model jest zgodny ze snapshotem. Przy tej migracji agent nie wykonywał interaktywnego wysłania formularza ani cofania migracji. Późniejszą implementację zmiany pól opisano poniżej.

**Zmiana statusu i priorytetu — wykonano:** `InputSelect` korzysta z `@bind-Value:get` i `@bind-Value:set`, wywołując `ChangeStatus` albo `ChangePriority`. Każda metoda sprawdza enum, tworzy kontekst, pobiera śledzony rekord i zapisuje odpowiednie pole wraz z `UpdatedAt` jednym `SaveChangesAsync`. Obiekt widoku aktualizowany jest po udanym zapisie. Wspólne `isUpdating` blokuje listy i kolejne wywołania obu metod. Błąd aktualizacji pojawia się obok list przez `updateErrorMessage`, czyszczone przy kolejnej próbie; brak rekordu i błąd początkowego odczytu mają osobny ekran. Commit `da2245f` zawiera również ustawienie `ClosedAt` przy `Closed` i wyczyszczenie przy innych statusach, z aktualizacją widoku. Użytkownik potwierdził podstawowe działanie zmian; agent potwierdził aktualny kod i kompilację bez błędów i ostrzeżeń. Nie wykonywał własnego zapisu w bazie ani prób interaktywnych tej wersji.

**Pozostało do domknięcia:** uzgodnić ewentualne ograniczenia przejść statusów; obecny kod dopuszcza każdą wartość enuma niezależnie od poprzedniej. Ustalić i obsłużyć wybór tej samej wartości, który obecnie również zmienia datę. Sprawdzić po odświeżeniu zamknięcie, ponowne otwarcie i daty, odrzucenie niepoprawnych enumów, błąd zapisu oraz udane ponowienie. Dodać odpowiednie testy uzgodnionych reguł. Poprawić powtórzone `id="ticket-priority"` dla statusu, teksty komunikatów aktualizacji i kategorii; usunąć nieaktywne przyciski „Edytuj” oraz „Zmień status”. Szczegóły wyjątków pozostają do przeniesienia do logów zgodnie z etapem 6. Te punkty pozostają w TODO; następny temat pracy to konta i logowanie.

**Kryteria ukończenia:**

- Kategoria wybrana przy tworzeniu pozostaje po ponownym odczycie. Status i priorytet zapisują się niezależnie, od razu po wyborze z listy, bez osobnego zatwierdzania; zmiana jednego pola nie nadpisuje pozostałych pól zgłoszenia.
- Nieprawidłowe dane i niedozwolone przejścia są odrzucane przez logikę po stronie serwera.
- Daty odpowiadają uzgodnionym regułom. Interfejs sygnalizuje trwający zapis i błąd; po nieudanej operacji nie przedstawia niezapisanej wartości jako zapisanej. Kolejne wybory podczas zapisu nie prowadzą do rozbieżności z bazą.
- Treść wysłanego zgłoszenia, komentarzy i notatek pozostaje bez edycji; nie ma nieaktywnego przycisku „Edytuj”.
- Najważniejsze reguły mają odpowiednie testy; użytkownik rozumie różnicę między walidacją formularza a regułą biznesową.

## 4. Konta, role i granice dostępu

**Status:** w toku — model i tabele Identity, administrator, logowanie, wylogowanie, tożsamość w menu, ochrona stron, tworzenie kont i ich lista są zaimplementowane. Użytkownik potwierdził próby formularza z SQL Server. Rozszerzenie nazw i lista kont zostały sprawdzone kompilacją, odczytem SQL oraz izolowanymi próbami HTTP. Pozostają własny ekran odmowy dostępu i dalsze części etapu z uprawnieniami do zgłoszeń oraz operacji.

**Efekt:** logowanie i wylogowanie oraz egzekwowane na serwerze uprawnienia ról użytkownika, agenta SD i administratora.

**Nauka:** uwierzytelnianie a autoryzacja, tożsamość użytkownika, role/polityki, dostęp do konkretnego rekordu i obsługa kont.

**Przygotowanie modelu — wykonano:** użytkownik dodał pakiet `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.27 i klasę `ApplicationUser : IdentityUser`. Dostosowano `AppDbContext` do `IdentityDbContext<ApplicationUser>` i dodano `base.OnModelCreating(modelBuilder)` przed własną konfiguracją komentarzy. Kontekst obejmuje teraz model użytkowników i ról Identity obok zgłoszeń. Kompilacja: 0 błędów i ostrzeżeń. Polecenie `has-pending-model-changes` poprawnie wskazuje zmiany wymagające nowej migracji; nie generowano jej i nie zmieniano bazy. Nie sprawdzano jeszcze kont ani logowania, ponieważ nie są zaimplementowane.

**Przygotowanie migracji — wykonano:** wygenerowano `20261021150554_AddIdentitySchema` oraz metadane i snapshot. `Up()` tworzy siedem tabel Identity z kluczami, relacjami i indeksami; nie zmienia `Tickets` ani `TicketComments`. `Down()` usuwa wyłącznie tabele Identity wraz z ich ewentualną zawartością. Sprawdzono pliki, wygenerowany SQL, kompilację (0 błędów i ostrzeżeń) oraz zgodność modelu ze snapshotem. Na etapie generowania nie łączono się z bazą.

**Zastosowanie migracji — potwierdzone przez użytkownika:** wykonano `database update AddIdentitySchema` i sprawdzono listę migracji przed i po. Użytkownik potwierdził sześć migracji bez `Pending`, siedem tabel `AspNet...` oraz `UserCount = 0`. Agent nie wykonywał niezależnej próby z bazą. Odczyt zgłoszeń i porównanie ich liczby oraz liczby komentarzy po tej migracji pozostają do potwierdzenia.

**Konfiguracja usług — wykonano:** użytkownik dodał do `Program.cs` `AddIdentity<ApplicationUser, IdentityRole>()` z magazynami EF dla `AppDbContext`, `AddAuthorization()` i `AddCascadingAuthenticationState()`. Wywołania `UseAuthentication()` oraz `UseAuthorization()` poprzedzają `UseAntiforgery()`. Agent potwierdził te zmiany w kodzie; użytkownik potwierdził poprawne uruchomienie aplikacji. Omówiono usługi kont i ról, cookie oraz różnicę między uwierzytelnianiem i autoryzacją. Nie zmieniano w tym kroku schematu bazy. Nie sprawdzano rzeczywistego logowania ani dostępu do zgłoszeń, ponieważ odpowiednie funkcje nie są jeszcze zaimplementowane.

**Konta i sesja — postęp 22.10.2026:** użytkownik przygotował `AdminAccountSetup` i jednorazowe polecenie `--bootstrap-admin=true`, z konfiguracją przez User Secrets. Potwierdził utworzenie konta i `UserCount = 1`. Zaimplementował statyczny formularz POST `/login`, POST `/logout` z tokenem, ochronę stron folderu `Main` oraz rzeczywistą nazwę i opis roli w menu. Potwierdził działanie logowania, wylogowania i odmowę wejścia po wylogowaniu także przez bezpośredni adres. `/profile` jest chronionym szkieletem przyszłych ustawień. Strony Counter i Weather zostały usunięte.

**Tworzenie kont — wykonano:** użytkownik potwierdził poprawne działanie scenariuszy formularza w lokalnej aplikacji z SQL Server; odpowiedni punkt TODO oznaczono jako wykonany. Panel `/admin` i operacja na serwerze wymagają roli `Admin`. Formularz działa w static SSR i zapisuje zwykłe konto przez `UserManager.CreateAsync`, zachowując sesję administratora. Identity odpowiada za reguły hasła, duplikaty, hash i zapis przez magazyn EF. Po błędzie panel otwiera się ponownie, a pola haseł są puste; po sukcesie następuje przekierowanie na GET. Administrator ustawia hasło ręcznie, bez wysyłania zaproszeń i potwierdzania e-maila.

**Imię i nazwisko — wdrożono po powyższych próbach użytkownika:** `ApplicationUser` zawiera `FirstName` i `LastName` z limitem po 100 znaków, a model formularza `UserData` dodatkowo wymaga obu wartości. Metoda tworzenia przycina spacje, waliduje dane i przekazuje je do encji konta. Migracja `20261022173719_AddUserNames` dodała obie kolumny `nvarchar(100)` i zgodnie z ustaleniem uzupełniła istniejące konta jako Admin Admin oraz Jan Kowalski. W `layZTicketsDb` są 7 migracji, 2 konta, 10 zgłoszeń i 13 komentarzy. Skrót wszystkich wcześniejszych pól użytkowników jest zgodny ze stanem sprzed migracji. Fabryka tożsamości dodaje imię i nazwisko do cookie przy logowaniu; menu pokazuje pełną nazwę i inicjały. Starsze cookie wymaga ponownego zalogowania. Login i `Id` pozostają bez zmian; przyszłe przypisania będą korzystały z `Id`, ponieważ nazwy osób mogą się powtarzać. Bootstrap nowego administratora domyślnie używa Admin Admin, z opcjonalną konfiguracją nazw.

**Weryfikacja nowego formularza:** kompilacja bez błędów i ostrzeżeń, 40 sprawdzeń HTTP skompilowanych stron z rzeczywistym Identity i testowym magazynem w pamięci. Sprawdzono poprawne utworzenie, hash, logowanie nowego konta, zachowanie sesji administratora, duplikat bez rozróżniania wielkości liter, wymagane pola i e-mail, różne i słabe hasła, błąd magazynu, brak hasła w HTML odpowiedzi, przekierowanie bez ponownego zapisu po odświeżeniu, token oraz odmowę GET i POST dla osoby niezalogowanej i zwykłego użytkownika. Nie korzystano z lokalnej bazy ani sekretów i nie wykonano interaktywnej próby przeglądarkowej nowego formularza.

**Weryfikacja rozszerzenia o nazwy:** kompilacja bez błędów i ostrzeżeń, model zgodny ze snapshotem, 51 izolowanych sprawdzeń HTTP skompilowanych stron z Identity i magazynem w pamięci. Oprócz wcześniejszych reguł potwierdzono wymagane nazwy, limit długości, przycinanie spacji, dwa imiona, polskie znaki i nazwiska z łącznikiem, wyświetlenie pełnej nazwy i inicjałów po logowaniu oraz różne identyfikatory kont o takiej samej nazwie. Agent potwierdził migrację i nowe dane w SQL, ale nie wykonywał interaktywnego logowania do lokalnej aplikacji po tej zmianie.

**Lista kont — wykonano:** panel `/admin` pokazuje wszystkie konta: imię, nazwisko i login, sortowane po nazwisku, imieniu i loginie, oraz liczbę rekordów. Krótko żyjący kontekst z fabryki EF pobiera tylko pola potrzebne do tabeli. Po utworzeniu konta istniejące przekierowanie na GET odświeża listę. Osobna obsługa błędu odczytu nie ukrywa formularza. Nie potrzeba migracji. Build bez błędów i ostrzeżeń, 60 sprawdzeń HTTP z danymi w pamięci (w tym stan pusty, błąd listy, nowy rekord po utworzeniu i odmowa dostępu) oraz 12 sprawdzeń wariantu z odczytem istniejących kont SQL. W wariancie SQL uwierzytelnianie korzystało z danych testowych w pamięci, a baza wyłącznie z odczytu listy. Wyniki dotyczą HTML serwera, bez interaktywnej próby nowej tabeli w przeglądarce.

**Najbliższy krok:** przygotować własny ekran odmowy dostępu zamiast domyślnego adresu Identity. Pełna macierz ról i dostęp do rekordów nadal należą do dalszych części etapu 4.

**Praca:** konta tworzy wyłącznie administrator; nie udostępniamy publicznej rejestracji ani publicznej operacji tworzenia kont. Przed implementacją ustalić szczegóły zakładania kont, a następnie macierz uprawnień do zgłoszeń. Powiązać autorów i przypisania z kontami, ustalając los dotychczasowych tekstowych danych. Zaimplementować przydzielanie zgłoszeń zgodnie z macierzą. Kliknięcie „Przypisz do mnie” od razu uruchamia zapis przypisania w bazie; serwer ustala identyfikator zalogowanej osoby i sprawdza jej uprawnienia. Ustalić zasady przejęcia już przypisanego zgłoszenia i aktualizacji daty. Chronić notatki wewnętrzne także przy pobieraniu danych. Objąć uprawnieniami również zmianę statusu i priorytetu.

**4a. Konta i sesja logowania — najbliższy mały etap:**

1. Ustalono: wszystkie konta tworzy administrator, bez publicznej rejestracji. Wymagamy imienia i nazwiska, e-mail jest loginem, administrator ręcznie ustawia hasło. Wysyłanie zaproszeń, potwierdzanie e-maila oraz rozbudowa profilu o zmianę loginu i hasła pozostają poza bieżącym zakresem.
2. Dodać ASP.NET Core Identity do istniejącego projektu .NET 8 i SQL Server. Identity zapewnia obsługę użytkowników i haseł; zaplanować integrację z kontekstem EF oraz migrację tabel kont. Przed zastosowaniem sprawdzić bazę docelową i zachowanie danych zgłoszeń. Nie tworzyć nowej aplikacji zamiast obecnej.
3. Zaplanować jednorazowe, jawnie uruchamiane utworzenie pierwszego administratora poza publicznym interfejsem, np. lokalnym poleceniem konfiguracji. Dane dostępowe pochodzą spoza repozytorium; nie dodawać stałego hasła do kodu ani automatycznego odtwarzania administratora przy każdym starcie. Przygotować logowanie i wylogowanie zgodne z Blazor Interactive Server. Omówić cookie uwierzytelniania, czyli zapis w przeglądarce pozwalający serwerowi rozpoznać zalogowaną osobę.
4. Zastąpić przykładową tożsamość w układzie danymi zalogowanej osoby. Sprawdzić logowanie administratora poprawnym i błędnym hasłem, odświeżenie strony oraz wylogowanie. Pierwszy efekt: administrator może wejść na własne konto i zobaczyć, kto jest zalogowany.
5. Tworzenie kont jest wykonane i potwierdzone przez użytkownika z SQL Server. Rozszerzenie o imię i nazwisko oraz lista kont są zaimplementowane i sprawdzone w zakresie opisanym powyżej. Pozostaje własny ekran odmowy dostępu. Ten punkt nie oznacza jeszcze wdrożenia uprawnień do zgłoszeń.

**4b. Autorzy powiązani z kontami:** ustalić sposób potraktowania dotychczasowych tekstowych autorów i przypisań, następnie zapisywać identyfikator zalogowanej osoby jako autora nowego zgłoszenia i komentarza. Użytkownik nie podaje autora dowolnym tekstem. Zasady dostępu dla niezalogowanej osoby sprawdzać na serwerze.

**4c. Role, dostęp i przypisywanie:** uzgodnić macierz ról użytkownika, agenta SD i administratora oraz sposób nadawania ról. Nowe konto nie może samodzielnie nadać sobie roli uprzywilejowanej. Egzekwować dostęp do rekordów i notatek wewnętrznych podczas odczytu oraz uprawnienia podczas zapisu. Dodać „Przypisz do mnie” i próby dozwolonych oraz zabronionych operacji, w tym wejścia bezpośrednio na adres zgłoszenia.

**Kryteria ukończenia:**

- Logowanie i wylogowanie działają, a przykładowe dane profilu zastąpiono rzeczywistą tożsamością.
- Pierwszy administrator powstaje w kontrolowanym procesie konfiguracji. Kolejne konta tworzy wyłącznie administrator; publiczna rejestracja nie jest dostępna, a zakaz tworzenia kont przez pozostałe osoby jest egzekwowany na serwerze.
- Dla każdej roli sprawdzono dozwolone i zabronione odczyty oraz zapisy, w tym wejście bezpośrednio na adres obcego zgłoszenia.
- Nieuprawniona osoba nie otrzymuje treści notatki wewnętrznej; samo ukrycie elementu UI nie wystarcza.
- „Przypisz do mnie” zapisuje przypisanie do zalogowanej osoby bez dodatkowego zatwierdzenia; wynik pozostaje po odświeżeniu. Identyfikator pochodzi z tożsamości na serwerze, a nie z dowolnego tekstu z formularza.
- Serwer egzekwuje uzgodnione uprawnienia do przypisywania oraz zmian statusu i priorytetu. Błąd zapisu przypisania jest widoczny w interfejsie.
- Najważniejsze granice dostępu mają testy, a sposób konfiguracji kont jest udokumentowany.

## 5. Wyszukiwanie, załączniki i dashboard

**Status:** do rozpoczęcia.

**Efekt:** wygodna praca na większej liczbie zgłoszeń i zastąpienie pozostałych makiet działającymi funkcjami.

**Nauka:** kompozycja zapytań EF, filtrowanie i paginacja, stan interfejsu, obsługa plików, dostępność i responsywność.

**Praca:** kolejno wdrożyć wyszukiwanie i filtry z paginacją, załączniki według uzgodnionych typów/limitów i zasad dostępu oraz dashboard z rzeczywistymi danymi. Uporządkować pozostałości szablonu i serwis JSON zgodnie z ich przydatnością. Zdecydować o usunięciu, odłożeniu lub implementacji pozostałych przycisków, np. ustawień i powiadomień, przed uznaniem interfejsu za gotowy.

**Kryteria ukończenia:**

- Filtry, wyszukiwanie i strony wyników współpracują; można dotrzeć do zgłoszeń poza pierwszą dziesiątką, z zachowaniem uprawnień.
- Załączniki można dodać i pobrać; sprawdzono limity, niedozwolony plik i próbę dostępu bez uprawnień.
- Dashboard pokazuje dane zgodne z dostępnymi zgłoszeniami; stany pusty, ładowania i błędu są czytelne.
- Główne ścieżki są używalne na wąskim ekranie i z klawiatury; widoczne kontrolki mają działanie lub jednoznacznie wskazany stan niedostępności.

## 6. Przygotowanie portfolio i demonstracji

**Status:** do rozpoczęcia.

**Efekt:** wersja możliwa do odtworzenia i pokazania wraz z objaśnieniem architektury, decyzji i ograniczeń.

**Nauka:** testy jednostkowe i integracyjne, diagnostyka i logowanie, konfiguracja środowisk, budowanie i wdrożenie.

**Praca:** uzupełnić testy najważniejszych reguł i scenariuszy, oddzielić przyjazne komunikaty od szczegółów diagnostycznych, przygotować przykładowe dane bez danych osobowych i sekretów. Uzgodnić docelowe środowisko demonstracyjne, zweryfikować wersję platformy przed wdrożeniem i opisać odtworzenie aplikacji oraz prezentację jej możliwości.

**Kryteria ukończenia:**

- Z czystego checkoutu można według instrukcji przygotować środowisko, bazę i konta demonstracyjne.
- Kompilacja oraz właściwe testy przechodzą; sprawdzono pełny scenariusz zgłoszenia od utworzenia po obsługę i zamknięcie w odpowiednich rolach.
- Szczegóły wyjątków nie są prezentowane użytkownikowi, a awarie można diagnozować w logach bez ujawniania sekretów.
- README zawiera aktualne funkcje, ograniczenia, sposób uruchomienia i demonstracji oraz potwierdzony sposób wdrożenia, jeśli jest częścią uzgodnionej prezentacji.
- Użytkownik ma omówione materiały pozwalające wyjaśnić przepływ danych, rolę frameworków i główne decyzje na rozmowie rekrutacyjnej.
