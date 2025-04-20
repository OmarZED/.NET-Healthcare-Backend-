using medical.Dto.Patient;
using medical.Dto.Shared;

namespace medical.Interface
{
    public interface IPatientProfileService
    {
        /// <summary>
        /// Gets the profile DTO for the specified patient user ID.
        /// </summary>
        /// <param name="patientUserId">The ID of the patient whose profile is requested.</param>
        /// <returns>The patient profile DTO, or null if not found.</returns>
        Task<PatientProfileDto?> GetMyProfileAsync(string patientUserId);

        /// <summary>
        /// Updates the profile for the specified patient user ID using the provided data.
        /// </summary>
        /// <param name="patientUserId">The ID of the patient whose profile is being updated.</param>
        /// <param name="updateDto">The DTO containing the fields to update.</param>
        /// <returns>A result indicating success or failure.</returns>
        Task<ResultDto> UpdateMyProfileAsync(string patientUserId, UpdatePatientProfileDto updateDto);
    }
}
