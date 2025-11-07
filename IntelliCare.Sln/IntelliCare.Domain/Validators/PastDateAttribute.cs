using System;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace IntelliCare.Domain.Validators

{

    public class PastDateAttribute : ValidationAttribute

    {

        public override bool IsValid(object value)

        {

            if (value is DateTime date)

            {

                return date < DateTime.Today; // Must be before today

            }

            return false;

        }

    }

}

