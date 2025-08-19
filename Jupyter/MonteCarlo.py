from typing import List
import numpy as np
import pandas as pd
from MonteCarloResult import MonteCarloResult
from Portfolio import Portfolio

class MonteCarlo:
    def __init__(self, portfolio: Portfolio, min_return: float, max_return: float, min_allocation: float, max_allocation:float):
        self._portfolio = portfolio
        self._min_return = min_return
        self._max_return = max_return
        self._min_allocation = min_allocation
        self._max_allocation = max_allocation

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

            results.append(MonteCarloResult(random_weights, portfolio_annual_return, portfolio_annual_standard_deviation, sharpe_ratio))

        return results
    
    def best_result(self, results: List[MonteCarloResult]) -> MonteCarloResult:
        results = [result for result in results if result.annual_return >= self._min_return]
        if(len(results) == 0):
            print(f"Unable to find portfolio with min return >= {self._min_return}")
            return None
        
        results = [result for result in results if result.annual_return <= self._max_return]
        if(len(results) == 0):
            print(f"Unable to find portfolio with max return <= {self._max_return}")
            return None
        
        results = [result for result in results if all(val >= self._min_allocation for val in result.weights)]
        if(len(results) == 0):
            print(f"Unable to find portfolio with min allocation >= {self._min_allocation}")
            return None
        
        results = [result for result in results if all(val <= self._max_allocation for val in result.weights)]
        if(len(results) == 0):
            print(f"Unable to find portfolio with max allocation <= {self._max_allocation}")
            return None
            
        best = max(results, key=lambda s: s.sharpe_ratio)
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
        

        
        