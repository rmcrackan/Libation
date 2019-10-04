using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForm
{
    public interface IValidatable
    {
        // forms has a framework for ValidateChildren and ErrorProvider.s
        // i don't feel like setting it up right now. doing this instead
        string StringBasedValidate();
    }
}
