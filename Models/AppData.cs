namespace TabletTracker.Models;

public class AppData
{
    public List<Station> Stations { get; set; } = new();
    public List<StudentClass> Classes { get; set; } = new();
    public List<ScanRecord> Scans { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}

public class AppSettings
{
    public string TeacherName { get; set; } = "";
}

public class Station
{
    public string Code { get; set; } = "";
    public string? TabletId { get; set; }
    public string? StudentId { get; set; }
    public DateTime? AssignedAt { get; set; }
}

public class Student
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = "";
}

public class StudentClass
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public List<Student> Students { get; set; } = new();
}

public class ScanRecord
{
    public DateTime Timestamp { get; set; }
    public string StationCode { get; set; } = "";
    public string TabletId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string TeacherName { get; set; } = "";
}