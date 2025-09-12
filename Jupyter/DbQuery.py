import pandas as pd
from datetime import timedelta

class DbQuery:
    def __init__(self):
        # https://stackoverflow.com/questions/4657919/sql-server-does-not-exist-or-access-denied
        # https://stackoverflow.com/questions/71082494/getting-a-warning-when-using-a-pyodbc-connection-object-with-pandas

        from sqlalchemy.engine import URL
        connection_string = 'DRIVER={ODBC Driver 17 for SQL Server};server=localhost;database=Portfolio;trusted_connection=yes;'
        connection_url = URL.create('mssql+pyodbc', query={'odbc_connect': connection_string})

        from sqlalchemy import create_engine
        self._db_engine = create_engine(connection_url)        

    def get_monthly_returns(self, fund_name, years):
        query = f"""
                    SELECT 
                        CAST(FORMAT(AR.[Date], 'yyyy-MM-01') as date) AS Date,
                        SUM(AR.LogValue) AS Value
                    FROM 
                        dbo.AdjustedReturn AR
                    join
                        dbo.Instrument I on Ar.InstrumentId = I.Id
                    where
                        I.Name = '{fund_name}'
                        and Date >= DATEADD(yy, -{years}, GETDATE())
                    GROUP BY 
                        FORMAT([Date], 'yyyy-MM-01')
                    ORDER BY 
                        Date ASC
        """
        
        with self._db_engine.connect() as connection:
            df = pd.read_sql_query(query, connection)
            df['Date'] = pd.to_datetime(df['Date'])
        
        df.set_index('Date', inplace=True)
        
        return df    
    
    def get_daily_log_adjusted_return(self, fund_name, years, proxy_funds: list = None):
        query = f"""
        select 
            AR.Date, 
            AR.LogValue
        from 
            dbo.Instrument I
            join dbo.AdjustedReturn AR on AR.InstrumentId = I.Id
        where 
            I.[Name] = '{fund_name}'
            and AR.Date >= DATEADD(yy, -{years}, GETDATE())
        order by AR.Date ASC
        """
        
        with self._db_engine.connect() as connection:
            df = pd.read_sql_query(query, connection)
            df['Date'] = pd.to_datetime(df['Date'])
        
        df.set_index('Date', inplace=True)

        return df

    def get_portfolio(self, names_csv):
        query = f"""select 
	Name, 
	InstrumentType as Type, 
	IMASectorName as Sector, 
	ISIN, 
	'https://uk.finance.yahoo.com/quote/' + y_symbol as Yahoo, 
	'http://ajbell.co.uk/market-research/' + MarketCode as AjBell,
    EquityHoldings as Holds,
	Round(PERatio, 1) as 'P/E',
	RatioUnitedStates as US,
	RatioTechnology as Tech,
	StockLong as 'Stock L',
	StockShort as 'Stock S',
	BondLong as 'Bond L',
	BondShort as 'Bond S', 
	CashLong as 'Cash L',
	CashShort as 'Cash S',
	OngoingCharge as 'Charge',
	Correlation as 'Corr',
	MaxDrawdown as 'Drawdown',
	YR_ReturnM12_5 / 100.0 as '2020',
	YR_ReturnM12_4 / 100.0 as '2021',
	YR_ReturnM12_3 / 100.0 as '2022',
	YR_ReturnM12_2 / 100.0 as '2023',
	YR_ReturnM12_1 / 100.0 as '2024'
from 
	dbo.Instrument 
where 
	name in ({names_csv})"""

        with self._db_engine.connect() as connection:
            results = pd.read_sql_query(query, connection)
            results.set_index("Name", inplace=True)
            return results



