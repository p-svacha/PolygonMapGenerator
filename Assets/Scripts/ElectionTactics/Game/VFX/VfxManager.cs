using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class VfxManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public ParticleSystem DistrictEffectPrefab;

        [Header("Textures")]
        public Texture PopulartiyUpIcon;
        public Texture PopularityDownIcon;


        public void Init(ElectionTacticsGame game)
        {
            
        }

        public void ShowDistrictPopularityImpactParticles(District district, int impact)
        {
            if (impact == 0) return;

            ParticleSystem system = Instantiate(DistrictEffectPrefab);

            system.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", impact > 0 ? PopulartiyUpIcon : PopularityDownIcon);

            var main = system.main;
            main.startColor = ColorManager.Singleton.GetImpactColor(impact);
            var emission = system.emission;
            emission.rateOverTime = district.Region.Area * 50;
            var shape = system.shape;
            shape.mesh = district.Region.gameObject.GetComponent<MeshFilter>().mesh;
            system.Play();
        }
    }
}
