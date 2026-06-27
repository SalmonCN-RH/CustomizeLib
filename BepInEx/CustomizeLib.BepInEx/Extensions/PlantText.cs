using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public class CustomHealthText : MonoBehaviour
    {
        public Dictionary<TextMeshProUGUI, Func<string>> registedTexts = new();

        public void Update()
        {
            foreach (var (key, value) in registedTexts)
                key.text = value.Invoke();
        }
    }
}
