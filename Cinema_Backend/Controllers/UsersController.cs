﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cinema_Backend.Data;
using Cinema_Backend.Models;
using Cinema_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Cinema_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //  GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<UserDto>> GetAll()
        {
            var users = _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/users/{id}
        // If admin: get any user
        // If logged in user: get their own profile
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var currentUserIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && currentUserIdString != id)
                return Forbid();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var dto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return Ok(dto);
        }

        //  DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        //  PUT: api/users/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userIdStr);
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        //  PUT: api/users/password
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userIdStr);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}
