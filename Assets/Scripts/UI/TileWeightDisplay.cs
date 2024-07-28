using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TileWeightDisplay : MonoBehaviour
    {
        public Image tile;
        public Text weightText;
        public Text letterText;

        public void DisplayData(Color color, string text, char letter)
        {
            tile.color = color;
            weightText.text = text;
            letterText.text = letter.ToString();
        }
    }
}
