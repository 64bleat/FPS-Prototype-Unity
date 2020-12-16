using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace MPCore
{
    public class SkillSet
    {
        private readonly Dictionary<string, Skill> skillset = new Dictionary<string, Skill>();
        private readonly Dictionary<string, List<SafeAction>> skillChangeListeners = new Dictionary<string, List<SafeAction>>();

        #region Public Methods

        /// <summary>
        /// Add a skill to the skill set.
        /// </summary>
        /// <param name="skillName"> Name of the skill to be added. </param>
        /// <param name="startingExperience"> Skill starts with this amount of experience. </param>
        /// <param name="previousLevelRequirement"> This skill will not recieve experience until its parent skill reaches this level. </param>
        public void AddSkill(string skillName, long startingExperience = 0, int previousLevelRequirement = 0)
        {
            Skill skill = new Skill
            {
                name = skillName,
                experience = startingExperience,
                previousLevelRequirement = previousLevelRequirement
            };

            if (!skillset.ContainsKey(skillName))
            {
                skillset.Add(skillName, skill);
                NotifyListenersFor(skill);
            }
        }

        /// <summary>
        /// Add experience to the selected skills, divided up by the factors.
        /// </summary>
        /// <param name="experience"> The total amount of experience to be added. </param>
        /// <param name="skillFactors"> format: "skill=factor;...;skill=factor" </param>
        public void AddExperience(double experience, string skillFactors)
        {
            int lastLevel = int.MaxValue;

            foreach (SkillFactor sf in ParseSkillFactorString(skillFactors))
                if (skillset.TryGetValue(sf.name, out Skill skill) && lastLevel >= skill.previousLevelRequirement)
                {
                    if (skill.AddExperience(experience * sf.factor))
                        NotifyListenersFor(skill);

                    lastLevel = skill.Level;
                }
                else
                    return;
        }

        /// <summary>
        /// Bind an action to be called when a specific skill level changes
        /// </summary>
        /// <param name="skillName"> The name of the skill to be listened for.</param>
        /// <param name="action"> The action to be called upon level change.</param>
        /// <param name="safetyKey"> An object that will become null when the action cannot be completed.</param>
        public void AddListenerFor(string skillName, Action<int> action, object safetyKey)
        {
            SafeAction safeAction = new SafeAction
            {
                action = action,
                key = safetyKey
            };

            if (skillChangeListeners.TryGetValue(skillName, out List<SafeAction> list))
                list.Add(safeAction);
            else
                skillChangeListeners.Add(skillName, new List<SafeAction>() { safeAction });

            if (skillset.TryGetValue(skillName, out Skill skill) && safetyKey != null)
                action(skill.Level);
        }

        /// <summary>
        /// Get the computed level for the provided skills and factors.
        /// </summary>
        /// <param name="skillFactors"> format: "skill=factor;...;skill=factor" </param>
        /// <returns> A positive int value representing level. (0 to 100+) </returns>
        public int GetLevel(string skillFactors)
        {
            double tally = 0;

            foreach (SkillFactor sf in ParseSkillFactorString(skillFactors))
                if (skillset.TryGetValue(sf.name, out Skill skill))
                    tally += skill.Level * sf.factor;

            return (int)Math.Floor(tally);
        }

        #endregion
        #region Private Methods

        /// <summary>
        /// Passes a skill's current level to the listening Actions.
        /// </summary>
        /// <param name="skill"> Actions listening to this skill will be called. </param>
        private void NotifyListenersFor(Skill skill)
        {
            int level = skill.Level;

            if (skillChangeListeners.TryGetValue(skill.name, out List<SafeAction> actionList))
                for (int i = 0; i < actionList.Count; i++)
                    if (actionList[i].key != null)
                        actionList[i].action(level);
                    else
                        actionList.RemoveAt(i--);
        }

        /// <summary>
        /// parses a string into a list of skill/factor pairs.
        /// </summary>
        /// <param name="skillFactors"> format: "skill=factor;...;skill=factor" </param>
        /// <returns> A list of skill/factor pairs. </returns>
        private static SkillFactor[] ParseSkillFactorString(string skillFactors)
        {
            string[] pairs = skillFactors.Split(';');
            SkillFactor[] list = new SkillFactor[pairs.Length];
            double factorTotal = 0;

            for (int i = 0; i < pairs.Length; i++)
            {
                string[] pair = pairs[i].Split('=');
                double factor = Math.Max(1, double.TryParse(pair[1], out factor) ? factor : 1);

                factorTotal += factor;

                list[i] = new SkillFactor { name = pair[0], factor = factor };
            }

            for (int i = 0; i < list.Length; i++)
                list[i].factor /= factorTotal;

            return list;
        }

        #endregion
        #region Private Structs

        private struct Skill
        {
            public double experience;
            public string name;
            public int previousLevelRequirement;

            public bool AddExperience(double exp)
            {
                int level = Level;

                experience = Math.Max(0, experience + exp);

                return level != Level;
            }

            public int Level
            {
                get => (int)Math.Floor(Math.Pow(Math.Log(experience) / Math.Log(100000), 4) * 100);
            }
        }

        private struct SkillFactor
        {
            public string name;
            public double factor;
        }

        private struct SafeAction
        {
            public Action<int> action;
            public object key;
        }
    }

    #endregion
}
