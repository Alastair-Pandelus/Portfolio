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

        monthly_returns = self.get_monthly_returns(fund_selection, price_years)
        monthly_returns = monthly_returns.dropna()
        self._monthly_returns = monthly_returns

        self._correlation_matrix = monthly_returns.corr()
        self._covariance_matrix = monthly_returns.cov()

        self._names_csv = ', '.join([f"'{f.name}'" for f in self.fund_selection])

        self._benchmark = benchmark
        self._benchmark_monthly_returns = self.get_monthly_returns(benchmark, price_years)

        #min_date, max_date = self.get_min_max_dates(price_years)
        self._daily_price_history = self.get_daily_price_history(price_years)
        
    @property
    def fund_selection(self):
        return self._fund_selection
    
    @property
    def monthly_returns(self):
        return self._monthly_returns
    
    @property
    def daily_price_history(self):
        return self._daily_price_history
    
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
    
    # def get_min_max_dates(self, years: int):
    #     minDate, maxDate = self._db_query.get_min_max_dates(self._names_csv)
    #     minDate = maxDate - timedelta(weeks=years*52.25)

        # return minDate, maxDate
    
    def get_monthly_returns(self, funds: List[FundSelection], years: int):
        monthly_returns = []

        for fund in funds:
            fund_monthly_returns = self._db_query.get_monthly_returns(fund.name, years)
            fund_monthly_returns_type_schema.validate(fund_monthly_returns)
            num_rows = fund_monthly_returns.shape[0]
            if num_rows < int(years * 12)-1:
                raise Exception(f'fund {fund.name} only has {num_rows} rows, expecting {years*12} months for {years} years')

            fund_monthly_returns = fund_monthly_returns.rename(columns={'Value': fund.name})

            monthly_returns.append(fund_monthly_returns)
            
        df_merged = reduce(lambda left, right: pd.merge(left,right,on=['Date'], how='outer'), monthly_returns)

        return df_merged
    
    from functools import reduce

    def get_daily_price_history(self, years):
        adjusted_returns = []

        for fund in self._fund_selection:
            fund_adjusted_returns = self._db_query.get_daily_log_adjusted_return(fund.name, years)
            fund_adjusted_returns_type_schema.validate(fund_adjusted_returns)
            fund_adjusted_returns = fund_adjusted_returns.rename(columns={'LogValue': fund.name})

            adjusted_returns.append(fund_adjusted_returns)

        df_merged = reduce(lambda left, right: pd.merge(left, right, on=['Date'], how='outer'), adjusted_returns)

        df_cumsum = df_merged.cumsum()
        df_exp = np.exp(df_cumsum) * 100
        df_values = df_exp
        
        return df_values
    
    def get_benchmark_daily_price_history(self, years):
        adjusted_returns = []

        for fund in self._benchmark:
            fund_adjusted_returns = self._db_query.get_daily_log_adjusted_return(fund.name, years)
            fund_adjusted_returns_type_schema.validate(fund_adjusted_returns)
            fund_adjusted_returns = fund_adjusted_returns.rename(columns={'LogValue': fund.name})

            adjusted_returns.append(fund_adjusted_returns)

        df_merged = reduce(lambda left, right: pd.merge(left, right, on=['Date'], how='outer'), adjusted_returns)

        df_cumsum = df_merged.cumsum()
        df_exp = np.exp(df_cumsum) * 100
        df_values = df_exp

        weights = [fund.weight for fund in self._benchmark]
        weighted_returns = df_values.dot(weights)
        
        return weighted_returns    