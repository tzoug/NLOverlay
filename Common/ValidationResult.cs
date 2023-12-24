using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class ValidationResult
    {
        /// <summary>
        /// Flag indicating if the validation has passed.
        /// </summary>
        public bool IsValid => Errors.Count == 0;
        
        /// <summary>
        /// List of errors for validation that haven't passed.
        /// </summary>
        public List<string> Errors { get; }

        public ValidationResult(List<ValidationResult> results)
        {
            Errors = results.SelectMany(r => r.Errors).ToList();
        }

        public ValidationResult(string error)
        {
            Errors = new List<string> { error };
        }
    }
}
