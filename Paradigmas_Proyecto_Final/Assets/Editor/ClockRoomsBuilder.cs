using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Explicit editor command: never rebuilds the scene automatically.
public static class ClockRoomsBuilder
{
    const string Folder = "Assets/Art/CasaReloj";
    static Material wood, cream, white, green, blue, gold, dark;
    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool collider = true)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, true); g.transform.position = pos; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = mat; g.layer = 8;
        if (!collider) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    static Transform Group(string name, Transform parent) { var g = new GameObject(name); g.transform.SetParent(parent, false); return g.transform; }
    static void Field(object o, string name, object value) { o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, value); }
    static Material Mat(string name) { return AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/" + name + ".mat"); }
    static void Wall(string name, Transform p, Vector3 center, Vector3 size, Material m)
    {
        Box(name,p,center,size,m);
        if (size.x > size.z) { Box(name+"_Friso",p,new Vector3(center.x,12.57f,center.z),new Vector3(size.x,1.14f,.31f),cream); Box(name+"_Moldura",p,new Vector3(center.x,13.18f,center.z),new Vector3(size.x,.08f,.35f),gold); }
        else { Box(name+"_Friso",p,new Vector3(center.x,12.57f,center.z),new Vector3(.31f,1.14f,size.z),cream); Box(name+"_Moldura",p,new Vector3(center.x,13.18f,center.z),new Vector3(.35f,.08f,size.z),gold); }
    }
    static void FaceClock(Transform parent, Vector3 c, float radius, bool pendulum)
    {
        var rim=GameObject.CreatePrimitive(PrimitiveType.Cylinder);rim.name="Marco_Reloj";rim.transform.SetParent(parent,true);rim.transform.position=c;rim.transform.rotation=Quaternion.Euler(90,0,0);rim.transform.localScale=new Vector3(radius*2,.07f,radius*2);rim.GetComponent<Renderer>().sharedMaterial=gold;UnityEngine.Object.DestroyImmediate(rim.GetComponent<Collider>());
        var face=UnityEngine.Object.Instantiate(rim,parent);face.name="Esfera";face.transform.position=c+Vector3.forward*.08f;face.transform.localScale=new Vector3(radius*1.78f,.025f,radius*1.78f);face.GetComponent<Renderer>().sharedMaterial=white;
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var tick=Box("Hora",parent,c+new Vector3(Mathf.Sin(a)*radius*.72f,Mathf.Cos(a)*radius*.72f,.115f),new Vector3(.045f,.12f,.025f),dark,false);tick.transform.rotation=Quaternion.Euler(0,0,-i*30);}
        var h=Box("Aguja_Horas",parent,c+new Vector3(-radius*.19f,radius*.12f,.14f),new Vector3(radius*.5f,.06f,.035f),dark,false);h.transform.rotation=Quaternion.Euler(0,0,-30);
        Box("Aguja_Minutos",parent,c+new Vector3(0,radius*.28f,.15f),new Vector3(.045f,radius*.6f,.035f),dark,false);
        if(pendulum){Box("Pendulo_Decorativo",parent,c+new Vector3(0,-radius*1.65f,0),new Vector3(.06f,radius*1.3f,.07f),gold,false);Box("Lenteja_Pendulo",parent,c+new Vector3(0,-radius*2.3f,0),new Vector3(radius*.65f,radius*.65f,.1f),gold,false);}
    }
    static void LightAt(Transform p, Vector3 pos)
    {
        var g=new GameObject("Luz_Calida");g.transform.SetParent(p,true);g.transform.position=pos;var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=19;l.intensity=2.6f;l.color=new Color(1,.88f,.7f);l.shadows=LightShadows.None;
        Box("Plafon",p,pos+Vector3.up*.4f,new Vector3(1,.12f,1),white,false);
    }
    public static string Build()
    {
        if(EditorApplication.isPlaying) throw new Exception("Stop Play first.");
        if(GameObject.Find("Casa/Primero/05_Galeria_Puente_Reloj")!=null) throw new Exception("Rooms already built; edit existing objects.");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CasaReloj");
        wood=Mat("Madera");cream=Mat("Crema");white=Mat("Marfil");green=Mat("Salvia");blue=Mat("Azul");gold=Mat("MaderaClara");dark=Mat("Carbon");
        var root=GameObject.Find("Casa/Primero/04_Sala_Paso_Pulsador").transform;
        // Retain the working pressure plate, door and east wall owned by the salon.
        foreach(var n in new[]{"Paredes","Suelos","Techos"})foreach(Transform t in root.Find(n).Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var walls=root.Find("Paredes");var floors=root.Find("Suelos");var roofs=root.Find("Techos");
        Box("Suelo_Sala_Relojes",floors,new Vector3(10,11.85f,8),new Vector3(20,.3f,16),wood);
        Wall("Norte",walls,new Vector3(10,17,16),new Vector3(20,10,.25f),green);
        Wall("Sur",walls,new Vector3(10,17,0),new Vector3(20,10,.25f),green);
        Wall("Este_Tramo_Nuevo",walls,new Vector3(20,17,3),new Vector3(.25f,10,6),green);
        Box("Este_Prolongacion_Alta",walls,new Vector3(20,19.75f,11),new Vector3(.25f,4.5f,10),green);
        Wall("Oeste_Sur",walls,new Vector3(0,17,4),new Vector3(.25f,10,8),blue);
        Wall("Oeste_Norte",walls,new Vector3(0,17,13.5f),new Vector3(.25f,10,5),blue);
        Box("Oeste_Bajo_Puerta_Alta",walls,new Vector3(0,13.8f,9.5f),new Vector3(.25f,3.6f,3),blue);
        Box("Friso_Central_Bajo_Puerta",walls,new Vector3(0,12.57f,9.5f),new Vector3(.31f,1.14f,3),cream);
        Box("Moldura_Central_Bajo_Puerta",walls,new Vector3(0,13.18f,9.5f),new Vector3(.35f,.08f,3),gold);
        Box("Oeste_Dintel_Alto",walls,new Vector3(0,20.4f,9.5f),new Vector3(.25f,3.2f,3),blue);
        Box("Techo_Alto",roofs,new Vector3(10,22.1f,8),new Vector3(20,.2f,16),white);
        var architecture=Group("Columnas_y_Desembarco",root);
        foreach(var c in new[]{new Vector3(4.5f,17,3.7f),new Vector3(11.6f,17,3.7f)}) {Box("Columna",architecture,c,new Vector3(1.1f,10,1.1f),cream);Box("Base_Columna",architecture,new Vector3(c.x,12.15f,c.z),new Vector3(1.4f,.3f,1.4f),gold);Box("Capitel",architecture,new Vector3(c.x,21.75f,c.z),new Vector3(1.4f,.4f,1.4f),gold);}
        Box("Desembarco_Puerta_Alta",architecture,new Vector3(.75f,15.45f,9.5f),new Vector3(1.5f,.3f,3),wood);
        var oldLight=root.Find("Luz_Sala_Pulsador");if(oldLight!=null)oldLight.position=new Vector3(15,20.5f,10);
        var oldPlafon=root.Find("Plafon_Sala_Pulsador");if(oldPlafon!=null)oldPlafon.position=new Vector3(15,21.85f,10);
        LightAt(root,new Vector3(5,20.8f,8));
        var furniture=root.Find("Muebles_Fijos");var clock=Group("Reloj_Alto_Pendulo_Caido",furniture);
        Box("Caja_Reloj",clock,new Vector3(16,13.7f,1),new Vector3(1.35f,3.4f,.7f),wood);
        Box("Peana_Reloj",clock,new Vector3(16,12.15f,1),new Vector3(1.65f,.3f,.9f),gold);
        Box("Cornisa_Reloj",clock,new Vector3(16,15.45f,1),new Vector3(1.65f,.22f,.9f),gold);
        Box("Hueco_Pendulo_Vacio",clock,new Vector3(16,13.3f,1.365f),new Vector3(.92f,1.45f,.04f),dark,false);
        FaceClock(clock,new Vector3(16,14.7f,1.42f),.52f,false);
        var variants=Group("Otros_Modelos_Reloj",furniture);
        FaceClock(variants,new Vector3(18.2f,15.2f,.2f),.65f,false);
        Box("Caja_Reloj_Pared_Pendulo",variants,new Vector3(13.8f,14.8f,.35f),new Vector3(1.1f,2,.35f),wood);
        FaceClock(variants,new Vector3(13.8f,15.25f,.57f),.43f,true);
        variants.gameObject.SetActive(false);
        var yellow=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaCocinaDoble/Tostadora_Amarilla.mat");
        Box("Enganche_Pendulo_Vacio",clock,new Vector3(16,13.92f,1.44f),new Vector3(.12f,.16f,.08f),yellow,false);
        var hand=new GameObject("Pieza_Pendulo_Interactuable");hand.transform.SetParent(furniture,true);hand.transform.position=new Vector3(14,12.22f,4.3f);hand.layer=6;
        Box("Brazo_Dorado",hand.transform,hand.transform.position+Vector3.right*.15f,new Vector3(3.1f,.14f,.25f),yellow,false);
        var bob=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bob.name="Lenteja_Pendulo";bob.transform.SetParent(hand.transform,false);bob.transform.localPosition=new Vector3(-1.25f,0,0);bob.transform.localScale=new Vector3(.62f,.115f,.62f);bob.GetComponent<Renderer>().sharedMaterial=yellow;UnityEngine.Object.DestroyImmediate(bob.GetComponent<Collider>());
        var hc=hand.AddComponent<BoxCollider>();hc.size=new Vector3(3.4f,.23f,.25f);var rb=hand.AddComponent<Rigidbody>();rb.mass=2;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;hand.AddComponent<ScaleDependentMass>();foreach(Transform t in hand.transform)t.gameObject.layer=6;
        hand.transform.localScale=Vector3.one*.55f;hand.transform.position=new Vector3(15.5f,12.09f,2.4f);
        BuildBridge(root.parent);
        var puzzle=Group("Acertijo_Tres_Escalones",root);
        var colors=new[]{blue,green,gold};var positions=new[]{new Vector3(10,13.07f,1.4f),new Vector3(4,14.27f,1.4f),new Vector3(1.4f,15.47f,6)};var sizes=new[]{new Vector3(6,.26f,2.6f),new Vector3(6,.26f,2.6f),new Vector3(2.6f,.26f,10)};var viewpoints=new[]{new Vector3(14,13.73f,7),new Vector3(9,14.93f,1.5f),new Vector3(7,13.73f,10)};
        RoomPerspectiveAlignment prior=null;
        for(int i=0;i<3;i++)
        {
            var node=Group("Escalon_"+(i+1),puzzle);var source=Box("Referencia_Escalon",node,positions[i],sizes[i],gold,false);source.layer=11+i;
            var physical=Box("Escalon_Materializado",node,positions[i],sizes[i],gold);physical.SetActive(false);
            var rt=new RenderTexture(1024,576,24,RenderTextureFormat.ARGB32);rt.name="RT_Escalon_"+(i+1);AssetDatabase.CreateAsset(rt,Folder+"/"+rt.name+".renderTexture");
            var mat=new Material(Shader.Find("Custom/RoomPerspectiveProjection"));mat.SetTexture("_ProjectionTex",rt);mat.SetFloat("_RoomProjectionVisibility",1);mat.SetFloat("_OcclusionTolerance",.12f);AssetDatabase.CreateAsset(mat,Folder+"/Proyeccion_Escalon_"+(i+1)+".mat");
            var cg=new GameObject("Camara_Proyeccion");cg.transform.SetParent(node,true);cg.transform.position=viewpoints[i];cg.transform.LookAt(positions[i]);var cam=cg.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.cullingMask=1<<(11+i);cam.nearClipPlane=.05f;cam.farClipPlane=40;cam.fieldOfView=60;cam.aspect=16f/9;cam.targetTexture=rt;cam.allowHDR=false;
            var projector=cg.AddComponent<PerspectiveProjector>();Field(projector,"projectionCamera",cam);Field(projector,"projectionMaterial",mat);
            var align=node.gameObject.AddComponent<RoomPerspectiveAlignment>();Field(align,"playerCamera",Camera.main);Field(align,"projectionCamera",cam);Field(align,"referenceObject",source);Field(align,"physicalObject",physical);Field(align,"projectionMaterial",mat);Field(align,"prerequisite",prior);Field(align,"positionTolerance",.24f);prior=align;
            var mark=Box("Marca_Alineacion_"+(i+1),puzzle,new Vector3(viewpoints[i].x,i==1?13.215f:12.015f,viewpoints[i].z),new Vector3(.45f,.025f,.45f),colors[i],false);
            if(i==1)mark.transform.SetParent(puzzle.Find("Escalon_1/Escalon_Materializado"),true);
            var text=new GameObject("Numero");text.transform.SetParent(mark.transform,false);text.transform.localPosition=new Vector3(0,.7f,0);text.transform.localRotation=Quaternion.Euler(90,0,0);var tm=text.AddComponent<TextMesh>();tm.text=(i+1).ToString();tm.fontSize=48;tm.characterSize=.18f;tm.anchor=TextAnchor.MiddleCenter;tm.color=Color.white;
            foreach(var mf in root.GetComponentsInChildren<MeshFilter>().Where(m=>m.gameObject.layer==8&&m.GetComponent<Renderer>()!=null).ToArray())
            {if(mf.GetComponent<Renderer>().sharedMaterial.shader.name=="Custom/RoomPerspectiveProjection")continue;var g=new GameObject("Proyeccion_Escalon_"+(i+1));g.transform.SetParent(mf.transform,false);g.layer=8;g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var mr=g.AddComponent<MeshRenderer>();mr.sharedMaterial=mat;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;mr.receiveShadows=false;}
            Bake(cam,mat,i+1);
        }
        Camera.main.cullingMask &= ~((1<<11)|(1<<12)|(1<<13));
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);return "Built clock hall, three projections and bridge gallery.";
    }
    static void BuildBridge(Transform parent)
    {
        var r=Group("05_Galeria_Puente_Reloj",parent);
        Box("Suelo_Recuperacion",r,new Vector3(-8,11.85f,8),new Vector3(16,.3f,16),wood);
        Box("Pared_Sur",r,new Vector3(-8,17,0),new Vector3(16,10,.25f),blue);
        Box("Pared_Norte",r,new Vector3(-8,17,16),new Vector3(16,10,.25f),blue);
        Box("Pared_Fondo",r,new Vector3(-16,17,8),new Vector3(.25f,10,16),green);
        Box("Techo",r,new Vector3(-8,22.1f,8),new Vector3(16,.2f,16),white);
        Box("Plataforma_Inicio",r,new Vector3(-1.5f,15.45f,11),new Vector3(3,.3f,10),wood);
        Box("Plataforma_Destino",r,new Vector3(-13.5f,15.45f,8),new Vector3(5,.3f,16),wood);
        for(int i=0;i<18;i++)Box("Escalon_Retorno_"+(i+1),r,new Vector3(-1.5f,12+(i+1)*.1f,.5f+i*.31f),new Vector3(2.2f,(i+1)*.2f,.32f),gold);
        Box("Puerta_Siguiente_Acertijo",r,new Vector3(-15.83f,17,11),new Vector3(.1f,2.8f,1.9f),gold);
        Box("Marco_Puerta_Izq",r,new Vector3(-15.72f,17,9.96f),new Vector3(.2f,3,.14f),white);Box("Marco_Puerta_Der",r,new Vector3(-15.72f,17,12.04f),new Vector3(.2f,3,.14f),white);
        LightAt(r,new Vector3(-4,20.8f,8));LightAt(r,new Vector3(-13,20.8f,8));
    }
    static void Bake(Camera cam,Material mat,int index)
    {
        Physics.SyncTransforms();const int n=1024;var path=Folder+"/Oclusion_Escalon_"+index+".asset";var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);bool fresh=tex==null;if(fresh)tex=new Texture2D(n,n,TextureFormat.RGBAFloat,false,true);else tex.Reinitialize(n,n,TextureFormat.RGBAFloat,false);var pixels=new Color[n*n];var vp=GL.GetGPUProjectionMatrix(cam.projectionMatrix,true)*cam.worldToCameraMatrix;var inv=vp.inverse;
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){var h=inv*new Vector4((x+.5f)/n*2-1,(y+.5f)/n*2-1,.5f,1);var w=new Vector3(h.x,h.y,h.z)/h.w;RaycastHit hit;if(Physics.Raycast(cam.transform.position,(w-cam.transform.position).normalized,out hit,40,1<<8,QueryTriggerInteraction.Ignore))pixels[y*n+x]=new Color(hit.point.x,hit.point.y,hit.point.z,1);}
        tex.SetPixels(pixels);tex.Apply();tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;if(fresh)AssetDatabase.CreateAsset(tex,path);else EditorUtility.SetDirty(tex);mat.SetTexture("_OcclusionTex",tex);mat.SetFloat("_OcclusionEnabled",1);mat.SetMatrix("_ProjectionVP",vp);cam.Render();
    }
}
