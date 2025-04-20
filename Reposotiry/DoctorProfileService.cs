using medical.DBContext;
using medical.Dto.Doctor;
using medical.Dto.Shared;
using medical.Interface;
using Microsoft.EntityFrameworkCore;

namespace medical.Reposotiry
{
    public class DoctorProfileService : IDoctorProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorProfileService> _logger;
        // private readonly UserManager<ApplicationUser> _userManager; // Inject if needed to update user fields

        public DoctorProfileService(ApplicationDbContext context, ILogger<DoctorProfileService> logger /*, UserManager<ApplicationUser> userManager*/)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _userManager = userManager;
        }

        public async Task<IEnumerable<DoctorSummaryDto>> GetAvailableDoctorsAsync() // Parameters removed
        {
            _logger.LogInformation("Fetching all available doctors."); // Updated log message
            try
            {
                // Base query: Users who have a DoctorProfile and are marked as verified.
                var query = _context.Users
                    .Include(u => u.DoctorProfile)
                    .Where(u => u.DoctorProfile != null && u.DoctorProfile.IsVerified);

               

                // Project the results into the DoctorSummaryDto
                var doctorsList = await query
                    .Select(u => new DoctorSummaryDto
                    {
                        DoctorUserId = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Specialization = u.DoctorProfile!.Specialization
                    })
                    .OrderBy(d => d.LastName).ThenBy(d => d.FirstName) // Keep ordering
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Found {Count} available doctors.", doctorsList.Count); // Updated log message
                return doctorsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available doctors.");
                return Enumerable.Empty<DoctorSummaryDto>();
            }
        }

        public async Task<DoctorProfileDto?> GetDoctorProfileByIdAsync(string doctorId)
        {
            _logger.LogInformation("Attempting to retrieve profile for doctor with User ID: {DoctorId}", doctorId);
            try
            {
                // Find the user, ensure they have a DoctorProfile (verifying they are indeed a doctor)
                var userWithProfile = await _context.Users
                    .Include(u => u.DoctorProfile) // Eager load profile data
                    .AsNoTracking() // Read-only operation
                    .FirstOrDefaultAsync(u => u.Id == doctorId && u.DoctorProfile != null); // Find by ID and ensure they ARE a doctor

                if (userWithProfile == null)
                {
                    // User not found, or the User ID belongs to someone without a DoctorProfile (e.g., a patient)
                    _logger.LogWarning("Doctor profile not found for User ID {DoctorId}, or user is not a doctor.", doctorId);
                    return null; // Indicate not found
                }

                // Map the data to the DTO
                // Consider if you want a different DTO for public view vs. self view (e.g., maybe hide LicenseNumber here)
                // For now, we reuse DoctorProfileDto
                var profileDto = new DoctorProfileDto
                {
                    UserId = userWithProfile.Id,
                    Email = userWithProfile.Email ?? "N/A", // Maybe hide email in public view?
                    FirstName = userWithProfile.FirstName,
                    LastName = userWithProfile.LastName,
                    Specialization = userWithProfile.DoctorProfile!.Specialization,
                    LicenseNumber = "********", // Example: Hide sensitive data for public view
                    YearsOfExperience = userWithProfile.DoctorProfile!.YearsOfExperience,
                    ClinicAddress = userWithProfile.DoctorProfile!.ClinicAddress,
                    ProfessionalBio = userWithProfile.DoctorProfile!.ProfessionalBio,
                    IsVerified = userWithProfile.DoctorProfile!.IsVerified
                };

                _logger.LogInformation("Successfully retrieved profile for doctor User ID: {DoctorId}", doctorId);
                return profileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving doctor profile for User ID {DoctorId}", doctorId);
                return null; // Return null on error
            }
        }
        // --- Interface Implementations ---

        /// <summary>
        /// Gets the profile DTO for the currently logged-in doctor.
        /// </summary>
        public async Task<DoctorProfileDto?> GetMyProfileAsync(string doctorUserId)
        {
            _logger.LogInformation("Attempting to retrieve profile for doctor User ID: {UserId}", doctorUserId);
            try
            {
                // Query the User AND their DoctorProfile together efficiently
                var userWithProfile = await _context.Users
                    .Include(u => u.DoctorProfile) // Eager load the related profile data
                    .AsNoTracking() // Read-only query
                    .FirstOrDefaultAsync(u => u.Id == doctorUserId && u.DoctorProfile != null); // Ensure it's the correct user AND they have a doctor profile

                if (userWithProfile == null)
                {
                    // Handles invalid user ID or user is not a doctor
                    _logger.LogWarning("Doctor profile not found for User ID {UserId}, or user is not a doctor.", doctorUserId);
                    return null;
                }

                // Map the retrieved data to the DTO
                var profileDto = new DoctorProfileDto
                {
                    UserId = userWithProfile.Id,
                    Email = userWithProfile.Email ?? "N/A",
                    FirstName = userWithProfile.FirstName,
                    LastName = userWithProfile.LastName,
                    // Use the null-forgiving operator (!) as we checked DoctorProfile != null
                    Specialization = userWithProfile.DoctorProfile!.Specialization,
                    LicenseNumber = userWithProfile.DoctorProfile!.LicenseNumber, // Consider if this should always be returned
                    YearsOfExperience = userWithProfile.DoctorProfile!.YearsOfExperience,
                    ClinicAddress = userWithProfile.DoctorProfile!.ClinicAddress,
                    ProfessionalBio = userWithProfile.DoctorProfile!.ProfessionalBio,
                    IsVerified = userWithProfile.DoctorProfile!.IsVerified
                };

                _logger.LogInformation("Successfully retrieved profile for doctor User ID: {UserId}", doctorUserId);
                return profileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving doctor profile for User ID {UserId}", doctorUserId);
                return null; // Return null on error
            }
        }

        /// <summary>
        /// Updates the profile for the currently logged-in doctor.
        /// </summary>
        public async Task<ResultDto> UpdateMyProfileAsync(string doctorUserId, UpdateDoctorProfileDto updateDto)
        {
            _logger.LogInformation("Attempting to update profile for doctor User ID: {UserId}", doctorUserId);
            try
            {
                // Retrieve the specific DoctorProfile entity linked to the user ID for updating
                var doctorProfile = await _context.DoctorProfiles
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == doctorUserId);

                if (doctorProfile == null)
                {
                    _logger.LogWarning("Update failed: Doctor profile not found for User ID {UserId}", doctorUserId);
                    return ResultDto.Fail("Doctor profile not found.");
                }

                // Apply updates from the DTO to the tracked entity
                // Only update fields included in the UpdateDoctorProfileDto
                doctorProfile.ClinicAddress = updateDto.ClinicAddress;
                doctorProfile.ProfessionalBio = updateDto.ProfessionalBio;
                if (updateDto.YearsOfExperience.HasValue) // Check if the DTO provided a value
                {
                    doctorProfile.YearsOfExperience = updateDto.YearsOfExperience.Value;
                }
                // Note: Updating Specialization, LicenseNumber, IsVerified might require admin roles/workflows

                _context.Entry(doctorProfile).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Doctor profile updated successfully for User ID {UserId}", doctorUserId);
                return ResultDto.Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating doctor profile for User ID {UserId}.", doctorUserId);
                return ResultDto.Fail("Failed to update profile due to a concurrency conflict. Please refresh and try again.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating doctor profile for User ID {UserId}", doctorUserId);
                return ResultDto.Fail("Failed to update profile due to a database error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating doctor profile for User ID {UserId}", doctorUserId);
                return ResultDto.Fail("An unexpected error occurred.");
            }
        }
    }
}
