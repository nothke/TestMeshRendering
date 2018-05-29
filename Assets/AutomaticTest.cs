using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AutomaticTest : MonoBehaviour
{

    public FPSCounter fpsCounter;

    public static string str;

    private void Start()
    {
        StartCoroutine(WaitAFewSeconds());
    }

    IEnumerator WaitAFewSeconds()
    {
        for (int i = 0; i < 700; i++)
        {
            yield return null;
        }

        if (MassRendering.typeMethod == 0)
        {
            str += "\n\nMesh: " +
                MassRendering.e.mesh.name + " " +
                " verts: " + MassRendering.e.mesh.vertexCount +
                " tris: " + MassRendering.e.mesh.triangles.Length;
        }

        str += "\n- " + MassRendering.e.type + " " + fpsCounter.AverageFPS;

        yield return null;

        MassRendering.e.SetNextMethod();
        
        yield return null;

        if (MassRendering.e.type == (MassRendering.Type)(MassRendering.typeCount - 1) &&
            MassRendering.meshNum == MassRendering.e.meshes.Length - 1)
        {
            Serialize();
            Application.Quit();
        }

        yield return null;

        if (MassRendering.typeMethod == 0)
        {
            MassRendering.e.SetNextMesh();
        }

        yield return null;

        MassRendering.e.Restart();
    }

    void Serialize()
    {
        File.WriteAllText("test_results.txt", str);
    }
}
