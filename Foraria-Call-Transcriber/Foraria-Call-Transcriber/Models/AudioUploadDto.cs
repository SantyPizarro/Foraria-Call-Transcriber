using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Foraria.CallTranscriber.Models;

public class AudioUploadDto
{
    [Required]
    public IFormFile Audio { get; set; }
}
