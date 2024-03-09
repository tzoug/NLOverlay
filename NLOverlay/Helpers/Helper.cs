using NetLimiter.Service;
using NLOverlay.Models;
using NLOverlay.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace NLOverlay.Helpers
{
    public class Helper
    {
        public RuleViewModel CreateRuleModel(Rule rule, IEnumerable<Filter> filters, Settings settings)
        {
            var ruleFor = filters.FirstOrDefault(f => f.Id == rule.FilterId)?.Name;
            
            return new RuleViewModel
            {
                Rule = rule,
                RuleFor = ruleFor,
                ShowOnOverlay = settings.RulesOnOverlay.Contains(rule.Id),
                HighlightThresholdEnabled = settings.HighlightThresholds.ContainsKey(rule.Id),
                HighlightThreshold = settings.HighlightThresholds.TryGetValue(rule.Id, out var highlightValue) ? highlightValue : 0,
                DisableThresholdEnabled = settings.DisableThresholds.ContainsKey(rule.Id),
                DisableThreshold = settings.DisableThresholds.TryGetValue(rule.Id, out var disableValue) ? disableValue : 0
            };
        }
        
        public void UpdateRuleModel(RuleViewModel model, Settings settings)
        {
            model.ShowOnOverlay = settings.RulesOnOverlay.Contains(model.Id);
            model.HighlightThresholdEnabled = settings.HighlightThresholds.ContainsKey(model.Id);
            model.HighlightThreshold = settings.HighlightThresholds.TryGetValue(model.Id, out var highlightValue) ? highlightValue : 0;
            model.DisableThresholdEnabled = settings.DisableThresholds.ContainsKey(model.Id);
            model.DisableThreshold = settings.DisableThresholds.TryGetValue(model.Id, out var disableValue) ? disableValue : 0;
        }
    }
}