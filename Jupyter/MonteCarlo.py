from typing import List
import numpy as np
import pandas as pd
from MonteCarloResult import MonteCarloResult
from Portfolio import Portfolio

class MonteCarlo:
    def __init__(self, portfolio: Portfolio, min_return: float, max_return: float, min_allocation: float, max_allocation:float, max_drawdown: float):
        self._portfolio = portfolio
        self._min_return = min_return
        self._max_return = max_return
        self._min_allocation = min_allocation
        self._max_allocation = max_allocation
        self._max_drawdown = max_drawdown

    # Plot the Efficient Frontier - https://www.youtube.com/watch?v=mJTrQfzr0R4&ab_channel=Algovibes
    def run(self, iterations: int, risk_free_rate: float) -> List[MonteCarloResult]:
        mean_results = self._portfolio.monthly_returns.mean()

        results = []

        for _ in range(iterations):
            random_weights = self.create_random_weights()

            # https://youtu.be/mJTrQfzr0R4?si=cGFZ8aCAzNUrLhVl&t=275
            portfolio_annual_return = np.dot(mean_results, random_weights) * 12

            # https://youtu.be/mJTrQfzr0R4?si=ZOyzSt5_Kc4e_GEz&t=367
            cov_dot_weights = np.dot(self._portfolio.covariance_matrix, random_weights)
            cov_dot_weights_dot_weights = np.dot(cov_dot_weights, random_weights)
            portfolio_annual_standard_deviation = np.sqrt(cov_dot_weights_dot_weights) * np.sqrt(12)

            sharpe_ratio = (portfolio_annual_return-risk_free_rate)/portfolio_annual_standard_deviation

            daily_price_weights = pd.Series(random_weights, self._portfolio.daily_price_history.columns)
            daily_price_history_values = self._portfolio.daily_price_history.dropna()
            daily_price_history = daily_price_history_values.dot(daily_price_weights)
            daily_price_history.columns = ['Value']
            max_drawdown = self.calc_max_drawdown(daily_price_history)

            results.append(MonteCarloResult(random_weights, portfolio_annual_return, portfolio_annual_standard_deviation, sharpe_ratio, max_drawdown))

        return results
    
    def calc_max_drawdown(self, price_history: pd.array):
        # Step 1: Get the cumulative maximum (peak so far)
        peak = np.maximum.accumulate(price_history)

        # Step 2: Calculate drawdown at each point (fraction below peak)
        drawdown = (price_history - peak) / peak

        # Step 3: Find the maximum drawdown (minimum drawdown value; it's negative)
        max_drawdown = drawdown.min()

        # -1 => -100% drawdown
        return max_drawdown
    
    def best_result(self, results: List[MonteCarloResult]) -> MonteCarloResult:
        results = [result for result in results if result.annual_return >= self._min_return]
        if(len(results) == 0):
            print(f"Unable to find portfolio with min return >= {self._min_return}")
            return None
        
        results = [result for result in results if result.annual_return <= self._max_return]
        if(len(results) == 0):
            print(f"Unable to find portfolio with max return <= {self._max_return}")
            return None
        
        results = [result for result in results if result.max_drawdown <= self._max_drawdown]
        if(len(results) == 0):
            print(f"Unable to find portfolio with max drawdown <= {self._max_drawdown}")
            return None
        
        results = [result for result in results if all(val >= self._min_allocation for val in result.weights)]
        if(len(results) == 0):
            print(f"Unable to find portfolio with min allocation >= {self._min_allocation}")
            return None
        
        results = [result for result in results if all(val <= self._max_allocation for val in result.weights)]
        if(len(results) == 0):
            print(f"Unable to find portfolio with max allocation <= {self._max_allocation}")
            return None
            
        # drawdowns are negative, so seek the smallest one, max value
        best = max(results, key=lambda s: s.annual_return)

        return best
    
    def create_random_weights(self) -> np.ndarray:
        fund_count = len(self._portfolio.fund_selection)

        fixed_weights = np.zeros(fund_count)
        random_weights = np.zeros(fund_count)
        
        # set any hard coded fund selection weights
        for i in range(fund_count):
            if self._portfolio.fund_selection[i].weight is None:
                random_weights[i] = np.random.random()
            else:
                fixed_weights[i] = self._portfolio.fund_selection[i].weight

        if random_weights.sum() == 0:
            weights = fixed_weights
        else:
            random_weights *= (1-fixed_weights.sum()) / random_weights.sum()
            weights = fixed_weights + random_weights

        return weights
        

        
        