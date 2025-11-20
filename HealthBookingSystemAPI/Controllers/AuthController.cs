using BusinessObject.Models;
using HealthBookingSystemAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Interface;
using Services.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        public AuthController(IUserService service, IPatientService patientService, IDoctorService doctorService)
        {
            _userService = service;
            _patientService = patientService;
            _doctorService = doctorService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDTO)
        {
            var account = await _userService.Login(loginDTO.Email, loginDTO.Password);
            if (account == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim("Role", account.Role.ToString()),
                    new Claim("AccountId", account.UserId.ToString()),
                };

            var symetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
            var signCredential = new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256);

            var preparedToken = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(16),
                signingCredentials: signCredential);

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(preparedToken);
            var role = account.Role;
            var accountId = account.UserId;

            return Ok(new AccountResponseDTO
            {
                Role = role,
                Token = generatedToken,
                AccountId = accountId,
            });
        }
        [HttpPost("register-patient")]
        public async Task<IActionResult> Register(RegisterPatientDTO registerDTO)
        {
            var existUser = await _userService.CheckUserExist(registerDTO.Email);
            if (existUser == null)
            {
                var user = new User
                {
                    Email = registerDTO.Email,
                    Password = registerDTO.Password,
                    FullName = registerDTO.FullName,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Role = "Patient",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    AvatarUrl = "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg"
                };

                await _userService.CreateUser(user);

                    user.Patient = new Patient
                    {
                        UserId = user.UserId,
                        DateOfBirth = registerDTO.Dob,
                        Gender = registerDTO.Gender,
                        Address = registerDTO.Address,
                        BloodType = registerDTO.BloodType,
                        EmergencyPhoneNumber = registerDTO.EmergencyContact,
                        Weight = registerDTO.Weight,
                        Height = registerDTO.Height,
                        Bmi = registerDTO.Bmi,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _patientService.AddPatient(user.Patient);
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDTO registerDTO)
        {
            var existUser = await _userService.CheckUserExist(registerDTO.Email);
            if (existUser == null)
            {
                var user = new User
                {
                    Email = registerDTO.Email,
                    Password = registerDTO.Password,
                    FullName = registerDTO.FullName,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Role = "Doctor",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    AvatarUrl = "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg"
                };
                await _userService.CreateUser(user);
                user.Doctor = new Doctor
                {
                    UserId = user.UserId,
                    SpecialtyId = registerDTO.SpecialtyId,
                    Bio = registerDTO.Bio,
                    Experience = registerDTO.Experience,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _doctorService.AddDoctor(user.Doctor);
                return Ok();
            }
            return BadRequest("User already exists.");
        }
    }
}
