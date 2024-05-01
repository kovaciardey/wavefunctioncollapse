using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TileWeightDisplay : MonoBehaviour
    {
        public Image tile;
        public Text weightText;

        public void SetColorAndText(Color color, string text)
        {
            tile.color = color;
            weightText.text = text;
        }
    }
}
