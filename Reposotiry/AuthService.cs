using medical.DBContext;
using medical.Dto.Auth;
using medical.Dto.Shared;
using medical.Interface;
using medical.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace medical.Reposotiry
{
    public class AuthService : IAuthService
    {
        // Injected dependencies
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context; // To save profiles simultaneously
        private readonly ILogger<AuthService> _logger;

        // Role Constants - prevents typos
        private const string PatientRole = "Patient";
        private const string DoctorRole = "Doctor";

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context, // Inject DbContext
            ILogger<AuthService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Add null check
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- Interface Implementations ---

        public async Task<ResultDto<AuthResponseDto>> RegisterPatientAsync(RegisterPatientDto registerDto)
        {
            _logger.LogInformation("Attempting to register patient with email {Email}", registerDto.Email);
            return await RegisterUserAsync(
                registerDto.Email,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName,
                PatientRole, // Pass the role name
                () => new PatientProfile // Pass a function that creates the PatientProfile
                {
                    DateOfBirth = registerDto.DateOfBirth,
                    Address = registerDto.Address
                    // Initialize other default patient fields here if any
                });
        }

        public async Task<ResultDto<AuthResponseDto>> RegisterDoctorAsync(RegisterDoctorDto registerDto)
        {
            _logger.LogInformation("Attempting to register doctor with email {Email}", registerDto.Email);
            return await RegisterUserAsync(
                registerDto.Email,
                registerDto.Password,
                registerDto.FirstName,
                registerDto.LastName,
                DoctorRole, // Pass the role name
                () => new DoctorProfile // Pass a function that creates the DoctorProfile
                {
                    Specialization = registerDto.Specialization,
                    LicenseNumber = registerDto.LicenseNumber,
                    YearsOfExperience = registerDto.YearsOfExperience,
                    IsVerified = false // Doctors usually start unverified
                    // Initialize other default doctor fields here if any
                });
        }

        public async Task<ResultDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Attempting login for user {Email}", loginDto.Email);
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User with email {Email} not found.", loginDto.Email);
                    return ResultDto<AuthResponseDto>.Fail("Invalid email or password.");
                }

                // Use SignInManager for password check (handles lockout etc.)
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login failed for user {Email}: Invalid password.", loginDto.Email);
                    return ResultDto<AuthResponseDto>.Fail("Invalid email or password.");
                }

                _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);

                // Generate token and response DTO
                var tokenInfo = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);

                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    Token = tokenInfo.Token,
                    ExpiresAt = tokenInfo.ExpiresAt
                };

                return ResultDto<AuthResponseDto>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for email {Email}.", loginDto.Email);
                return ResultDto<AuthResponseDto>.Fail("An internal error occurred. Please try again later.");
            }
        }


        // --- Private Helper Methods ---

        /// <summary>
        /// Generic helper to register a user, assign a role, and create a profile.
        /// </summary>
        private async Task<ResultDto<AuthResponseDto>> RegisterUserAsync<TProfile>(
            string email, string password, string firstName, string lastName,
            string roleName, Func<TProfile> createProfileFunc) where TProfile : class // TProfile must be a class (PatientProfile or DoctorProfile)
        {
            // Use a transaction to ensure atomicity (User, Role, Profile are created together or not at all)
            // Note: EF Core might implicitly create a transaction for SaveChangesAsync, but explicit is safer for multiple operations.
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already in use.", email);
                    return ResultDto<AuthResponseDto>.Fail("Email is already registered.");
                }

                // 2. Ensure the role exists (critical before assigning)
                await EnsureRoleExists(roleName); // Throws if role creation fails

                // 3. Create the ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = email, // Default UserName to Email
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // Set to true for simplicity, implement confirmation later if needed
                };

                // 4. Create the user identity
                var identityResult = await _userManager.CreateAsync(user, password);
                if (!identityResult.Succeeded)
                {
                    var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for {Email}. Errors: {Errors}", email, errors);
                    await transaction.RollbackAsync(); // Rollback on failure
                    return ResultDto<AuthResponseDto>.Fail($"Registration failed: {errors}");
                }
                _logger.LogInformation("User identity created for {Email} with ID {UserId}.", email, user.Id);

                // 5. Add the user to the specified role
                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add user {Email} to role {RoleName}. Errors: {Errors}", email, roleName, errors);
                    await transaction.RollbackAsync(); // Rollback on failure
                    // Potentially delete the created user here for cleanup, but rollback handles the DB state.
                    return ResultDto<AuthResponseDto>.Fail($"Failed to assign role: {errors}");
                }
                _logger.LogInformation("User {Email} added to role {RoleName}.", email, roleName);

                // 6. Create and save the specific profile (Patient or Doctor)
                TProfile profile = createProfileFunc();

                // Find and set the ApplicationUserId property on the profile instance
                var userIdProperty = typeof(TProfile).GetProperty("ApplicationUserId");
                if (userIdProperty == null || userIdProperty.PropertyType != typeof(string))
                {
                    _logger.LogError("Profile type {ProfileType} does not have a suitable 'ApplicationUserId' string property.", typeof(TProfile).Name);
                    await transaction.RollbackAsync();
                    return ResultDto<AuthResponseDto>.Fail("Internal error creating profile link.");
                }
                userIdProperty.SetValue(profile, user.Id);

                // Add the profile to the DbContext
                _context.Add(profile);
                await _context.SaveChangesAsync(); // Save the profile within the transaction
                _logger.LogInformation("Profile of type {ProfileType} created and saved for user {Email}.", typeof(TProfile).Name, email);

                // 7. Generate JWT Token
                var tokenInfo = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user); // Get roles again to be sure

                // 8. Commit Transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Registration transaction committed successfully for {Email}", email);

                // 9. Prepare and return success response
                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    Token = tokenInfo.Token,
                    ExpiresAt = tokenInfo.ExpiresAt
                };
                return ResultDto<AuthResponseDto>.Ok(response);
            }
            catch (Exception ex) // Catch broader exceptions (including potential EnsureRoleExists failure)
            {
                _logger.LogError(ex, "An unexpected error occurred during registration transaction for email {Email}.", email);
                await transaction.RollbackAsync(); // Ensure rollback on any exception
                return ResultDto<AuthResponseDto>.Fail("An internal error occurred during registration.");
            }
        }

        /// <summary>
        /// Ensures a role exists in the database, creating it if necessary.
        /// Throws an exception if role creation fails.
        /// </summary>
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogInformation("Role '{RoleName}' does not exist. Creating it.", roleName);
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create role '{RoleName}'. Errors: {Errors}", roleName, errors);
                    // This is critical, so throw an exception to halt registration
                    throw new Exception($"Failed to create essential role: {roleName}. Errors: {errors}");
                }
                _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
            }
        }


        /// <summary>
        /// Generates a JWT token for the given user.
        /// </summary>
        private async Task<(string Token, DateTime ExpiresAt)> GenerateJwtToken(ApplicationUser user)
        {
            // Retrieve JWT settings (handle potential nulls gracefully)
            var issuer = _configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer configuration missing.");
            var audience = _configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience configuration missing.");
            var signingKey = _configuration["JWT:SigningKey"] ?? throw new InvalidOperationException("JWT:SigningKey configuration missing.");
            var durationInMinutes = _configuration.GetValue<int>("JWT:DurationInMinutes", 60); // Default 60 mins

            if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32) // Check for empty/short key
            {
                _logger.LogError("JWT:SigningKey is missing, empty, or insecurely short.");
                throw new InvalidOperationException("JWT SigningKey is not configured properly or is insecure.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            // Define claims for the token payload
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),             // Subject (User ID) - Standard
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),   // Email - Standard
                new Claim(ClaimTypes.NameIdentifier, user.Id),                // User ID - ASP.NET Core Identity standard
                new Claim(ClaimTypes.Name, user.UserName ?? ""),              // Username (often same as email) - ASP.NET Core Identity standard
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName), // First Name - Standard
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName), // Last Name - Standard
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID - Standard, prevents replay attacks
            };

            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Role - ASP.NET Core Identity standard
            }

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(durationInMinutes);

            // Create the token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            // Generate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogDebug("JWT token generated for user {UserId} expiring at {ExpiryTime}", user.Id, expires);
            return (tokenHandler.WriteToken(token), expires);
        }
    }
}