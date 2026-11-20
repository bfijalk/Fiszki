# Plan Testów - Aplikacja Fiszki

## 1. Wprowadzenie i Cele Testowania

### 1.1 Cel dokumentu
Niniejszy dokument określa strategię testowania dla aplikacji Fiszki - systemu do nauki z wykorzystaniem fiszek cyfrowych z generowaniem treści przez AI.

### 1.2 Cele testowania
- Zapewnienie funkcjonalności systemu generowania fiszek z wykorzystaniem AI
- Weryfikacja poprawności algorytmu spaced repetition (SM-2)
- Kontrola bezpieczeństwa uwierzytelniania i autoryzacji użytkowników
- Walidacja integralności danych w bazie PostgreSQL
- Sprawdzenie wydajności przy dużej liczbie użytkowników i fiszek
- Testowanie interfejsu użytkownika Blazor Server

### 1.3 Zakres projektu
Aplikacja Fiszki to system e-learningowy oparty na .NET 10 z Blazor Server, wykorzystujący:
- Generowanie fiszek przez AI (OpenRouter)
- Algorytm SM-2 dla spaced repetition
- Własną implementację uwierzytelniania
- PostgreSQL z Entity Framework Core
- MudBlazor dla komponentów UI

## 2. Zakres Testów

### 2.1 Funkcjonalności w zakresie testów
- **Moduł użytkowników**: rejestracja, logowanie, autoryzacja
- **Moduł fiszek**: tworzenie, edycja, wyświetlanie, usuwanie
- **Moduł generowania AI**: integracja z OpenRouter, przetwarzanie odpowiedzi
- **Moduł nauki**: algorytm SM-2, śledzenie postępów
- **Moduł bazy danych**: migracje, integralność danych, wydajność
- **Interfejs użytkownika**: komponenty Blazor, responsywność

### 2.2 Funkcjonalności poza zakresem testów
- Zewnętrzne API OpenRouter (mockowane w testach)
- Infrastruktura DigitalOcean (planowana)
- Komponenty shadcn/ui (planowane)

## 3. Typy Testów do Przeprowadzenia

### 3.1 Testy Jednostkowe (xUnit)
**Priorytet: Wysoki**
- Serwisy domenowe (UserService, FlashcardService, GenerationService)
- Walidatory FluentValidation
- Mapowanie między DTO a encjami
- Algorytm SM-2
- Hashowanie haseł (BCrypt)

### 3.2 Testy Integracyjne
**Priorytet: Wysoki**
- Integracja z bazą danych PostgreSQL
- Kompleksowe scenariusze użytkownika
- Testy kontrolerów/komponentów z rzeczywistą bazą
- Integracja z zewnętrznym API (z mockami)

### 3.3 Testy UI (bUnit/Playwright)
**Priorytet: Średni**
- Komponenty Blazor Server
- Nawigacja między stronami
- Formularze (logowanie, rejestracja, tworzenie fiszek)
- Responsywność interfejsu

### 3.4 Testy Wydajnościowe
**Priorytet: Średni**
- Czas odpowiedzi generowania fiszek
- Wydajność zapytań do bazy danych
- Symulacja równoczesnych użytkowników
- Testy obciążeniowe dla SignalR (Blazor Server)

### 3.5 Testy Bezpieczeństwa
**Priorytet: Wysoki**
- Uwierzytelnianie i autoryzacja
- Ochrona przed SQL Injection
- Walidacja danych wejściowych
- Bezpieczne przechowywanie haseł
- Izolacja danych między użytkownikami

## 4. Scenariusze Testowe dla Kluczowych Funkcjonalności

### 4.1 Rejestracja i Logowanie Użytkownika

#### TC001: Pomyślna rejestracja nowego użytkownika
- **Warunki wstępne**: Brak użytkownika w systemie
- **Kroki**: 
  1. Wprowadź unikalny email i hasło
  2. Kliknij "Zarejestruj"
- **Oczekiwany rezultat**: Utworzenie konta, przekierowanie do strony głównej

#### TC002: Logowanie z poprawnymi danymi
- **Warunki wstępne**: Istniejący użytkownik w systemie
- **Kroki**:
  1. Wprowadź email i hasło
  2. Kliknij "Zaloguj"
- **Oczekiwany rezultat**: Pomyślne zalogowanie, dostęp do aplikacji

#### TC003: Próba rejestracji z istniejącym emailem
- **Warunki wstępne**: Użytkownik już istnieje
- **Kroki**: Próba rejestracji z tym samym emailem
- **Oczekiwany rezultat**: Błąd walidacji, brak utworzenia duplikatu

