namespace ReservationApi.Utils
{
    public static class Helpers
    {
        public static bool IsValidTime(DateTime time)
        {
            int minute = time.Minute;
            return minute % 15 == 0;
        }
    }
}