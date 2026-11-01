using JobLyzerProject.ViewModels;
using JobLyzerProject_Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JoblyzerProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // --- SHARED LOGIN CORE VIEW ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Authenticate user credentials against the database repository layer
            var user = await _authService.ValidateUserAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                return View(model);
            }

            // 2. Build explicit role-based identity claims (0: Student, 1: Recruiter, 2: Admin)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserType == 0 ? "Student" : (user.UserType == 1 ? "Recruiter" : "Admin"))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 3. Issue the secure browser session tracking cookie ticket
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });

            // 4. Redirect all authenticated entities directly to the landing cockpit
            return RedirectToAction("WelcomeIndex");
        }

        // --- STUDENT SIGNUP FLOW ---
        [HttpGet]
        public IActionResult RegisterStudent() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterStudentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool success = await _authService.RegisterStudentWithProfileAsync(
                model.Email,
                model.Password,
                model.ResumeFilePath,
                model.HighestInstitution,
                model.DegreeSpecialization,
                model.LatestCompany,
                model.LatestRole,
                model.CoreTechnicalSkill,
                model.CoreSkillProficiency
            );

            if (success)
            {
                TempData["SuccessMessage"] = "Student profile registered successfully! Sign in here.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "This email address is already tied to an active candidate record.");
            return View(model);
        }

        // --- RECRUITER SIGNUP FLOW ---
        [HttpGet]
        public IActionResult RegisterRecruiter() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterRecruiter(RegisterRecruiterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool success = await _authService.RegisterRecruiterAsync(
                model.Email,
                model.Password,
                model.CompanyName,
                model.CompanyWebsite,
                model.TeamSizeAllocation
            );

            if (success)
            {
                TempData["SuccessMessage"] = "Corporate Recruiter account registered successfully! Sign in here.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "This corporate email address is already registered.");
            return View(model);
        }

        // --- ADMIN SIGNUP FLOW ---
        [HttpGet]
        public IActionResult RegisterAdmin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminViewModel model)
        {
            // Bypass the security token model field check since we removed it from the UI form view
            ModelState.Remove("AdminSecurityToken");

            if (!ModelState.IsValid) return View(model);

            bool success = await _authService.RegisterAdminAsync(model.Email, model.Password);
            if (success)
            {
                TempData["SuccessMessage"] = "System Administration account provisioned successfully! Sign in here.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "This administrative email address is already registered.");
            return View(model);
        }

        // --- AUTHENTICATION PORTAL LANDING COCKPIT ---
        [HttpGet]
        public IActionResult WelcomeIndex()
        {
            string activeRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "None";

            // Provides a clean, styled landing dashboard frame confirming successful session parsing
            return Content($@"
                <div style='font-family:""Segoe UI"",sans-serif; max-width:600px; margin:60px auto; padding:40px; background:#fff; border-radius:12px; box-shadow:0 8px 24px rgba(0,0,0,0.06); border-top: 5px solid #2563eb;'>
                    <h2 style='color:#1e293b; margin-top:0;'>{activeRole} Login Successful!</h2>
                    <p style='color:#64748b; font-size:0.95rem;'>You have signed in successfully to the Joblyzer workspace control gateway.</p>
                    <hr style='border:0; border-top:1px solid #e2e8f0; margin:25px 0;' />
                    <div style='margin-bottom:12px; font-size:1rem; color:#334155;'><strong>Active Identity Account:</strong> {User.Identity.Name}</div>
                    <div style='margin-bottom:25px; font-size:1rem; color:#334155;'><strong>Assigned Security Scope:</strong> 
                        <span style='background:#dbeafe; color:#1e40af; padding:4px 12px; border-radius:20px; font-size:0.85rem; font-weight:bold; margin-left:5px;'>{activeRole}</span>
                    </div>
                    <a href='/Account/Logout' style='display:inline-block; background:#1e293b; color:#fff; padding:12px 24px; text-decoration:none; border-radius:8px; font-weight:600; font-size:0.95rem; transition: background 0.2s;'>Sign Out</a>
                </div>", "text/html");
        }

        // --- SESSION LOGOUT TERMINATION ---
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
