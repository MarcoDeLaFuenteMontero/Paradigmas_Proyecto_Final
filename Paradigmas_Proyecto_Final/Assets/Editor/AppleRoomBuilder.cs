using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AppleRoomBuilder
{
    const string Folder="Assets/Art/CasaManzana";
    static Material darkWood, wood, cream, white, wall, black, red, leaf;
    static Transform Group(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static Material Asset(string name){return AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/"+name+".mat");}
    static Material ColorMat(string name,Material basis,Color color){var m=new Material(basis);m.name=name;m.color=color;AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;}
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,true);g.transform.position=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;g.layer=8;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void Field(object o,string n,object v){o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,v);}
    static void Light(Transform parent,Vector3 pos){var g=new GameObject("Luz_Calida");g.transform.SetParent(parent,true);g.transform.position=pos;var l=g.AddComponent<Light>();l.type=LightType.Point;l.intensity=2.4f;l.range=15;l.color=new Color(1,.87f,.7f);l.shadows=UnityEngine.LightShadows.None;Box("Plafon",parent,pos+Vector3.up*.4f,new Vector3(.8f,.12f,.8f),white,false);}
    static void Wall(string name,Transform p,Vector3 center,Vector3 size,float floor){Box(name,p,center,size,wall);bool alongX=size.x>size.z;Box(name+"_Friso",p,new Vector3(center.x,floor+.55f,center.z),alongX?new Vector3(size.x,1.1f,.31f):new Vector3(.31f,1.1f,size.z),cream);Box(name+"_Moldura",p,new Vector3(center.x,floor+1.15f,center.z),alongX?new Vector3(size.x,.08f,.36f):new Vector3(.36f,.08f,size.z),wood);}
    static Mesh AppleMesh()
    {
        const int rings=20,segments=32;var vertices=new Vector3[(rings+1)*(segments+1)];var triangles=new int[rings*segments*6];int k=0;
        for(int i=0;i<=rings;i++){float t=Mathf.PI*i/rings;for(int j=0;j<=segments;j++){float a=2*Mathf.PI*j/segments;float radius=.24f*Mathf.Sin(t)*(1+.055f*Mathf.Cos(5*a)*Mathf.Sin(t));float y=.255f*Mathf.Cos(t)-.047f*Mathf.Exp(-t*t*12);vertices[i*(segments+1)+j]=new Vector3(radius*Mathf.Cos(a),y,radius*Mathf.Sin(a));}}
        for(int i=0;i<rings;i++)for(int j=0;j<segments;j++){int a=i*(segments+1)+j,b=a+segments+1;triangles[k++]=a;triangles[k++]=a+1;triangles[k++]=b;triangles[k++]=a+1;triangles[k++]=b+1;triangles[k++]=b;}
        var m=new Mesh();m.name="Manzana";m.vertices=vertices;m.triangles=triangles;m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,Folder+"/Manzana.asset");return m;
    }
    static GameObject Apple(string name,Transform parent,Vector3 pos,Mesh mesh,int layer,bool physical)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,true);g.transform.position=pos;
        var body=new GameObject("Fruta");body.transform.SetParent(g.transform,false);body.AddComponent<MeshFilter>().sharedMesh=mesh;body.AddComponent<MeshRenderer>().sharedMaterial=red;
        var stem=Box("Tallo",g.transform,pos+new Vector3(.015f,.255f,0),new Vector3(.04f,.15f,.04f),darkWood,false);stem.transform.localRotation=Quaternion.Euler(0,0,-14);
        var blade=Box("Hoja",g.transform,pos+new Vector3(.095f,.3f,0),new Vector3(.16f,.025f,.075f),leaf,false);blade.transform.localRotation=Quaternion.Euler(0,0,20);
        foreach(var t in g.GetComponentsInChildren<Transform>())t.gameObject.layer=layer;
        if(physical){var c=g.AddComponent<SphereCollider>();c.radius=.25f;var rb=g.AddComponent<Rigidbody>();rb.mass=1;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;g.AddComponent<ScaleDependentMass>();}return g;
    }
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play first");
        if(GameObject.Find("Casa/Primero/06_Sala_Manzana_Suelo_Fracturado")!=null)throw new Exception("Already built");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","CasaManzana");
        wood=Asset("MaderaClara");cream=Asset("Crema");white=Asset("Marfil");wall=Asset("Salmon");black=Asset("Carbon");darkWood=ColorMat("Madera_Oscura",Asset("Madera"),new Color(.19f,.105f,.055f));red=ColorMat("Manzana_Roja",white,new Color(.78f,.035f,.025f));leaf=ColorMat("Verde_Intenso",white,new Color(.06f,.65f,.16f));
        var pink=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaPistaDucha/Armario_Rosa.mat");var blue=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaLibros/Portada_Azul.mat");var colors=new[]{pink,blue,leaf};var f=BindingFlags.NonPublic|BindingFlags.Instance;
        var steps=GameObject.Find("Casa/Primero/04_Sala_Paso_Pulsador").GetComponentsInChildren<RoomPerspectiveAlignment>(true).OrderBy(p=>p.name).ToArray();for(int i=0;i<3;i++){foreach(var n in new[]{"referenceObject","physicalObject"})((GameObject)typeof(RoomPerspectiveAlignment).GetField(n,f).GetValue(steps[i])).GetComponent<Renderer>().sharedMaterial=colors[i];((Camera)typeof(RoomPerspectiveAlignment).GetField("projectionCamera",f).GetValue(steps[i])).Render();}
        var parent=GameObject.Find("Casa/Primero").transform;var room=Group("06_Sala_Manzana_Suelo_Fracturado",parent);var walls=Group("Paredes",room);var floor=Group("Suelos",room);var props=Group("Muebles_Fijos",room);
        Wall("Sur",walls,new Vector3(-22,19.6f,6),new Vector3(12,8,.25f),15.6f);Wall("Norte",walls,new Vector3(-22,19.6f,16),new Vector3(12,8,.25f),15.6f);Wall("Oeste",walls,new Vector3(-28,19.6f,11),new Vector3(.25f,8,10),15.6f);Box("Este_Cierre_Alto",walls,new Vector3(-16,22.8f,11),new Vector3(.25f,1.6f,10),wall);Box("Techo",room,new Vector3(-22,23.7f,11),new Vector3(12,.2f,10),white);
        // Keep a solid perimeter while the cracked central patch can disappear.
        Box("Suelo_Borde_Oeste",floor,new Vector3(-26,15.45f,11),new Vector3(4,.3f,10),darkWood);Box("Suelo_Borde_Este",floor,new Vector3(-17.5f,15.45f,11),new Vector3(3,.3f,10),darkWood);Box("Suelo_Borde_Sur",floor,new Vector3(-21.5f,15.45f,7),new Vector3(5,.3f,2),darkWood);Box("Suelo_Borde_Norte",floor,new Vector3(-21.5f,15.45f,14.5f),new Vector3(5,.3f,3),darkWood);
        var breakRoot=Group("Suelo_Quebradizo",floor);breakRoot.position=new Vector3(-21.5f,15.45f,10.5f);var collider=breakRoot.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(5,.3f,5);var boards=Group("Tablas_y_Grieta",breakRoot);
        for(int i=0;i<10;i++)Box("Tabla_Agrietada",boards,new Vector3(-23.75f+i*.5f,15.445f,10.5f),new Vector3(.485f,.29f,5),darkWood,false);
        var points=new[]{new Vector3(-23.95f,15.596f,10.6f),new Vector3(-23,15.596f,10.3f),new Vector3(-22.3f,15.596f,10.65f),new Vector3(-21.7f,15.596f,10.2f),new Vector3(-20.8f,15.596f,10.7f),new Vector3(-20.1f,15.596f,10.4f),new Vector3(-19.05f,15.596f,10.6f)};
        for(int i=0;i<points.Length-1;i++){var delta=points[i+1]-points[i];var crack=Box("Raja_Visible",boards,(points[i+1]+points[i])*.5f,new Vector3(delta.magnitude,.012f,.075f),black,false);crack.transform.rotation=Quaternion.Euler(0,-Mathf.Atan2(delta.z,delta.x)*Mathf.Rad2Deg,0);}
        Box("Mesa_Manzana",props,new Vector3(-26.6f,16.05f,10),new Vector3(1.5f,.9f,1.5f),wood);
        Light(room,new Vector3(-22,22.5f,11));
        // Replace the gallery's terminal wall with a real doorway.
        var gallery=GameObject.Find("Casa/Primero/05_Galeria_Puente_Reloj").transform;var oldWall=gallery.Find("Pared_Fondo");oldWall.gameObject.SetActive(false);gallery.Find("Puerta_Siguiente_Acertijo").gameObject.SetActive(false);
        Box("Fondo_Sur_Puerta",gallery,new Vector3(-16,17,5),new Vector3(.25f,10,10),Asset("Salvia"));Box("Fondo_Norte_Puerta",gallery,new Vector3(-16,17,14),new Vector3(.25f,10,4),Asset("Salvia"));Box("Dintel_Nueva_Sala",gallery,new Vector3(-16,20.2f,11),new Vector3(.25f,3.6f,2),Asset("Salvia"));
        var lower=Group("04_Regreso_Desde_Manzana",GameObject.Find("Casa/Bajo").transform);Box("Suelo_Recepcion",lower,new Vector3(-22,5.85f,11),new Vector3(12,.3f,10),wood);Wall("Sur",lower,new Vector3(-22,10.8f,6),new Vector3(12,9.6f,.25f),6);Wall("Norte",lower,new Vector3(-22,10.8f,16),new Vector3(12,9.6f,.25f),6);Wall("Oeste",lower,new Vector3(-28,10.8f,11),new Vector3(.25f,9.6f,10),6);
        Wall("Este_Sur",lower,new Vector3(-16,10.8f,8),new Vector3(.25f,9.6f,4),6);Wall("Este_Norte",lower,new Vector3(-16,10.8f,14.5f),new Vector3(.25f,9.6f,3),6);Box("Este_Dintel",lower,new Vector3(-16,12.2f,11.5f),new Vector3(.25f,6.8f,3),wall);Light(lower,new Vector3(-22,9.5f,11));
        Box("Pasillo_Suelo",lower,new Vector3(-8,5.85f,11.5f),new Vector3(16,.3f,3),wood);Box("Pasillo_Techo",lower,new Vector3(-8,9.1f,11.5f),new Vector3(16,.2f,3),white);Wall("Pasillo_Sur",lower,new Vector3(-8,7.5f,10),new Vector3(16,3,.25f),6);Wall("Pasillo_Norte",lower,new Vector3(-8,7.5f,13),new Vector3(16,3,.25f),6);Light(lower,new Vector3(-10,8.5f,11.5f));Light(lower,new Vector3(-2,8.5f,11.5f));
        // A locked return door prevents entering this route from the start of Bajo.
        var gate=Box("Puerta_Regreso_Tras_Rotura",lower,new Vector3(0,7.4f,11.5f),new Vector3(.3f,2.8f,2.2f),wood);
        var shared=GameObject.Find("Casa/Sotano/09_Ascenso_Al_Bajo/Paredes/Oeste");shared.SetActive(false);var pw=shared.transform.parent;Box("Oeste_Bajo_Paso_Regreso",pw,new Vector3(0,3,13),new Vector3(.25f,6,6),Asset("Salvia"));Box("Oeste_Lateral_Regreso_Sur",pw,new Vector3(0,7.4f,10.2f),new Vector3(.25f,2.8f,.4f),Asset("Salvia"));Box("Oeste_Lateral_Regreso_Norte",pw,new Vector3(0,7.4f,14.3f),new Vector3(.25f,2.8f,3.4f),Asset("Salvia"));
        var facing=GameObject.Find("Casa/Bajo/01_Recibidor_Desde_Sotano/Paredes/Acabado_Interior_Oeste_Recibidor");facing.SetActive(false);Box("Acabado_Oeste_Lado_Sur",facing.transform.parent,new Vector3(.17f,7.4f,10.2f),new Vector3(.04f,2.8f,.4f),wall);Box("Acabado_Oeste_Lado_Norte",facing.transform.parent,new Vector3(.17f,7.4f,14.3f),new Vector3(.04f,2.8f,3.4f),wall);
        var puzzle=Group("Acertijo_Manzana",room);var mesh=AppleMesh();var applePos=new Vector3(-26.6f,16.75f,10);var source=Apple("Manzana_Referencia",puzzle,applePos,mesh,14,false);var physical=Apple("Manzana_Interactuable",puzzle,applePos,mesh,6,true);physical.SetActive(false);
        foreach(Transform t in lower)if(t.name.StartsWith("Este_"))t.position+=Vector3.left*.04f;
        var mat=new Material(Shader.Find("Custom/RoomPerspectiveProjection"));AssetDatabase.CreateAsset(mat,Folder+"/Proyeccion_Manzana.mat");var rt=new RenderTexture(1024,576,24,RenderTextureFormat.ARGB32);AssetDatabase.CreateAsset(rt,Folder+"/RT_Manzana.renderTexture");mat.SetTexture("_ProjectionTex",rt);mat.SetFloat("_RoomProjectionVisibility",1);mat.SetFloat("_OcclusionTolerance",.12f);
        var cg=new GameObject("Camara_Proyeccion");cg.transform.SetParent(puzzle,true);cg.transform.position=new Vector3(-24,17.33f,12.5f);cg.transform.LookAt(applePos);var cam=cg.AddComponent<Camera>();cam.targetTexture=rt;cam.cullingMask=1<<14;cam.backgroundColor=Color.clear;cam.clearFlags=CameraClearFlags.SolidColor;cam.nearClipPlane=.05f;cam.farClipPlane=30;cam.fieldOfView=60;cam.aspect=16f/9;cam.allowHDR=false;
        var projector=cg.AddComponent<PerspectiveProjector>();Field(projector,"projectionCamera",cam);Field(projector,"projectionMaterial",mat);var align=puzzle.gameObject.AddComponent<RoomPerspectiveAlignment>();Field(align,"playerCamera",Camera.main);Field(align,"projectionCamera",cam);Field(align,"referenceObject",source);Field(align,"physicalObject",physical);Field(align,"projectionMaterial",mat);Field(align,"positionTolerance",.24f);
        Box("Marca_Manzana",puzzle,new Vector3(-24,15.615f,12.5f),new Vector3(.45f,.025f,.45f),red,false);
        foreach(var mf in room.GetComponentsInChildren<MeshFilter>().Where(m=>m.gameObject.layer==8).ToArray()){var g=new GameObject("Proyeccion_Manzana");g.transform.SetParent(mf.transform,false);g.layer=8;g.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
        var floorScript=breakRoot.gameObject.AddComponent<BreakableWoodFloor>();Field(floorScript,"apple",physical.GetComponent<Rigidbody>());Field(floorScript,"boards",boards.gameObject);Field(floorScript,"floorCollider",collider);Field(floorScript,"returnDoor",gate);
        Camera.main.cullingMask&=~(1<<14);Bake(cam,mat);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.gameObject.scene);EditorSceneManager.SaveScene(room.gameObject.scene);return "Colored platforms and apple room with breakable floor and return to Bajo built.";
    }
    static void Bake(Camera cam,Material mat)
    {
        Physics.SyncTransforms();const int n=1024;var tex=new Texture2D(n,n,TextureFormat.RGBAFloat,false,true);var pixels=new Color[n*n];var vp=GL.GetGPUProjectionMatrix(cam.projectionMatrix,true)*cam.worldToCameraMatrix;var inv=vp.inverse;for(int y=0;y<n;y++)for(int x=0;x<n;x++){var h=inv*new Vector4((x+.5f)/n*2-1,(y+.5f)/n*2-1,.5f,1);var w=new Vector3(h.x,h.y,h.z)/h.w;RaycastHit hit;if(Physics.Raycast(cam.transform.position,(w-cam.transform.position).normalized,out hit,30,1<<8,QueryTriggerInteraction.Ignore))pixels[y*n+x]=new Color(hit.point.x,hit.point.y,hit.point.z,1);}tex.SetPixels(pixels);tex.Apply();tex.filterMode=FilterMode.Point;tex.wrapMode=TextureWrapMode.Clamp;AssetDatabase.CreateAsset(tex,Folder+"/Oclusion_Manzana.asset");mat.SetTexture("_OcclusionTex",tex);mat.SetFloat("_OcclusionEnabled",1);mat.SetMatrix("_ProjectionVP",vp);cam.Render();
    }
}
