using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models;

public class ConfirmViewModel
{
    [Required]
    public string Code { get; set; }
}