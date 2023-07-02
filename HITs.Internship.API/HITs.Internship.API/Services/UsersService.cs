using HITs.Internship.API.Dto.UsersService;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Net.Http;

namespace HITs.Internship.API.Services
{
    public class UsersService
    {
        private readonly HttpClient _client;
        private readonly ILogger<UsersService> _logger;
        private readonly IConfiguration _configuration;

        public UsersService(HttpClient httpClient, 
            ILogger<UsersService> logger,
            IConfiguration configuration)
        {
            _client = httpClient;
            _logger = logger;
            _configuration = configuration;

            Console.WriteLine(_configuration["UsersServiceBaseUrl"]);
            _client.BaseAddress = new Uri(_configuration["UsersServiceBaseUrl"]);

            _client.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json");
        }

        public async Task<string> GetAdminToken()
        {
            string adminLogin = _configuration["AdminLogin"];
            string adminPassword = _configuration["AdminPassword"];

            var loginDto = new LoginDto
            {
                Login = adminLogin,
                Password = adminPassword
            };

            try
            {
                var requestJsonContent = JsonContent.Create(loginDto);
                var response = await _client.PostAsync("auth/login", requestJsonContent);

                var responceDto = await response.Content.ReadFromJsonAsync<TokenDto>();
                return responceDto.Token;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<CompanyDto>> GetAllCompanies()
        {
            var adminToken = await GetAdminToken();

            _client.DefaultRequestHeaders.Add("Auth", adminToken);

            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyDto>>("companies");
                return companies;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _client.DefaultRequestHeaders.Remove("Auth");
            }
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            var adminToken = await GetAdminToken();

            _client.DefaultRequestHeaders.Add("Auth", adminToken);

            try
            {
                var users = await _client.GetFromJsonAsync<List<UserDto>>("companies");
                return users;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _client.DefaultRequestHeaders.Remove("Auth");
            }
        }

        public async Task<List<string>> GetUserRoles(string token)
        {
            return new List<string>();
        }

        public async Task<CompanyDto?> GetCompanyById(int id)
        {
            var adminToken = await GetAdminToken();

            _client.DefaultRequestHeaders.Add("Auth", adminToken);

            try
            {
                var companies = await _client.GetFromJsonAsync<List<CompanyDto>>("companies");
                return companies?.FirstOrDefault(x => x.Id == id);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _client.DefaultRequestHeaders.Remove("Auth");
            }
        }

        public async Task<UserDto?> GetUserById(int id)
        {
            var adminToken = await GetAdminToken();

            _client.DefaultRequestHeaders.Add("Auth", adminToken);

            try
            {
                var users = await _client.GetFromJsonAsync<List<UserDto>>("users");
                return users?.FirstOrDefault(x => x.Id == id);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _client.DefaultRequestHeaders.Remove("Auth");
            }
        }

        public async Task<List<string>> GetUserAuthorities(string userToken)
        {
            _client.DefaultRequestHeaders.Add("Auth", userToken);

            try
            {
                var authorities = await _client.GetFromJsonAsync<List<string>>("users/roles");
                return authorities;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _client.DefaultRequestHeaders.Remove("Auth");
            }
        }
    }
}
