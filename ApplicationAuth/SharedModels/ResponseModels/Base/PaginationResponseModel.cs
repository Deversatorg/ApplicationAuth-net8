using System.Collections.Generic;

namespace ApplicationAuth.SharedModels.ResponseModels
{
    public record PaginationResponseModel<T>
    {
        public PaginationResponseModel()
        {
        }

        public PaginationResponseModel(List<T> data, int totalCount)
        {
            Data = data;
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public List<T> Data { get; set; }
    }
}
