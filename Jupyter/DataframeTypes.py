import pandera.pandas as pa
from pandera.typing import Series, Index

fund_monthly_returns_type_schema = pa.DataFrameSchema(
    {
        "Value": pa.Column(float)
    },
    index = pa.Index(str),
    strict = True,
    coerce = True
)

fund_adjusted_returns_type_schema = pa.DataFrameSchema(
    {
        "LogValue": pa.Column(float)
    },
    index = pa.Index(str),
    strict = True,
    coerce = True
)