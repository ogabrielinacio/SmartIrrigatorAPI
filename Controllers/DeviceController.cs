using System.Security.Claims;
using AutoRainAPI.Data;
using AutoRainAPI.Models;
using AutoRainAPI.Utils;
using AutoRainAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoRainAPI.Controllers;

[Authorize(Roles = "User")]
[ApiController]
public class DeviceController : ControllerBase
{
   private readonly DataContext _dataContext;

   public DeviceController(DataContext dataContext)
   {
      _dataContext = dataContext;
   }

   [HttpPost("device-login")]
   public async Task<IActionResult> DeviceLogin([FromBody] DeviceLoginViewModel request)
   {
       Device device = await _dataContext.Devices.Where(s => s.SerialNumber.Equals(request.SerialNumber)).FirstOrDefaultAsync();
       if(device == null)
           return Unauthorized("Device not Found");
       if (AuthenticationUtils.VerifyDevicePasswordHash(device, request.Password))
       {
           var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
           var user = await _dataContext.Users.Where(i => i.UserId.Equals(userId)).FirstOrDefaultAsync();
           if(user == null)
               return NotFound("User not found");
           // user.Devices ??= new List<Device>();
           // user.Devices?.Add(device);
           device.FKUserId = user.UserId;
           // _dataContext.Entry(user.Devices).State = EntityState.Modified;
           await _dataContext.Devices.AddAsync(device);
           await _dataContext.SaveChangesAsync();
           return Ok($"device Added with successfully ->");
       }
       return Unauthorized("Serial Number or password incorrected");
       return Ok();
   }
   
   [HttpPost("test-device-register")]
   public async Task<IActionResult> DeviceRegister([FromBody]  DeviceRegisterViewModel request) {

       if(_dataContext.Devices.Any(e => e.SerialNumber.Equals(request.SerialNumber)))
           return BadRequest("Serial Number already registered");
       AuthenticationUtils.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);
       var device = new Device
       {
           SerialNumber = request.SerialNumber,
           Password = hash,
           Salt = salt,
       };
       // device.FKUserId = new Guid();
       await _dataContext.Devices.AddAsync(device);
       await _dataContext.SaveChangesAsync();

       return Ok("registered Device");
   }
}