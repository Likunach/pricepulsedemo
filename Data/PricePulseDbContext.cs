using Microsoft.EntityFrameworkCore;
using PricePulse.Models;

namespace PricePulse.Data
{
    public class PricePulseDbContext : DbContext
    {
        public PricePulseDbContext(DbContextOptions<PricePulseDbContext> options) : base(options)
        {
        }

        // DbSets for all 13 tables
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<Competitor> Competitors { get; set; }
        public DbSet<CompetitorProduct> CompetitorProducts { get; set; }
        public DbSet<CompetitorProductAnalysis> CompetitorProductAnalyses { get; set; }
        public DbSet<CompetitorProductRetailer> CompetitorProductRetailers { get; set; }
        public DbSet<OwnProduct> OwnProducts { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Authentication> Authentications { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<RegistrationVerification> RegistrationVerifications { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
                entity.Property(e => e.LastLoginDate).HasColumnName("last_login_date");
                entity.Property(e => e.AccountStatus).HasColumnName("account_status");
                
                entity.HasIndex(e => e.Email).IsUnique();

                // One-to-many: User has many CompanyProfiles
                entity
                    .HasMany(u => u.CompanyProfiles)
                    .WithOne(cp => cp.User)
                    .HasForeignKey(cp => cp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Profile entity
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.ToTable("profiles");
                entity.HasKey(e => e.ProfileId);
                entity.Property(e => e.ProfileId).HasColumnName("profile_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FirstName).HasColumnName("first_name");
                entity.Property(e => e.LastName).HasColumnName("last_name");
                entity.Property(e => e.Bio).HasColumnName("bio");

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<Profile>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CompanyProfile entity
            modelBuilder.Entity<CompanyProfile>(entity =>
            {
                entity.ToTable("company_profiles");
                entity.HasKey(e => e.CompanyProfileId);
                entity.Property(e => e.CompanyProfileId).HasColumnName("company_profile_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.CompanyName).HasColumnName("company_name");
                entity.Property(e => e.CompanyProfileName).HasColumnName("company_profile");
                entity.Property(e => e.Summary).HasColumnName("summary");
                entity.Property(e => e.CompanyWebsite).HasColumnName("company_website");
                entity.Property(e => e.Industry).HasColumnName("industry");
            });

            // Configure Address entity
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("addresses");
                entity.HasKey(e => e.AddressId);
                entity.Property(e => e.AddressId).HasColumnName("address_id");
                entity.Property(e => e.CompanyProfileId).HasColumnName("company_profile_id");
                entity.Property(e => e.AddressLine1).HasColumnName("address_line1");
                entity.Property(e => e.AddressLine2).HasColumnName("address_line2");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.State).HasColumnName("state");
                entity.Property(e => e.PostalCode).HasColumnName("postal_code");
                entity.Property(e => e.Country).HasColumnName("country");

                entity.HasOne(a => a.CompanyProfile)
                      .WithOne(cp => cp.Address)
                      .HasForeignKey<Address>(a => a.CompanyProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Competitor entity
            modelBuilder.Entity<Competitor>(entity =>
            {
                entity.ToTable("competitors");
                entity.HasKey(e => e.CompetitorId);
                entity.Property(e => e.CompetitorId).HasColumnName("competitor_id");
                entity.Property(e => e.CompanyProfileId).HasColumnName("company_profile_id");
                entity.Property(e => e.CompetitorName).HasColumnName("competitor_name");
                entity.Property(e => e.CompetitorCompanyProfile).HasColumnName("company_profile");
                entity.Property(e => e.CompetitorWebsite).HasColumnName("competitor_website");

                entity.HasOne(c => c.CompanyProfile)
                      .WithMany(cp => cp.Competitors)
                      .HasForeignKey(c => c.CompanyProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OwnProduct entity
            modelBuilder.Entity<OwnProduct>(entity =>
            {
                entity.ToTable("own_products");
                entity.HasKey(e => e.OwnProductId);
                entity.Property(e => e.OwnProductId).HasColumnName("own_product_id");
                entity.Property(e => e.CompanyProfileId).HasColumnName("company_profile_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ProductName).HasColumnName("product_name");
                entity.Property(e => e.ProductWebsiteUrl).HasColumnName("product_website_url");

                entity.HasOne(op => op.CompanyProfile)
                      .WithMany(cp => cp.OwnProducts)
                      .HasForeignKey(op => op.CompanyProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(op => op.User)
                      .WithMany(u => u.OwnProducts)
                      .HasForeignKey(op => op.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CompetitorProduct entity
            modelBuilder.Entity<CompetitorProduct>(entity =>
            {
                entity.ToTable("competitor_products");
                entity.HasKey(e => e.CompProductId);
                entity.Property(e => e.CompProductId).HasColumnName("comp_product_id");
                entity.Property(e => e.OwnProductId).HasColumnName("own_product_id");
                entity.Property(e => e.CompetitorId).HasColumnName("competitor_id");
                entity.Property(e => e.CProductName).HasColumnName("c_product_name");
                entity.Property(e => e.CProductWebsiteUrl).HasColumnName("c_product_website_url");

                entity.HasOne(cp => cp.OwnProduct)
                      .WithMany(op => op.CompetitorProducts)
                      .HasForeignKey(cp => cp.OwnProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cp => cp.Competitor)
                      .WithMany(c => c.CompetitorProducts)
                      .HasForeignKey(cp => cp.CompetitorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Price entity
            modelBuilder.Entity<Price>(entity =>
            {
                entity.ToTable("prices");
                entity.HasKey(e => e.PriceId);
                entity.Property(e => e.PriceId).HasColumnName("price_id");
                entity.Property(e => e.OwnProductId).HasColumnName("own_product_id");
                entity.Property(e => e.CompProductId).HasColumnName("comp_product_id");
                entity.Property(e => e.PriceValue).HasColumnName("price");
                entity.Property(e => e.PriceDate).HasColumnName("price_date");

                entity.HasOne(p => p.OwnProduct)
                      .WithMany(op => op.Prices)
                      .HasForeignKey(p => p.OwnProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.CompetitorProduct)
                      .WithMany(cp => cp.Prices)
                      .HasForeignKey(p => p.CompProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Contact entity
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("contacts");
                entity.HasKey(e => e.ContactId);
                entity.Property(e => e.ContactId).HasColumnName("contact_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.SecondaryEmail).HasColumnName("secondary_email");
                entity.Property(e => e.PreferredContactMethod).HasColumnName("preferred_contact_method");

                entity.HasOne(c => c.User)
                      .WithOne(u => u.Contact)
                      .HasForeignKey<Contact>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Authentication entity
            modelBuilder.Entity<Authentication>(entity =>
            {
                entity.ToTable("authentications");
                entity.HasKey(e => e.AuthId);
                entity.Property(e => e.AuthId).HasColumnName("auth_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
                entity.Property(e => e.RecoveryEmail).HasColumnName("recovery_email");
                entity.Property(e => e.SecurityQuestions).HasColumnName("security_questions");
                entity.Property(e => e.LastPasswordChange).HasColumnName("last_password_change");

                entity.HasOne(a => a.User)
                      .WithOne(u => u.Authentication)
                      .HasForeignKey<Authentication>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Session entity
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("sessions");
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.SessionId).HasColumnName("session_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.LoginTimestamp).HasColumnName("login_timestamp");
                entity.Property(e => e.ExpiryTimestamp).HasColumnName("expiry_timestamp");
                entity.Property(e => e.IpAddress).HasColumnName("ip_address");
                entity.Property(e => e.DeviceInfo).HasColumnName("device_info");

                entity.HasOne(s => s.User)
                      .WithMany(u => u.Sessions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RegistrationVerification entity
            modelBuilder.Entity<RegistrationVerification>(entity =>
            {
                entity.ToTable("registration_verifications");
                entity.HasKey(e => e.VerificationId);
                entity.Property(e => e.VerificationId).HasColumnName("verification_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.VerificationToken).HasColumnName("verification_token");
                entity.Property(e => e.TokenExpiry).HasColumnName("token_expiry");
                entity.Property(e => e.VerificationStatus).HasColumnName("verification_status");

                entity.HasOne(rv => rv.User)
                      .WithMany(u => u.RegistrationVerifications)
                      .HasForeignKey(rv => rv.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(e => e.UserRoleId);
                entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.RoleName).HasColumnName("role_name");
                entity.Property(e => e.AssignedDate).HasColumnName("assigned_date");

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CompetitorProductAnalysis entity (enhanced version)
            modelBuilder.Entity<CompetitorProductAnalysis>(entity =>
            {
                entity.ToTable("competitor_product_analyses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CompetitorDomain).HasColumnName("competitor_domain");
                entity.Property(e => e.ProductName).HasColumnName("product_name");
                entity.Property(e => e.ProductDescription).HasColumnName("product_description");
                entity.Property(e => e.CompetitorPrice).HasColumnName("competitor_price");
                entity.Property(e => e.CompetitorCurrency).HasColumnName("competitor_currency");
                entity.Property(e => e.ProductCategory).HasColumnName("product_category");
                entity.Property(e => e.ProductImageUrl).HasColumnName("product_image_url");
                entity.Property(e => e.CompetitorProductUrl).HasColumnName("competitor_product_url");
                entity.Property(e => e.DiscoveredAt).HasColumnName("discovered_at");
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.HasMany(cp => cp.Retailers)
                      .WithOne(r => r.CompetitorProductAnalysis)
                      .HasForeignKey(r => r.CompetitorProductAnalysisId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CompetitorProductRetailer entity
            modelBuilder.Entity<CompetitorProductRetailer>(entity =>
            {
                entity.ToTable("competitor_product_retailers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CompetitorProductAnalysisId).HasColumnName("competitor_product_analysis_id");
                entity.Property(e => e.RetailerName).HasColumnName("retailer_name");
                entity.Property(e => e.RetailerUrl).HasColumnName("retailer_url");
                entity.Property(e => e.ProductUrl).HasColumnName("product_url");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Currency).HasColumnName("currency");
                entity.Property(e => e.Availability).HasColumnName("availability");
                entity.Property(e => e.ShippingInfo).HasColumnName("shipping_info");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.ReviewsCount).HasColumnName("reviews_count");
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
            });
        }
    }
}