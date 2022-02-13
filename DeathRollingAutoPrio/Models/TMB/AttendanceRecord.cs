namespace DeathRollingAutoPrio.Models.TMB
{
    public class AttendanceRecord
    {

        public int CharacterId { get; set; }

        public string Name { get; set; }

        public double Attendance { get; set; }

        public double LastFourWeeksAttendance { get; set; }

        public int PossibleRaids { get; set; }

        public double BasicScore { get; set; }

        public double AdvancedScore { get; set; }

    }
}
