using System.Collections;
using UnityEngine;
 
public class CameraMirror : MonoBehaviour
{
    public static bool inverted;

    Camera cam;
    Matrix4x4 scale = Matrix4x4.Scale(new Vector3(-1, 1, 1));

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnPreCull()
    {
        cam.ResetWorldToCameraMatrix();
        cam.ResetProjectionMatrix();
        if(inverted)
            cam.projectionMatrix = cam.projectionMatrix * scale;
    }

    void OnPreRender()
    {
        if(inverted) GL.invertCulling = true;
    }

    void OnPostRender()
    {
        if(inverted) GL.invertCulling = false;
    }
}