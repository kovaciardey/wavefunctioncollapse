using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TileWeightDisplay : MonoBehaviour
    {
        public Image tile;
        public Text weightText;
        public Text tileHashText;

        public void DisplayData(Color color, string text, string tileHash)
        {
            tile.color = color;
            weightText.text = text;
            tileHashText.text = tileHash;
        }
    }
}
