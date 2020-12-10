using System;
using System.Collections.Generic;

namespace EFCoreMistake.Responses
{
    public class Response<T>
    {
        public Response(IEnumerable<T> results = null)
        {
            Results = results ?? Array.Empty<T>();
        }
        
        public IEnumerable<T> Results { get; set; }
    }
}