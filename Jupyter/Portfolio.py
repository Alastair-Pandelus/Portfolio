from datetime import timedelta
from typing import List
from colorhash import ColorHash
import numpy as np
import pandas as pd
from functools import reduce
from DbQuery import DbQuery
from DataframeTypes import fund_monthly_returns_type_schema, fund_adjusted_returns_type_schema
from FundSelection import FundSelection
import scipy.optimize as sco
from matplotlib import pyplot as plt
from pyfolio import utils, plotting
import empyrical as ep
import matplotlib.gridspec as gridspec

TRADING_DAYS_IN_YEAR = 252

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
    
    def __get_efficient_frontier(self, avg_rtns, cov_mat, rtns_range):
        efficient_portfolios = []
        n_assets = len(avg_rtns)
        args = (avg_rtns, cov_mat)
        bounds = tuple((0,1) for asset in range(n_assets))
        initial_guess = n_assets * [1. / n_assets, ]
        for ret in rtns_range:
            constraints = ({'type': 'eq',
                            'fun': lambda x: self.__get_portf_rtn(x, avg_rtns) - ret},
                        {'type': 'eq',
                            'fun': lambda x: np.sum(x) - 1})
            efficient_portfolio = sco.minimize(self.__get_portf_vol,
                                            initial_guess,
                                            args=args,
                                            method='SLSQP',
                                            constraints=constraints,
                                            bounds=bounds)
            efficient_portfolios.append(efficient_portfolio)

        return efficient_portfolios

    # Optimize weights using Monte Carlo simulation and scipy.optimize
    def optimize_weights(self, iterations, risk_free_rate):
        # make it deterministic
        np.random.seed(42)

        abridged_weights = np.random.random(size=(iterations, len(self.fund_selection)))
        abridged_weights /= np.sum(abridged_weights, axis=1)[:, np.newaxis]

        abridged_avg_returns = self._adjusted_returns.mean() * TRADING_DAYS_IN_YEAR
        abridged_cov_mat = self._adjusted_returns.cov() * TRADING_DAYS_IN_YEAR

        abridged_portf_rtns = np.dot(abridged_weights, abridged_avg_returns)
        abridged_portf_vol = []

        # run Monte Carlo
        for i in range(0, len(abridged_weights)):
            abridged_portf_vol.append(np.sqrt(np.dot(abridged_weights[i].T, np.dot(abridged_cov_mat, abridged_weights[i]))))

        abridged_portf_vol = np.array(abridged_portf_vol)
        abridged_portf_sharpe_ratio = (abridged_portf_rtns - risk_free_rate) / abridged_portf_vol

        abridged_portf_results_df = pd.DataFrame({'returns': abridged_portf_rtns,
                                        'volatility': abridged_portf_vol,
                                        'sharpe_ratio': abridged_portf_sharpe_ratio})
        
        # Calculate the efficient frontier to 'efficient_portfolios' variable using scipy.optimize
        rtns_range = np.linspace(start=0.07, stop=0.13, num=200)
        constraints = ({'type': 'eq',
                        'fun': lambda x: np.sum(x) - 1})

        # Abridged Portfolio - calculate max Sharpe Ratio and associated weights
        abridged_efficient_portfolios = self.__get_efficient_frontier(abridged_avg_returns, abridged_cov_mat, rtns_range)
        abridged_vols_range = [x['fun'] for x in abridged_efficient_portfolios]
        abridged_n_assets = len(abridged_avg_returns)
        abridged_args = (abridged_avg_returns, abridged_cov_mat, risk_free_rate)
        abridged_bounds = tuple((0,1) for asset in range(abridged_n_assets))
        abridged_initial_guess = abridged_n_assets * [1. / abridged_n_assets]
        abridged_max_sharpe_portf = sco.minimize(self.__neg_sharpe_ratio,
                                        x0=abridged_initial_guess,
                                        args=abridged_args,
                                        method='SLSQP',
                                        bounds=abridged_bounds,
                                        constraints=constraints)
        abridged_max_sharpe_portf_weights = abridged_max_sharpe_portf['x']
        abridged_max_sharpe_portf = {
            'AverageReturns': abridged_avg_returns,
            'CovarianceMatrix': abridged_cov_mat,
            'VolsRange': abridged_vols_range,
            'ReturnsRange': rtns_range,
            'Results': abridged_portf_results_df,
            'Return': self.__get_portf_rtn(abridged_max_sharpe_portf_weights, abridged_avg_returns),
            'Volatility': self.__get_portf_vol(abridged_max_sharpe_portf_weights, abridged_avg_returns, abridged_cov_mat),
            'Sharpe Ratio': -abridged_max_sharpe_portf['fun'],
            'Weights': abridged_max_sharpe_portf_weights
        }

        # update weights in abridged_fund_selection, this also updates the proxy funds in full_fund_selection
        for i in range(len(self._fund_selection)):
            self._fund_selection[i].weight = abridged_max_sharpe_portf['Weights'][i]        
        
        return abridged_max_sharpe_portf
    
    #Plot the calculated Efficient Frontier, together with the simulated portfolios

    def plot_efficient_frontier(self, full_max_sharpe_portf):
        fig, ax = plt.subplots()
        full_max_sharpe_portf['Results'].plot(kind='scatter', x='volatility',
                            y='returns', c='sharpe_ratio',
                            cmap='PuBu', edgecolors='white',
                            figsize=(9, 6),
                            alpha=0.1,
                            ax=ax)

        # draw the efficient frontier lines
        ax.plot(full_max_sharpe_portf['VolsRange'], full_max_sharpe_portf['ReturnsRange'], 'b--', linewidth=2, alpha=0.3)

        # Add full individual funds    
        for asset_index in range(len(self.fund_selection)):
            fund = self.fund_selection[asset_index]
            ax.scatter(x=np.sqrt(full_max_sharpe_portf['CovarianceMatrix'].iloc[asset_index, asset_index]),
                    y=full_max_sharpe_portf['AverageReturns'][asset_index],
                    marker='*',
                    s=200,
                    alpha=1,
                    color=ColorHash(fund.name).hex,
                    label=fund.name[:30]+'..')

        ax.scatter(x=full_max_sharpe_portf['Volatility'],   
                y=full_max_sharpe_portf['Return'],
                marker='*',
                s=240,
                alpha=1,
                color='yellowgreen', edgecolors='black', linewidths=1,
                label='Max Sharpe Ratio (Full)')    

        ax.legend()

        ax.set(xlabel='Volatility',ylabel='Expected Returns', title='Efficient Frontier')

    def plot_backtest_portfolio(self, full_max_sharpe_portf):
        import plotly.express as plot

        full_weights = full_max_sharpe_portf['Weights']
        graph_prices = self.daily_price_history.copy()

        names_with_weights = []
        weight_colours=[]

        for i in range(len(self.fund_selection)):
            fund = self.fund_selection[i]
            names_with_weights.append(f"{round(full_weights[i],4)*100.0:.2f}% - {fund.name[:30]+'..'}")
            colour = ColorHash(fund.name, lightness=[0.28]).hex
            weight_colours.append(colour)

        graph_prices.columns = names_with_weights

        # Add total to graph
        title = f'Highest Sharpe Ratio ({full_max_sharpe_portf["Sharpe Ratio"]:.2f}) for given return portfolio {full_max_sharpe_portf["Return"]*100.0:.2f}% return'
        total_prices = self.daily_price_history.dot(full_max_sharpe_portf['Weights'])
        graph_prices['Highest Sharpe Ratio for given return portfolio'] = total_prices
        names_with_weights.append(title)
        weight_colours.append("yellow")

        # Add benchmark to graph
        graph_prices['Benchmark (4PHUXACHE)'] = self.get_benchmark_daily_price_history()
        names_with_weights.append(f'Benchmark (4PHUXACHE)')
        weight_colours.append("orange")

        fig = plot.line(graph_prices, x=graph_prices.index, y=graph_prices.columns, 
                        title=title,
                        labels={'value': 'Price', 'variable': 'Instrument'},
                        template='plotly_dark',
                        color_discrete_sequence=weight_colours
                        )
        fig.update_layout(autosize=False, width=1200, height=800 )
        fig.show()

    def plot_returns_tear_sheet(self, full_max_sharpe_portf):
        portfolio_returns = self.adjusted_returns.dot(full_max_sharpe_portf['Weights'])
        benchmark_weights = [f.weight for f in self.benchmark]
        benchmark_returns = self.benchmark_adjusted_returns.dot(benchmark_weights)

        return self.__create_returns_tear_sheet(portfolio_returns, benchmark_rets=benchmark_returns)

    # https://github.com/quantopian/pyfolio/blob/master/pyfolio/tears.py#L409
    def __create_returns_tear_sheet(self, returns, positions=None,
                                    transactions=None,
                                    live_start_date=None,
                                    cone_std=(1.0, 1.5, 2.0),
                                    benchmark_rets=None,
                                    bootstrap=False,
                                    turnover_denom='AGB',
                                    header_rows=None,
                                    return_fig=False):
            
        if benchmark_rets is not None:
            returns = utils.clip_returns_to_benchmark(returns, benchmark_rets)

        plotting.show_perf_stats(returns, benchmark_rets,
                                positions=positions,
                                transactions=transactions,
                                turnover_denom=turnover_denom,
                                bootstrap=bootstrap,
                                live_start_date=live_start_date,
                                header_rows=header_rows)

        plotting.show_worst_drawdown_periods(returns)

        vertical_sections = 11

        if live_start_date is not None:
            vertical_sections += 1
            live_start_date = ep.utils.get_utc_timestamp(live_start_date)

        if benchmark_rets is not None:
            vertical_sections += 1

        fig = plt.figure(figsize=(14, vertical_sections * 4))
        gs = gridspec.GridSpec(vertical_sections, 3, wspace=0.5, hspace=0.5)

        i = 2

        ax_rolling_returns = plt.subplot(gs[i, :-1])
        ax_annual_returns = plt.subplot(gs[i, 2])

        i+=1

        ax_drawdown = plt.subplot(gs[i, 0])
        ax_monthly_heatmap = plt.subplot(gs[i, 1])
        ax_returns = plt.subplot(gs[i, 2])

        i+=1 

        ax_underwater = plt.subplot(gs[i, 0])
        ax_monthly_dist = plt.subplot(gs[i, 1])
        ax_rolling_volatility = plt.subplot(gs[i, 2])
        i += 1

        plotting.plot_returns(returns, live_start_date=live_start_date, ax=ax_returns)
        ax_returns.set_title('Returns')

        plotting.plot_rolling_volatility(returns, factor_returns=benchmark_rets, ax=ax_rolling_volatility)

        # Drawdowns
        plotting.plot_drawdown_periods(returns, top=5, ax=ax_drawdown)
        plotting.plot_drawdown_underwater(returns=returns, ax=ax_underwater)

        plotting.plot_rolling_returns(
            returns,
            factor_returns=benchmark_rets,
            live_start_date=live_start_date,
            cone_std=cone_std,
            ax=ax_rolling_returns)
        ax_rolling_returns.set_title('Cumulative returns')

        plotting.plot_monthly_returns_heatmap(returns, ax=ax_monthly_heatmap)
        plotting.plot_annual_returns(returns, ax=ax_annual_returns)
        plotting.plot_monthly_returns_dist(returns, ax=ax_monthly_dist)

        for ax in fig.axes:
            plt.setp(ax.get_xticklabels(), visible=True)

        if return_fig:
            return fig

    def __get_portf_rtn(self, w, avg_rtns):
        return np.sum(avg_rtns * w)

    def __get_portf_vol(self, w, avg_rtns, cov_mat):
        return np.sqrt(np.dot(w.T, np.dot(cov_mat, w)))

    def __neg_sharpe_ratio(self, w, avg_rtns, cov_mat, rf_rate):
        portf_returns = np.sum(avg_rtns * w)
        portf_volatility = np.sqrt(np.dot(w.T, np.dot(cov_mat, w)))
        portf_sharpe_ratio = (portf_returns - rf_rate) / portf_volatility
        return -portf_sharpe_ratio
    
