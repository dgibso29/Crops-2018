using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops
{
    /// <summary>
    /// Applied to all loadable classes (namely manager classes).
    /// </summary>
    interface ILoadable
    {

        void InitializeFromSave();

    }
}
