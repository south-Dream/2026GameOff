var report=new System.Text.StringBuilder();int overlaps=0;
foreach(var id in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Resources/Prefab/Level"})){
var path=AssetDatabase.GUIDToAssetPath(id);if(path.Contains("/Map/"))continue;var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;var moving=root.GetComponentsInChildren<PlaneShift.MapMechanism>(true).Select(m=>(Transform)m.GetType().GetField("movingRoot",flags).GetValue(m)).ToArray();
var banks=root.GetComponentsInChildren<Collider>(true).Where(c=>!moving.Any(t=>c.transform.IsChildOf(t))&&c.GetComponent<MeshFilter>()!=null&&c.GetComponent<MeshFilter>().sharedMesh!=null).ToArray();
for(int a=0;a<banks.Length;a++)for(int b=a+1;b<banks.Length;b++){
var ma=banks[a].GetComponent<MeshFilter>().sharedMesh;var mb=banks[b].GetComponent<MeshFilter>().sharedMesh;
var va=ma.vertices.Select(v=>banks[a].transform.TransformPoint(v)).ToArray();var vb=mb.vertices.Select(v=>banks[b].transform.TransformPoint(v)).ToArray();var ta=ma.triangles;var tb=mb.triangles;bool hit=false;
for(int i=0;i<ta.Length&&!hit;i+=3){var pa=new[]{va[ta[i]],va[ta[i+1]],va[ta[i+2]]};if(Vector3.Cross(pa[1]-pa[0],pa[2]-pa[0]).y<.000001f)continue;
for(int j=0;j<tb.Length&&!hit;j+=3){var pb=new[]{vb[tb[j]],vb[tb[j+1]],vb[tb[j+2]]};if(Mathf.Abs(pa[0].y-pb[0].y)>.001f||Vector3.Cross(pb[1]-pb[0],pb[2]-pb[0]).y<.000001f)continue;
if(Mathf.Max(pa[0].x,pa[1].x,pa[2].x)<=Mathf.Min(pb[0].x,pb[1].x,pb[2].x)||Mathf.Min(pa[0].x,pa[1].x,pa[2].x)>=Mathf.Max(pb[0].x,pb[1].x,pb[2].x)||Mathf.Max(pa[0].z,pa[1].z,pa[2].z)<=Mathf.Min(pb[0].z,pb[1].z,pb[2].z)||Mathf.Min(pa[0].z,pa[1].z,pa[2].z)>=Mathf.Max(pb[0].z,pb[1].z,pb[2].z))continue;
bool separated=false;
foreach(var tri in new[]{pa,pb})for(int e=0;e<3;e++){var d=tri[(e+1)%3]-tri[e];var axis=new Vector2(-d.z,d.x).normalized;var da=pa.Select(v=>v.x*axis.x+v.z*axis.y).ToArray();var db=pb.Select(v=>v.x*axis.x+v.z*axis.y).ToArray();if(Mathf.Min(da.Max(),db.Max())-Mathf.Max(da.Min(),db.Min())<=.0001f)separated=true;}if(!separated)hit=true;
}}
if(hit){overlaps++;report.AppendLine(path+" "+banks[a].name+" / "+banks[b].name);}
}}
if(overlaps!=0)throw new Exception(report.ToString());
return "PASS static coplanar overlaps=0";


