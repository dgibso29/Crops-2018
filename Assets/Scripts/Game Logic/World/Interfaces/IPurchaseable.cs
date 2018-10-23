using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.World
{
    /// <summary>
    /// Assign to all classes that can be purchased or sold.
    /// </summary>
    public interface IPurchaseable
    {
        /// <summary>
        /// Base value of item before adjustments.
        /// </summary>
        float BaseValue { get; set; }

    }
}
