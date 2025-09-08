from colorhash import ColorHash

class FundSelection:
    def __init__(self, name: str, weight: float = None, proxy_funds: list = None):
        self.name = name
        self.weight = weight
        self.proxy_funds = proxy_funds if proxy_funds is not None else []

    def short_name(self):
        return self.name[:30]+'..'
    
    def colour(self):
        return ColorHash(self.name, lightness=[0.28]).hex
        