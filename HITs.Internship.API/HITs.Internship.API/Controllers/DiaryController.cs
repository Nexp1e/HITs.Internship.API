using HITs.Internship.API.Data;
using HITs.Internship.API.Dto;
using HITs.Internship.API.Dto.Local.Diary;
using HITs.Internship.API.Dto.UsersService;
using HITs.Internship.API.Services;
using HITs.Internship.API.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.XWPF.UserModel;

namespace HITs.Internship.API.Controllers
{
    [Route("api/diary-generator")]
    [ApiController]
    [Consumes("application/json")]
    public class DiaryController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationDbContext _context;
        private int UserId => (int)HttpContext.Items["userId"];
        private string Authority => (string)HttpContext.Items["authority"];

        private string currUserToken => HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        public DiaryController(UsersService usersService, 
            ApplicationDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        /// <summary>
        /// Сгенерировать дневник практики для 5 семестра
        /// </summary>
        /// <param name="internshipId">Id стажировки</param>
        /// <returns>Документ дневника</returns>
        [HttpPost("{internshipId:int}/5")]
        [Authorize]
        public async Task<IActionResult> GenerateFirstInternshipDiary(int internshipId, [FromBody]FifthSemesterDiaryModel model)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == internshipId);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if (userAuthorities.Contains("REPRESENTATIVE") && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            if (userAuthorities.Contains("STUDENT")  && internship.InternId != UserId)
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(internship.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            return await GenerateFifthDiaryFromTemplate(5, internshipId, model);
        }
        /// <summary>
        /// Прикрепить дневник практики к стажировке
        /// </summary>
        /// <param name="internshipId">Id стажировки</param>
        /// <returns>Статус 200, если дневник успешно загружен</returns>
        [HttpPost("{internshipId:int}/attach-diary")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFifthInternshipDiary(int internshipId, IFormFile file)
        {
            long fileSize = file.Length;

            if (fileSize > 0)
            {
                using var stream = new MemoryStream();

                await file.CopyToAsync(stream);
                var bytes = stream.ToArray();

                var internship = await _context.Internships.FirstOrDefaultAsync(x => x.Id == internshipId);
                if (internship == null)
                {
                    return NotFound(new Response { Message = "Internship record not found." });
                }

                internship.Diary = bytes;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        /// <summary>
        /// Загрузить на сервер шаблон для дневника
        /// </summary>
        /// <param name="semesterId">Номер семестра дневника (от 5 до 8)</param>
        /// <returns></returns>
        [HttpPost("templates/{semesterNumber:int}")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> UploadFifthDiaryTemplate(int semesterNumber, [FromForm]AddFifthSemesterDiary model)
        {
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => x == "ADMIN"))
            {
                return Forbid();
            }

            if (semesterNumber > 8 || semesterNumber < 5)
            {
                return BadRequest(new { message = "Invalid semester number" });
            }

            long fileSize = model.File.Length;

            if (fileSize > 0)
            {
                using var stream = new MemoryStream();

                await model.File.CopyToAsync(stream);
                var bytes = stream.ToArray();

                var existingTemplate = await _context.DiaryTemplates.FindAsync(semesterNumber);
                if (existingTemplate == null)
                {
                    existingTemplate = new DiaryTemplate
                    {
                        Semester = semesterNumber,
                        Order = model.Order
                    };

                    _context.DiaryTemplates.Add(existingTemplate);
                }

                existingTemplate.File = bytes;
                await _context.SaveChangesAsync();

                existingTemplate = await _context.DiaryTemplates.FindAsync(semesterNumber);
                Console.WriteLine(existingTemplate);
            }

            return Ok();
        }


        private async Task<IActionResult> GenerateFifthDiaryFromTemplate(int semesterNumber, int internshipId, FifthSemesterDiaryModel model)
        {
            var template = await _context.DiaryTemplates.FirstOrDefaultAsync(x => x.Semester == semesterNumber);
            if (template == null)
            {
                return BadRequest(new { message = "Template not found" });
            }

            var internship = await _context.Internships
                .FirstOrDefaultAsync(x => x.Id == internshipId);
            if (internship == null)
            {
                Console.WriteLine("Internship data not found.");
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            var intern = await _usersService.GetUserById(internship.InternId);
            if (intern == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            var supervisor = await _usersService.GetUserById(company.Supervisor.Id);
            if (supervisor == null)
            {
                return NotFound(new Response { Message = "Supervisor not found." });
            }

            var ms = new MemoryStream(template.File);

            var generateFile = @"output1.docx";

            

            using (var doc = new XWPFDocument(ms))
            {
                foreach (var para in doc.Paragraphs)
                {
                    ReplacePlaceholders(para, intern, supervisor, company, internship, template.Order);
                }

                foreach (var table in doc.Tables)
                {
                    foreach (var row in table.Rows)
                    {
                        foreach (var cell in row.GetTableCells())
                        {
                            foreach (var cellParagraph in cell.Paragraphs)
                            {
                                ReplacePlaceholders(cellParagraph, intern, supervisor, company, internship, template.Order);
                            }
                        }
                    }
                }

                var tasksTable = doc.Tables.SkipLast(1).Last();

                foreach (var task in model.Tasks)
                {
                    var row = tasksTable.CreateRow();
                    row.CreateCell();
                    row.GetCell(0).SetText(task.TaskStart.ToString("dd.MM.yyyy"));
                    row.GetCell(1).SetText(task.TaskEnd.ToString("dd.MM.yyyy"));
                    row.GetCell(2).SetText(task.TaskName);
                    row.GetCell(3).SetText(task.TimeSpent.ToString());
                    row.RemoveCell(4);
                }

                using (var ws = System.IO.File.Create(generateFile))
                {
                    doc.Write(ws);
                    doc.Close();
                }
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(@"output1.docx");

            return File(fileBytes, "application/octet-stream", $"{intern.GetFullName()}_дневник_практики_${semesterNumber}_семестр.docx");
        }

        private void ReplacePlaceholders(XWPFParagraph para, UserDto intern, UserDto supervisor, CompanyDto company, Data.Internship internship, string order)
        {
            para.ReplaceText("{{studentFullName}}", intern.GetFullName());
            para.ReplaceText("{{studentShortName}}", $"{intern.FullName.FirstName} {intern.FullName.LastName.First()}. {intern.FullName.MiddleName.First()}.");
            para.ReplaceText("{{companyFullName}}", company.OfficialName);
            para.ReplaceText("{{orderNameAndDate}}", order);
            para.ReplaceText("{{curatorFullName}}", supervisor.GetFullName());
            para.ReplaceText("{{curatorShortName}}", $"{supervisor.FullName.FirstName} {supervisor.FullName.LastName.First()}. {supervisor.FullName.MiddleName.First()}.");
            para.ReplaceText("{{representativeShortName}}", $"{company.Representative.FullName.FirstName} {company.Representative.FullName.LastName.First()}. {company.Representative.FullName.MiddleName.First()}.");
            para.ReplaceText("{{characteristic}}", internship.Characteristic);
            para.ReplaceText("{{mark}}", internship.Mark.ToString());

            var semStartYear = DateTime.UtcNow.Month >= 9 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year - 1;
            var semEndYear = semStartYear + 1;

            para.ReplaceText("{{startYear}}", semStartYear.ToString());
            para.ReplaceText("{{nextYear}}", semEndYear.ToString());
        }
    }
}
