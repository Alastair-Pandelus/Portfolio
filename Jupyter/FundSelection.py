class FundSelection:
    def __init__(self, name: str, weight: float = None, proxy_funds: list = None):
        self.name = name
        self.weight = weight
        self.proxy_funds = proxy_funds if proxy_funds is not None else []
        