var list=AssetDatabase.LoadAssetAtPath<PlaneShift.LevelData>("Assets/Resources/Config/Level/LevelList.asset");
var bank=list.levels[9].prefab.transform.Find("Map/Roads/GateCourt").GetComponent<MeshCollider>();
var controller=new PlaneShift.MapController();
var sources=(System.Collections.Generic.Dictionary<Collider,PlaneShift.RoadData>)typeof(PlaneShift.MapController).GetField("sources",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(controller);
var source=new PlaneShift.RoadData("GateCourt",Vector3.zero,Vector3.one);sources.Add(bank,source);
if(controller.HasSurface(source,new Vector3(50,22,28)))throw new Exception("Circular clearance cutaway is treated as ground");
if(!controller.HasSurface(source,new Vector3(51.5f,22,28)))throw new Exception("Valid landing is rejected");
return "PASS real mesh support rejects cutaway and accepts the gate landing";
