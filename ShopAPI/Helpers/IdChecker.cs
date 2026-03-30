namespace ShopAPI.Helpers
{
    public static class IdChecker
    {
        public static bool IsInvalidId(this int id)
        {
            return id <= 0;
        }
    }
}
