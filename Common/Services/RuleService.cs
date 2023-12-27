using NetLimiter.Service;
using System.Linq;

namespace Common.Services
{
    public class RuleService : IRuleService
    {
        public bool IsRuleActive(Rule rule)
        {
            return rule.State == RuleState.Active;
        }

        public void DisableRuleById(string ruleId)
        {
            using (var client = new NLClient())
            {
                client.Connect();

                var rules = client.Rules;

                var ruleToDisable = rules.FirstOrDefault(r => r.Id == ruleId);

                if (ruleToDisable != null)
                {
                    ruleToDisable.IsEnabled = false;
                    client.UpdateRule(ruleToDisable);
                }
            }
        }
    }
}
