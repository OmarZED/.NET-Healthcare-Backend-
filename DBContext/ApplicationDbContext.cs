using medical.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace medical.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor that accepts DbContextOptions, allowing configuration (like connection string)
        // to be passed in via dependency injection.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for each of your custom models.
        // These tell EF Core which classes map to database tables.
        // The base IdentityDbContext already includes DbSets for Users, Roles, etc.
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Message> Messages { get; set; }

        // Override OnModelCreating to configure relationships and constraints using Fluent API
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // IMPORTANT: Always call the base method first!
            // This ensures the Identity models are configured correctly.
            base.OnModelCreating(builder);

            // --- Configure ApplicationUser Relationships (One-to-One with Profiles) ---

            // An ApplicationUser has one optional PatientProfile
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.PatientProfile)
                .WithOne(p => p.ApplicationUser)
                .HasForeignKey<PatientProfile>(p => p.ApplicationUserId)
                .IsRequired(false) // Explicitly state the profile is optional for a user
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their patient profile

            // An ApplicationUser has one optional DoctorProfile
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.DoctorProfile)
                .WithOne(d => d.ApplicationUser)
                .HasForeignKey<DoctorProfile>(d => d.ApplicationUserId)
                .IsRequired(false) // Explicitly state the profile is optional for a user
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their doctor profile


            // --- Configure Appointment Relationships (Many-to-One with User for Patient/Doctor) ---

            // An Appointment has one Patient (User)
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient) // Navigation property in Appointment
                .WithMany(u => u.PatientAppointments) // Collection navigation property in ApplicationUser
                .HasForeignKey(a => a.PatientId) // Foreign key in Appointment
                .IsRequired() // An appointment must have a patient
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a User if they have Appointments as Patient
                                                    // Or consider Cascade if business logic allows deleting appointments when user is deleted

            // An Appointment has one Doctor (User)
            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor) // Navigation property in Appointment
                .WithMany(u => u.DoctorAppointments) // Collection navigation property in ApplicationUser
                .HasForeignKey(a => a.DoctorId) // Foreign key in Appointment
                .IsRequired() // An appointment must have a doctor
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a User if they have Appointments as Doctor
                                                    // Note: Using Restrict on both PatientId and DoctorId might be strict.
                                                    // If a user can be both, deleting them might be complex.
                                                    // Careful consideration of cascade paths is needed. Restrict is often safer initially.

            // --- Configure Message Relationships (Many-to-One with User for Sender/Receiver) ---

            // A Message has one Sender (User)
            builder.Entity<Message>()
                .HasOne(m => m.Sender) // Navigation property in Message
                .WithMany(u => u.SentMessages) // Collection navigation property in ApplicationUser
                .HasForeignKey(m => m.SenderId) // Foreign key in Message
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if they sent messages. Alternative: Cascade.

            // A Message has one Receiver (User)
            builder.Entity<Message>()
                .HasOne(m => m.Receiver) // Navigation property in Message
                .WithMany(u => u.ReceivedMessages) // Collection navigation property in ApplicationUser
                .HasForeignKey(m => m.ReceiverId) // Foreign key in Message
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if they received messages. Alternative: Cascade.

            // --- Configure Indexes (Optional but Recommended for Performance) ---

            // Index for faster lookup of appointments by date
            builder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDateTime);

            // Index for finding doctor appointments by status and date
            builder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.Status, a.AppointmentDateTime });

            // Index for finding patient appointments by status and date
            builder.Entity<Appointment>()
                .HasIndex(a => new { a.PatientId, a.Status, a.AppointmentDateTime });

            // Index for efficiently querying messages for a receiver, especially unread ones ordered by time
            builder.Entity<Message>()
                 .HasIndex(m => new { m.ReceiverId, m.IsRead, m.SentAt }); // Useful for "get my unread messages"

            // Index for efficiently querying the conversation between two users
            builder.Entity<Message>()
                 .HasIndex(m => new { m.SenderId, m.ReceiverId, m.SentAt }); // Useful for loading chat history

            // --- Other Configurations (Example: Table Names - Optional) ---
            // builder.Entity<Appointment>().ToTable("PatientAppointments"); // If you want custom table names

        }
    }
}
