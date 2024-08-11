using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedUtilityTestMethods;

public class ValidateClass
{

    /// <summary>
    /// Validation of the class models
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="results"></param>
    /// <returns></returns>
    public static bool Validate<T>(T obj, out ICollection<ValidationResult> results)
    {
        results = new List<ValidationResult>();

        return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
    }

}
