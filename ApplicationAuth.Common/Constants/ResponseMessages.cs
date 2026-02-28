namespace ApplicationAuth.Common.Constants
{
    public static class ResponseMessages
    {
        #region Base Response Messages

        public const string RequestSuccessful = "Request successful";

        public const string InvalidData = "Invalid data";

        public const string BadRequest = "Bad request";

        public const string Unauthorized = "Unauthorized";

        public const string Forbidden = "Forbidden";

        public const string NotFound = "Not Found";

        public const string InternalServerError = "Internal server error";

        #endregion

        #region Registration/Login messages

        public const string SuccessfulRegistration = "Successful registration";

        public const string SuccessfulLogin = "Successful login";

        public const string InvalidCredentials = "Invalid credentials";

        public const string MessageSent = "Message sent";

        public const string LinkSent = "Link has been sent";

        public const string AccountBlocked = "Account is blocked";

        public const string EmailAlreadyRegistered = "Email address already registered";

        public const string EmailInvalidOrNotConfirmed = "Email Invalid Or Not Confirmed";

        #endregion

        #region Payment Messages

        public const string SuccessfulPayment = "Successful payment";

        public const string SuccessfulTransaction = "Successful transaction";

        public const string SuccessfulSubscription = "Successful subscription";

        #endregion
    }
}
