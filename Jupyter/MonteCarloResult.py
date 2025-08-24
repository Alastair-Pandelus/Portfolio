class MonteCarloResult:
    def __init__(self, weights, annual_return, standard_deviation, sharpe_ratio, max_drawdown):
        self.weights = weights
        self.annual_return = annual_return
        self.standard_deviation = standard_deviation
        self.sharpe_ratio = sharpe_ratio
        self.max_drawdown = max_drawdown
