using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View2D : MonoBehaviour {
    void OnGUI()
    {
        int colorWidth = SourceManager.getColorWidth() / 5;
        int colorHeight = SourceManager.getColorHeight() / 5;
        GUI.Label(new Rect(0, 0, colorWidth, colorHeight), SourceManager.getColorTexture());

        int infraredWidth = SourceManager.getInfraredWidth() / 2;
        int infraredHeight = SourceManager.getInfraredHeight() / 2;
        GUI.Label(new Rect(0, colorHeight, infraredWidth, infraredHeight), SourceManager.getInfraredTexture());
    }
}
