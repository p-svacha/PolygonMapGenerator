using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionTactics
{
    public class UI_ElectionTactics : MonoBehaviour
    {
        public UI_DistrictInfo DistrictInfo;

        void Start()
        {
            DistrictInfo.gameObject.SetActive(false);
        }
    }
}
