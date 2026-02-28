using MediatR;
using System.Text.Json.Serialization;

namespace ApplicationAuth.Features.AdminUsers.Delete;

public record DeleteUserRequest(int Id) : IRequest<UserResponse>
{
    [JsonIgnore]
    public bool IsSuperAdmin { get; set; }
}
