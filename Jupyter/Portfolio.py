from datetime import timedelta
from typing import List
import numpy as np
import pandas as pd
from functools import reduce
from DbQuery import DbQuery
from DataframeTypes import fund_monthly_returns_type_schema, fund_adjusted_returns_type_schema
from FundSelection import FundSelection

class Portfolio:
    def __init__(self, fund_selection: List[FundSelection], benchmark: List[FundSelection], price_years: int):
        self._db_query = DbQuery()

        self._fund_selection = fund_selection
        self._price_years = price_years

        monthly_returns = self.get_monthly_returns(fund_selection)
        monthly_returns = monthly_returns.dropna()
        self._monthly_returns = monthly_returns

        self._correlation_matrix = monthly_returns.corr()
        self._covariance_matrix = monthly_returns.cov()

        self._names_csv = ', '.join([f"'{f.name}'" for f in self.fund_selection])

        self._benchmark = benchmark
        self._benchmark_monthly_returns = self.get_monthly_returns(benchmark)

        adjusted_returns, daily_price_history = self.get_daily_adjusted_returns_and_price_history()
        self._adjusted_returns = adjusted_returns
        self._daily_price_history = daily_price_history

        benchmark_adjusted_returns, benchmark_daily_price_history = self.get_benchmark_daily_adjusted_returns_and_price_history()
        self._benchmark_adjusted_returns = benchmark_adjusted_returns
        self._benchmark_daily_price_history = benchmark_daily_price_history
        
    @property
    def fund_selection(self):
        return self._fund_selection
    
    @property
    def price_years(self):
        return self._price_years
    
    @property
    def monthly_returns(self):
        return self._monthly_returns
    
    @property
    def adjusted_returns(self):
        return self._adjusted_returns
    
    @property
    def benchmark_adjusted_returns(self):
        return self._benchmark_adjusted_returns
    
    @property
    def daily_price_history(self):
        return self._daily_price_history
    
    @property
    def benchmark_daily_price_history(self):
        return self._benchmark_daily_price_history
    
    @property
    def correlation_matrix(self):
        return self._correlation_matrix
    
    @property
    def covariance_matrix(self):
        return self._covariance_matrix
    
    @property
    def benchmark(self):
        return self._benchmark        

    @property
    def benchmark_montly_returns(self):
        return self._benchmark_monthly_returns
    
    def get_weight(self, name: str):
        for i in range(len(self._fund_selection)):
            if(self._fund_selection[i].name == name):
                return self._fund_selection[i].weight
            
        return -1
    
    def get_monthly_returns(self, funds: List[FundSelection]):
        monthly_returns = []

        for fund in funds:
            fund_monthly_returns = self._db_query.get_monthly_returns(fund.name, self._price_years)
            fund_monthly_returns_type_schema.validate(fund_monthly_returns)
            num_rows = fund_monthly_returns.shape[0]

            if num_rows < int(self._price_years * 12)-1:
                min_date = fund_monthly_returns.index.min()
                combined_proxy_funds_monthly_returns = pd.DataFrame()

                for proxy_fund in fund.proxy_funds:
                    proxy_fund_monthly_returns = self._db_query.get_monthly_returns(proxy_fund.name, self._price_years)
                    proxy_fund_monthly_returns = proxy_fund_monthly_returns.loc[proxy_fund_monthly_returns.index < min_date] * proxy_fund.weight
                    combined_proxy_funds_monthly_returns = combined_proxy_funds_monthly_returns.add(proxy_fund_monthly_returns, fill_value=0)

                fund_monthly_returns = fund_monthly_returns.combine_first(combined_proxy_funds_monthly_returns)

            fund_monthly_returns = fund_monthly_returns.rename(columns={'Value': fund.name})

            monthly_returns.append(fund_monthly_returns)
            
        df_merged = reduce(lambda left, right: pd.merge(left,right,on=['Date'], how='outer'), monthly_returns)

        return df_merged
    
    from functools import reduce

    def get_daily_adjusted_returns_and_price_history(self):
        adjusted_returns = self.get_portfolio_daily_adjusted_returns()

        adjusted_returns_df = reduce(lambda left, right: pd.merge(left, right, on=['Date'], how='outer'), adjusted_returns)

        df_cumsum = adjusted_returns_df.cumsum()
        df_exp = np.exp(df_cumsum) * 100
        df_prices = df_exp
        
        return adjusted_returns_df, df_prices
    
    def get_benchmark_daily_adjusted_returns_and_price_history(self):
        adjusted_returns = self.get_benchmark_daily_adjusted_returns()

        adjusted_returns_df = reduce(lambda left, right: pd.merge(left, right, on=['Date'], how='outer'), adjusted_returns)

        df_cumsum = adjusted_returns_df.cumsum()
        df_exp = np.exp(df_cumsum) * 100
        df_prices = df_exp
        
        return adjusted_returns_df, df_prices
    
    def get_benchmark_daily_adjusted_returns(self):
        return self.get_daily_adjusted_returns(self._benchmark)
    
    def get_portfolio_daily_adjusted_returns(self):
        return self.get_daily_adjusted_returns(self.fund_selection)
    
    def get_daily_adjusted_returns(self, funds: List[FundSelection]):
        adjusted_returns = []

        #for fund in self._fund_selection: 
        for i in range(len(funds)):
            fund = funds[i]

            fund_adjusted_returns = self._db_query.get_daily_log_adjusted_return(fund.name, self._price_years, fund.proxy_funds)

            min_date = fund_adjusted_returns.index.min()
            proxy_funds_adjusted_returns = pd.DataFrame()

            if min_date > (pd.Timestamp.today() - pd.DateOffset(years=self._price_years) + pd.DateOffset(days=3)):
                for proxy_fund in fund.proxy_funds:
                    proxy_fund_adjusted_returns = self._db_query.get_daily_log_adjusted_return(proxy_fund.name, self._price_years)
                    proxy_fund_adjusted_returns = proxy_fund_adjusted_returns.loc[proxy_fund_adjusted_returns.index < min_date] * proxy_fund.weight
                    proxy_funds_adjusted_returns = proxy_funds_adjusted_returns.add(proxy_fund_adjusted_returns, fill_value=0)

                fund_adjusted_returns = fund_adjusted_returns.combine_first(proxy_funds_adjusted_returns)

            fund_adjusted_returns_type_schema.validate(fund_adjusted_returns)
            fund_adjusted_returns = fund_adjusted_returns.rename(columns={'LogValue': fund.name})

            adjusted_returns.append(fund_adjusted_returns)

        return adjusted_returns
    
    def get_benchmark_daily_price_history(self):
        return self.get_daily_price_history(self._benchmark)
    
    def get_daily_price_history(self, funds: List[FundSelection]):
        adjusted_returns = []

        for fund in funds:
            proxy_funds = [(proxy_fund.name, proxy_fund.weight) for proxy_fund in fund.proxy_funds]

            fund_adjusted_returns = self._db_query.get_daily_log_adjusted_return(fund.name, self._price_years, proxy_funds)
            fund_adjusted_returns_type_schema.validate(fund_adjusted_returns)
            fund_adjusted_returns = fund_adjusted_returns.rename(columns={'LogValue': fund.name})

            adjusted_returns.append(fund_adjusted_returns)

        df_merged = reduce(lambda left, right: pd.merge(left, right, on=['Date'], how='outer'), adjusted_returns)

        df_cumsum = df_merged.cumsum()
        df_exp = np.exp(df_cumsum) * 100
        df_values = df_exp

        weights = [fund.weight for fund in funds]
        weighted_returns = df_values.dot(weights)
        
        return weighted_returns    