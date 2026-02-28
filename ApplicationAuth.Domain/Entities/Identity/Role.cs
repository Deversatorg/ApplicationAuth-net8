namespace ApplicationAuth.Domain.Entities.Identity
{
    public static class Role
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";

        public const string Admins = Admin + ", " + SuperAdmin;
    }
}
