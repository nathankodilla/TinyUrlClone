using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TinyUrl.Data.Models
{
    public class Link
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Alias { get; set; }

        [Required]
        [MaxLength(2048)]
        public string OriginalUrl { get; set; }

        [Required]
        public DateTime CreationDateTime { get; set; }
    }
}
