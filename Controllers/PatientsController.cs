using medical.Dto.Patient;
using medical.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace medical.Controllers
{
    [Route("api/[controller]")] // Base route: /api/patients
    [ApiController]
    [Authorize(Roles = "Patient")] // IMPORTANT: Only authenticated users with the "Patient" role can access this controller
    public class PatientsController : ControllerBase
    {
        private readonly IPatientProfileService _patientProfileService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientProfileService patientProfileService, ILogger<PatientsController> logger)
        {
            _patientProfileService = patientProfileService;
            _logger = logger;
        }

        // Helper method to safely get the User ID from the claims principal
        private string? GetCurrentUserId()
        {
            // ClaimTypes.NameIdentifier is the standard claim type for the user ID in ASP.NET Core Identity JWTs
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Could not find User ID claim (NameIdentifier) in the current user's token.");
            }
            return userId;
        }

        /// <summary>
        /// Gets the profile details for the currently authenticated patient.
        /// </summary>
        /// <returns>The patient's profile information.</returns>
        [HttpGet("profile")] // Route: GET /api/patients/profile
        [ProducesResponseType(typeof(PatientProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // If token is missing or invalid
        [ProducesResponseType(StatusCodes.Status403Forbidden)]    // If user is authenticated but not in "Patient" role
        [ProducesResponseType(StatusCodes.Status404NotFound)]    // If profile doesn't exist for this user
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PatientProfileDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                // This technically shouldn't happen if [Authorize] is working, but good practice to check
                return Unauthorized("User ID claim not found in token.");
            }

            _logger.LogInformation("Fetching profile for authenticated patient User ID: {UserId}", userId);
            try
            {
                var profileDto = await _patientProfileService.GetMyProfileAsync(userId);

                if (profileDto == null)
                {
                    _logger.LogWarning("Patient profile not found for User ID: {UserId}", userId);
                    // Return 404 if the service couldn't find the profile for this specific user
                    return NotFound(new { message = "Patient profile not found." });
                }

                return Ok(profileDto); // Return 200 OK with the profile data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching profile for User ID {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred retrieving the profile." });
            }
        }

        /// <summary>
        /// Updates the profile details for the currently authenticated patient.
        /// </summary>
        /// <param name="updateDto">The data containing the fields to update.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("profile")] // Route: PUT /api/patients/profile
        [ProducesResponseType(StatusCodes.Status204NoContent)]       // Success
        [ProducesResponseType(StatusCodes.Status400BadRequest)]       // Validation error or service failure
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]     // If token is missing or invalid
        [ProducesResponseType(StatusCodes.Status403Forbidden)]        // If user is not in "Patient" role
        [ProducesResponseType(StatusCodes.Status404NotFound)]        // If profile doesn't exist for this user
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientProfileDto updateDto)
        {
            if (!ModelState.IsValid) // Check DTO validation attributes (if any added)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID claim not found in token.");
            }

            _logger.LogInformation("Attempting to update profile for authenticated patient User ID: {UserId}", userId);
            try
            {
                var result = await _patientProfileService.UpdateMyProfileAsync(userId, updateDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to update profile for User ID {UserId}: {Error}", userId, result.ErrorMessage);
                    // Check common failure reasons
                    if (result.ErrorMessage != null && result.ErrorMessage.Contains("not found"))
                    {
                        return NotFound(new { message = result.ErrorMessage });
                    }
                    // For other errors (like concurrency, database issues, etc.) return BadRequest
                    return BadRequest(new { message = result.ErrorMessage ?? "Failed to update profile." });
                }

                _logger.LogInformation("Profile updated successfully for User ID: {UserId}", userId);
                return NoContent(); // Return 204 No Content on successful update
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating profile for User ID {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred updating the profile." });
            }
        }
    }
}