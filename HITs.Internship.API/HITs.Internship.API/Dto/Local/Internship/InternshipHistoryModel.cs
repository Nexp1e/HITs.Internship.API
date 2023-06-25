﻿using HITs.Internship.API.Dto.UsersService;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class InternshipHistoryModel
    {
        public int Id { get; set; }
        public int Semester { get; set; }
        public int StudyYear { get; set; }
        public double? Mark { get; set; }
        public string? Characteristic { get; set; }

        public CompanyDto Company { get; set; }

        public InternshipHistoryModel(Data.Internship internship, CompanyDto company)
        {
            Id = internship.Id;
            Semester = internship.Semester;
            StudyYear = internship.StudyYear;
            Characteristic = internship.Characteristic;
            Mark = internship.Mark;

            Company = company;
        }
    }
}
