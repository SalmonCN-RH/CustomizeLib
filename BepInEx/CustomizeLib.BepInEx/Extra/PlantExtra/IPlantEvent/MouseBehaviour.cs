using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    public class MouseBehaviour : MonoBehaviour
    {
        public static MouseBehaviour Instance = null!;
        public Mouse mouse = null!;

        public void Awake()
        {
            Instance = this;
            mouse = Mouse.Instance;
        }

        public void ProcMouse(TriggerType trigger)
        {
            var plants = Lawnf.GetAllPlants();
            foreach (var val in Enum.GetValues<MouseClick>())
            {
                ProcState(plants, val, trigger);
            }
        }

        public void ProcState(Il2CppSystem.Collections.Generic.List<Plant> plants, MouseClick click, TriggerType trigger)
        {
            if (Input.GetMouseButtonUp((int)click))
                foreach (var p in plants)
                    PlantEvent.MouseEvent(p, mouse, MouseState.Up, click, trigger);
            if (Input.GetMouseButton((int)click))
                foreach (var p in plants)
                    PlantEvent.MouseEvent(p, mouse, MouseState.Hold, click, trigger);
            if (Input.GetMouseButtonDown((int)click))
                foreach (var p in plants)
                    PlantEvent.MouseEvent(p, mouse, MouseState.Down, click, trigger);
        }
    }
}
