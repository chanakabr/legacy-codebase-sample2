using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.Response
{
    public class GenericListResponse<T>
    {
        public Status Status { get; private set; }
        public List<T> Objects { get; set; }
        public int TotalItems { get; set; }

        public GenericListResponse()
        {
            Status = new Status(eResponseStatus.Error);
            Objects = new List<T>();
            TotalItems = 0;
        }

        public GenericListResponse(Status status, List<T> objs)
        {
            Status = status;
            Objects = objs;
        }

        public GenericListResponse(Status status, IEnumerable<T> objs, int totalItems)
        {
            Status = status;
            Objects = objs.ToList();
            TotalItems = totalItems;
        }

        public void SetStatus(eResponseStatus responseStatus, string message = null, List<KeyValuePair> args = null)
        {
            this.Status.Set(responseStatus, message, args);
        }

        public void SetStatus(int responseStatusCode, string message = null, List<KeyValuePair> args = null)
        {
            this.Status.Set(responseStatusCode, message, args);
        }

        public void SetStatus(Status status)
        {
            this.Status.Set(status);
        }

        public bool HasObjects()
        {
            return (IsOkStatusCode() && Objects != null && Objects.Count > 0);
        }

        public bool IsOkStatusCode()
        {
            return (Status != null && Status.IsOkStatusCode());
        }

        public List<T> GetOrThrow(Exception customException = null)
        {
            if (IsOkStatusCode()) return Objects;
            throw customException ?? new KalturaException(Status.Message, Status.Code);
        }
    }

    public static class GenericListResponse
    {
        public static GenericListResponse<T> Ok<T>(IEnumerable<T> objs) => new GenericListResponse<T>(Status.Ok, objs.ToList());
        public static GenericListResponse<T> Error<T>(eResponseStatus responseStatus, string message = null, List<KeyValuePair> args = null) =>
            new GenericListResponse<T>(new Status(responseStatus, message, args), null);
    }

    public static class GenericListResponseExtensions
    {
        public static GenericListResponse<T> Map<T>(this GenericListResponse<T> response, Func<List<T>, List<T>> mapFn)
        {
            return response.HasObjects()
                ? GenericListResponse.Ok(mapFn(response.Objects))
                : response;
        }
    }
}