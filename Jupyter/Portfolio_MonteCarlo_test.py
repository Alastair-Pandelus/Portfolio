import unittest
from unittest.mock import patch
import numpy as np
import pandas as pd
import pandera as pa
from datetime import datetime, timedelta
from FundSelection import FundSelection
from MonteCarlo import MonteCarlo
from MonteCarloResult import MonteCarloResult
from Portfolio import Portfolio

FUND_COUNT = 4
PRICES_YEARS = 10

class DummyDbQuery:
    def get_monthly_returns(self, name, years):
        end_date = pd.Timestamp.today().normalize()
        start_date = end_date - pd.DateOffset(years=PRICES_YEARS)
        dates = pd.date_range(start=start_date, end=end_date, freq='ME')

        hash_value = (hash(name) % 1000) / 1000

        returns = []
        for i in range(len(dates)):
            returns.append(0.01 * i * hash_value)
        
        df = pd.DataFrame({'Date': dates, 'Value': returns})
        df.set_index('Date', inplace=True)
        
        return df
    
    def get_daily_log_adjusted_return(self, name, years):
        end_date = pd.Timestamp.today().normalize()
        start_date = end_date - pd.DateOffset(years=PRICES_YEARS)
        dates = pd.date_range(start=start_date, end=end_date, freq='ME')

        hash_value = (hash(name) % 1000) / 1000

        adjusted_returns = []
        for i in range(len(dates)):
            value = 0.002+(0.0002*i) if i % 2 else -0.001-(i*0.0001)
            adjusted_returns.append(value * hash_value)

        df = pd.DataFrame({'Date': dates, 'LogValue': adjusted_returns})
        df.set_index('Date', inplace=True)

        return df

class TestPortfolio(unittest.TestCase):
    def setUp(self):
        fund_selections = []
        for i in range(FUND_COUNT):
            fund_selections.append(FundSelection(f'Fund{i+1}', None if i%2 else 0.1))

        self.funds = fund_selections
        self.benchmark = [FundSelection('Benchmark', 1.0)]
        self.years = PRICES_YEARS

        patcher = patch('Portfolio.DbQuery', DummyDbQuery)
        self.addCleanup(patcher.stop)
        self.mock_db = patcher.start()

        self.portfolio = Portfolio(self.funds, self.benchmark, self.years)
        self.monte_carlo = MonteCarlo(self.portfolio, min_return=0.0, max_return=999.9, min_allocation=0.0, max_allocation=1.0)

    def test_fund_selection_property(self):
        self.assertEqual(self.portfolio.fund_selection, self.funds)

    def test_monthly_returns_shape(self):
        df = self.portfolio.monthly_returns
        self.assertIsInstance(df, pd.DataFrame)
        self.assertEqual(df.index.name, 'Date')
        self.assertEqual(len(list(df.columns)),FUND_COUNT)

    def test_correlation_matrix(self):
        corr = self.portfolio.correlation_matrix
        self.assertIsInstance(corr, pd.DataFrame)
        self.assertTrue(np.all(corr.values != 0))
        self.assertTrue(np.issubdtype(corr.values.dtype, np.floating))

    def test_benchmark_property(self):
        self.assertEqual(self.portfolio.benchmark, self.benchmark)

    def test_get_weight(self):
        weight = self.portfolio.get_weight('Fund1')
        self.assertEqual(weight, 0.1)
        weight = self.portfolio.get_weight('NonExistent')
        self.assertEqual(weight, -1)

    # def test_get_min_max_dates(self):
    #     min_date, max_date = self.portfolio.get_min_max_dates(10)
    #     self.assertTrue(isinstance(min_date, datetime))
    #     self.assertTrue(isinstance(max_date, datetime))
    #     self.assertEqual((max_date - min_date).days, 7*10)

    def test_get_monthly_returns(self):
        df = self.portfolio.get_monthly_returns(self.funds, 10)
        self.assertIsInstance(df, pd.DataFrame)
        self.assertEqual(df.index.name, 'Date')
        self.assertEqual(len(list(df.columns)),FUND_COUNT)

    def test_covariance_matrix_created(self):
        matrix = self.portfolio.covariance_matrix

    def test_get_daily_price_history(self):
        df = self.portfolio.get_daily_price_history(6)
        self.assertIsInstance(df, pd.DataFrame)
        self.assertEqual(df.index.name, 'Date')
        self.assertEqual(len(list(df.columns)),FUND_COUNT)
        self.assertTrue((df >= 90).all().all())

    def test_create_random_weights_sum_to_one(self):
        weights = self.monte_carlo.create_random_weights()
        print(weights)
        self.assertAlmostEqual(np.sum(weights), 1.0, places=6)

    def test_best_portfolio_offset_returns_none_for_empty(self):
        self.monte_carlo._min_allocation = 1.0
        result = self.monte_carlo.run(iterations=10, risk_free_rate=0.04)
        self.assertEqual(result, None)

    def test_monte_carlo_returns_results(self):
        result = self.monte_carlo.run(iterations=10, risk_free_rate=0.04)
        self.assertIsInstance(result, MonteCarloResult)


if __name__ == '__main__':
    unittest.main()