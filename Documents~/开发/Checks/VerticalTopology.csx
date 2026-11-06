var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Level/Level11/LevelPkg11.prefab");
var roads=root.transform.Find("Map/Roads").GetComponentsInChildren<BoxCollider>().Concat(root.GetComponentsInChildren<PlaneShift.MapMechanism>(true).SelectMany(m=>m.Roads).Cast<BoxCollider>()).ToArray();
var rects=roads.Select(c=>{var p=c.transform.TransformPoint(c.center);var s=Vector3.Scale(c.size,c.transform.lossyScale);return new Rect(p.x-s.x/2,p.z-s.z/2,s.x,s.z);}).ToArray();
var group=Enumerable.Repeat(-1,roads.Length).ToArray();int count=0;for(int i=0;i<roads.Length;i++){if(group[i]>=0)continue;var queue=new System.Collections.Generic.Queue<int>();queue.Enqueue(i);group[i]=count;while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<roads.Length;b++){if(group[b]>=0)continue;float x=Mathf.Min(rects[a].xMax,rects[b].xMax)-Mathf.Max(rects[a].xMin,rects[b].xMin);float z=Mathf.Min(rects[a].yMax,rects[b].yMax)-Mathf.Max(rects[a].yMin,rects[b].yMin);if(x>=-.001f&&z>=-.001f&&(x>.65f||z>.65f)){group[b]=count;queue.Enqueue(b);}}}count++;}
Func<string,int> g=name=>group[Array.FindIndex(roads,c=>c.name==name)];
var names=new[]{"LowerSouthStart","UpperA","CrossGalleryEnd","WestLanding","GoalCross","GoalCourt"};
for(int i=0;i<names.Length;i++)for(int j=i+1;j<names.Length;j++)if(g(names[i])==g(names[j]))throw new Exception("Top projection shortcuts "+names[i]+" to "+names[j]);
var report="PASS "+count+" disconnected top/bottom regions\n"+string.Join("\n",Enumerable.Range(0,count).Select(i=>"Region "+i+": "+string.Join(", ",roads.Where((c,k)=>group[k]==i).Select(c=>c.name).ToArray())).ToArray());
System.IO.File.WriteAllText("Library/VerticalTopology-check.log",report);
return report;
