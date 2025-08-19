class MonteCarloResult:
    def __init__(self, weights, annual_return, standard_deviation, sharpe_ratio):
        self.weights = weights
        self.annual_return = annual_return
        self.standard_deviation = standard_deviation
        self.sharpe_ratio = sharpe_ratio
