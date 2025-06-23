using System;

namespace Imperium.Core
{
    public static class Constants
    {
        public static readonly Guid SYSTEM_UESER_ID = new Guid("00000000-0000-0000-0000-000000000001");

        // OTP Configuration
        public const int OTP_COUNT = 5; // Maximum OTP attempts
        public const int OTP_EXPIRY_MINUTES = 15; // OTP expiry time
        public const int OTP_RESEND_COOLDOWN_MINUTES = 1; // Cooldown between resends

        // Password Reset
        public const int PASSWORD_RESET_EXPIRY_HOURS = 1; // Password reset link expiry

        // Account Lockout
        public const int MAX_LOGIN_ATTEMPTS = 5;
        public const int LOCKOUT_DURATION_MINUTES = 30;

        // Phone Validation
        public static readonly string[] KAZAKHSTAN_PHONE_PREFIXES = { "+7701", "+7702", "+7705", "+7707", "+7708", "+7747", "+7750", "+7771", "+7775", "+7776", "+7777", "+7778" };
    }
}
