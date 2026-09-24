namespace TabletTracker
{
    // Współdzielony stan przepływu przypisania (QR -> stanowisko -> uczeń).
    // Unika przekazywania parametrów przez adresy tras Shell, które w modelu
    // ukrytych zakładek bywają zawodne.
    public static class AssignmentFlow
    {
        public static string TabletId = "";
        public static string? StationCode;

        public static void Reset()
        {
            TabletId = "";
            StationCode = null;
        }
    }
}