using System.Text.Json.Serialization;
using System.Collections.Generic;

using ApplicationAuth.Features.Account.Login;
using ApplicationAuth.Features.Account.AdminLogin;
using ApplicationAuth.Features.Account.Register;
using ApplicationAuth.Features.Account.RefreshToken;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.Features.AdminUsers;
using ApplicationAuth.Features.Test;
using ApplicationAuth.Features.Telegram;
using ApplicationAuth.Features.Telegram.Models;
using ApplicationAuth.SharedModels.ResponseModels;

namespace ApplicationAuth.SharedModels;

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(AdminLoginRequest))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(TelegramMessageRequest))]
[JsonSerializable(typeof(TelegramMessageRequestModel))]

[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(RegisterResponse))]
[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(ApplicationAuth.Features.Account.Shared.UserResponse), TypeInfoPropertyName = "AccountUserResponse")]
[JsonSerializable(typeof(ApplicationAuth.Features.AdminUsers.UserResponse), TypeInfoPropertyName = "AdminUserResponse")]
[JsonSerializable(typeof(UserTableRowResponse))]
[JsonSerializable(typeof(TelegramMessageResponse))]
[JsonSerializable(typeof(TelegramStickerResponseModel))]
[JsonSerializable(typeof(TelegramMessageResponseModel))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(ShortAuthorizationRequestModel))]

// Envelopes
[JsonSerializable(typeof(JsonResponse<LoginResponse>))]
[JsonSerializable(typeof(JsonResponse<RegisterResponse>))]
[JsonSerializable(typeof(JsonResponse<TokenResponse>))]
[JsonSerializable(typeof(JsonResponse<ApplicationAuth.Features.Account.Shared.UserResponse>), TypeInfoPropertyName = "JsonResponseAccountUserResponse")]
[JsonSerializable(typeof(JsonResponse<ApplicationAuth.Features.AdminUsers.UserResponse>), TypeInfoPropertyName = "JsonResponseAdminUserResponse")]
[JsonSerializable(typeof(JsonResponse<bool>))]
[JsonSerializable(typeof(JsonResponse<string>))]
[JsonSerializable(typeof(JsonResponse<object>))]

// Collections
[JsonSerializable(typeof(IEnumerable<UserTableRowResponse>))]
[JsonSerializable(typeof(JsonResponse<IEnumerable<UserTableRowResponse>>))]
[JsonSerializable(typeof(JsonPaginationResponse<IEnumerable<UserTableRowResponse>>))]
[JsonSerializable(typeof(List<UserTableRowResponse>))]
[JsonSerializable(typeof(IEnumerable<TelegramMessageResponse>))]
[JsonSerializable(typeof(IEnumerable<TelegramMessageResponseModel>))]
[JsonSerializable(typeof(IEnumerable<TelegramStickerResponseModel>))]
[JsonSerializable(typeof(List<TelegramMessageResponse>))]
[JsonSerializable(typeof(List<TelegramMessageResponseModel>))]
[JsonSerializable(typeof(List<TelegramStickerResponseModel>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]

public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
