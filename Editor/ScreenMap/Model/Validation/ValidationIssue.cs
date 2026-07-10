namespace ScreenNavigators.Editors
{
    public class ValidationIssue
    {
        private readonly ValidationSeverity _severity;
        private readonly string _screenId;
        private readonly string _message;

        public ValidationSeverity Severity => _severity;
        public string ScreenId => _screenId;
        public string Message => _message;

        public ValidationIssue(ValidationSeverity severity, string screenId, string message)
        {
            _severity = severity;
            _screenId = screenId;
            _message = message;
        }
    }
}
