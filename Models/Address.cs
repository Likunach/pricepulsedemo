using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        [ForeignKey("CompanyProfile")]
        public int CompanyProfileId { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? AddressLine1 { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? AddressLine2 { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? City { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? State { get; set; }

        // Alias for State to match DAO expectations
        public string? StateProvince 
        { 
            get => State; 
            set => State = value; 
        }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? PostalCode { get; set; }

        // Add Country property expected by DAOs
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? Country { get; set; }

        // Navigation properties
        public virtual CompanyProfile CompanyProfile { get; set; } = null!;
    }
}