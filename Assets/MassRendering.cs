using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class MassRendering : MonoBehaviour
{
    public static MassRendering e;
    void Awake() { e = this; }

    public int size = 60;

    public int instancingMatrixSize = 1000;

    public float separation = 5;

    public Mesh mesh;
    public Material material;

    public const int typeCount = 5;
    public enum Type { GameObject, MeshCombine, StaticBatch, DrawMesh, DrawMeshInstanced };
    public Type type;

    static bool hasReset = false;

    public Text typeDisplay;
    static int staticMatrixSize = 1000;
    public static int meshNum = 0;

    public Mesh[] meshes;

    void Start()
    {
        if (hasReset)
        {
            type = (Type)typeMethod;
            instancingMatrixSize = staticMatrixSize;
            mesh = meshes[meshNum];
        }

        if (typeDisplay) typeDisplay.text = type.ToString() +
                (type == Type.DrawMeshInstanced ? " matrix size: " + instancingMatrixSize : "");

        typeDisplay.text += "\nvrts " + mesh.vertexCount;
        typeDisplay.text += "\ntris " + mesh.triangles.Length;

        array = new Vector3[size * size];
        matrixArray = new Matrix4x4[size * size];



        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new Vector3(i / size, 0, i % size) * separation;
            matrixArray[i] = Matrix4x4.TRS(array[i], Quaternion.identity, Vector3.one);

            if (type == Type.GameObject || type == Type.MeshCombine || type == Type.StaticBatch)
            {
                GameObject go = new GameObject();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().material = material;
                go.transform.position = array[i];
                go.transform.parent = transform;
            }
        }

        if (type == Type.StaticBatch)
        {
            StaticBatchingUtility.Combine(gameObject);
            return;
        }

        if (type == Type.DrawMeshInstanced)
        {
            int k = 0;

            matrixArrayInstanced = new Matrix4x4[(size * size) / instancingMatrixSize + 1][];

            for (int m = 0; m < matrixArrayInstanced.Length; m++)
            {
                if (m != matrixArrayInstanced.Length - 1)
                    matrixArrayInstanced[m] = new Matrix4x4[instancingMatrixSize];
                else
                    matrixArrayInstanced[m] = new Matrix4x4[(size * size) % instancingMatrixSize];

                for (int i = 0; i < matrixArrayInstanced[m].Length; i++)
                {
                    matrixArrayInstanced[m][i] = matrixArray[k];
                    k++;
                }

                //Debug.Log(matrixArrayInstanced[m].Length);
            }
        }

        if (type == Type.MeshCombine)
        {
            gameObject.CombineChildMeshes();
        }
    }

    Vector3[] array;
    Matrix4x4[] matrixArray;
    Matrix4x4[][] matrixArrayInstanced;

    public static int typeMethod = 0;

    public void Next()
    {
        typeMethod++;

        if (typeMethod >= typeCount)
        {
            typeMethod = 0;
            SetNextMesh();
        }

        ResetTo(typeMethod);

    }

    void ResetTo(int i)
    {
        typeMethod = i;
        Restart();
    }

    public void Restart()
    {
        hasReset = true;
        Application.LoadLevel(0);
    }

    public void SetNextMethod()
    {
        typeMethod++;

        if (typeMethod >= typeCount)
            typeMethod = 0;
    }

    public void SetNextMesh()
    {
        meshNum++;
        if (meshNum >= meshes.Length) meshNum = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ResetTo(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ResetTo(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ResetTo(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ResetTo(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) { staticMatrixSize = 50; ResetTo(4); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { staticMatrixSize = 1000; ResetTo(4); }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetNextMesh();
            ResetTo(typeMethod);
        }

        if (type == Type.GameObject || type == Type.MeshCombine || type == Type.StaticBatch) return;

        if (type == Type.DrawMesh)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Graphics.DrawMesh(mesh, matrixArray[i], material, 0);
            }
        }

        if (type == Type.DrawMeshInstanced)
        {
            for (int m = 0; m < matrixArrayInstanced.Length; m++)
            {
                Graphics.DrawMeshInstanced(mesh, 0, material, matrixArrayInstanced[m]);
            }
        }
    }
}
