using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionTactics
{
    public class CameraHandler : MonoBehaviour
    {
        private Vector2 Offset = new Vector2(0.4f, 0.03f);

        private void Start()
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        public void FocusMap(Map map)
        {
            transform.position = new Vector3(map.Width * 0.985f, map.Height * 0.96f, map.Height * 0.5f);
        }

        public void FocusDistricts(List<District> districts)
        {
            float minX = districts.Min(x => x.Region.MinWorldX);
            float minY = districts.Min(x => x.Region.MinWorldY);
            float maxX = districts.Max(x => x.Region.MaxWorldX);
            float maxY = districts.Max(x => x.Region.MaxWorldY);
            float width = maxX - minX;
            float height = maxY - minY;

            float altitude = height > width ? height : width;
            altitude *= 1.2f;

            transform.position = new Vector3(minX + (width / 2) + Offset.x*altitude, altitude, minY + (height / 2) + Offset.y*altitude);
        }

    }
}
