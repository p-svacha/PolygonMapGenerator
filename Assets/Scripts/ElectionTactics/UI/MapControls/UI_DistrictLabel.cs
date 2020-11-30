using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionTactics
{
    public class UI_DistrictLabel : MonoBehaviour
    {
        public District District;
        public TextMesh Text;

        public void Init(District d)
        {
            District = d;
            Text.text = d.Name;
            transform.position = new Vector3(District.Region.Center.x, 0.01f, District.Region.Center.y);
        }
    }
}
