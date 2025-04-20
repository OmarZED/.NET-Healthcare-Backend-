using medical.DBContext;
using medical.Dto.Patient;
using medical.Dto.Shared;
using medical.Interface;
using Microsoft.EntityFrameworkCore;

namespace medical.Reposotiry
{
    // Implement the specific interface for patient profiles
    public class PatientProfileService : IPatientProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientProfileService> _logger;
       

        public PatientProfileService(ApplicationDbContext context, ILogger<PatientProfileService> logger )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
          
        }

        // --- Interface Implementations ---

        /// <summary>
        /// Gets the profile DTO for the currently logged-in patient.
        /// </summary>
        public async Task<PatientProfileDto?> GetMyProfileAsync(string patientUserId)
        {
            _logger.LogInformation("Attempting to retrieve profile for patient User ID: {UserId}", patientUserId);
            try
            {
                // Query the User AND their PatientProfile together efficiently
                var userWithProfile = await _context.Users
                    .Include(u => u.PatientProfile) // Eager load the related profile data
                    .AsNoTracking() // Read-only query, improves performance
                    .FirstOrDefaultAsync(u => u.Id == patientUserId && u.PatientProfile != null); // Ensure it's the correct user AND they have a patient profile

                if (userWithProfile == null)
                {
                    // This can happen if the user ID is invalid OR if the user exists but doesn't have a PatientProfile (e.g., they are a doctor)
                    _logger.LogWarning("Patient profile not found for User ID {UserId}, or user is not a patient.", patientUserId);
                    return null;
                }

                // Map the retrieved data to the DTO
                var profileDto = new PatientProfileDto
                {
                    UserId = userWithProfile.Id,
                    Email = userWithProfile.Email ?? "N/A", // Handle potential null email
                    FirstName = userWithProfile.FirstName,
                    LastName = userWithProfile.LastName,
                    // Use the null-forgiving operator (!) because we explicitly checked PatientProfile != null in the query
                    DateOfBirth = userWithProfile.PatientProfile!.DateOfBirth,
                    Address = userWithProfile.PatientProfile!.Address,
                    MedicalHistorySummary = userWithProfile.PatientProfile!.MedicalHistorySummary,
                    Allergies = userWithProfile.PatientProfile!.Allergies,
                    CurrentMedications = userWithProfile.PatientProfile!.CurrentMedications
                };

                _logger.LogInformation("Successfully retrieved profile for patient User ID: {UserId}", patientUserId);
                return profileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving patient profile for User ID {UserId}", patientUserId);
                return null; // Return null on error; controller will handle response
            }
        }

        /// <summary>
        /// Updates the profile for the currently logged-in patient.
        /// </summary>
        public async Task<ResultDto> UpdateMyProfileAsync(string patientUserId, UpdatePatientProfileDto updateDto)
        {
            _logger.LogInformation("Attempting to update profile for patient User ID: {UserId}", patientUserId);
            try
            {
                // Retrieve the specific PatientProfile entity linked to the user ID
                // We need to track this entity for updates, so no AsNoTracking() here.
                var patientProfile = await _context.PatientProfiles
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == patientUserId);

                if (patientProfile == null)
                {
                    _logger.LogWarning("Update failed: Patient profile not found for User ID {UserId}", patientUserId);
                    return ResultDto.Fail("Patient profile not found.");
                }

                // Apply updates from the DTO to the tracked entity
                patientProfile.Address = updateDto.Address; // Assumes null from DTO means "no change" or "clear field" based on your logic
                patientProfile.MedicalHistorySummary = updateDto.MedicalHistorySummary;
                patientProfile.Allergies = updateDto.Allergies;
                patientProfile.CurrentMedications = updateDto.CurrentMedications;

              

                // Mark the profile entity as modified (often handled implicitly, but explicit is clear)
                _context.Entry(patientProfile).State = EntityState.Modified;

                // Save changes to the database
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient profile updated successfully for User ID {UserId}", patientUserId);
                return ResultDto.Ok(); // Indicate success
            }
            catch (DbUpdateConcurrencyException ex) // Specific exception for concurrency issues
            {
                _logger.LogError(ex, "Concurrency error updating patient profile for User ID {UserId}. The record might have been modified or deleted.", patientUserId);
                return ResultDto.Fail("Failed to update profile due to a concurrency conflict. Please refresh and try again.");
            }
            catch (DbUpdateException ex) // Catch general database update errors
            {
                _logger.LogError(ex, "Database error updating patient profile for User ID {UserId}", patientUserId);
                return ResultDto.Fail("Failed to update profile due to a database error.");
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                _logger.LogError(ex, "An unexpected error occurred while updating patient profile for User ID {UserId}", patientUserId);
                return ResultDto.Fail("An unexpected error occurred.");
            }
        }
    }
}