### 4.2 Generowanie Fiszek przez AI

#### TC004: Pomyślne generowanie fiszek z tekstu
- **Warunki wstępne**: Zalogowany użytkownik
- **Kroki**:
  1. Wprowadź tekst źródłowy
  2. Wybierz parametry generowania
  3. Kliknij "Generuj fiszki"
- **Oczekiwany rezultat**: Lista propozycji fiszek do zaakceptowania

#### TC005: Obsługa błędu API OpenRouter
- **Warunki wstępne**: Symulowany błąd API
- **Kroki**: Próba generowania fiszek
- **Oczekiwany rezultat**: Wyświetlenie komunikatu o błędzie, możliwość ponownej próby

#### TC006: Zapisywanie zaakceptowanych fiszek
- **Warunki wstępne**: Wygenerowane propozycje fiszek
- **Kroki**:
  1. Zaznacz wybrane fiszki
  2. Kliknij "Zapisz wybrane"
- **Oczekiwany rezultat**: Fiszki zapisane w bazie, dostępne w bibliotece

### 4.3 System Nauki (SM-2)

#### TC007: Pierwszy przegląd nowej fiszki
- **Warunki wstępne**: Nowa fiszka w systemie
- **Kroki**: Oznacz trudność pierwszego przeglądu
- **Oczekiwany rezultat**: Ustawienie parametrów SM-2, zaplanowanie następnego przeglądu

#### TC008: Aktualizacja parametrów po przegladzie
- **Warunki wstępne**: Fiszka z historią przeglądów
- **Kroki**: Wykonaj przegląd z oceną trudności
- **Oczekiwany rezultat**: Aktualizacja EaseFactor, Interval, NextReviewDate

### 4.4 Zarządzanie Fiszkami

#### TC009: Przeglądanie biblioteki fiszek
- **Warunki wstępne**: Użytkownik z utworzonymi fiszkami
- **Kroki**: Przejdź do sekcji "Twoje fiszki"
- **Oczekiwany rezultat**: Lista fiszek użytkownika, paginacja

#### TC010: Edycja istniejącej fiszki
- **Warunki wstępne**: Istniejąca fiszka
- **Kroki**: Edytuj treść przedniej/tylnej strony
- **Oczekiwany rezultat**: Zapisanie zmian, aktualizacja UpdatedAt

## 5. Środowisko Testowe

### 5.1 Środowiska
- **Lokalne**: Docker Compose z PostgreSQL dla development
- **CI/CD**: GitHub Actions z testową bazą danych
- **Staging**: Środowisko zbliżone do produkcyjnego (planowane)

### 5.2 Dane testowe
- **Baza testowa**: Oddzielna instancja PostgreSQL
- **Fixtures**: Przygotowane zestawy danych dla różnych scenariuszy
- **Mockowane API**: Symulacja odpowiedzi OpenRouter

### 5.3 Konfiguracja
```json
{
  "ConnectionStrings": {
    "FiszkiDatabase": "Host=localhost;Database=fiszki_test;Username=test;Password=test"
  },
  "OpenRouter": {
    "BaseUrl": "https://api.openrouter.ai/mock",
    "ApiKey": "test_key"
  }
}
```

## 6. Narzędzia do Testowania

### 6.1 Framework testowy
- **xUnit**: Testy jednostkowe i integracyjne
- **FluentAssertions**: Czytelne asercje
- **AutoFixture**: Generowanie danych testowych
- **Moq**: Mockowanie zależności

### 6.2 Testy UI
- **bUnit**: Testowanie komponentów Blazor
- **Playwright**: Testy end-to-end (planowane)
- **AngleSharp**: Parsowanie HTML w testach

### 6.3 Baza danych
- **Microsoft.EntityFrameworkCore.InMemory**: Testy szybkie
- **Testcontainers**: Integracja z rzeczywistą PostgreSQL
- **EF Core Migrations**: Testowanie migracji

### 6.4 Wydajność
- **NBomber**: Testy obciążeniowe
- **MiniProfiler**: Profilowanie zapytań EF
- **BenchmarkDotNet**: Benchmarki algorytmów

## 7. Harmonogram Testów

### 7.1 Faza 1: Fundament (Sprint 1-2)
- ✅ Konfiguracja środowiska testowego
- ✅ Testy jednostkowe serwisów domenowych
- ✅ Testy walidatorów FluentValidation
- ✅ Podstawowe testy bazy danych

### 7.2 Faza 2: Funkcjonalności Core (Sprint 3-4)
- 🔄 Testy integracyjne API
- 🔄 Testy komponentów Blazor (bUnit)
- 🔄 Testy algorytmu SM-2
- 🔄 Mockowanie OpenRouter API

