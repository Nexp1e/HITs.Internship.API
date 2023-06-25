using HITs.Internship.API.Data;
using HITs.Internship.API.Dto;
using HITs.Internship.API.Services;
using HITs.Internship.API.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using HITs.Internship.API.Dto.Local.Internship;
using Microsoft.EntityFrameworkCore;

namespace HITs.Internship.API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationDbContext _context;
        private int UserId => (int)HttpContext.Items["userId"];
        private string Authority => (string)HttpContext.Items["authority"];

        public StudentsController(UsersService usersService, 
            ApplicationDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        /// <summary>
        /// История всех стажировок студента. Доступна студенту про себя и админу про всех
        /// </summary>
        /// <param name="studentId">Id студента</param>
        /// <returns>Данные о стажировках и компаниях, в которых они проходили</returns>
        [HttpGet("{studentId:int}/internships")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipHistoryModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(List<Response>))]
        public async Task<IActionResult> GetStudentInternshipsHistory(int studentId)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(studentId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Student not found." });
            }

            if (Authority == "STUDENT" && user.Id != UserId)
            {
                return Forbid();
            }

            var internships = await _context.Internships.Where(x => x.InternId == user.Id).ToListAsync();
            List<InternshipHistoryModel> history = new();
            foreach (var internship in internships)
            {
                var company = await _usersService.GetCompanyById(internship.CompanyId);
                var preview = new InternshipHistoryModel(internship, company);
                history.Add(preview);
            }

            return Ok(history);
        }

        /// <summary>
        /// Последняя стажировка студента. Доступна студенту про себя и админу про всех
        /// </summary>
        /// <param name="studentId">Id студента</param>
        /// <returns>Данные о стажировке и компании, в которой она проходила</returns>
        [HttpGet("{studentId:int}/internships/current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipHistoryModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        public async Task<IActionResult> GetCurrentInternshipInfo(int studentId)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(studentId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Student not found." });
            }

            if (Authority == "STUDENT" && user.Id != UserId)
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Where(x => x.InternId == user.Id)
                .OrderBy(x => x.StudyYear)
                .ThenBy(x => x.Semester)
                .LastAsync();

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            var preview = new InternshipHistoryModel(internship, company);

            return Ok(preview);
        }
    }
}
