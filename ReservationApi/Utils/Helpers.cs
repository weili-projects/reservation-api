namespace ReservationApi.Utils
{
    public static class Helpers
    {
        public static bool IsValidTime(DateTime time)
        {
            int minute = time.Minute;
            int second = time.Second;
            return minute % 15 == 0 && second == 0;

        }
    }
}