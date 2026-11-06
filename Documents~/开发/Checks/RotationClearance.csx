// Authored mesh/collider regression; execute in the Unity Editor without entering Play mode.
// 720-sided radial clearance boundaries align exactly at every sampled five-degree angle.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var report=new System.Text.StringBuilder();int rotators=0;
foreach(var id in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/Prefab/Level"})){
 var path=AssetDatabase.GUIDToAssetPath(id);if(path.Contains("/Map/"))continue;
 var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
 foreach(var mechanism in root.GetComponentsInChildren<PlaneShift.MapMechanism>(true)){
  if(mechanism.Data.turnAngle==0)continue;rotators++;
  var pivot=(Transform)mechanism.GetType().GetField("movingRoot",flags).GetValue(mechanism);
  float radius=mechanism.name=="HighCorner"?14:mechanism.name=="OptionalTurnBridge"?5:9;
  foreach(var road in pivot.GetComponentsInChildren<Collider>(true)){
   var meshCollider=road as MeshCollider;if(meshCollider==null)throw new System.Exception(mechanism.name+" road missing authored MeshCollider");
   if(meshCollider.sharedMesh!=road.GetComponent<MeshFilter>().sharedMesh)throw new System.Exception("Visual/collider mesh mismatch");
   foreach(int degrees in Enumerable.Range(0,72).Select(i=>i*5))foreach(var v in meshCollider.sharedMesh.vertices){
    var point=Quaternion.Euler(0,degrees,0)*(road.transform.TransformPoint(v)-pivot.position);
    if(new Vector2(point.x,point.z).magnitude>radius+.0001f)throw new System.Exception(mechanism.name+" exceeds sweep radius");
   }
  }
  foreach(var bank in root.GetComponentsInChildren<Collider>(true)){
   if(bank.transform.IsChildOf(pivot)||bank.GetComponent<MeshFilter>()==null)continue;
   var mf=bank.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;if(mesh==null)continue;
   var vs=mesh.vertices.Select(v=>bank.transform.TransformPoint(v)-pivot.position).ToArray();
   if(vs.Max(v=>v.y)<-.49f||vs.Min(v=>v.y)>.49f)continue;
   var ts=mesh.triangles;
   for(int j=0;j<ts.Length;j+=3){var a3=vs[ts[j]];var b3=vs[ts[j+1]];var c3=vs[ts[j+2]];
    if(Mathf.Abs(a3.y-b3.y)>.001f||Mathf.Abs(a3.y-c3.y)>.001f)continue;
    var a=new Vector2(a3.x,a3.z);var b=new Vector2(b3.x,b3.z);var c=new Vector2(c3.x,c3.z);
    float minimum=float.MaxValue;var points=new[]{a,b,c};
    for(int e=0;e<3;e++){var start=points[e];var delta=points[(e+1)%3]-start;float t=Mathf.Clamp01(-Vector2.Dot(start,delta)/Mathf.Max(.0000001f,delta.sqrMagnitude));minimum=Mathf.Min(minimum,(start+t*delta).magnitude);}
    if(minimum<radius*Mathf.Cos(Mathf.PI/720)-.001f)throw new System.Exception(mechanism.name+" bank invades radial sweep: "+bank.name+" radius="+minimum);
   }
  }
  report.AppendLine(mechanism.name+": visual/collider identity and 72 sampled radial sweeps PASS; radius="+radius);
 }
}
if(rotators!=8)throw new System.Exception("Expected eight rotators; found "+rotators);
return report.ToString();


