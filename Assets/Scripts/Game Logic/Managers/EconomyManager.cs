using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.World;

namespace Crops.Economy
{
    
    public enum EconomyState { Depression, Recession, Stable, Growing, Booming }
    
    public class EconomyManager : MonoBehaviour
    {

        Map worldMap;

        public EconomyData Data
        {
            get { return gameManager.gameData.economyData; }
            set { gameManager.gameData.economyData = value; }
        }

        public GameManager gameManager;

        /// <summary>
        /// Current state of the economy.
        /// </summary>
        public static EconomyState StateOfEconomy = EconomyState.Stable;

        /// <summary>
        /// Initial valueModifier
        /// </summary>
        private float valueModifier = 1;

        /// <summary>
        /// Percentage by which to modify sale prices.
        /// </summary>
        public static float SalePriceModifier = 0.95f;

        /// <summary>
        /// Percentage by which to modify purchase prices.
        /// </summary>
        public static float PurchasePriceModifier = 1.05f;

        /// <summary>
        /// Amount by which to vary BaseValueModifier.
        /// </summary>
        private float valueModifierVariance = .25f;

        /// <summary>
        /// Values should not fall below this percentage of their base.
        /// </summary>
        private float valueFloor = 0.5f;

        /// <summary>
        /// Values should not rise above this percentage of their base.
        /// </summary>
        private float valueCeiling = 5f;

        /// <summary>
        /// Current interest rate. Applies to loans.
        /// </summary>
        private float InterestRate
        {
            get { return Data.interestRate; }
            set { Data.interestRate = value; }
        }
        
        public int StateOfEconomyEvaluationInterval = 6;

        /// <summary>
        /// Number of cycles in the current economic state.
        /// </summary>
        int CyclesInCurrentEconomicState
        {
            get { return Data.cyclesInCurrentEconomicState; }
            set { Data.cyclesInCurrentEconomicState = value; }
        }

        int MonthsSinceLastStateEvaluation
        {
            get { return Data.monthsSinceLastStateEvaluation; }
            set { Data.monthsSinceLastStateEvaluation = value; }
        }


        /// <summary>
        /// Applied to all value/price calculations.
        /// </summary>
        public float BaseValueModifier
        {            
            get
            {
                return Random.Range(valueModifier - valueModifierVariance, valueModifier + valueModifierVariance);
            }
        }

        private void Start()
        {
            worldMap = gameManager.worldMap;
        }

        private void Update()
        {

        }
  
        /// <summary>
        /// Check monthly if it is time to evaluate the state of the economy.
        /// </summary>
        public void CheckForStateEvaluation()
        {
            if(MonthsSinceLastStateEvaluation >= StateOfEconomyEvaluationInterval)
            {
                MonthsSinceLastStateEvaluation = 0;
                EvaluateStateOfEconomy();
            }
            else
            {
                MonthsSinceLastStateEvaluation++;
            }
        }

        /// <summary>
        /// Updates everything affected by state of economy.
        /// </summary>
        void UpdateEconomy()
        {
            UpdateValueModifier();
            UpdatePriceModifiers();
            UpdateLandValues();
            UpdateCityGrowth();
        }

        /// <summary>
        /// Update value modifier based on state of economy.
        /// </summary>
        void UpdateValueModifier()
        {
            switch (StateOfEconomy)
            {
                case EconomyState.Depression:
                    {
                        valueModifier = 0.5f;

                        break;
                    }
                case EconomyState.Recession:
                    {
                        valueModifier = 0.8f;

                        break;
                    }
                case EconomyState.Stable:
                    {
                        valueModifier = 1;
                        break;
                    }
                case EconomyState.Growing:
                    {
                        valueModifier = 1.3f;

                        break;
                    }
                case EconomyState.Booming:
                    {
                        valueModifier = 1.7f;
                        break;
                    }
            }
        }

        /// <summary>
        /// Update price modifiers based on state of economy.
        /// </summary>
        void UpdatePriceModifiers()
        {
            switch (StateOfEconomy)
            {
                case EconomyState.Depression:
                    {
                        PurchasePriceModifier = 1.25f;
                        SalePriceModifier = 0.75f;
                        break;
                    }
                case EconomyState.Recession:
                    {
                        PurchasePriceModifier = 1.15f;
                        SalePriceModifier = 0.85f;
                        break;
                    }
                case EconomyState.Stable:
                    {
                        PurchasePriceModifier = 1.05f;
                        SalePriceModifier = 0.95f;
                        break;
                    }
                case EconomyState.Growing:
                    {
                        PurchasePriceModifier = 0.9f;
                        SalePriceModifier = 1.1f;
                        break;
                    }
                case EconomyState.Booming:
                    {
                        PurchasePriceModifier = 0.7f;
                        SalePriceModifier = 1.3f; break;
                    }
            }
        }

        /// <summary>
        /// Update land values of all map tiles.
        /// </summary>
        void UpdateLandValues()
        {
            for(int x = 0; x < worldMap.MapSize; x++)
            {
                for(int y = 0; y < worldMap.MapSize; y++)
                {
                    LandPlot plot = worldMap.MapData[x, y];
                    worldMap.MapData[x, y].CurrentLandValue = GetUpdatedValue(plot.CurrentLandValue, plot.BaseLandValue);
                }
            }
        }

