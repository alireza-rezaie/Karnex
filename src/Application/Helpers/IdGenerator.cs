using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Http;

namespace Application.Helpers
{
    public class IdGenerator
    {
        public static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}"); // save the path
            foreach (var (key, value) in request.Query.OrderBy(x => x.Key)) //ex : pageId=1,sort = title,type : desc,take = 30
                keyBuilder.Append($"|{key}-{value}"); // save query
            return keyBuilder.ToString();
        }
    }
}
