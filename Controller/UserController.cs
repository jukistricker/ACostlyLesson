// Create User
// - /api/users
// Request body: { "name": string, "email": string, "age": number }
// Returns: The created user with a unique id.
// Get All Users
// -  /api/users
// Returns: A list of all users.
// Get User by ID
// -  /api/users/{id}
// Returns: The user with the specified id, or 404 if not found.
//     Update User
//     -  /api/users/{id}
// Request body: { "name": string, "email": string, "age": number }
// Returns: The updated user, or 404 if not found.
//     Delete User
//     -  /api/users/{id}
// Retrns: 204 No Content if deleted, or 404 if not found.
//  
//     Validate that email is unique and properly formatted.
// Return appropriate HTTP status codes for errors (400, 404, etc.).
//     Use in-memory storage (list/array)

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace UseAPI.Controller;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    // Use static to persist data across different HTTP requests in memory
    private static readonly List<User> _users = new List<User>();
    
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        user.Id = Guid.NewGuid();
        _users.Add(user);
        
        // Returns 201 Created with the location of the new resource
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        return Ok(_users);
    }

    [HttpGet("{id}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        return Ok(user);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(Guid id, [FromBody] User updatedUser)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
        {
            return NotFound();
        }

        if (_users.Any(u => u.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase) && u.Id != id))
        {
            return BadRequest(new { message = "Email already in use by another user" });
        }

        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;
        existingUser.Age = updatedUser.Age;

        return Ok(existingUser);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        _users.Remove(user);
        return NoContent(); // 204
    }
}

public class User
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
    public int Age { get; set; }
}