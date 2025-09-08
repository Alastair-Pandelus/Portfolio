from collections import OrderedDict
from datetime import timedelta
from typing import List
from colorhash import ColorHash
from matplotlib.ticker import FuncFormatter
import numpy as np
import pandas as pd
from functools import reduce
from DbQuery import DbQuery
from DataframeTypes import fund_monthly_returns_type_schema, fund_adjusted_returns_type_schema
from FundSelection import FundSelection
import scipy.optimize as sco
from matplotlib import pyplot as plt
from matplotlib.lines import Line2D
from pyfolio import utils, plotting, timeseries
import empyrical as ep
import matplotlib.gridspec as gridspec
import seaborn as sns

TRADING_DAYS_IN_YEAR = 252
APPROX_BDAYS_PER_MONTH = 21
PORTFOLIO_COLOUR = "red"

STAT_FUNCS_PCT = [
    "Annual return",
    "Cumulative returns",
    "Annual volatility",
    "Max drawdown",
    "Daily value at risk",
    "Daily turnover",
]

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

        weights = np.random.random(size=(iterations, len(self.fund_selection)))
        weights /= np.sum(weights, axis=1)[:, np.newaxis]

        avg_returns = self._adjusted_returns.mean() * TRADING_DAYS_IN_YEAR
        cov_mat = self._adjusted_returns.cov() * TRADING_DAYS_IN_YEAR

        portf_rtns = np.dot(weights, avg_returns)
        portf_vol = []

        # run Monte Carlo
        for i in range(0, len(weights)):
            portf_vol.append(np.sqrt(np.dot(weights[i].T, np.dot(cov_mat, weights[i]))))

        portf_vol = np.array(portf_vol)
        portf_sharpe_ratio = (portf_rtns - risk_free_rate) / portf_vol

        portf_results_df = pd.DataFrame({'returns': portf_rtns,
                                        'volatility': portf_vol,
                                        'sharpe_ratio': portf_sharpe_ratio})
        
        # Calculate the efficient frontier to 'efficient_portfolios' variable using scipy.optimize
        rtns_range = np.linspace(start=0.07, stop=0.13, num=200)
        constraints = ({'type': 'eq',
                        'fun': lambda x: np.sum(x) - 1})

        # Abridged Portfolio - calculate max Sharpe Ratio and associated weights
        efficient_portfolios = self.__get_efficient_frontier(avg_returns, cov_mat, rtns_range)
        vols_range = [x['fun'] for x in efficient_portfolios]
        n_assets = len(avg_returns)
        args = (avg_returns, cov_mat, risk_free_rate)
        bounds = tuple((0,1) for asset in range(n_assets))
        initial_guess = n_assets * [1. / n_assets]
        max_sharpe_portf = sco.minimize(self.__neg_sharpe_ratio,
                                        x0=initial_guess,
                                        args=args,
                                        method='SLSQP',
                                        bounds=bounds,
                                        constraints=constraints)
        max_sharpe_portf_weights = max_sharpe_portf['x']
        max_sharpe_portf = {
            'AverageReturns': avg_returns,
            'CovarianceMatrix': cov_mat,
            'VolsRange': vols_range,
            'ReturnsRange': rtns_range,
            'Results': portf_results_df,
            'Return': self.__get_portf_rtn(max_sharpe_portf_weights, avg_returns),
            'Volatility': self.__get_portf_vol(max_sharpe_portf_weights, avg_returns, cov_mat),
            'Sharpe Ratio': -max_sharpe_portf['fun'],
            'Weights': max_sharpe_portf_weights
        }

        # update weights in fund_selection, this also updates the proxy funds in fund_selection
        for i in range(len(self._fund_selection)):
            self._fund_selection[i].weight = max_sharpe_portf['Weights'][i]        
        
        return max_sharpe_portf
    
    #Plot the calculated Efficient Frontier, together with the simulated portfolios
    def __plot_efficient_frontier(self, max_sharpe_portf, ax=None):
        if ax == None:
            ax = plt.subplots()

        max_sharpe_portf['Results'].plot(kind='scatter', x='volatility',
                             y='returns', c='sharpe_ratio',
                             cmap='turbo', edgecolors='white',
                             alpha=0.6,
                             ax=ax)

        # draw the efficient frontier lines
        ax.plot(max_sharpe_portf['VolsRange'], max_sharpe_portf['ReturnsRange'], 'b--', linewidth=2, alpha=0.3)

        # Add full individual funds    
        for asset_index in range(len(self.fund_selection)):
            fund = self.fund_selection[asset_index]
            ax.scatter(x=np.sqrt(max_sharpe_portf['CovarianceMatrix'].iloc[asset_index, asset_index]),
                    y=max_sharpe_portf['AverageReturns'][asset_index],
                    marker='*',
                    s=200,
                    alpha=1,
                    color=ColorHash(fund.name).hex,
                    label=fund.short_name())

        ax.scatter(x=max_sharpe_portf['Volatility'],   
                y=max_sharpe_portf['Return'],
                marker='*',
                s=240,
                alpha=1,
                color=PORTFOLIO_COLOUR, edgecolors='black', linewidths=1,
                label='Max Sharpe Ratio (Full)')    

        ax.legend(fontsize='small')

        ax.set(xlabel='Volatility',ylabel='Expected Returns', title='Efficient Frontier')

    def plot_returns_tear_sheet(self, max_sharpe_portf):
        portfolio_returns = self.adjusted_returns.dot(max_sharpe_portf['Weights'])
        benchmark_weights = [f.weight for f in self.benchmark]
        benchmark_returns = self.benchmark_adjusted_returns.dot(benchmark_weights)

        return self.__create_returns_tear_sheet(portfolio_returns, max_sharpe_portf=max_sharpe_portf, benchmark_rets=benchmark_returns)

    # https://github.com/quantopian/pyfolio/blob/master/pyfolio/tears.py#L409
    def __create_returns_tear_sheet(self, 
                                    returns, 
                                    max_sharpe_portf,
                                    positions=None,
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

        performace_stats_df = plotting.show_perf_stats(returns, 
                               benchmark_rets,
                                positions=positions,
                                transactions=transactions,
                                turnover_denom=turnover_denom,
                                bootstrap=bootstrap,
                                live_start_date=live_start_date,
                                header_rows=header_rows, 
                                return_df=True).drop('Kurtosis').drop('Calmar ratio').drop('Stability').drop('Omega ratio').drop('Tail ratio').drop('Alpha').drop('Beta').drop('Skew').drop('Daily value at risk')
        
        vertical_sections = 11

        if live_start_date is not None:
            vertical_sections += 1
            live_start_date = ep.utils.get_utc_timestamp(live_start_date)

        if benchmark_rets is not None:
            vertical_sections += 1

        fig = plt.figure(figsize=(14, vertical_sections * 4))
        gs = gridspec.GridSpec(vertical_sections, 3, wspace=0.5, hspace=0.5)

        i = 2

        ax_top_0 = plt.subplot(gs[i, 0])
        # correlation matrix spills leftwards
        ax_top_12 = plt.subplot(gs[i, 2])
        i+=1 

        ax_text_table = plt.subplot(gs[i, 0])
        ax_rolling_returns = plt.subplot(gs[i, 1:])
        i+=1

        ax_drawdown = plt.subplot(gs[i, 0])
        ax_monthly_heatmap = plt.subplot(gs[i, 1])
        ax_annual_returns = plt.subplot(gs[i, 2])
        i+=1 

        ax_underwater = plt.subplot(gs[i, 0])
        ax_efficient_frontier = plt.subplot(gs[i,1:])
        i += 1

        ax_returns = plt.subplot(gs[i, 0])
        ax_monthly_dist = plt.subplot(gs[i, 1])
        ax_rolling_volatility = plt.subplot(gs[i, 2])

        #i+=1
        #ax_covariance_matrix = plt.subplot(gs[i,0])

        self.__plot_dataframe_text(performace_stats_df, ax=ax_top_0)
        self.__plot_heatmap(ax=ax_top_12)

        self.__plot_legend(ax_text_table)

        plotting.plot_returns(returns, live_start_date=live_start_date, ax=ax_returns)
        ax_returns.set_title('Returns')

        plotting.plot_rolling_volatility(returns, factor_returns=benchmark_rets, ax=ax_rolling_volatility)

        # Drawdowns
        plotting.plot_drawdown_periods(returns, top=5, ax=ax_drawdown)
        plotting.plot_drawdown_underwater(returns=returns, ax=ax_underwater)

        self.__plot_rolling_returns(returns, factor_returns=benchmark_rets, live_start_date=live_start_date, ax=ax_rolling_returns)
        ax_rolling_returns.set_title('Cumulative returns')

        plotting.plot_monthly_returns_heatmap(returns, ax=ax_monthly_heatmap)
        plotting.plot_annual_returns(returns, ax=ax_annual_returns)
        plotting.plot_monthly_returns_dist(returns, ax=ax_monthly_dist)

        self.__plot_efficient_frontier(max_sharpe_portf, ax=ax_efficient_frontier)

        
        #self.plot_backtest_portfolio(max_sharpe_portf, ax=ax_covariance_matrix)

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
    
    def __plot_dataframe_text(self, df : pd.DataFrame, ax):
        ax.xaxis.set_visible(False)
        ax.yaxis.set_visible(False)
        ax.set_frame_on(False)

        table_data = [[key, df.loc[key]['Backtest']] for key in [key for key in df.index]]

        table = ax.table(cellText=table_data, colLabels=['Portfolio', f'{self._price_years} Years'], loc='center')
        table.auto_set_font_size(False)
        table.scale(1.5, 1.5)
        table.set_fontsize(10)
    
    def __plot_legend(self, ax):
        legend_elements = []

        legend_elements.append(Line2D([0], [0], color=PORTFOLIO_COLOUR, lw=2, label="Portfolio"))
        legend_elements.append(Line2D([0], [0], color="black", lw=2, label="Benchmark"))
        for fund in self.fund_selection:
            legend_elements.append(Line2D([0], [0], color=fund.colour(), lw=1, label=f"{(fund.weight*100):.2f}% - {fund.short_name()}"))

        ax.axis('off')
        ax.legend(handles=legend_elements, loc='center')

    def __plot_heatmap(
        self, 
        ax
    ):
        sns.heatmap(self.correlation_matrix, annot=True, fmt=".2f", cmap="Blues", cbar=True, ax=ax, yticklabels=True, xticklabels=False)
        plt.title("Correlation Matrix")
        #plt.tight_layout()

        return

    def __plot_rolling_returns(
        self,
        returns,
        factor_returns=None,
        live_start_date=None,
        logy=False,
        cone_std=None,
        legend_loc="best",
        volatility_match=False,
        cone_function=timeseries.forecast_cone_bootstrap,
        ax=None,
        **kwargs,
    ):
        if ax is None:
            ax = plt.gca()

        ax.set_xlabel("")
        ax.set_ylabel("Cumulative returns")
        ax.set_yscale("log" if logy else "linear")

        if volatility_match and factor_returns is None:
            raise ValueError("volatility_match requires passing of factor_returns.")
        elif volatility_match and factor_returns is not None:
            bmark_vol = factor_returns.loc[returns.index].std()
            returns = (returns / returns.std()) * bmark_vol

        cum_rets = ep.cum_returns(returns, 1.0)
        
        fund_cum_returns=[]
        for fund in self.fund_selection:
            fund_cum_returns.append(ep.cum_returns(self.adjusted_returns[fund.name], 1.0))

        y_axis_formatter = FuncFormatter(utils.two_dec_places)
        ax.yaxis.set_major_formatter(FuncFormatter(y_axis_formatter))

        # benchmark
        if factor_returns is not None:
            cum_factor_returns = ep.cum_returns(factor_returns.loc[cum_rets.index], 1.0)
            cum_factor_returns.plot(
                lw=2,
                color="black",
                alpha=0.8,
                ax=ax, 
                **kwargs,
            )

        if live_start_date is not None:
            live_start_date = ep.utils.get_utc_timestamp(live_start_date)
            is_cum_returns = cum_rets.loc[cum_rets.index < live_start_date]
            oos_cum_returns = cum_rets.loc[cum_rets.index >= live_start_date]
        else:
            is_cum_returns = cum_rets
            oos_cum_returns = pd.Series([], dtype="float64")

        is_cum_returns.plot(
            lw=2, 
            color=PORTFOLIO_COLOUR, 
            alpha=0.8, 
            ax=ax, 
            **kwargs
        )
        for i in range(len(fund_cum_returns)):
            fund = self.fund_selection[i]
            fund_cum_returns[i].plot(
                lw=1, 
                color=fund.colour(), 
                alpha=0.6, 
                ax=ax, 
                **kwargs)

        if len(oos_cum_returns) > 0:
            oos_cum_returns.plot(
                lw=2, color="red", alpha=0.6, label="Live", ax=ax, **kwargs
            )

        ax.axhline(1.0, linestyle="--", color="black", lw=1)

        return ax
    
    def show_perf_stats(
        self,
        returns,
        factor_returns=None,
        positions=None,
        transactions=None,
        turnover_denom="AGB",
        live_start_date=None,
        bootstrap=False,
        header_rows=None,
        return_df=False,
    ):
        perf_func = timeseries.perf_stats

        perf_stats_all = perf_func(
            returns,
            factor_returns=factor_returns,
            positions=positions,
            transactions=transactions,
            turnover_denom=turnover_denom,
        )

        date_rows = OrderedDict()
        if len(returns.index) > 0:
            date_rows["Start date"] = returns.index[0].strftime("%Y-%m-%d")
            date_rows["End date"] = returns.index[-1].strftime("%Y-%m-%d")

        if live_start_date is not None:
            live_start_date = ep.utils.get_utc_timestamp(live_start_date)
            returns_is = returns[returns.index < live_start_date]
            returns_oos = returns[returns.index >= live_start_date]

            positions_is = None
            positions_oos = None
            transactions_is = None
            transactions_oos = None

            if positions is not None:
                positions_is = positions[positions.index < live_start_date]
                positions_oos = positions[positions.index >= live_start_date]
                if transactions is not None:
                    transactions_is = transactions[(transactions.index < live_start_date)]
                    transactions_oos = transactions[(transactions.index > live_start_date)]

            perf_stats_is = perf_func(
                returns_is,
                factor_returns=factor_returns,
                positions=positions_is,
                transactions=transactions_is,
                turnover_denom=turnover_denom,
            )

            perf_stats_oos = perf_func(
                returns_oos,
                factor_returns=factor_returns,
                positions=positions_oos,
                transactions=transactions_oos,
                turnover_denom=turnover_denom,
            )
            if len(returns.index) > 0:
                date_rows["In-sample months"] = int(
                    len(returns_is) / APPROX_BDAYS_PER_MONTH
                )
                date_rows["Out-of-sample months"] = int(
                    len(returns_oos) / APPROX_BDAYS_PER_MONTH
                )

            perf_stats = pd.concat(
                OrderedDict(
                    [
                        ("In-sample", perf_stats_is),
                        ("Out-of-sample", perf_stats_oos),
                        ("All", perf_stats_all),
                    ]
                ),
                axis=1,
            )
        else:
            if len(returns.index) > 0:
                date_rows["Total months"] = int(len(returns) / APPROX_BDAYS_PER_MONTH)
            perf_stats = pd.DataFrame(perf_stats_all, columns=["Backtest"])

        for column in perf_stats.columns:
            for stat, value in perf_stats[column].items():
                if stat in STAT_FUNCS_PCT:
                    perf_stats.loc[stat, column] = str(np.round(value * 100, 3)) + "%"
        if header_rows is None:
            header_rows = date_rows
        else:
            header_rows = OrderedDict(header_rows)
            header_rows.update(date_rows)

        if return_df:
            return perf_stats
        utils.print_table(
            perf_stats,
            float_format="{0:.2f}".format,
            header_rows=header_rows,
        )


            
