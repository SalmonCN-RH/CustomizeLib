using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.Patch
{
    public class PositionRecorder : MonoBehaviour
    {
        public static List<RecordPosition> positions = new();

        public struct RecordPosition
        {
            public Vector2 position;
            public float time = 0.05f;
            public PlantType plantType = PlantType.Nothing;
            public int index = -1;
            public bool remove = false;

            public RecordPosition(Vector2 position, PlantType plantType)
            {
                this.position = position;
                this.plantType = plantType;
                remove = true;
            }
        }

        public static void AddPositonToList(Vector2 position, PlantType fromType)
        {
            var stru = new RecordPosition(position, fromType);
            stru.index = positions.Count;
            positions.Add(stru);
        }

        public static void RemovePosition(int index)
        {
            if (index < 0 || index >= positions.Count)
                return;
            var item = positions[index];
            item.remove = true;
            positions[index] = item;
        }

        public void Update()
        {
            for (int i = positions.Count - 1; i >= 0; i--)
            {
                var item = positions[i];
                item.time -= Time.deltaTime;
                positions[i] = item;
                if (item.time <= 0f) positions.Remove(item);
                if (item.remove) positions.Remove(item);
            }
        }

        public static List<RecordPosition> GetRecordPositions(Vector2 center, float radius)
        {
            var result = new List<RecordPosition>();
            foreach (var item in positions)
            {
                if (Vector2.Distance(center, item.position) < radius)
                    result.Add(item);
            }
            return result;
        }
    }
}
