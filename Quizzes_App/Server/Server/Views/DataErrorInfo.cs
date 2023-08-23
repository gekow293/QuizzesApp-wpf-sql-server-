using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Server.Views
{
    public class DataErrorInfo : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value.ToString();
            if (input.ToArray().Length < 1) return new ValidationResult(false, "Слишком короткий текст");
            return new ValidationResult(true, null);
        }
    }
}
