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
        private string Authority => (string)HttpContext.Items["authority"];

        public InternshipsController(UsersService usersService, 
            ApplicationDbContext context)
        {
            _usersService = usersService;
            _context = context;
        }

        //[HttpGet("test")]
        //public async Task<IActionResult> Test()
        //{
        //    var token = await _usersService.GetAdminToken();

        //    return Ok(new TokenDto{ Token = token });
        //}

        //[HttpGet("authTest")]
        //[Swagger.Authorize]
        //public IActionResult AuthTest()
        //{
        //    return Ok(new { userId = UserId, authority = Authority });
        //}

        
        /// <summary>
        /// Детали о стажировке. Могут быть получены студентом или куратором, с которыми связана стажировка, или админом.
        /// </summary>
        /// <param name="id">Id стажировки</param>
        /// <returns>Подробные данные о стажировке</returns>
        [HttpGet("{id:int}/details")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InternshipModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Response))]
        public async Task<IActionResult> GetInternshipInfo(int id)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
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
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
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
        public async Task<IActionResult> UploadInternshipDiary(int id, IFormFile diary)
        {
            List<string> AllowedAuthorities = new() { "ADMIN", "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            if (Authority == "STUDENT" && internship.InternId != UserId)
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
            List<string> AllowedAuthorities = new() { "REPRESENTATIVE", "STUDENT" };
            if (!AllowedAuthorities.Contains(Authority))
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

            if (Authority == "REPRESENTATIVE" && company.Representative.Id != UserId)
            {
                return Forbid();
            }

            if (Authority == "STUDENT" && internship.InternId != UserId)
            {
                return Forbid();
            }

            var user = await _usersService.GetUserById(UserId);
            var comment = new Comment()
            {
                AuthorId = user.Id,
                AuthorName = user.GetFullName(),
                CreatedAt = DateTime.Now,
                InternshipId = internship.Id,
                Text = model.Text
            };
            internship.DiaryComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new InternshipModel(internship, company, user));
        }
    }
}
