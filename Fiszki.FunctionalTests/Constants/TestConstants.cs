namespace Fiszki.FunctionalTests.Constants;

public static class TestConstants
{
    public static class Timeouts
    {
        public const int DefaultWaitMs = 2000; // Increased from 1000
        public const int FormValidationWaitMs = 500; // Increased from 200
        public const int PasswordMismatchWaitMs = 600; // Increased from 300
        public const int NavigationTimeoutMs = 30000; // Increased from 15000 to 30000 for CI environment
        public const int FlashcardGenerationWaitMs = 30000; // Increased from 15000 to 30000 for CI environment
        public const int PlaywrightDefaultTimeoutMs = 30000; // Increased from 15000 to 30000 for CI environment
        public const int LoginTimeoutMs = 30000; // Increased from 10000 to 30000 for CI environment
        
        // Optimized timeouts for improved test performance
        public const int ElementWaitMs = 10000; // Wait for elements to be visible/available
        public const int RedirectWaitMs = 10000; // Wait for page redirects after login
        public const int ContentLoadWaitMs = 5000; // Wait for page content to load
        public const int NetworkIdleWaitMs = 5000; // Wait for network activity to settle
        public const int ButtonStateWaitMs = 5000; // Wait for button state changes
    }

    public static class Messages
    {
        public const string PasswordMismatch = "Passwords don't match";
    }

    public static class Routes
    {
        public const string Home = "/";
        public const string Login = "/login";
        public const string Register = "/register";
        public const string Generate = "/generate";
        public const string Flashcards = "/flashcards";
    }

    public static class Selectors
    {
        public const string Navigation = "nav";
        public const string EmailInput = "#emailInput";
        public const string PasswordInput = "#passwordInput";
        public const string PasswordConfirmInput = "#passwordConfirmInput";
        public const string PasswordMismatchMessage = ".form-text.text-danger";
        public const string Alert = ".mud-alert, .alert-danger";
    }

    public static class Labels
    {
        public const string Email = "Email";
        public const string Password = "Password";
        public const string Login = "Login";
        public const string CreateAccount = "Create Account";
        public const string Register = "Register";
        public const string SourceText = "Source Text";
        public const string MaximumCards = "Maximum Cards";
        public const string GenerateFlashcards = "Generate Flashcards";
        public const string AcceptAll = "Accept All";
        public const string SaveSelected = "Save Selected";
        public const string Flashcards = "Flashcards";
        public const string CreateManually = "Create Manually";
        public const string QuestionFront = "Question (Front)*";
        public const string AnswerBack = "Answer (Back)*";
        public const string TagsOptional = "Tags (optional)";
        public const string CreateCard = "Create Card";
        public const string GetStartedSignIn = "Get Started - Sign In";
    }

    public static class TestData
    {
        public const string SampleSourceText = @"W 1923 roku, podczas wykopalisk archeologicznych w północnej Grecji, archeolog **dr Helena Markos** odkryła ruiny starożytnego miasta **Heliora**. Według inskrypcji znalezionych na kamiennych tablicach, Heliora została założona około **500 roku p.n.e.** i była ważnym ośrodkiem handlowym w regionie.

Miasto słynęło z produkcji **ceramiki z czarno-czerwonym ornamentem**, a jego mieszkańcy czcili boginię **Selinę**, patronkę księżyca i rzemiosła. W centrum miasta znajdowała się monumentalna **świątynia Seliny**, w której odkryto srebrny posąg przedstawiający boginię trzymającą sierp księżyca.

W 1956 roku zespół naukowców z Uniwersytetu Ateńskiego przeprowadził pierwsze **badania datowania radiowęglowego** artefaktów, które potwierdziły ich wiek na ponad 2500 lat. W 1978 roku ruiny Heliori zostały wpisane na **listę światowego dziedzictwa UNESCO**.

Dziś w muzeum w Salonikach można zobaczyć największą kolekcję artefaktów z Heliori, w tym słynną **Amforę Markos**, ozdobioną sceną przedstawiającą nocne rytuały ku czci Seliny.";

        public static readonly string[] ExpectedFlashcards = 
        {
            "świątynia Seliny",
            "lista światowego dziedzictwa",
            "ceramika z czarno-czerwonym",
            "ruiny starożytnego miasta",
            "dr Helena Markos"
        };

        // Manual flashcard creation test data
        public const string ManualQuestion = "Dodaj pytanie na przod formatki";
        public const string ManualAnswer = "Dodaj odpowiedz na koncu formatki";
        public const string ManualTags = "tagi1";
        public const string TestUserEmail = "noob@noob.pl";
        public const string TestUserPassword = "noob2";
    }
}
