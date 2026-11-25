using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TwoPlatePictureBuilder
{
    static void Set(object o,string n,object v){o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,v);}
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,true);g.transform.position=pos;g.transform.localScale=size;g.layer=8;g.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play first");var room=GameObject.Find("Casa/Bajo/06_Sala_Tras_Pasillo_Ilustrado");if(room.transform.Find("Acertijo_Dos_Pulsadores_Cuadro")!=null)throw new Exception("Already built");
        var puzzle=new GameObject("Acertijo_Dos_Pulsadores_Cuadro");puzzle.transform.SetParent(room.transform,false);
        var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/MaderaClara.mat");
        var gate=Box("Puerta_Sala_Queso_Dos_Pulsadores",puzzle.transform,new Vector3(-43.78f,7.4f,7.8f),new Vector3(.16f,2.8f,2.1f),wood);
        var source=GameObject.Find("Casa/Sotano/08_Acertijo_04_Puerta_Pulsador").GetComponentInChildren<WeightedPressurePlate>(true);
        var plates=new WeightedPressurePlate[2];for(int i=0;i<2;i++){var g=UnityEngine.Object.Instantiate(source.gameObject,puzzle.transform);g.name="Pulsador_"+(i+1)+"_Presion_Continua";g.transform.SetPositionAndRotation(new Vector3(-40.4f,6.03f,i==0?5.2f:10.8f),Quaternion.identity);g.transform.localScale=Vector3.one;var p=g.GetComponent<WeightedPressurePlate>();Set(p,"door",null);Set(p,"minimumMass",8f);Set(p,"stayOpenAfterPress",false);plates[i]=p;}
        var controller=puzzle.AddComponent<TwoPressurePlateDoor>();Set(controller,"first",plates[0]);Set(controller,"second",plates[1]);Set(controller,"door",gate.transform);
        // Reuse the existing house picture's frame and coloured canvas as one rigid board.
        var sampleRoom=GameObject.Find("Casa/Sotano/01_Tutorial_Escala");var sampleFrame=sampleRoom.GetComponentsInChildren<Transform>().First(t=>t.name=="Marco_Cuadro").GetComponent<Renderer>();var sampleCanvas=sampleRoom.GetComponentsInChildren<Transform>().First(t=>t.name=="Lamina_Cuadro").GetComponent<Renderer>();
        var picture=new GameObject("Cuadro_Interactuable_Para_Dos_Pulsadores");picture.transform.SetParent(puzzle.transform,true);picture.transform.position=new Vector3(-37.4f,6.34f,11.5f);picture.layer=6;
        var frame=Box("Marco_Cuadro",picture.transform,picture.transform.position,new Vector3(1,.1f,1.5f),sampleFrame.sharedMaterial,false);frame.layer=6;
        var canvas=Box("Lamina_Cuadro",picture.transform,picture.transform.position+Vector3.up*.063f,new Vector3(.82f,.025f,1.32f),sampleCanvas.sharedMaterial,false);canvas.layer=6;
        var motif=Box("Motivo_Cuadro",picture.transform,picture.transform.position+Vector3.up*.083f,new Vector3(.3f,.012f,.5f),AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaEstilo/Materiales/Crema.mat"),false);motif.layer=6;
        var collider=picture.AddComponent<BoxCollider>();collider.center=new Vector3(0,.02f,0);collider.size=new Vector3(1,.14f,1.5f);var body=picture.AddComponent<Rigidbody>();body.mass=1;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;picture.AddComponent<ScaleDependentMass>();
        Box("Expositor_Bajo_Cuadro",puzzle.transform,new Vector3(-37.4f,6.125f,11.5f),new Vector3(1.1f,.25f,1.6f),wood);
        // The old hidden-hall paint remains continuous from its original viewing zone.
        var illusion=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/CasaPasilloManzana/Pared_Aparente.mat");foreach(var mf in puzzle.GetComponentsInChildren<MeshFilter>().Where(m=>!m.transform.IsChildOf(picture.transform)).ToArray()){var overlay=new GameObject("Pintura_Anamorfica_Fondo");overlay.transform.SetParent(mf.transform,false);overlay.layer=8;overlay.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=overlay.AddComponent<MeshRenderer>();r.sharedMaterial=illusion;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
        Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(room.scene);EditorSceneManager.SaveScene(room.scene);return "Two basement-style momentary plates, AND-controlled door, scalable rigid picture.";
    }
}
