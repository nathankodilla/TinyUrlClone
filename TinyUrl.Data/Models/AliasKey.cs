using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TinyUrl.Data.Models
{
    public class AliasKey
    {
        [Key]
        [MaxLength(32)]
        [Column(TypeName = "NVARCHAR(32) COLLATE SQL_Latin1_General_CP1_CS_AS")] // see https://stackoverflow.com/questions/48393309/sql-server-collations-in-ef-core as this column must be case insensitive
        public string Alias { get; set; }
    }
}
