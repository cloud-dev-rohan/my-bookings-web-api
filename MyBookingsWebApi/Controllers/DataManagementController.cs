﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBookingsWebApi.Services;

namespace MyBookingsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataManagementController : ControllerBase
    {
        private readonly ICsvUploadService _csvUploadService;

        public DataManagementController(ICsvUploadService csvUploadService)
        {
            _csvUploadService = csvUploadService;
        }

        [HttpPost("upload-members")]
        public async Task<IActionResult> UploadMembers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using var stream = file.OpenReadStream();
            await _csvUploadService.UploadMembersAsync(stream);
            return Ok("Members uploaded successfully");
        }

        [HttpPost("upload-inventory")]
        public async Task<IActionResult> UploadInventory(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using var stream = file.OpenReadStream();
            await _csvUploadService.UploadInventoryAsync(stream);
            return Ok("Inventory uploaded successfully");
        }
    }
}
