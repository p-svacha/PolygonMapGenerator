using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class NewsEvent_DistrictDensityChange : NewsEvent
    {
        public District District { get; private set; }
        public DensityDef DensityBefore { get; private set; }
        public DensityDef DensityAfter { get; private set; }
        private bool IsDenser => DensityAfter.SortingOrder > DensityBefore.SortingOrder;

        public NewsEvent_DistrictDensityChange(District district, DensityDef densityBefore, DensityDef densityAfter) : base()
        {
            District = district;
            DensityBefore = densityBefore;
            DensityAfter = densityAfter;
        }

        public override string GetArticleHeadline()
        {
            List<string> denser = new List<string>()
            {
                $"{District.Name} Grows Denser",
                $"The Crowds Arrive in {District.Name}",
                $"{District.Name} Fills Up",
                $"Bustle Comes to {District.Name}",
            };

            List<string> sparser = new List<string>()
            {
                $"{District.Name} Thins Out",
                $"Quiet Settles Over {District.Name}",
                $"{District.Name} Empties Out",
                $"Room to Breathe in {District.Name}",
            };

            return IsDenser ? denser.RandomElement() : sparser.RandomElement();
        }

        public override string GetArticleBody()
        {
            List<string> facts = new List<string>()
            {
                $"{District.Name} is now classed as a {DensityAfter.Label.ToLower()}-density district, up from {DensityBefore.Label.ToLower()}.",
                $"Officials have reclassified {District.Name} from {DensityBefore.Label.ToLower()} to {DensityAfter.Label.ToLower()} density.",
                $"The character of {District.Name} is shifting: it has moved from {DensityBefore.Label.ToLower()} to {DensityAfter.Label.ToLower()} density.",
            };

            var pool = new List<string>();
            if (IsDenser)
            {
                pool.Add("New arrivals are reshaping the streets and squares.");
                pool.Add("Housing is in short supply as more people pour in.");
                pool.Add($"The booming {District.Economy1.Label.ToLower()} sector is widely credited for the influx.");
                pool.Add("Some longtime residents grumble about the noise and bustle.");
                pool.Add("Urban planners are scrambling to keep up.");
            }
            else
            {
                pool.Add("Once-busy streets have grown noticeably quieter.");
                pool.Add("Empty homes are becoming a common sight.");
                pool.Add($"A slump in {District.Economy1.Label.ToLower()} may be behind the exodus.");
                pool.Add("Those who remain speak fondly of the slower pace.");
                pool.Add("Local businesses worry about the dwindling foot traffic.");
            }

            return $"{facts.RandomElement()} {string.Join(" ", pool.RandomElement())}";
        }

        public override int GetArticlePriority() => 40;
    }
}
