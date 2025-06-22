using System.ComponentModel.DataAnnotations;

namespace dissertation_test_repo.DTOs
{
    public class CarCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2030)]
        public int Year { get; set; }

        [StringLength(20)]
        public string Color { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
