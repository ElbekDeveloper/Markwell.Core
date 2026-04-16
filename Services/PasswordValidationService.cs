using System.Text.RegularExpressions;

namespace Markwell.Core.Services
{
    public class PasswordValidationService
    {
        private const int MinLength = 8;

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinLength)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\,.<>?/\\|`~]");

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        public string GetPasswordStrengthMessage(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Password is required";

            if (password.Length < MinLength)
                return $"Password must be at least {MinLength} characters long";

            var errors = new List<string>();

            if (!password.Any(char.IsUpper))
                errors.Add("uppercase letter");

            if (!password.Any(char.IsLower))
                errors.Add("lowercase letter");

            if (!password.Any(char.IsDigit))
                errors.Add("digit");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\,.<>?/\\|`~]"))
                errors.Add("special character");

            if (errors.Count > 0)
                return $"Password must contain at least one {string.Join(", ", errors)}";

            return "Password is valid";
        }
    }
}
