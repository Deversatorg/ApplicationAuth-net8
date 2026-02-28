using System.Collections.Generic;

namespace ApplicationAuth.SharedModels.ResponseModels.Base.CursorPagination
{
    public record CursorPaginationBaseResponseModel<T>
    {
        public CursorPaginationBaseResponseModel() { }

        public CursorPaginationBaseResponseModel(List<T> data, int? lastId)
        {
            Data = data;
            LastId = lastId;
        }

        public int? LastId { get; set; }

        public List<T> Data { get; set; }
    }
}
