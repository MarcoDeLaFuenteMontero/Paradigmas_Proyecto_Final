using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HiddenAppleHallBuilder
{
    static Material salmon,cream,wood,white,green;
    static Transform Group(string n,Transform p){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
    static Material Mat(string n){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/"+n+".mat");}
    static GameObject Box(string n,Transform p,Vector3 c,Vector3 s,Material m,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,true);g.transform.position=c;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;g.layer=8;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void Wall(string n,Transform p,Vector3 c,Vector3 s,Material m){Box(n,p,c,s,m);bool x=s.x>s.z;Box(n+"_Friso",p,new Vector3(c.x,6.55f,c.z),x?new Vector3(s.x,1.1f,.31f):new Vector3(.31f,1.1f,s.z),cream);Box(n+"_Moldura",p,new Vector3(c.x,7.15f,c.z),x?new Vector3(s.x,.08f,.36f):new Vector3(.36f,.08f,s.z),wood);Box(n+"_Zocalo",p,new Vector3(c.x,6.09f,c.z),x?new Vector3(s.x,.18f,.39f):new Vector3(.39f,.18f,s.z),wood);}
    static void Door(Transform p,string n,Vector3 c,float yaw){var g=Group(n,p);g.position=c;g.rotation=Quaternion.Euler(0,yaw,0);var panel=Box("Hoja_Cerrada_Falsa",g,c,new Vector3(1.8f,2.8f,.12f),wood);panel.transform.localPosition=new Vector3(0,1.4f,0);panel.transform.localRotation=Quaternion.identity;foreach(float side in new[]{-1f,1f}){var jamb=Box("Marco",g,c,new Vector3(.12f,2.95f,.18f),white);jamb.transform.localPosition=new Vector3(side*.98f,1.475f,0);jamb.transform.localRotation=Quaternion.identity;}var top=Box("Dintel",g,c,new Vector3(2.08f,.12f,.18f),white);top.transform.localPosition=new Vector3(0,2.95f,0);top.transform.localRotation=Quaternion.identity;var knob=Box("Pomo",g,c,new Vector3(.1f,.1f,.13f),cream,false);knob.transform.localPosition=new Vector3(.62f,1.35f,.11f);knob.transform.localRotation=Quaternion.identity;}
    static void Light(Transform p,Vector3 c){var g=new GameObject("Luz_Calida");g.transform.SetParent(p,true);g.transform.position=c;var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=12;l.intensity=2;l.shadows=LightShadows.None;l.color=new Color(1,.9f,.77f);}
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play first");
        if(GameObject.Find("Casa/Bajo/05_Pasillo_Oculto_Tras_Manzana")!=null)throw new Exception("Already built");
        salmon=Mat("Salmon");cream=Mat("Crema");wood=Mat("MaderaClara");white=Mat("Marfil");green=Mat("Salvia");
        var room=GameObject.Find("Casa/Bajo/04_Regreso_Desde_Manzana").transform;
        // Remove the actual route and its lights, then restore the original house wall.
        foreach(var t in room.Cast<Transform>().ToArray())if(t.name.StartsWith("Pasillo_")||t.name.StartsWith("Este_")||t.name=="Puerta_Regreso_Tras_Rotura"||((t.name=="Luz_Calida"||t.name=="Plafon")&&t.position.x>-16))UnityEngine.Object.DestroyImmediate(t.gameObject);
        var old=GameObject.Find("Casa/Sotano/09_Ascenso_Al_Bajo/Paredes").transform;foreach(var t in old.Cast<Transform>().ToArray())if(t.name=="Oeste_Bajo_Paso_Regreso"||t.name.StartsWith("Oeste_Lateral_Regreso"))UnityEngine.Object.DestroyImmediate(t.gameObject);old.Find("Oeste").gameObject.SetActive(true);
        var foyer=GameObject.Find("Casa/Bajo/01_Recibidor_Desde_Sotano/Paredes").transform;foreach(var t in foyer.Cast<Transform>().ToArray())if(t.name.StartsWith("Acabado_Oeste_Lado_"))UnityEngine.Object.DestroyImmediate(t.gameObject);foyer.Find("Acabado_Interior_Oeste_Recibidor").gameObject.SetActive(true);
        var breakFloor=GameObject.FindFirstObjectByType<BreakableWoodFloor>();typeof(BreakableWoodFloor).GetField("returnDoor",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(breakFloor,null);
        Wall("Este_Cerrado",room,new Vector3(-16.04f,10.8f,11),new Vector3(.25f,9.6f,10),salmon);
        foreach(var t in room.Cast<Transform>().ToArray())if(t.name=="Oeste"||t.name.StartsWith("Oeste_"))UnityEngine.Object.DestroyImmediate(t.gameObject);
        Wall("Oeste_Lateral_Sur",room,new Vector3(-28,10.8f,6.35f),new Vector3(.25f,9.6f,.7f),salmon);Wall("Oeste_Lateral_Norte",room,new Vector3(-28,10.8f,12.45f),new Vector3(.25f,9.6f,7.1f),salmon);Box("Oeste_Dintel_Paso_Oculto",room,new Vector3(-28,12.2f,7.8f),new Vector3(.25f,6.8f,2.2f),salmon);
        // Four centered dead ends; the real exit is offset toward the southwest corner.
        var fake=Group("Cuatro_Puertas_Falsas",room);Door(fake,"Puerta_Falsa_Sur",new Vector3(-22,6,6.25f),0);Door(fake,"Puerta_Falsa_Norte",new Vector3(-22,6,15.75f),180);Door(fake,"Puerta_Falsa_Este",new Vector3(-16.25f,6,11),270);Door(fake,"Puerta_Falsa_Oeste",new Vector3(-27.75f,6,11),90);
        var hall=Group("05_Pasillo_Oculto_Tras_Manzana",room.parent);Box("Suelo",hall,new Vector3(-32,5.85f,7.8f),new Vector3(8,.3f,2.6f),wood);Box("Techo",hall,new Vector3(-32,10.6f,7.8f),new Vector3(8,.2f,2.6f),white);Wall("Sur",hall,new Vector3(-32,8.25f,6.5f),new Vector3(8,4.5f,.25f),green);Wall("Norte",hall,new Vector3(-32,8.25f,9.1f),new Vector3(8,4.5f,.25f),green);Light(hall,new Vector3(-32,9.5f,7.8f));
        var next=Group("06_Sala_Tras_Pasillo_Ilustrado",room.parent);Box("Suelo",next,new Vector3(-40,5.85f,8),new Vector3(8,.3f,10),wood);Box("Techo",next,new Vector3(-40,10.6f,8),new Vector3(8,.2f,10),white);Wall("Oeste",next,new Vector3(-44,8.25f,8),new Vector3(.25f,4.5f,10),green);Wall("Sur",next,new Vector3(-40,8.25f,3),new Vector3(8,4.5f,.25f),green);Wall("Norte",next,new Vector3(-40,8.25f,13),new Vector3(8,4.5f,.25f),green);Wall("Este_Sur",next,new Vector3(-36,8.25f,4.85f),new Vector3(.25f,4.5f,3.7f),green);Wall("Este_Norte",next,new Vector3(-36,8.25f,10.95f),new Vector3(.25f,4.5f,4.1f),green);Box("Dintel_Entrada",next,new Vector3(-36,9.65f,7.8f),new Vector3(.25f,1.7f,2.2f),green);Light(next,new Vector3(-40,9.5f,8));Box("Plafon",next,new Vector3(-40,10.35f,8),new Vector3(.8f,.15f,.8f),white,false);
        const string folder="Assets/Art/CasaPasilloManzana";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","CasaPasilloManzana");var paint=new Material(Shader.Find("Custom/AnamorphicAppleHallway"));paint.name="Pared_Aparente_Manzana";AssetDatabase.CreateAsset(paint,folder+"/Pared_Aparente.mat");
        // Paint on the physical corridor and destination surfaces, never on a blocking plane.
        var west=room.GetComponentsInChildren<MeshFilter>().Where(m=>m.name.StartsWith("Oeste_")&&m.transform.parent==room);
        foreach(var mf in west.Concat(hall.GetComponentsInChildren<MeshFilter>()).Concat(next.GetComponentsInChildren<MeshFilter>()).ToArray()){var g=new GameObject("Pintura_Anamorfica");g.transform.SetParent(mf.transform,false);g.layer=8;g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=paint;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
        var view=Group("Punto_Vista_Pared_Aparente",room);view.position=new Vector3(-21.5f,7.73f,10.5f);
        // Continue the painted skirting up to the projecting real skirting edge.
        Box("Pintura_Zocalo_Adelantada_Entrada",room,new Vector3(-27.9f,6.001f,7.8f),new Vector3(.2f,.002f,2.24f),paint,false);
        Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.gameObject.scene);EditorSceneManager.SaveScene(room.gameObject.scene);return "Removed return corridor, restored house separation, four false doors and anamorphic exit built.";
    }
}
