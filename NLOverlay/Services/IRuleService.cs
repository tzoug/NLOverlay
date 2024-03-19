using NetLimiter.Service;
using NLOverlay.Models;
using NLOverlay.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        /// <summary>
        /// Get information about the rules from the NetLimiter service
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>RuleViewModel list</returns>
        ObservableCollection<RuleViewModel> GetRules(Settings settings);

        /// <summary>
        /// Creates a RuleViewModel from the values returned by the NetLimiter service
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="filters"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        RuleViewModel CreateRuleModel(Rule rule, IEnumerable<Filter> filters, Settings settings);

        /// <summary>
        /// Updates a RuleModel
        /// </summary>
        /// <param name="model"></param>
        /// <param name="settings"></param>
        void UpdateRuleModel(RuleViewModel model, Settings settings);
    }
}
