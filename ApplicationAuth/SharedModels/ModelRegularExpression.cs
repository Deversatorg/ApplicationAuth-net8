namespace ApplicationAuth.SharedModels
{
    public static class ModelRegularExpression
    {
        public const string REG_EMAIL = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        public const string REG_EMAIL_DOMAINS = @"^.*$";
        public const string REG_ONE_LATER_DIGIT = @"^(?=.*[A-Za-z])(?=.*\d).*$";
        public const string REG_NOT_CONTAIN_SPACES_ONLY = @"^(?!\s*$).+";
        public const string REG_PHONE = @"^\+?\d{10,14}$";
    }
}
