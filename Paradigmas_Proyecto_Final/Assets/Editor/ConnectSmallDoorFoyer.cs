using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ConnectSmallDoorFoyer
{
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static Material salmon,cream,wood,gold;
    static Material Mat(string n){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/"+n+".mat");}
    static Transform Group(string n,Transform p){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
    static GameObject Box(string n,Transform p,Vector3 c,Vector3 size,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,true);g.transform.position=c;g.transform.localScale=size;g.layer=8;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
    static void Trim(string n,Transform p,float x,float width){Box(n+"_Friso",p,new Vector3(x,6.55f,-3),new Vector3(width,1.1f,.31f),cream);Box(n+"_Moldura",p,new Vector3(x,7.15f,-3),new Vector3(width,.08f,.36f),gold);Box(n+"_Zocalo",p,new Vector3(x,6.09f,-3),new Vector3(width,.18f,.39f),wood);}
    static void Paint(MeshFilter mf,Material m,string n){var g=new GameObject(n);g.transform.SetParent(mf.transform,false);g.layer=8;g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=m;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
        var room=GameObject.Find("Casa/Bajo/07_Sala_Cuna_Queso");var shell=room.transform.Find("Arquitectura");
        if(shell.Find("Sur_Acceso_Recibidor")!=null)throw new Exception("Already connected");
        salmon=Mat("Salmon");cream=Mat("Crema");wood=Mat("MaderaClara");gold=Mat("Moldura");
        var p=room.GetComponentsInChildren<RoomPerspectiveAlignment>(true).Single();var projection=(Material)typeof(RoomPerspectiveAlignment).GetField("projectionMaterial",Flags).GetValue(p);
        var column=room.transform.Find("Zona_Alta_Fija/Columnas_Mirador/Columna_Fragmento_Puerta");
        column.position=new Vector3(-53.57f,column.position.y,column.position.z);column.localScale=new Vector3(.96f,column.localScale.y,column.localScale.z);column.gameObject.layer=8;
        Paint(column.GetComponent<MeshFilter>(),projection,"Proyeccion_Puerta_Referencia_Columna");

        const float x=-54.1928f;var opening=Group("Sur_Acceso_Recibidor",shell);
        foreach(var t in shell.Cast<Transform>().Where(t=>t.name=="Sur"||t.name.StartsWith("Sur_")&&t!=opening).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        float left=x-1.1f,right=x+1.1f;
        Box("Pared_Oeste",opening,new Vector3((-64+left)*.5f,10.8f,-3),new Vector3(left+64,9.6f,.25f),salmon);Trim("Oeste",opening,(-64+left)*.5f,left+64);
        Box("Pared_Este",opening,new Vector3((right-44)*.5f,10.8f,-3),new Vector3(-44-right,9.6f,.25f),salmon);Trim("Este",opening,(right-44)*.5f,-44-right);
        Box("Dintel",opening,new Vector3(x,12.3f,-3),new Vector3(2.2f,6.6f,.25f),salmon);
        var blocked=Group("Pared_Hasta_Revelar_Puerta",opening);Box("Panel_Ciego",blocked,new Vector3(x,7.5f,-3),new Vector3(2.2f,3,.25f),salmon);Trim("Ciego",blocked,x,2.2f);
        var anamorphic=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaPasilloManzana/Pared_Aparente.mat");
        foreach(var mf in opening.GetComponentsInChildren<MeshFilter>().ToArray()){Paint(mf,projection,"Proyeccion_Puerta");Paint(mf,anamorphic,"Pintura_Anamorfica_Fondo");}
        // Move and turn the entire foyer around its old entrance, preserving all furnishings.
        var foyer=GameObject.Find("Casa/Bajo/08_Recibidor_Puerta_Principal").transform;var children=foyer.Cast<Transform>().ToArray();var positions=children.Select(t=>t.position).ToArray();var rotations=children.Select(t=>t.rotation).ToArray();var turn=Quaternion.Euler(0,-90,0);var oldPivot=new Vector3(-64,6,8);var newPivot=new Vector3(x,6,-3);
        foyer.SetPositionAndRotation(new Vector3(x,6,-8),turn);
        for(int i=0;i<children.Length;i++)children[i].SetPositionAndRotation(newPivot+turn*(positions[i]-oldPivot),turn*rotations[i]);
        var reveal=room.GetComponentInChildren<RevealDoorBehindSmallDoor>(true);typeof(RevealDoorBehindSmallDoor).GetField("blockedOpening",Flags).SetValue(reveal,blocked.gameObject);var real=(GameObject)typeof(RevealDoorBehindSmallDoor).GetField("realDoor",Flags).GetValue(reveal);typeof(RevealDoorBehindSmallDoor).GetField("leafHinge",Flags).SetValue(reveal,real.transform.Find("Bisagra_Hoja"));real.SetActive(false);real.transform.Find("Bisagra_Hoja").localRotation=Quaternion.identity;
        CheeseRoomRevision.Rebake();projection.SetFloat("_RoomProjectionVisibility",1);EditorUtility.SetDirty(projection);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.scene);EditorSceneManager.SaveScene(room.scene);
        return "Column alignment fragment added, real door opens on reveal, foyer connected on south side.";
    }
}
