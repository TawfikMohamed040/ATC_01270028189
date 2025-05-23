﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventy_System.DTOs;
using Eventy_System.Models;
using Eventy_System.Services.AccountService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterUserDTO userDto)
    {
        if (ModelState.IsValid)
        {
            IdentityResult result = await _accountService.CreateUserAsync(userDto);
            if (result.Succeeded)
                return Ok("User created successfully".ToJson());
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }
        }
        return BadRequest(ModelState);
    }
    
    [HttpPost ("Login")]
    public async Task<IActionResult> Login(LoginDTO userDto)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser user = await _accountService.FindByNameAsync(userDto);
            if (user != null)
            {
                bool isPasswordValid = await _accountService.CheckPasswordAsync(user, userDto);
                if (isPasswordValid)
                {
                    JwtSecurityToken token = await _accountService.BuildToken(user, userDto);
                    
                    return Ok(
                        new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token) , 
                            expiration = DateTime.Now.AddDays(30)
                        });
                }
            }
            ModelState.AddModelError("" , "Invalid username or password");
        }
        
        return  BadRequest();
    }
}