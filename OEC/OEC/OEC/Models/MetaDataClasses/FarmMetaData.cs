using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OEC.Models
{
    [ModelMetadataType(typeof(FarmMetaData))]
    public partial class Farm : IValidatableObject
    {
        OECContext _context = new OECContext();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Name = TrimInput(Name);
            Address = TrimInput(Address);
            Town = TrimInput(Town);
            County = TrimInput(County);
            ProvinceCode = TrimInput(ProvinceCode).ToUpper();
            PostalCode = TrimInput(PostalCode).ToUpper();
            HomePhone = TrimInput(HomePhone);
            CellPhone = TrimInput(CellPhone);
            Email = TrimInput(Email);
            Directions = TrimInput(Directions);

            if (string.IsNullOrEmpty(Town) && string.IsNullOrEmpty(County))
            {
                yield return new ValidationResult("Town and County cannot be both empty", new[] { "Town" });
            }

            Regex rgProvinceCode = new Regex(@"^[A-Z]{2}$");
            Province province = _context.Province.Find(ProvinceCode);
            if (!rgProvinceCode.IsMatch(ProvinceCode))
                yield return new ValidationResult("Province Code must be exactly 2 letters", new[] { "ProvinceCode" });
            else if (province == null)
                yield return new ValidationResult("Province Code is not on file", new[] { "ProvinceCode" });
            else
            {
                Country country = _context.Country.Find(province.CountryCode);
                Regex rgCountry = new Regex(country.PostalPattern + "$");
                if (!rgCountry.IsMatch(PostalCode))
                    yield return new ValidationResult("Postal Code is not valid", new[] { "PostalCode" });
            }

            if (!string.IsNullOrEmpty(HomePhone))
            {
                string extractDigits = ExtractDigits(HomePhone);
                extractDigits = Convert.ToInt64(extractDigits).ToString("###-###-####");
                Regex rgHomePhone = new Regex(@"^\d{3}[-]\d{3}[-]\d{4}$");
                if (!rgHomePhone.IsMatch(extractDigits))
                {
                    yield return new ValidationResult("Home Phone, if provided, must contain a phone number (123-123-1234)", new[] { "HomePhone" });
                }
            }

            if (DateJoined != null && DateJoined > DateTime.Now)
                yield return new ValidationResult("Date Joined cannot be in the future", new[] { "DateJoined" });

            if (LastContactDate != null)
            {
                if (LastContactDate > DateTime.Now)
                    yield return new ValidationResult("Last Contact Date cannot be in the future", new[] { "LastContactDate" });

                if (DateJoined == null)
                {
                    yield return new ValidationResult("if Last Contact Date is provided Date Joined is required", new[] { "LastContactDate" });
                }
                else
                {
                    if (LastContactDate < DateJoined)
                        yield return new ValidationResult("Last Contact Date cannot be before Date Joined", new[] { "LastContactDate" });
                }
            }
                
        }

        public string TrimInput(string input)
        {
            if (input == null || input.Length == 0)
                return "";
            else
                return input.Trim();
        }

        public string ExtractDigits(string input)
        {
            String numericInput = "";
            for (int x = 0; x < input.Length; x++)
            {
                if (Char.IsDigit(input[x]))
                {
                    numericInput += input[x];
                }
            }

            return numericInput;
        }
    }
    public class FarmMetaData
    {
        public int FarmId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        [Required]
        public string ProvinceCode { get; set; }
        [Required]
        public string PostalCode { get; set; }
        public string HomePhone { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
        public string Directions { get; set; }
        public DateTime? DateJoined { get; set; }
        public DateTime? LastContactDate { get; set; }
    }
}
