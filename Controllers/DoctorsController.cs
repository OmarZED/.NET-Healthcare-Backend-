using medical.Dto.Doctor;
using medical.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
// MODIFIED: Require authentication, but NOT a specific role at the controller level
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorProfileService _doctorProfileService;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(IDoctorProfileService doctorProfileService, ILogger<DoctorsController> logger)
    {
        _doctorProfileService = doctorProfileService;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        // ClaimTypes.NameIdentifier is the standard claim type for the user ID in ASP.NET Core Identity JWTs
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Could not find User ID claim (NameIdentifier) in the current user's token.");
            return null; // Explicitly return null if the claim is not found
        }
        return userId; // Return the found userId if it's not null or empty
    }

    /// <summary>
    /// Gets the profile details for the currently authenticated doctor.
    /// </summary>
    [HttpGet("profile")]
    // KEPT: Restrict THIS action specifically to the Doctor role
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
    // ... other produces types
    public async Task<ActionResult<DoctorProfileDto>> GetMyProfile()
    {
        // ... implementation as before ...
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID claim not found in token.");
        var profileDto = await _doctorProfileService.GetMyProfileAsync(userId);
        if (profileDto == null) return NotFound(new { message = "Doctor profile not found." });
        return Ok(profileDto);
    }

    /// <summary>
    /// Updates the profile details for the currently authenticated doctor.
    /// </summary>
    [HttpPut("profile")]
    // KEPT: Restrict THIS action specifically to the Doctor role
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    // ... other produces types
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileDto updateDto)
    {
        // ... implementation as before ...
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID claim not found in token.");
        var result = await _doctorProfileService.UpdateMyProfileAsync(userId, updateDto);
        if (!result.Success)
        {
            if (result.ErrorMessage != null && result.ErrorMessage.Contains("not found")) return NotFound(new { message = result.ErrorMessage });
            return BadRequest(new { message = result.ErrorMessage ?? "Failed to update profile." });
        }
        return NoContent();
    }

    /// <summary>
    /// Gets a list of all available (verified) doctors.
    /// Accessible by any authenticated user.
    /// </summary>
    [HttpGet("available")]
    // NO specific [Authorize(Roles=...)] attribute here, so it uses the controller's [Authorize]
    [ProducesResponseType(typeof(IEnumerable<DoctorSummaryDto>), StatusCodes.Status200OK)]
    // ... other produces types
    public async Task<ActionResult<IEnumerable<DoctorSummaryDto>>> GetAvailableDoctors()
    {
        // ... implementation as before ...
        _logger.LogInformation("Fetching all available doctors.");
        var doctors = await _doctorProfileService.GetAvailableDoctorsAsync();
        return Ok(doctors);
    }

    [HttpGet("{doctorId}")] // Route: GET /api/doctors/xyz-doctor-id
    [ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // From controller [Authorize]
    [ProducesResponseType(StatusCodes.Status404NotFound)]    // If doctor ID is invalid or user is not a doctor
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DoctorProfileDto>> GetDoctorById([FromRoute] string doctorId)
    {
        _logger.LogInformation("Request received to get profile for Doctor ID: {DoctorId}", doctorId);
        try
        {
            var profileDto = await _doctorProfileService.GetDoctorProfileByIdAsync(doctorId);

            if (profileDto == null)
            {
                _logger.LogWarning("Doctor profile not found for specified ID: {DoctorId}", doctorId);
                // Return 404 if the service couldn't find a doctor profile for this ID
                return NotFound(new { message = "Doctor not found." });
            }

            return Ok(profileDto); // Return 200 OK with the profile data
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching profile for Doctor ID {DoctorId}", doctorId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred retrieving the doctor profile." });
        }
    }
}
