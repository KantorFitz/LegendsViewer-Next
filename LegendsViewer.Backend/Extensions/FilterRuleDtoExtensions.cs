using LegendsViewer.Backend.Contracts;

namespace LegendsViewer.Backend.Extensions;

public static class FilterRuleDtoExtensions
{
    public static bool ViolatesIntegerCriteria(this FilterRuleDto rule, int actualValue, int ruleValue)
    {
        if (rule.Operator == FilterOperator.Equals && actualValue != ruleValue)
        {
            return true;
        }
        if (rule.Operator == FilterOperator.NotEquals && actualValue == ruleValue)
        {
            return true;
        }
        if (rule.Operator == FilterOperator.GreaterThan && actualValue <= ruleValue)
        {
            return true;
        }
        if (rule.Operator == FilterOperator.LessThan && actualValue >= ruleValue)
        {
            return true;
        }

        return false;
    }

    public static bool ViolatesBooleanCriteria(this FilterRuleDto rule, bool actualValue)
    {
        if (rule.Operator == FilterOperator.Equals)
        {
            if (rule.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) && !actualValue)
            {
                return true;
            }
            else if (rule.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase) && actualValue)
            {
                return true;
            }
        }
        if (rule.Operator == FilterOperator.NotEquals)
        {
            if (rule.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase) && !actualValue)
            {
                return true;
            }
            else if (rule.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) && actualValue)
            {
                return true;
            }
        }

        return false;
    }
}
