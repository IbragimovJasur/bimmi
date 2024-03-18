using System;
namespace backend.Validators
{
    public class QueryParamValidator
    {
        private const int PARAM_MIN_LENGHT = 5;

        public static bool isValid(string queryParam)
        {
            if (queryParam == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(queryParam) | queryParam.Length < PARAM_MIN_LENGHT)
            {
                return false;
            }
            return true;
        }
    }
}
