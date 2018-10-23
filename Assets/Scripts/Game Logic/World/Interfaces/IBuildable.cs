using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.World
{
    /// <summary>
    /// Assign to all classes that can be build or destroyed.
    /// </summary>
    public interface IBuildable
    {

        /// <summary>
        /// Cost to build this object.
        /// </summary>
        float BuildCost { get; set; }

        /// <summary>
        /// Cost to destroy this object.
        /// </summary>
        float DestructionCost { get; set; }

    }
}
