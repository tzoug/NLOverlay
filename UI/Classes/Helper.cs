using System.Collections.Generic;
using System.Linq;
using Common;
using NetLimiter.Service;
using UI.ViewModels;

namespace UI.Classes
{
    public class Helper
    {
        public RuleViewModel CreateRuleModel(Rule rule, IEnumerable<Filter> filters, SettingsData settings)
        {
            var ruleFor = filters.FirstOrDefault(f => f.Id == rule.FilterId)?.Name;
            
            return new RuleViewModel
            {
                Rule = rule,
                RuleFor = ruleFor,
                ShowOnOverlay = settings.RulesOnOverlay.Contains(rule.Id),
                FlashThresholdEnabled = settings.FlashThresholds.ContainsKey(rule.Id),
                FlashThreshold = settings.FlashThresholds.TryGetValue(rule.Id, out var flashValue) ? flashValue : 0,
                DisableThresholdEnabled = settings.DisableThresholds.ContainsKey(rule.Id),
                DisableThreshold = settings.DisableThresholds.TryGetValue(rule.Id, out var disableValue) ? disableValue : 0
            };
        }
        
        public void UpdateRuleModel(RuleViewModel model, SettingsData settings)
        {
            model.ShowOnOverlay = settings.RulesOnOverlay.Contains(model.Id);
            model.FlashThresholdEnabled = settings.FlashThresholds.ContainsKey(model.Id);
            model.FlashThreshold = settings.FlashThresholds.TryGetValue(model.Id, out var flashValue) ? flashValue : 0;
            model.DisableThresholdEnabled = settings.DisableThresholds.ContainsKey(model.Id);
            model.DisableThreshold =
                settings.DisableThresholds.TryGetValue(model.Id, out var disableValue) ? disableValue : 0;
        }
    }
}