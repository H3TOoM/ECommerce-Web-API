using System.ComponentModel.DataAnnotations;

namespace ShopAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Username { get; set; }
        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; }


        public int CartId { get; set; }
        public Cart Cart { get; set; }


        public ICollection<Address> Addresses { get; set; }
    }
}
