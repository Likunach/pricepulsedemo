using Microsoft.AspNetCore.Mvc;
using PricePulse.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;
using PricePulse.ViewModels;

namespace PricePulse.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserDAO _userDAO;
        private readonly IProfileDAO _profileDAO;
        private readonly ICompanyProfileDAO _companyDAO;
        private readonly IAddressDAO _addressDAO;
        private readonly IContactDAO _contactDAO;

        public AccountController(IRegistrationService registrationService, ILogger<AccountController> logger, IUserDAO userDAO, IProfileDAO profileDAO, ICompanyProfileDAO companyDAO, IAddressDAO addressDAO, IContactDAO contactDAO)
        {
            _registrationService = registrationService;
            _logger = logger;
            _userDAO = userDAO;
            _profileDAO = profileDAO;
            _companyDAO = companyDAO;
            _addressDAO = addressDAO;
            _contactDAO = contactDAO;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string email, string secondaryEmail, string phoneNumber, string firstName, string lastName, string? areaCode)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return View();
                }

                // Normalize and combine area code with phone number if provided
                var normalizedArea = (areaCode ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(normalizedArea) && !normalizedArea.StartsWith("+"))
                {
                    normalizedArea = "+" + normalizedArea;
                }
                var fullPhoneNumber = string.IsNullOrWhiteSpace(normalizedArea)
                    ? phoneNumber
                    : $"{normalizedArea} {phoneNumber}".Trim();

                var verificationToken = await _registrationService.StartRegistrationAsync(
                    firstName, lastName, email, secondaryEmail, fullPhoneNumber);

                ViewBag.Email = email;
                return View("EmailVerification");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(ex, "Account already exists for {Email}", email);
                ViewBag.Email = email;
                TempData["Error"] = "An account with this email already exists. You can resend the verification email below.";
                return View("EmailVerification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for {Email}", email);
                TempData["Error"] = "An error occurred while creating your account. Please try again.";
                return View();
            }
        }

        public IActionResult EmailVerification()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerification(string email)
        {
            try
            {
                var success = await _registrationService.ResendVerificationEmailAsync(email);
                if (success)
                {
                    TempData["Message"] = "Verification email sent successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to send verification email.";
                }

                ViewBag.Email = email;
                return View("EmailVerification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification for {Email}", email);
                TempData["Error"] = "An error occurred. Please try again.";
                ViewBag.Email = email;
                return View("EmailVerification");
            }
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Error"] = "Invalid verification link.";
                    return RedirectToAction("Create");
                }

                var success = await _registrationService.VerifyEmailAsync(token);
                if (success)
                {
                    return RedirectToAction("AccountSetup", new { token = token });
                }
                else
                {
                    TempData["Error"] = "Invalid or expired verification link.";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for token {Token}", token);
                TempData["Error"] = "An error occurred during verification.";
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        public IActionResult AccountSetup(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid session. Please start the registration process again.";
                return RedirectToAction("Create");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AccountSetup(string token, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid session. Please start the registration process again.";
                ViewBag.Token = token;
                return View();
            }

            if (string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match or are empty.";
                ViewBag.Token = token;
                return View();
            }

            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters long.";
                ViewBag.Token = token;
                return View();
            }

            try
            {
                var success = await _registrationService.CompleteAccountSetupAsync(token, password);
                
                if (success)
                {
                    TempData["Message"] = "Account setup completed successfully! Please complete your company information.";
                    return RedirectToAction("CompanyInfo", new { token = token });
                }
                else
                {
                    ViewBag.Error = "Invalid or expired token. Please start the registration process again.";
                    ViewBag.Token = token;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing account setup for token {Token}", token);
                ViewBag.Error = "An error occurred while setting up your account. Please try again.";
                ViewBag.Token = token;
                return View();
            }
        }

        [HttpGet]
        public IActionResult CompanyInfo(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid session. Please start the registration process again.";
                return RedirectToAction("Create");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompanyInfo(string token, string companyName, string industry, string website, string description, string address, string city, string country, string zipCode)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid session. Please start the registration process again.";
                ViewBag.Token = token;
                return View();
            }

            try
            {
                var success = await _registrationService.CreateCompanyProfileAsync(
                    token, companyName, industry, website, description, address, city, country, zipCode);
                
                if (success)
                {
                    TempData["Message"] = "Registration completed successfully! Welcome to PricePulse.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Failed to save company information. Please try again.";
                    ViewBag.Token = token;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving company info for token {Token}", token);
                ViewBag.Error = "An error occurred while saving your company information. Please try again.";
                ViewBag.Token = token;
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required.";
                return View();
            }

            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                TempData["Error"] = "Invalid credentials.";
                return View();
            }

            // Verify password using same hashing as RegistrationService
            var hashed = HashPassword(password);
            if (!string.Equals(hashed, user.PasswordHash, StringComparison.Ordinal))
            {
                TempData["Error"] = "Invalid credentials.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, (await _profileDAO.GetByUserIdAsync(user.UserId))?.FirstName ?? user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

            TempData["Message"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
        {
            // Read external identity from the current HttpContext
            var result = await HttpContext.AuthenticateAsync();
            var principal = result.Principal ?? HttpContext.User;
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
            var name = principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.Identity?.Name ?? "";
            var surname = principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "External provider did not return an email address.";
                return RedirectToAction("Login");
            }

            // Find or create user
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    PasswordHash = string.Empty,
                    RegistrationDate = DateTime.UtcNow,
                    AccountStatus = "Active"
                };
                user = await _userDAO.CreateAsync(user);

                // Create minimal profile
                var profile = new Profile
                {
                    UserId = user.UserId,
                    FirstName = name,
                    LastName = surname
                };
                await _profileDAO.CreateAsync(profile);
            }

            // Issue auth cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, name)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

            return Redirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "PricePulse_Salt");
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        [HttpGet]
        public async Task<IActionResult> Info()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login");
            }

            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var profile = await _profileDAO.GetByUserIdAsync(user.UserId);
            var contact = await _contactDAO.GetByUserIdAsync(user.UserId);

            var vm = new AccountInfoViewModel
            {
                Email = user.Email,
                FirstName = profile?.FirstName ?? string.Empty,
                LastName = profile?.LastName ?? string.Empty,
                Bio = profile?.Bio,
                PhoneNumber = contact?.PhoneNumber,
                SecondaryEmail = contact?.SecondaryEmail,
                AvatarUrl = user.AvatarPath
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInfo(string firstName, string lastName, string bio, string phoneNumber, IFormFile? avatar)
        {
            _logger.LogInformation("SaveInfo method called");
            _logger.LogInformation("Parameters - FirstName: {FirstName}, LastName: {LastName}, Bio: {Bio}, PhoneNumber: {PhoneNumber}", 
                firstName, lastName, bio, phoneNumber);
            _logger.LogInformation("Avatar: {Avatar}", avatar?.FileName ?? "null");
            
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                _logger.LogWarning("User not authenticated");
                return Json(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Update profile
                var profile = await _profileDAO.GetByUserIdAsync(user.UserId) ?? new Profile { UserId = user.UserId };
                profile.FirstName = firstName?.Trim();
                profile.LastName = lastName?.Trim();
                profile.Bio = bio;

                if (profile.ProfileId == 0)
                {
                    await _profileDAO.CreateAsync(profile);
                    _logger.LogInformation("Profile created");
                }
                else
                {
                    await _profileDAO.UpdateAsync(profile);
                    _logger.LogInformation("Profile updated");
                }

                // Update contact
                var contact = await _contactDAO.GetByUserIdAsync(user.UserId) ?? new Contact { UserId = user.UserId };
                contact.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

                if (contact.ContactId == 0)
                {
                    await _contactDAO.CreateAsync(contact);
                    _logger.LogInformation("Contact created");
                }
                else
                {
                    await _contactDAO.UpdateAsync(contact);
                    _logger.LogInformation("Contact updated");
                }

                // Handle avatar upload
                if (avatar != null && avatar.Length > 0)
                {
                    _logger.LogInformation("Processing avatar upload");
                    try
                    {
                        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                        
                        var fileName = $"user_{user.UserId}_{Guid.NewGuid():N}{Path.GetExtension(avatar.FileName)}";
                        var fullPath = Path.Combine(uploadsDir, fileName);
                        
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await avatar.CopyToAsync(stream);
                        }
                        
                        var relativePath = $"/images/avatars/{fileName}";
                        user.AvatarPath = relativePath;
                        await _userDAO.UpdateAsync(user);
                        _logger.LogInformation("Avatar saved: {Path}", relativePath);
                    }
                    catch (Exception avatarEx)
                    {
                        _logger.LogError(avatarEx, "Error saving avatar: {Message}", avatarEx.Message);
                    }
                }

                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile: {Message}", ex.Message);
                return Json(new { success = false, message = "Error saving profile. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestSaveInfo(string firstName, string lastName, string bio, string phoneNumber, IFormFile? avatar)
        {
            _logger.LogInformation("TestSaveInfo method called");
            _logger.LogInformation("Parameters - FirstName: {FirstName}, LastName: {LastName}, Bio: {Bio}, PhoneNumber: {PhoneNumber}", 
                firstName, lastName, bio, phoneNumber);
            _logger.LogInformation("Avatar: {Avatar}", avatar?.FileName ?? "null");
            return Json(new { success = true, message = "Test method called successfully" });
        }

        // Companies management
        [HttpGet]
        public async Task<IActionResult> Companies(int? id, bool edit = false)
        {
            // In a real app, get current user from auth cookie; for now assume single test user by email from TempData or query
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
            ViewBag.Companies = companies;
            if (id.HasValue)
            {
                var editCompany = await _companyDAO.GetCompanyWithFullDetailsAsync(id.Value);
                if (editCompany != null && editCompany.UserId == user.UserId)
                {
                    ViewBag.EditCompany = editCompany;
                    ViewBag.IsEditMode = edit;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(int companyProfileId, string address1, string? address2, string city, string? state, string? postalCode, string? country)
        {
            try
            {
                // Check if company already has an address
                var existingAddress = await _addressDAO.GetByCompanyProfileIdAsync(companyProfileId);
                
                if (existingAddress != null)
                {
                    // Update existing address instead of creating new one
                    existingAddress.AddressLine1 = address1;
                    existingAddress.AddressLine2 = address2;
                    existingAddress.City = city;
                    existingAddress.State = state;
                    existingAddress.PostalCode = postalCode;
                    existingAddress.Country = country;
                    
                    await _addressDAO.UpdateAsync(existingAddress);
                    return Json(new { success = true, message = "Address updated successfully!" });
                }
                else
                {
                    // Create new address
                    var address = new Address
                    {
                        CompanyProfileId = companyProfileId,
                        AddressLine1 = address1,
                        AddressLine2 = address2,
                        City = city,
                        State = state,
                        PostalCode = postalCode,
                        Country = country
                    };

                    await _addressDAO.CreateAsync(address);
                    return Json(new { success = true, message = "Address added successfully!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating address: {Message}", ex.Message);
                return Json(new { success = false, message = "Error saving address. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCompany(int? companyProfileId, string companyName, string? website, string? description, string? industry, string? address1, string? address2, string? city, string? state, string? postalCode, string? country)
        {
            _logger.LogInformation("AddCompany method called");
            _logger.LogInformation("Parameters - CompanyProfileId: {CompanyProfileId}, CompanyName: {CompanyName}, Website: {Website}", 
                companyProfileId, companyName, website);
            
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User not authenticated");
                return RedirectToAction("Login");
            }
            
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return RedirectToAction("Login");
            }
            
            _logger.LogInformation("User found: {UserId}", user.UserId);

            if (companyProfileId.HasValue)
            {
                var existing = await _companyDAO.GetCompanyWithFullDetailsAsync(companyProfileId.Value);
                if (existing == null || existing.UserId != user.UserId)
                {
                    return RedirectToAction("Companies");
                }

                existing.CompanyName = companyName;
                existing.CompanyWebsite = website;
                existing.CompanyDescription = description;
                existing.Industry = industry;
                await _companyDAO.UpdateAsync(existing);

                if (!string.IsNullOrWhiteSpace(address1) || !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(state) || !string.IsNullOrWhiteSpace(postalCode) || !string.IsNullOrWhiteSpace(country))
                {
                    if (existing.Address == null)
                    {
                        var newAddr = new Address
                        {
                            CompanyProfileId = existing.CompanyProfileId,
                            AddressLine1 = address1,
                            AddressLine2 = address2,
                            City = city,
                            State = state,
                            PostalCode = postalCode,
                            Country = country
                        };
                        await _addressDAO.CreateAsync(newAddr);
                    }
                    else
                    {
                        existing.Address.AddressLine1 = address1;
                        existing.Address.AddressLine2 = address2;
                        existing.Address.City = city;
                        existing.Address.State = state;
                        existing.Address.PostalCode = postalCode;
                        existing.Address.Country = country;
                        await _addressDAO.UpdateAsync(existing.Address);
                    }
                }

                return RedirectToAction("Companies");
            }
            else
            {
                try
                {
                    _logger.LogInformation("Creating new company: {CompanyName}", companyName);
                    
                    var company = new CompanyProfile
                    {
                        UserId = user.UserId,
                        CompanyName = companyName,
                        CompanyWebsite = website,
                        CompanyDescription = description,
                        Industry = industry
                    };
                    var created = await _companyDAO.CreateAsync(company);
                    _logger.LogInformation("Company created successfully with ID: {CompanyId}", created.CompanyProfileId);
                    TempData["Success"] = $"Company '{companyName}' created successfully!";

                    if (!string.IsNullOrWhiteSpace(address1) || !string.IsNullOrWhiteSpace(city))
                    {
                        _logger.LogInformation("Creating address for company: {CompanyId}", created.CompanyProfileId);
                        var addr = new Address
                        {
                            CompanyProfileId = created.CompanyProfileId,
                            AddressLine1 = address1,
                            AddressLine2 = address2,
                            City = city,
                            State = state,
                            PostalCode = postalCode,
                            Country = country
                        };
                        await _addressDAO.CreateAsync(addr);
                        _logger.LogInformation("Address created successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating company: {Message}", ex.Message);
                    TempData["Error"] = "Error creating company. Please try again.";
                    return RedirectToAction("Companies");
                }
            }

            _logger.LogInformation("Redirecting to Companies page");
            return RedirectToAction("Companies");
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Login");
        }

        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var profile = await _profileDAO.GetByUserIdAsync(user.UserId);
            var model = new AccountSettingsViewModel
            {
                FirstName = profile?.FirstName ?? string.Empty,
                LastName = profile?.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = string.Empty, // Profile doesn't have PhoneNumber property
                Bio = profile?.Bio ?? string.Empty
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading account settings");
            TempData["Error"] = "An error occurred while loading settings. Please try again.";
            return View(new AccountSettingsViewModel());
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(AccountSettingsViewModel model)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View("Settings", model);
        }

        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var profile = await _profileDAO.GetByUserIdAsync(user.UserId);
            if (profile == null)
            {
                profile = new Profile
                {
                    UserId = user.UserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Bio = model.Bio
                };
                await _profileDAO.CreateAsync(profile);
            }
            else
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.Bio = model.Bio;
                await _profileDAO.UpdateAsync(profile);
            }

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Settings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            TempData["Error"] = "An error occurred while updating your profile. Please try again.";
            return View("Settings", model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Login");
        }

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            TempData["Error"] = "All password fields are required.";
            return RedirectToAction("Info");
        }

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "New passwords do not match.";
            return RedirectToAction("Info");
        }

        if (newPassword.Length < 8)
        {
            TempData["Error"] = "New password must be at least 8 characters long.";
            return RedirectToAction("Info");
        }

        try
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userDAO.GetByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Verify old password
            var oldPasswordHash = HashPassword(oldPassword);
            if (!string.Equals(oldPasswordHash, user.PasswordHash, StringComparison.Ordinal))
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Info");
            }

            // Hash and save new password
            var newPasswordHash = HashPassword(newPassword);
            user.PasswordHash = newPasswordHash;
            await _userDAO.UpdateAsync(user);

            TempData["Message"] = "Password changed successfully! You will be logged out of all other sessions.";
            return RedirectToAction("Info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            TempData["Error"] = "An error occurred while changing your password. Please try again.";
            return RedirectToAction("Info");
        }
    }
}
}
