using System;
using System.ComponentModel.DataAnnotations;

namespace DataTables.NetStandard.Enhanced.Sample.Models
{
    public class Person
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public EGender Gender { get; set; }

        public Location Location { get; set; }
    }
}
