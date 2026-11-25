using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit migration of the cheese room to a permanent mezzanine and a floor/column illusion.
public static class CheeseRoomRevision
{
    static Material wood, cream, white, salmon, green, gold;
    const string Folder="Assets/Art/CasaEntradaQueso";
    static Material Mat(string n){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/"+n+".mat");}
    static Transform Group(string n,Transform p){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
    static GameObject Box(string n,Transform p,Vector3 c,Vector3 s,Material m,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,true);g.transform.position=c;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;g.layer=8;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void Wall(string n,Transform p,Vector3 c,Vector3 s,Material m){Box(n,p,c,s,m);bool x=s.x>s.z;Box(n+"_Friso",p,new Vector3(c.x,6.55f,c.z),x?new Vector3(s.x,1.1f,.31f):new Vector3(.31f,1.1f,s.z),cream);Box(n+"_Moldura",p,new Vector3(c.x,7.15f,c.z),x?new Vector3(s.x,.08f,.36f):new Vector3(.36f,.08f,s.z),gold);Box(n+"_Zocalo",p,new Vector3(c.x,6.09f,c.z),x?new Vector3(s.x,.18f,.39f):new Vector3(.39f,.18f,s.z),wood);}
    static object Get(object p,string n){return p.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(p);}
    static void Set(object p,string n,object v){p.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(p,v);}
    static void Overlay(Transform root,Material mat,string name)
    {
        foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true).Where(m=>m.gameObject.layer==8&&m.GetComponent<Renderer>()!=null&&m.GetComponent<Renderer>().sharedMaterial.shader.name=="Universal Render Pipeline/Lit").ToArray()){var g=new GameObject(name);g.transform.SetParent(mf.transform,false);g.layer=8;g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
    }
    static void Chair(Transform p,Vector3 c,Material fabric)
    {
        Box("Sillon_Asiento",p,c+Vector3.up*.45f,new Vector3(1.25f,.3f,1.25f),fabric);
        Box("Sillon_Respaldo",p,c+new Vector3(0,1.05f,.52f),new Vector3(1.25f,1.2f,.25f),fabric);
        foreach(float side in new[]{-1f,1f})Box("Sillon_Brazo",p,c+new Vector3(side*.58f,.7f,0),new Vector3(.18f,.65f,1.25f),fabric);
        foreach(float x in new[]{-.45f,.45f})foreach(float z in new[]{-.45f,.45f})Box("Pata_Sillon",p,c+new Vector3(x,.16f,z),new Vector3(.12f,.32f,.12f),wood);
    }
    static void Light(Transform p,Vector3 pos){var g=new GameObject("Luz_Calida");g.transform.SetParent(p,true);g.transform.position=pos;var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=20;l.intensity=2.6f;l.color=new Color(1,.9f,.77f);l.shadows=LightShadows.None;}
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
        var room=GameObject.Find("Casa/Bajo/07_Sala_Cuna_Queso").transform;
        if(room.Find("Zona_Alta_Fija")!=null)throw new Exception("Revision already applied");
        wood=Mat("MaderaClara");cream=Mat("Crema");white=Mat("Marfil");salmon=Mat("Salmon");green=Mat("Salvia");gold=Mat("Moldura");
        var shell=room.Find("Arquitectura");foreach(var t in shell.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var puzzle=room.Find("Acertijo_Pasarela_y_Puerta");UnityEngine.Object.DestroyImmediate(puzzle.Find("Pasarela").gameObject);var oldMark=puzzle.Find("Marca_Proyeccion_Pasarela");if(oldMark!=null)UnityEngine.Object.DestroyImmediate(oldMark.gameObject);
        foreach(var t in room.Cast<Transform>().Where(t=>t.name=="Luz_Calida"||t.name=="Plafon").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        // Expand westwards and on both sides; move the foyer to remain beyond this room.
        GameObject.Find("Casa/Bajo/08_Recibidor_Puerta_Principal").transform.position+=Vector3.left*4;
        Box("Suelo_Ampliado",shell,new Vector3(-54,5.85f,8),new Vector3(20,.3f,22),wood);
        Box("Techo_Ampliado",shell,new Vector3(-54,15.7f,8),new Vector3(20,.2f,22),white);
        Wall("Sur",shell,new Vector3(-54,10.8f,-3),new Vector3(20,9.6f,.25f),salmon);
        Wall("Norte",shell,new Vector3(-54,10.8f,19),new Vector3(20,9.6f,.25f),green);
        Wall("Este_Sur",shell,new Vector3(-44,10.8f,0),new Vector3(.25f,9.6f,6),green);
        Wall("Este_Norte",shell,new Vector3(-44,10.8f,16),new Vector3(.25f,9.6f,6),green);
        Box("Este_Cierre_Superior",shell,new Vector3(-44,13.05f,8),new Vector3(.25f,5.1f,10),green);
        Wall("Oeste_Sur",shell,new Vector3(-64,10.8f,1.95f),new Vector3(.25f,9.6f,9.9f),salmon);
        Wall("Oeste_Norte",shell,new Vector3(-64,10.8f,14.05f),new Vector3(.25f,9.6f,9.9f),salmon);
        Box("Oeste_Dintel",shell,new Vector3(-64,12.3f,8),new Vector3(.25f,6.6f,2.2f),salmon);
        var blocker=Group("Pared_Hasta_Materializar_Puerta",shell);
        Box("Panel_Ciego",blocker,new Vector3(-64,7.5f,8),new Vector3(.25f,3,2.2f),salmon);
        Box("Friso_Ciego",blocker,new Vector3(-64,6.55f,8),new Vector3(.31f,1.1f,2.2f),cream);
        Box("Moldura_Ciega",blocker,new Vector3(-64,7.15f,8),new Vector3(.36f,.08f,2.2f),gold);
        Box("Zocalo_Ciego",blocker,new Vector3(-64,6.09f,8),new Vector3(.39f,.18f,2.2f),wood);
        var mezz=Group("Zona_Alta_Fija",room);
        Box("Volumen_Solido_Hasta_Suelo",mezz,new Vector3(-54,7.425f,16.5f),new Vector3(20,2.85f,5),cream);
        Box("Pavimento_Mirador",mezz,new Vector3(-54,8.925f,16.5f),new Vector3(20,.15f,5),wood);
        Box("Moldura_Frontal_Zona_Alta",mezz,new Vector3(-54,8.9f,13.96f),new Vector3(20,.2f,.16f),gold);
        foreach(var v in new[]{new Vector3(-61.25f,9.45f,14),new Vector3(-47.5f,9.45f,14)}){Box("Antepecho",mezz,v,new Vector3(v.x<-55?5.5f:7,.9f,.25f),cream);Box("Pasamanos",mezz,v+Vector3.up*.5f,new Vector3(v.x<-55?5.6f:7.1f,.1f,.36f),wood);}
        var cols=Group("Columnas_Mirador",mezz);
        foreach(float x in new[]{-53.5f,-60f}){Box(x>-55?"Columna_Fragmento_Puerta":"Columna_Lateral",cols,new Vector3(x,12.3f,14.2f),new Vector3(.82f,6.6f,.75f),white);Box("Base_Columna",cols,new Vector3(x,9.1f,14.2f),new Vector3(1,.2f,.95f),gold);Box("Capitel",cols,new Vector3(x,15.45f,14.2f),new Vector3(1,.3f,.95f),gold);}
        var decor=Group("Mobiliario_Mirador",mezz);var pink=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaPistaDucha/Armario_Rosa.mat");
        Box("Alfombra_Mirador",decor,new Vector3(-60,9.012f,17.2f),new Vector3(4,.024f,2.6f),pink,false);
        Chair(decor,new Vector3(-61.4f,9,17.7f),green);Chair(decor,new Vector3(-58.7f,9,17.7f),green);
        Box("Mesa_Te",decor,new Vector3(-60,9.5f,16.7f),new Vector3(1.4f,.15f,.8f),wood);Box("Pie_Mesa",decor,new Vector3(-60,9.2f,16.7f),new Vector3(.18f,.4f,.18f),wood);
        foreach(float x in new[]{-62.5f,-45.5f}){Box("Maceta_Mirador",decor,new Vector3(x,9.3f,17.8f),new Vector3(.55f,.6f,.55f),wood);Box("Planta_Mirador",decor,new Vector3(x,10.1f,17.8f),new Vector3(.45f,1.1f,.45f),green,false);}
        // Decorative windows do not add cameras or expose the sky.
        foreach(float x in new[]{-60,-54,-48}){Box("Marco_Ventana",decor,new Vector3(x,12.1f,18.8f),new Vector3(3,3.2f,.12f),white,false);Box("Cristal_Azul",decor,new Vector3(x,12.1f,18.71f),new Vector3(2.7f,2.9f,.04f),Mat("Azul"),false);Box("Montante_Ventana",decor,new Vector3(x,12.1f,18.65f),new Vector3(.09f,2.9f,.05f),white,false);Box("Travesano_Ventana",decor,new Vector3(x,12.1f,18.64f),new Vector3(2.7f,.09f,.05f),white,false);}
        Light(room,new Vector3(-50,13.8f,4));Light(room,new Vector3(-60,13.8f,6));Light(mezz,new Vector3(-55,14.2f,17));
        var cheese=room.Find("Objetos/Cuna_Queso_Interactuable");cheese.position=new Vector3(-47.5f,6.35f,6);cheese.localScale=Vector3.one;
        var alignment=puzzle.GetComponentsInChildren<RoomPerspectiveAlignment>(true).Single();var source=(GameObject)Get(alignment,"referenceObject");var physical=(GameObject)Get(alignment,"physicalObject");
        source.transform.SetPositionAndRotation(new Vector3(-54,6,8),Quaternion.identity);source.transform.localScale=new Vector3(.85f,.7f,1);
        physical.transform.SetPositionAndRotation(new Vector3(-63.7f,6,8),Quaternion.Euler(0,90,0));physical.transform.localScale=Vector3.one;physical.SetActive(false);
        foreach(var root in new[]{source,physical})foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {var n=r.name;if(n=="Hoja")r.sharedMaterial=Mat("Azul");else if(n=="Panel_Alto"||n=="Panel_Bajo")r.sharedMaterial=Mat("Carbon");else if(n.StartsWith("Marco")||n=="Dintel")r.sharedMaterial=white;}
        Set(alignment,"prerequisite",null);Set(alignment,"preservePhysicalPose",true);
        var cam=(Camera)Get(alignment,"projectionCamera");cam.transform.position=new Vector3(-54,10.73f,15.7f);cam.transform.LookAt(new Vector3(-54,7.1f,8));
        var opener=puzzle.GetComponent<MaterializedDoorway>();Set(opener,"blockedOpening",blocker.gameObject);
        var mat=(Material)Get(alignment,"projectionMaterial");mat.SetFloat("_RoomProjectionVisibility",1);
        // Remove stale receiver copies on the moved physical doorway, then repaint only real surfaces.
        foreach(var t in physical.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Proyeccion_")||t.name.StartsWith("Pintura_Anamorfica")).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        Overlay(shell,mat,"Proyeccion_Puerta");Overlay(mezz,mat,"Proyeccion_Puerta");
        Box("Marca_Alineacion_Puerta_Alta",mezz,new Vector3(-54,9.014f,15.7f),new Vector3(.4f,.025f,.4f),gold,false);
        var oldPaint=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaPasilloManzana/Pared_Aparente.mat");Overlay(shell,oldPaint,"Pintura_Anamorfica_Fondo");Overlay(mezz,oldPaint,"Pintura_Anamorfica_Fondo");
        Rebake();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.gameObject.scene);EditorSceneManager.SaveScene(room.gameObject.scene);return "Expanded room, permanent furnished mezzanine, floor/column door illusion and relocated wall doorway.";
    }
    public static string Rebake()
    {
        var room=GameObject.Find("Casa/Bajo/07_Sala_Cuna_Queso");var p=room.GetComponentsInChildren<RoomPerspectiveAlignment>(true).Single();var cam=(Camera)Get(p,"projectionCamera");var mat=(Material)Get(p,"projectionMaterial");var tex=(Texture2D)mat.GetTexture("_OcclusionTex");int n=tex.width;var pixels=new Color[n*n];var vp=GL.GetGPUProjectionMatrix(cam.projectionMatrix,true)*cam.worldToCameraMatrix;var inv=vp.inverse;Physics.SyncTransforms();
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){var h=inv*new Vector4((x+.5f)/n*2-1,(y+.5f)/n*2-1,.5f,1);var w=new Vector3(h.x,h.y,h.z)/h.w;RaycastHit hit;if(Physics.Raycast(cam.transform.position,(w-cam.transform.position).normalized,out hit,35,1<<8,QueryTriggerInteraction.Ignore))pixels[y*n+x]=new Color(hit.point.x,hit.point.y,hit.point.z,1);}
        tex.SetPixels(pixels);tex.Apply();EditorUtility.SetDirty(tex);mat.SetMatrix("_ProjectionVP",vp);EditorUtility.SetDirty(mat);cam.Render();return "Rebaked floor and column projection";
    }
}
