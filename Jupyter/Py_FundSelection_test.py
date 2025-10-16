import unittest
from Py_FundSelection import FundSelection

class TestFundSelection(unittest.TestCase):
    def test_initialization_with_name_and_weight(self):
        fund = FundSelection("FundA", 0.5)
        self.assertEqual(fund.name, "FundA")
        self.assertEqual(fund.weight, 0.5)

    def test_initialization_with_name_only(self):
        fund = FundSelection("FundB")
        self.assertEqual(fund.name, "FundB")
        self.assertIsNone(fund.weight)

    def test_weight_can_be_float_zero(self):
        fund = FundSelection("FundC", 0.0)
        self.assertEqual(fund.weight, 0.0)

    def test_weight_can_be_negative(self):
        fund = FundSelection("FundD", -0.2)
        self.assertEqual(fund.weight, -0.2)

    def test_name_is_required(self):
        with self.assertRaises(TypeError):
            FundSelection()

if __name__ == "__main__":
    unittest.main()