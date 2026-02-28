namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record UserTableRowResponseModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string RegisteredAt { get; set; }

        public bool IsBlocked { get; set; }
    }
}
