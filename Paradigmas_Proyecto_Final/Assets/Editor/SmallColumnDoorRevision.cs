using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SmallColumnDoorRevision
{
    static readonly BindingFlags Fields=BindingFlags.NonPublic|BindingFlags.Instance;
    static object Get(object o,string n){return o.GetType().GetField(n,Fields).GetValue(o);}
    static void Set(object o,string n,object v){o.GetType().GetField(n,Fields).SetValue(o,v);}
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
        var room=GameObject.Find("Casa/Bajo/07_Sala_Cuna_Queso");
        if(room.GetComponentInChildren<RevealDoorBehindSmallDoor>(true)!=null)throw new Exception("Already revised");
        var alignment=room.GetComponentsInChildren<RoomPerspectiveAlignment>(true).Single();
        var source=(GameObject)Get(alignment,"referenceObject");var real=(GameObject)Get(alignment,"physicalObject");
        var old=room.GetComponentInChildren<MaterializedDoorway>(true);
        ((GameObject)Get(old,"blockedOpening")).SetActive(true);
        UnityEngine.Object.DestroyImmediate(old);

        // Small near door and distant real door share the elevated line of sight.
        const float x=-54.1928f;
        source.name="Puerta_Diminuta_Referencia";
        source.transform.SetPositionAndRotation(new Vector3(x,9.8f,14.65f),Quaternion.identity);
        source.transform.localScale=Vector3.one*.22f;
        var small=UnityEngine.Object.Instantiate(source,source.transform.parent);small.name="Puerta_Diminuta_Fija_Columna";
        foreach(var t in small.GetComponentsInChildren<Transform>(true))t.gameObject.layer=8;
        foreach(var collider in small.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(collider);
        var solid=small.AddComponent<BoxCollider>();solid.center=new Vector3(0,1.5f,0);solid.size=new Vector3(2.48f,3.2f,.24f);
        small.SetActive(false);
        Set(alignment,"physicalObject",small);Set(alignment,"preservePhysicalPose",false);
        real.name="Puerta_Real_Revelada_Pared_Inferior";
        real.transform.SetPositionAndRotation(new Vector3(x,6,-2.7f),Quaternion.identity);real.transform.localScale=Vector3.one;
        real.transform.Find("Bisagra_Hoja").localRotation=Quaternion.identity;real.SetActive(false);
        var reveal=alignment.gameObject.AddComponent<RevealDoorBehindSmallDoor>();Set(reveal,"smallDoorAlignment",alignment);Set(reveal,"realDoor",real);

        var cam=(Camera)Get(alignment,"projectionCamera");cam.transform.position=new Vector3(x,10.73f,17.8f);cam.transform.LookAt(new Vector3(x,10.15f,14.65f));
        var mark=room.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Marca_Alineacion_Puerta_Alta");mark.position=new Vector3(x,9.014f,17.8f);
        // The column is architecture beside the small object, not a receiver of its paint.
        var cols=room.transform.Find("Zona_Alta_Fija/Columnas_Mirador");var projection=(Material)Get(alignment,"projectionMaterial");
        foreach(var r in cols.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.sharedMaterial==projection).ToArray())UnityEngine.Object.DestroyImmediate(r.gameObject);
        foreach(var t in cols.GetComponentsInChildren<Transform>(true))if(t.GetComponent<Collider>()!=null||t.GetComponent<MeshFilter>()!=null)t.gameObject.layer=0;
        var support=GameObject.CreatePrimitive(PrimitiveType.Cube);support.name="Anclaje_Puerta_Diminuta";support.transform.SetParent(cols,true);support.transform.position=new Vector3(-53.927f,10.13f,14.64f);support.transform.localScale=new Vector3(.06f,.08f,.13f);support.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/Moldura.mat");support.layer=0;UnityEngine.Object.DestroyImmediate(support.GetComponent<Collider>());
        projection.SetFloat("_RoomProjectionVisibility",1);EditorUtility.SetDirty(projection);
        CheeseRoomRevision.Rebake();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.scene);EditorSceneManager.SaveScene(room.scene);
        return "Small fixed column door projects onto floor/wall; solving reveals separate closed real door. Foyer untouched.";
    }
}
