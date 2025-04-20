using medical.Dto.Auth;
using medical.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace medical.Controllers
{
    [Route("api/[controller]")] // Base route: /api/auth
    [ApiController]
    [AllowAnonymous] // IMPORTANT: Allow access to registration/login without prior authentication
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; // Optional: For logging requests/errors

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new Patient user.
        /// </summary>
        /// <param name="registerDto">Patient registration details.</param>
        /// <returns>Authentication details including JWT token upon successful registration.</returns>
        [HttpPost("register-patient")] // POST /api/auth/register-patient
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)] // Or 201 Created if preferred
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For validation errors or existing email
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> RegisterPatient([FromBody] RegisterPatientDto registerDto)
        {
            if (!ModelState.IsValid) // Check DTO validation attributes
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received request to register patient {Email}", registerDto.Email);
            var result = await _authService.RegisterPatientAsync(registerDto);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Patient registration failed for {Email}: {Error}", registerDto.Email, result.ErrorMessage);
                // Return 400 Bad Request for known registration failures (like email exists)
                return BadRequest(new { message = result.ErrorMessage ?? "Patient registration failed." });
            }

            _logger.LogInformation("Patient {Email} registered successfully.", registerDto.Email);
            // Return 200 OK with the AuthResponseDto (contains token etc.)
            // Could also return 201 Created with a location header if you have a "get user details" endpoint
            return Ok(result.Data);
        }

        /// <summary>
        /// Registers a new Doctor user.
        /// </summary>
        /// <param name="registerDto">Doctor registration details.</param>
        /// <returns>Authentication details including JWT token upon successful registration.</returns>
        [HttpPost("register-doctor")] // POST /api/auth/register-doctor
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)] // Or 201 Created
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> RegisterDoctor([FromBody] RegisterDoctorDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received request to register doctor {Email}", registerDto.Email);
            var result = await _authService.RegisterDoctorAsync(registerDto);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Doctor registration failed for {Email}: {Error}", registerDto.Email, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage ?? "Doctor registration failed." });
            }

            _logger.LogInformation("Doctor {Email} registered successfully.", registerDto.Email);
            return Ok(result.Data);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginDto">User login credentials.</param>
        /// <returns>Authentication details including JWT token upon successful login.</returns>
        [HttpPost("login")] // POST /api/auth/login
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For invalid credentials or validation errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Explicitly for failed login attempt
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received login request for {Email}", loginDto.Email);
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Login failed for {Email}: {Error}", loginDto.Email, result.ErrorMessage);
                // Return 401 Unauthorized for failed login attempts specifically
                return Unauthorized(new { message = result.ErrorMessage ?? "Login failed." });
            }

            _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
            return Ok(result.Data);
        }

        // Note on Logout:
        // With JWT, true logout is typically handled client-side by deleting the token.
        // If you need server-side invalidation (e.g., for immediate revocation),
        // you'd need a more complex setup involving token blocklists or refresh tokens,
        // which would require additional service methods and potentially database storage.
        // A simple endpoint here wouldn't achieve secure JWT logout.
        // [HttpPost("logout")] // Example placeholder - not a real JWT logout
        // public IActionResult Logout()
        // {
        //     // Server-side: Nothing to do for basic JWT logout.
        //     // Client-side: Discard the JWT.
        //     _logger.LogInformation("Logout endpoint called (client should discard token).");
        //     return Ok(new { message = "Logged out successfully. Please discard your token." });
        // }
    }
}
