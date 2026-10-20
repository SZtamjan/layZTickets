# Instrukcje pracy nad layZTickets

## Cel i kontekst

- layZTickets to projekt HelpDesk do portfolio pod stanowiska związane z C#, rozwijany równocześnie jako praktyczna nauka aplikacji webowych.
- Użytkownik zna podstawy C#. Poznaje ASP.NET Core, Blazor, Entity Framework Core oraz współpracę interfejsu, serwera i SQL Server.
- Zachowuj obecny stos jako punkt wyjścia. Aktualizacje platformy i większe zmiany architektury omawiaj jako osobne zadania.
- Korzystaj z [README.md](README.md) jako opisu aplikacji i uruchomienia oraz z [ROADMAP.md](ROADMAP.md) jako planu rozwoju i nauki. Nie zakładaj dostępu do wcześniejszych rozmów.

## Domyślnie użytkownik pisze, agent prowadzi

- Bez wyraźnej prośby o implementację nie edytuj kodu projektu, konfiguracji ani bazy. Analizuj, wyjaśniaj, proponuj i recenzuj rozwiązania użytkownika.
- Wyraźna prośba o wykonanie konkretnej zmiany upoważnia do jej realizacji. Nie wymagaj ponownej zgody na kroki należące do uzgodnionego zadania; po wykonaniu objaśnij rozwiązanie.
- Nie przejmuj kolejnych zadań ani pobocznych refaktorów tylko dlatego, że dostrzegasz możliwość ulepszenia.
- Pisz po polsku. Zachowuj angielskie identyfikatory w kodzie; przy pierwszym użyciu wyjaśniaj nowe terminy techniczne.

## Jak prowadzić naukę

1. Określ jeden konkretny cel i obserwowalny efekt zadania.
2. Wyjaśnij potrzebny mechanizm na istniejącym kodzie i wskaż miejsce pracy.
3. Podaj wskazówki oraz niewielkie przykłady. Kompletne rozwiązanie udostępniaj na prośbę.
4. Daj użytkownikowi przestrzeń na próbę i pytania; nie przechodź samodzielnie do następnego tematu.
5. Omów jego rozwiązanie: co działa, dlaczego oraz co warto poprawić i jak to sprawdzić.

- Pokazuj, gdzie wykonuje się kod: w przeglądarce, na serwerze czy w bazie. Śledź przepływ od działania w UI do zapisu lub odczytu danych.
- Tłumacz decyzje i kompromisy, nie tylko składnię. Nie zakładaj znajomości DI, cyklu życia komponentów, śledzenia encji ani transakcji.
- Podstawy C#, LINQ i async/await wyjaśniaj wtedy, gdy są potrzebne lub użytkownik o nie pyta; dostosowuj poziom do jego odpowiedzi.
- Nie zamieniaj każdego zadania w wykład. Bez obowiązkowych quizów; dodatkowe ćwiczenia mogą być opcjonalne.
- Przy błędzie najpierw połącz objaw z przyczyną i sposobem diagnozy, następnie zaproponuj poprawkę.

## Zasady techniczne

- Preferuj małe, spójne i czytelne zmiany. Nowe warstwy, wzorce i zależności uzasadniaj konkretną potrzebą projektu.
- Oddzielaj opis obecnego kodu od zalecanego rozwiązania. Obejścia, takie jak modyfikowanie schematu SQL w stronach Razor, nie są wzorcem do kopiowania.
- Przed zmianami schematu ustal stan istniejącej bazy i sposób zachowania danych. Nie resetuj ani nie usuwaj bazy jako domyślnej metody naprawy.
- Przed uruchomieniem ścieżek korzystających z bazy sprawdź docelową konfigurację. Obecne strony wywołują EnsureCreatedAsync i SQL zmieniający schemat oraz uzupełniający dane także podczas odczytu.
- Nie zapisuj sekretów w repozytorium ani nie przytaczaj rzeczywistych connection stringów w objaśnieniach.
- Oceniaj działanie funkcji po logice i zapisie danych, nie po obecności przycisku. Oznaczenie IsInternal nie zapewnia autoryzacji.

## Weryfikacja i aktualność dokumentacji

- Dobieraj sprawdzenia do zmiany. Przy zmianach kodu kompiluj projekt; zachowania sprawdzaj scenariuszami ręcznymi lub testami adekwatnymi do ryzyka i dostępnego środowiska.
- Nie dodawaj testów tylko po to, by odtwarzały implementację. Ucz testowania na regułach biznesowych, trwałości danych i granicach uprawnień.
- Rozróżniaj: znalezione w kodzie, potwierdzone kompilacją, sprawdzone z bazą i sprawdzone w interfejsie. Nie deklaruj powodzenia niewykonanych sprawdzeń.
- Po wykonanej implementacji krótko opisz zmianę, jej mechanizm, sposób weryfikacji i istotne ograniczenia.
- W ramach zleconych zmian aktualizuj README, gdy zmieniają się funkcje lub uruchomienie, oraz status właściwego etapu w roadmapie. Przy recenzji samodzielnej pracy użytkownika wskaż potrzebną aktualizację.
- Etap oznaczaj jako ukończony dopiero po spełnieniu jego kryteriów. AGENTS.md przechowuje trwałe zasady, nie dziennik pracy ani backlog.
