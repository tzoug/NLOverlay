using NetLimiter.Service;

namespace NLOverlay.Services
{
    public interface IRuleService
    {
        /// <summary>
        /// Flag if the rule state is Active
        /// </summary>
        /// <param name="rule">Rule object</param>
        /// <returns>True or False</returns>
        bool IsRuleActive(Rule rule);

        /// <summary>
        /// Disables a rule with a the provided ID.
        /// </summary>
        /// <param name="ruleId">Rule ID string</param>
        void DisableRuleById(string ruleId);
    }
}
