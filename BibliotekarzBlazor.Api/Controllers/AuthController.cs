using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text;
using Bibliotekarz.Data.Model;
using BibliotekarzBlazor.Shared;
using BibliotekarzBlazor.Shared.Auth;
using BibliotekarzBlazor.Shared.DTOs;
using BibliotekarzBlazor.Shared.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BibliotekarzBlazor.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtOptions> jwtOptions)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto? request)
    {
        if (request == null || ModelState.IsValid == false)
        {
            return BadRequest(Result.Fail("Nieprawidłowy Request"));
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password!);
        if (result.Succeeded)
        {
            if (request.Role != Constants.Role_Admin && request.Role != Constants.Role_User)
            {
                request.Role = Constants.Role_User;
            }

            if (await roleManager.RoleExistsAsync(request.Role) == false)
            {
                await roleManager.CreateAsync(new IdentityRole { Name = request.Role });
            }

            result = await userManager.AddToRoleAsync(user, request.Role);

            if (result.Succeeded)
            {
                return Created("", Result.Success());
            }
        }

        return BadRequest(Result.Fail("Nie udało się utworzyć użytkownika."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request == null || !ModelState.IsValid)
        {
            return BadRequest(Result.Fail("Nieprawidłowy Request"));
        }

        try
        {
            var result = await signInManager.PasswordSignInAsync(
                request.UserName!,
                request.Password!,
                true,
                false);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(request.UserName!);
                if (user != null)
                {
                    JwtOptions keyOptions = jwtOptions.Value;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyOptions.Key!));
                    var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.UserName!),
                        new(ClaimTypes.Email, user.Email!),
                        new("Id", user.Id!),
                    };

                    var roles = await userManager.GetRolesAsync(user);
                    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r))); //Do tokena JWT dodajemy role. TO WAŻNE.

                    var tokenOptions = new JwtSecurityToken(
                        issuer: keyOptions.Issuer,
                        audience: keyOptions.Audience,
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: signInCredentials
                    );

                    var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                    return Ok(Result<LoginResponseDto>.Success(new LoginResponseDto
                    {
                        Token = token,
                        UserDto = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.PhoneNumber
                        }
                    }));
                }
            }

            return Unauthorized(Result.Fail("Invalid login"));
        }
        catch (Exception e)
        {
            return Unauthorized(Result.Fail("Error login"));
        }
    }
}
