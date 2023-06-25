using HITs.Internship.API.Data;
using HITs.Internship.API.Dto;
using HITs.Internship.API.Dto.Local.Company;
using HITs.Internship.API.Dto.Local.Internship;
using HITs.Internship.API.Dto.UsersService;
using HITs.Internship.API.Services;
using HITs.Internship.API.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace HITs.Internship.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationDbContext _context;
        private int UserId => (int)HttpContext.Items["userId"];
        private string Authority => (string)HttpContext.Items["authority"];

        public CompaniesController(UsersService usersService, 
            ApplicationDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        /// <summary>
        /// Общая информация о компании. 
        /// </summary>
        /// <param name="companyId">Id компании</param>
        /// <returns>Данные компании. Если запрашивает админ или куратор данной компании - выводит еще и список стажеров/стажировок</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        [HttpGet("company/{companyId:int}/details")]
        [Authorize]
        public async Task<IActionResult> GetCompanyInfo(int companyId)
        {
            var company = await _usersService.GetCompanyById(companyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            var interns = new List<InternshipPreview>();

            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE" };
            if (AllowedAuthorities.Contains(Authority))
            {
                var internships = await _context.Internships.Where(x => x.CompanyId == companyId).ToListAsync();

                foreach (var internship in internships)
                {
                    var user = await _usersService.GetUserById(internship.InternId);
                    if (user == null)
                    {
                        return NotFound(new Response { Message = "Intern not found." });
                    }
                    var internshipPreview = new InternshipPreview(internship, user);
                    interns.Add(internshipPreview);
                }
            }

            var companyModel = new CompanyModel(company, interns);

            return Ok(companyModel);
        }

        /// <summary>
        /// Привязать стажера к компании на указанный семестр
        /// </summary>
        /// <param name="companyId">Id компании</param>
        /// <returns>Данные о созданной связи стажировки</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        [HttpPost("company/{companyId:int}/interns")]
        [Authorize]
        public async Task<IActionResult> AddInternToCompany([FromRoute]int companyId,
            [FromBody] AddInternToCompanyModel model)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE" };
            if (!AllowedAuthorities.Contains(Authority))
            {
                return Forbid();
            }

            var company = await _usersService.GetCompanyById(companyId);
            if (company == null)
            {
                return NotFound(new Response {Message = "Company not found."});
            }

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(model.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            var duplicateRecord = _context.Internships.FirstOrDefault(x => x.InternId == user.Id
                                                                           && x.StudyYear ==
                                                                                Data.Internship.GetCurrentStudyYear()
                                                                           && x.Semester == model.Semester
                                                                           && x.CompanyId == companyId);
            if (duplicateRecord != null)
            {
                return BadRequest(new Response
                    { Message = "A record of intern in this company at this semester already exists." });
            }

            var newRecord = new Data.Internship()
            {
                CompanyId = companyId,
                Semester = model.Semester,
                StudyYear = Data.Internship.GetCurrentStudyYear(),
                InternId = model.InternId,
                DiaryComments = new List<Comment>()
            };
            _context.Internships.Add(newRecord);
            await _context.SaveChangesAsync();

            return Ok(new InternshipModel(newRecord, company, user));
        }

        /// <summary>
        /// Отвязать стажера от компании
        /// </summary>
        /// <param name="companyId">Id компании</param>
        /// <param name="internshipId">Id записи о стажировке</param>
        /// <returns>Статус 200, если удаление прошло успешно</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        [HttpDelete("company/{companyId:int}/interns/{internshipId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveInternFromCompany(int companyId, int internshipId)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE" };
            if (!AllowedAuthorities.Contains(Authority))
            {
                return Forbid();
            }

            var company = await _usersService.GetCompanyById(companyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            var internship = await _context.Internships.FirstOrDefaultAsync(x => x.Id == internshipId);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            _context.Remove(internship);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