### 7.3 Faza 3: Zaawansowane (Sprint 5-6)
- ⏳ Testy wydajnościowe
- ⏳ Testy bezpieczeństwa
- ⏳ Testy end-to-end (Playwright)
- ⏳ Automatyzacja w CI/CD

### 7.4 Faza 4: Optymalizacja (Sprint 7+)
- ⏳ Testy regresyjne
- ⏳ Monitoring wydajności
- ⏳ Testy dostępności
- ⏳ Testy kompatybilności przeglądarek

## 8. Kryteria Akceptacji Testów

### 8.1 Pokrycie kodu
- **Minimum**: 70% pokrycia dla warstwy serwisów
- **Cel**: 85% pokrycia dla logiki biznesowej
- **Wyjątki**: Klasy konfiguracyjne, DTO, migracje

### 8.2 Wydajność
- **Czas odpowiedzi**: < 2s dla generowania fiszek
- **Zapytania DB**: < 100ms dla standardowych operacji
- **Concurrent users**: System obsługuje min. 50 równoczesnych użytkowników

### 8.3 Bezpieczeństwo
- **Brak podatności**: OWASP Top 10
- **Uwierzytelnienie**: 100% pokrycie testami
- **Autoryzacja**: Weryfikacja izolacji danych między użytkownikami

### 8.4 Funkcjonalność
- **Testy krytyczne**: 100% przechodzących
- **Regresja**: Brak nowych błędów w istniejących funkcjach
- **Cross-browser**: Wsparcie dla Chrome, Firefox, Safari, Edge

## 9. Role i Odpowiedzialności

### 9.1 Developer
- Pisanie testów jednostkowych
- Utrzymywanie testów przy zmianach kodu
- Lokalne uruchamianie testów przed commit
- Code review testów

### 9.2 QA Engineer (w przyszłości)
- Projektowanie scenariuszy testowych
- Testy eksploracyjne
- Testy akceptacyjne
- Raportowanie błędów

### 9.3 DevOps
- Konfiguracja środowisk testowych
- Integracja testów z CI/CD
- Monitoring wydajności testów
- Utrzymanie infrastruktury testowej

## 10. Procedury Raportowania Błędów

### 10.1 Klasyfikacja błędów
- **Critical**: Aplikacja nie uruchamia się, utrata danych
- **High**: Główne funkcje nie działają
- **Medium**: Funkcje działają z ograniczeniami
- **Low**: Problemy kosmetyczne, usprawnienia UX

### 10.2 Format raportu błędu
```markdown
## ID: BUG-YYYY-MM-DD-XXX

### Priorytet: [Critical/High/Medium/Low]

### Środowisko:
- OS: [macOS/Windows/Linux]
- Przeglądarka: [Chrome/Firefox/Safari/Edge] + wersja
- .NET Runtime: 10.0

### Kroki do reprodukcji:
1. [Krok 1]
2. [Krok 2]
3. [Krok 3]

### Oczekiwany rezultat:
[Opis oczekiwanego zachowania]

### Aktualny rezultat:
[Opis rzeczywistego zachowania]

### Logi/Screenshots:
[Załączniki]

### Dodatkowe informacje:
[Kontekst, warunki środowiskowe]
```

### 10.3 Proces workflow
1. **Zgłoszenie**: GitHub Issues z odpowiednimi labelami
2. **Triage**: Klasyfikacja i przypisanie w ciągu 24h
3. **Fixing**: Development i testy w branchu feature
4. **Verification**: QA weryfikuje fix w środowisku testowym
5. **Closure**: Zamknięcie po potwierdzeniu rozwiązania

### 10.4 Metryki jakości
- **Bug escape rate**: < 5% błędów wykrytych w produkcji
- **Time to fix**: < 2 dni dla High/Critical
- **Test automation rate**: > 80% testów zautomatyzowanych
- **Flaky test rate**: < 2% niestabilnych testów

## 11. Monitoring i Utrzymanie Testów

### 11.1 Ciągłe doskonalenie
- Cotygodniowy przegląd metryk testowych
- Miesięczna analiza pokrycia kodu
- Kwartalna ocena strategii testowej
- Roczny audyt narzędzi i procesów

### 11.2 Dokumentacja
- Aktualizacja planu testów przy większych zmianach
- Dokumentowanie nowych wzorców testowych
- Sharing knowledge sessions w zespole
- Onboarding materials dla nowych członków zespołu

---

**Dokument stworzony**: 24 października 2025  
**Wersja**: 1.0  
**Następny przegląd**: 24 stycznia 2026
