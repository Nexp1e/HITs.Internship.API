namespace HITs.Internship.API.Dto.Local.Diary
{
    public class FifthSemesterDiaryModel
    {
        public List<WorkTaskReport5> Tasks { get; set; }
    }

    public class WorkTaskReport5
    {
        public DateTime TaskStart { get; set; }
        public DateTime TaskEnd { get; set; }
        public string TaskName { get; set; }
        public double TimeSpent { get; set; }
    }
}
