﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma.Abilities
{
    class AbilityConditionCharacter : AbilityConditionData
    {
        private readonly List<TargetType> targetTypes;

        private readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

        public AbilityConditionCharacter(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
        {
            targetTypes = ParseTargetTypes(
                conditionElement.GetAttributeStringArray("targettypes", 
                conditionElement.GetAttributeStringArray("targettype", Array.Empty<string>())));

            foreach (ContentXElement subElement in conditionElement.Elements())
            {
                if (subElement.NameAsIdentifier() == "conditional")
                {
                    conditionals.AddRange(PropertyConditional.FromXElement(subElement));
                }
            }

            if (!targetTypes.Any() && !conditionals.Any())
            {
                DebugConsole.ThrowError($"Error in talent \"{characterTalent}\". No target types or conditionals defined - the condition will match any character.",
                    contentPackage: conditionElement.ContentPackage);
            }
        }

        protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
        {
            if (abilityObject is IAbilityCharacter abilityCharacter)
            {
                if (abilityCharacter.Character is not Character character) { return false; }
                if (!IsViableTarget(targetTypes, character)) { return false; }
                foreach (var conditional in conditionals)
                {
                    if (!conditional.Matches(character)) { return false; }
                }
                return true;
            }
            else
            {
                LogAbilityConditionError(abilityObject, typeof(IAbilityCharacter));
                return false;
            }
        }
    }
}
