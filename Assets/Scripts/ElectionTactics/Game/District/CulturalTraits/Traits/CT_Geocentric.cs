using System;
using System.Collections.Generic;
using System.Text;

namespace ElectionTactics
{
    public class CT_Geocentric : CulturalTrait
    {
        public override void ModifyPolicyPointImpact(District targetDistrict, Policy policy, ref int impact)
        {
            if (policy is GeographyPolicy gp) impact += policy.GetSinglePointBaseImpact(targetDistrict);
        }
    }
}
