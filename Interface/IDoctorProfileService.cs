using medical.Dto.Doctor;
using medical.Dto.Shared;

namespace medical.Interface
{
    /// <summary>
    /// Manages retrieval and updates for a doctor's own profile.
    /// </summary>
    public interface IDoctorProfileService
    {
        /// <summary>
        /// Gets the profile DTO for the specified doctor user ID.
        /// </summary>
        /// <param name="doctorUserId">The ID of the doctor whose profile is requested.</param>
        /// <returns>The doctor profile DTO, or null if not found.</returns>
        Task<DoctorProfileDto?> GetMyProfileAsync(string doctorUserId);

        /// <summary>
        /// Updates the profile for the specified doctor user ID using the provided data.
        /// </summary>
        /// <param name="doctorUserId">The ID of the doctor whose profile is being updated.</param>
        /// <param name="updateDto">The DTO containing the fields to update.</param>
        /// <returns>A result indicating success or failure.</returns>
        Task<ResultDto> UpdateMyProfileAsync(string doctorUserId, UpdateDoctorProfileDto updateDto);

        Task<IEnumerable<DoctorSummaryDto>> GetAvailableDoctorsAsync();

        Task<DoctorProfileDto?> GetDoctorProfileByIdAsync(string doctorId);
    }
}
