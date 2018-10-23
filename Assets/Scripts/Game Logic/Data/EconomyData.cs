using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.Economy
{
    [System.Serializable]
    public class EconomyData
    {
        /// <summary>
        /// Current state of the economy.
        /// </summary>
        public EconomyState stateOfEconomy = EconomyState.Stable;

        /// <summary>
        /// Percentage by which to modify sale prices.
        /// </summary>
        public float salePriceModifier = 0.95f;

        /// <summary>
        /// Percentage by which to modify purchase prices.
        /// </summary>
        public float purchasePriceModifier = 1.05f;

        /// <summary>
        /// Current interest rate. Applies to loans.
        /// </summary>
        public float interestRate = 0.05f;

        /// <summary>
        /// Number of cycles in the current economic state.
        /// </summary>
        public int cyclesInCurrentEconomicState = 1;

        public int monthsSinceLastStateEvaluation = 1;
    }
}
