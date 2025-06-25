using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CSL.Helpers
{
    public static class WebUtils
    {
        public static string BuildAsQueryString(this IEnumerable<KeyValuePair<string,StringValues>> pairs)
        {
            StringBuilder toReturn = new StringBuilder();
            char PairStartToken = '?';
            foreach (KeyValuePair<string, StringValues> QueryPair in pairs)
            {
                foreach (string? value in QueryPair.Value)
                {
                    toReturn.Append(PairStartToken + HttpUtility.UrlEncode(QueryPair.Key) + "=" + HttpUtility.UrlEncode(value ?? ""));
                    PairStartToken = '&';
                }
            }
            return toReturn.ToString();
        }
    }
}
