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

    # def get_min_max_dates(self, names_csv):
    #     min_max_data_query = f"WITH FundDateRanges as \
    #     ( \
    #         select I.Id, I.[Name], min(AR.Date) as minDate, max(AR.Date) as maxDate \
    #             from dbo.Instrument I \
    #             join dbo.AdjustedReturn AR on AR.InstrumentId = i.Id \
    #         where name in \
    #         ( \
    #             {names_csv} \
    #         ) \
    #         group by I.Id, I.[Name] \
    #     ) \
    #     select max(minDate) as minDate, min(maxDate) as maxDate from FundDateRanges" 
    #     #print(minMaxDateQuery)

    #     with self._db_engine.connect() as connection:
    #         min_max_date = pd.read_sql_query(min_max_data_query, connection)

    #     min_date = min_max_date['minDate'][0]
    #     max_date = min_max_date['maxDate'][0]

    #     return min_date, max_date
    
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
        
        df.set_index('Date', inplace=True)
        
        return df    
    
    def get_daily_log_adjusted_return(self, fund_name, years):
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
        
        df.set_index('Date', inplace=True)

        return df



