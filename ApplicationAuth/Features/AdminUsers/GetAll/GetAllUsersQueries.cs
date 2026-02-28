using ApplicationAuth.SharedModels.Enums;
using ApplicationAuth.SharedModels.RequestModels;
using ApplicationAuth.SharedModels.RequestModels.Base.CursorPagination;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.SharedModels.ResponseModels.Base.CursorPagination;
using MediatR;

namespace ApplicationAuth.Features.AdminUsers.GetAll;

public record GetAllUsersOffsetQuery(PaginationRequestModel<UserTableColumn> Model, bool GetAdmins, bool IsSuperAdmin, bool IsAdmin) : IRequest<PaginationResponseModel<UserTableRowResponse>>;

public record GetAllUsersCursorQuery(CursorPaginationRequestModel<UserTableColumn> Model, bool GetAdmins, bool IsSuperAdmin, bool IsAdmin) : IRequest<CursorPaginationBaseResponseModel<UserTableRowResponse>>;
