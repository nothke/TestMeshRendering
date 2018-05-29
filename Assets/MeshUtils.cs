using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{
    public class MeshList
    {
        public Material material;
        public List<CombineInstance> instances = new List<CombineInstance>();
        public int totalVertices;

        public void AddInstance(MeshFilter mf, Matrix4x4 parentMatrix)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = parentMatrix * mf.transform.localToWorldMatrix;
            instances.Add(ci);

            totalVertices += mf.sharedMesh.vertexCount;
        }
    }

    const int MAX_INDICES = 60000;

    public static void CombineChildMeshes(this GameObject gameObject)
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        List<MeshList> meshLists = new List<MeshList>();
        //List<Material> materials = new List<Material>();

        Matrix4x4 goMatrix = gameObject.transform.worldToLocalMatrix;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var mr = meshFilters[i].GetComponent<MeshRenderer>();

            MeshList ml;

            int vertexCount = meshFilters[i].sharedMesh.vertexCount;

            bool done = false;

            for (int mlindex = 0; mlindex < meshLists.Count; mlindex++)
            {
                if (meshLists[mlindex].material == mr.sharedMaterial &&
                    meshLists[mlindex].totalVertices + vertexCount < MAX_INDICES)
                {
                    meshLists[mlindex].AddInstance(meshFilters[i], goMatrix);

                    Object.Destroy(mr);
                    Object.Destroy(meshFilters[i]);

                    done = true;
                    break;
                }
            }

            if (done) continue;

            ml = new MeshList();
            ml.material = mr.sharedMaterial;
            ml.AddInstance(meshFilters[i], goMatrix);
            meshLists.Add(ml);

            //meshFilters[i].gameObject.SetActive(false);
            Object.Destroy(mr);
            Object.Destroy(meshFilters[i]);
        }

        // combine meshes
        for (int i = 0; i < meshLists.Count; i++)
        {
            var go = gameObject.CreateChild();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            Mesh m = new Mesh();
            m.CombineMeshes(meshLists[i].instances.ToArray());
            mf.sharedMesh = m;
            mr.sharedMaterial = meshLists[i].material;
        }
    }

    public static GameObject CreateChild(this GameObject gameObject, string name = "New Child")
    {
        GameObject go = new GameObject(name);
        go.transform.parent = gameObject.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        return go;
    }
}
