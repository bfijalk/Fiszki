namespace Fiszki.FunctionalTests.Constants;

public static class TestConstants
{
    public static class Timeouts
    {
        public const int DefaultWaitMs = 1000;
        public const int FormValidationWaitMs = 200;
        public const int PasswordMismatchWaitMs = 300;
        public const int NavigationTimeoutMs = 5000;
        public const int FlashcardGenerationWaitMs = 10000;
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
    }
}
