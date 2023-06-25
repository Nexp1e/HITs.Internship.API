using HITs.Internship.API.Data;
using HITs.Internship.API.Dto;
using HITs.Internship.API.Services;
using HITs.Internship.API.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HITs.Internship.API.Controllers
{
    [Route("api/diary-generator")]
    [ApiController]
    public class DiaryController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationDbContext _context;
        private int UserId => (int)HttpContext.Items["userId"];
        private string Authority => (string)HttpContext.Items["authority"];

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
        [HttpGet("{internshipId:int}/5")]
        [Authorize]
        public async Task<IActionResult> GenerateFirstInternshipDiary(int internshipId)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            if (Authority == "STUDENT" && internship.InternId != UserId)
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(internship.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            return Ok();
        }
        /// <summary>
        /// Загрузить дневник для 5 семестра
        /// </summary>
        /// <param name="internshipId">Id стажировки</param>
        /// <returns>Статус 200, если дневник успешно загружен</returns>
        [HttpPost("{internshipId:int}/5")]
        public async Task<IActionResult> UploadFifthInternshipDiary(int internshipId)
        {
            return Ok();
        }
    }
}
