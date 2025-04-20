using medical.Dto.Auth;
using medical.Dto.Shared;

namespace medical.Interface
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the Patient role and creates their profile.
        /// </summary>
        /// <param name="registerDto">Patient registration details.</param>
        /// <returns>Result containing authentication response (with token) if successful.</returns>
        Task<ResultDto<AuthResponseDto>> RegisterPatientAsync(RegisterPatientDto registerDto);

        /// <summary>
        /// Registers a new user with the Doctor role and creates their profile.
        /// </summary>
        /// <param name="registerDto">Doctor registration details.</param>
        /// <returns>Result containing authentication response (with token) if successful.</returns>
        Task<ResultDto<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorDto registerDto);

        /// <summary>
        /// Authenticates a user based on login credentials.
        /// </summary>
        /// <param name="loginDto">User login credentials.</param>
        /// <returns>Result containing authentication response (with token) if successful.</returns>
        Task<ResultDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    }
}
