using EPCSystemAPI.models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Event
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Event_Type { get; set; }  // Type of event, e.g., "TRANSFER"

    public int Reference_Id { get; set; }  // Reference to a specific event record in another table

    [ForeignKey("User")]
    public int User_Id { get; set; }  // User associated with the event
    public virtual User User { get; set; }  // Navigation property to User

    [Required]
    public DateTime Timestamp { get; set; }  // Time when the event occurred
}