        void UpdateCityGrowth()
        {
            switch (StateOfEconomy)
            {
                case EconomyState.Depression:
                    {
                        gameManager.city.CityGrowthInterval = 24;
                        break;
                    }
                case EconomyState.Recession:
                    {
                        gameManager.city.CityGrowthInterval = 15;
                        break;
                    }
                case EconomyState.Stable:
                    {
                        gameManager.city.CityGrowthInterval = 9;
                        break;
                    }
                case EconomyState.Growing:
                    {
                        gameManager.city.CityGrowthInterval = 6;
                        break;
                    }
                case EconomyState.Booming:
                    {
                        gameManager.city.CityGrowthInterval = 3;
                        break;
                    }
            }
        }

        // TO-DO: Add more variables to state evaluation -- player performance/finances, for example.

        /// <summary>
        /// Check if the state of the economy should change, and, if so, change it.
        /// </summary>
        public void EvaluateStateOfEconomy()
        {
            MathHelper.IntRange[] weightedRange;
            EconomyState newState = StateOfEconomy;
            // Get state of economy based on current state
            switch (StateOfEconomy)
            {
                case EconomyState.Depression:
                    {
                        weightedRange = new MathHelper.IntRange[2] { new MathHelper.IntRange(0, 0, 60), new MathHelper.IntRange(1, 1, 40 + (5 * CyclesInCurrentEconomicState)) };
                        newState = (EconomyState)MathHelper.RandomRange.WeightedRange(weightedRange);
                        break;
                    }
                case EconomyState.Recession:
                    {
                        weightedRange = new MathHelper.IntRange[3] { new MathHelper.IntRange(0, 0, 20 + (2 * CyclesInCurrentEconomicState)), new MathHelper.IntRange(1, 1, 50), new MathHelper.IntRange(2, 2, 30 + (3 * CyclesInCurrentEconomicState)) };
                        newState = (EconomyState)MathHelper.RandomRange.WeightedRange(weightedRange);
                        break;
                    }
                case EconomyState.Stable:
                    {
                        weightedRange = new MathHelper.IntRange[3] { new MathHelper.IntRange(1, 1, 25 + (2 * CyclesInCurrentEconomicState)), new MathHelper.IntRange(2, 2, 50), new MathHelper.IntRange(3, 3, 25 + (2 * CyclesInCurrentEconomicState)) };
                        newState = (EconomyState)MathHelper.RandomRange.WeightedRange(weightedRange);
                        break;
                    }
                case EconomyState.Growing:
                    {
                        weightedRange = new MathHelper.IntRange[3] { new MathHelper.IntRange(2, 2, 30 + (3 * CyclesInCurrentEconomicState)), new MathHelper.IntRange(3, 3, 50), new MathHelper.IntRange(4, 4, 20 + (2 * CyclesInCurrentEconomicState)) };
                        newState = (EconomyState)MathHelper.RandomRange.WeightedRange(weightedRange);
                        break;
                    }
                case EconomyState.Booming:
                    {
                        weightedRange = new MathHelper.IntRange[2] { new MathHelper.IntRange(4, 4, 55), new MathHelper.IntRange(3, 3, 45 + (5 * CyclesInCurrentEconomicState)) };
                        newState = (EconomyState)MathHelper.RandomRange.WeightedRange(weightedRange);
                        break;
                    }
            }
            // If the state has not changed, simply continue after iterating the cycles in the current state
            if (newState == StateOfEconomy)
            {
                CyclesInCurrentEconomicState++;
                return;
            }
            else
            {
                // Otherwise, reset the cycles in the current state 
                CyclesInCurrentEconomicState = 1;
            }
        }

        /// <summary>
        /// Returns updated value based on current economic parameters, clamped between a min and max value.
        /// </summary>
        /// <param name="valueToUpdate"></param>
        /// <returns></returns>
        public float GetUpdatedValue(float valueToUpdate, float baseValue)
        {
            return Mathf.Clamp((valueToUpdate * BaseValueModifier), (baseValue * valueFloor), (baseValue * valueCeiling));
        }

        public static float GetSalePrice(float priceToCheck)
        {
            return priceToCheck * SalePriceModifier;
        }

        public static float GetPurchasePrice(float priceToCheck)
        {
            return priceToCheck * PurchasePriceModifier;

        }

        /// <summary>
        /// If true, purchase can be made.
        /// </summary>
        /// <returns></returns>
        public static bool ValidatePurchase(float purchasePrice, Player activePlayer)
        {
            bool canPurchase = false;

            
            if(activePlayer.data.funds - purchasePrice >= 0)
            {
                canPurchase = true;
            }

            return canPurchase;
        }
        /// <summary>
        /// Deduct the given purchase amount from the given player's account.
        /// </summary>
        /// <param name="purchaseAmount"></param>
        /// <param name="activePlayer"></param>
        public static void ProcessPurchase(float purchaseAmount, Player activePlayer)
        {
            activePlayer.data.funds -= purchaseAmount;
        }
        /// <summary>
        /// Add the given sale amount to the given player's account.
        /// </summary>
        /// <param name="saleAmount"></param>
        /// <param name="activePlayer"></param>
        public static void ProcessSale(float saleAmount, Player activePlayer)
        {
            activePlayer.data.funds += saleAmount;
        }


    }

}
