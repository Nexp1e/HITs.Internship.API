using HITs.Internship.API.Data;
using HITs.Internship.API.Dto;
using HITs.Internship.API.Dto.Local.Company;
using HITs.Internship.API.Dto.Local.Internship;
using HITs.Internship.API.Dto.UsersService;
using HITs.Internship.API.Services;
using HITs.Internship.API.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authorize = HITs.Internship.API.Swagger.AuthorizeAttribute;

namespace HITs.Internship.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternshipsController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly ApplicationDbContext _context;
        private int UserId => (int)HttpContext.Items["userId"];
        private string currUserToken => HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        public InternshipsController(UsersService usersService, 
            ApplicationDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        [HttpGet("{id:int}/details")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        public async Task<IActionResult> GetInternshipInfo(int id)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == id);
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

            if ((userAuthorities.Contains("REPRESENTATIVE") && company.Representative?.Id != UserId) || 
                (userAuthorities.Contains("SUPERVISOR") && company.Supervisor?.Id != UserId))
            {
                Console.WriteLine("User is not a representative/supervisor of the company of internship.");
                return Forbid();
            }

            if (userAuthorities.Contains("STUDENT") && internship.InternId != UserId)
            {
                Console.WriteLine("Student is not part of the internship record.");
                return Forbid();
            }

            var user = await _usersService.GetUserById(internship.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            return Ok(new InternshipModel(internship, company, user));
        }

        /// <summary>
        /// Изменить оценку за стажировку
        /// </summary>
        /// <param name="id">Id стажировки</param>
        /// <returns>Подробную информацию о стажировке</returns>
        [HttpPatch("{id:int}/mark")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        public async Task<IActionResult> SetInternshipMark(int id, EditMarkModel model)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if ((userAuthorities.Contains("REPRESENTATIVE") && company.Representative?.Id != UserId) ||
                (userAuthorities.Contains("SUPERVISOR") && company.Supervisor?.Id != UserId))
            {
                Console.WriteLine("User is not a representative/supervisor of the company of internship.");
                return Forbid();
            }

            internship.Mark = model.NewMark;
            await _context.SaveChangesAsync();

            var user = await _usersService.GetUserById(internship.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            return Ok(new InternshipModel(internship, company, user));
        }

        /// <summary>
        /// Изменить характеристику за стажировку
        /// </summary>
        /// <param name="id">Id стажировки</param>
        /// <returns>Подробную информацию о стажировке</returns>
        [HttpPatch("{id:int}/characteristic")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        public async Task<IActionResult> SetInternshipCharacteristic(int id, EditCharacteristicModel model)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if ((userAuthorities.Contains("REPRESENTATIVE") && company.Representative?.Id != UserId) ||
                (userAuthorities.Contains("SUPERVISOR") && company.Supervisor?.Id != UserId))
            {
                Console.WriteLine("User is not a representative/supervisor of the company of internship.");
                return Forbid();
            }

            internship.Characteristic = model.NewCharacteristic;
            await _context.SaveChangesAsync();

            var user = await _usersService.GetUserById(internship.InternId);
            if (user == null)
            {
                return NotFound(new Response { Message = "Intern not found." });
            }

            return Ok(new InternshipModel(internship, company, user));
        }

        /// <summary>
        /// Загрузить новый дневник практики
        /// </summary>
        /// <param name="id">Id стажировки</param>
        /// <returns>Подробную информацию о стажировке</returns>
        [HttpPatch("{id:int}/diary")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadInternshipDiary(int id, IFormFile diary)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if (userAuthorities.Contains("REPRESENTATIVE") && company.Representative?.Id != UserId)
            {
                return Forbid();
            }

            if (userAuthorities.Contains("STUDENT") && internship.InternId != UserId)
            {
                return Forbid();
            }

            using (var memoryStream = new MemoryStream())
            {
                await diary.CopyToAsync(memoryStream);

                internship.Diary = memoryStream.ToArray();

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        /// <summary>
        /// Добавить комментарий к дневнику
        /// </summary>
        /// <param name="id">Id стажировки</param>
        /// <returns>Подробную информацию о стажировке</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        [HttpPost("{id:int}/diary/comments")]
        [Authorize]
        public async Task<IActionResult> CommentOnDiary(int id, PostDiaryCommentModel model)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "SUPERVISOR", "STUDENT" };
            List<string> userAuthorities = await _usersService.GetUserAuthorities(currUserToken);
            if (!userAuthorities.Any(x => AllowedAuthorities.Any(y => y == x)))
            {
                return Forbid();
            }

            var internship = await _context.Internships
                .Include(x => x.DiaryComments)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (internship == null)
            {
                return NotFound(new Response { Message = "Internship data not found." });
            }

            var company = await _usersService.GetCompanyById(internship.CompanyId);
            if (company == null)
            {
                return NotFound(new Response { Message = "Company not found." });
            }

            if ((userAuthorities.Contains("REPRESENTATIVE") && company.Representative?.Id != UserId) ||
                (userAuthorities.Contains("SUPERVISOR") && company.Supervisor?.Id != UserId))
            {
                Console.WriteLine("User is not a representative/supervisor of the company of internship.");
                return Forbid();
            }

            if (userAuthorities.Contains("STUDENT") && internship.InternId != UserId)
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(UserId);
            var comment = new Comment()
            {
                AuthorId = user.Id,
                AuthorName = user.GetFullName(),
                CreatedAt = DateTime.UtcNow,
                InternshipId = internship.Id,
                Text = model.Text
            };
            internship.DiaryComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new InternshipModel(internship, company, user));
        }
    }
}
