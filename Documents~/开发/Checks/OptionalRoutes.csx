var ui=UnityEngine.Object.FindObjectOfType<PlaneShift.UIMgr>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var save=PlayerPrefs.GetString("PlayerRecord","");bool hadSave=PlayerPrefs.HasKey("PlayerRecord");var keys=Enumerable.Range(0,10).Select(i=>PlayerPrefs.GetInt("LevelComplete_"+i,-1)).ToArray();var oldScale=Time.timeScale;var oldBackground=Application.runInBackground;
ui.helpUI.Hide();ui.settingsUI.Hide();ui.guide.enabled=false;ui.guideUI.Hide();Time.timeScale=2;Application.runInBackground=true;
var commands=new[]{new {n=5,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-12f,4f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-12f,4f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Use",p=new Vector3(0f,0f,0f),name="GalleryLift",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(18f,12f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(18f,12f,12f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-22f,12f,12f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-54f,12f,12f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=5,k="FlatWalk",p=new Vector3(-54f,0f,30f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-54f,20f,40f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Walk",p=new Vector3(-24f,20f,40f),name="",d=PlaneShift.ViewDirection.North},
new {n=5,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-13f,8f,1f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Use",p=new Vector3(0f,0f,0f),name="ReturnBridge",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-12f,8f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-7.5f,8f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-7.5f,8f,14f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=6,k="FlatWalk",p=new Vector3(0f,0f,16f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(24f,14f,16f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(24f,14f,42f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=6,k="FlatWalk",p=new Vector3(24f,0f,44f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="FlatWalk",p=new Vector3(8f,0f,44f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-8f,22f,44f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Walk",p=new Vector3(-8f,22f,22f),name="",d=PlaneShift.ViewDirection.North},
new {n=6,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(-12f,8f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(-12f,8f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Use",p=new Vector3(0f,0f,0f),name="OptionalLift",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(0f,14f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(0f,14f,22f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.West},
new {n=7,k="FlatWalk",p=new Vector3(0f,0f,38f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="FlatWalk",p=new Vector3(0f,0f,45f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(44f,6f,45f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(44f,6f,26f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(44f,6f,56f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(14f,6f,56f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.West},
new {n=7,k="FlatWalk",p=new Vector3(28f,0f,74f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(-6f,6f,82f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=7,k="FlatWalk",p=new Vector3(-6f,0f,96f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Walk",p=new Vector3(-6f,14f,104f),name="",d=PlaneShift.ViewDirection.North},
new {n=7,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(-12f,10.5f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(-12f,10.5f,5f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Use",p=new Vector3(0f,0f,0f),name="GalleryLift",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(0f,16.5f,5f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(0f,16.5f,20f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Use",p=new Vector3(0f,0f,0f),name="OptionalTurnBridge",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(0f,16.5f,24f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(14f,16.5f,24.8f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(14f,16.5f,38f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.West},
new {n=9,k="FlatWalk",p=new Vector3(14f,0f,45f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(44f,8.5f,44.5f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.West},
new {n=9,k="FlatWalk",p=new Vector3(35f,0f,26f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(74f,8.5f,26f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.East},
new {n=9,k="FlatWalk",p=new Vector3(60f,0f,48f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(92f,8.5f,52f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=9,k="FlatWalk",p=new Vector3(92f,0f,66f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(92f,18.5f,78f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Walk",p=new Vector3(62f,18.5f,78f),name="",d=PlaneShift.ViewDirection.North},
new {n=9,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(-14f,10f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(-14f,10f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Use",p=new Vector3(0f,0f,0f),name="WestLift",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(0f,18f,6f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(0f,18f,10f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Use",p=new Vector3(0f,0f,0f),name="HighCorner",d=PlaneShift.ViewDirection.North},
new {n=10,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="FlatWalk",p=new Vector3(4.5f,0f,10f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(18f,18f,26f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=10,k="FlatWalk",p=new Vector3(28f,0f,26f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(30f,22f,28f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Use",p=new Vector3(0f,0f,0f),name="GateFork",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(52f,22f,28f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(52f,22f,58f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.East},
new {n=10,k="FlatWalk",p=new Vector3(52f,0f,76f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(76f,22f,84f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.Top},
new {n=10,k="FlatWalk",p=new Vector3(76f,0f,88f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="FlatWalk",p=new Vector3(92f,0f,88f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(108f,32f,88f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Walk",p=new Vector3(108f,32f,66f),name="",d=PlaneShift.ViewDirection.North},
new {n=10,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North}};
var log=System.IO.Path.Combine(Application.dataPath,"../Library/OptionalRoutes-check.log");System.IO.File.WriteAllText(log,"Optional actual-controller traversal\n");
int index=0,phase=0,last=-1;double since=EditorApplication.timeSinceStartup;PlaneShift.SpaceManager space=null;PlaneShift.PlayerController mover=null;PlaneShift.MapMechanism used=null;
Action finish=()=>{Time.timeScale=oldScale;Application.runInBackground=oldBackground;if(hadSave)PlayerPrefs.SetString("PlayerRecord",save);else PlayerPrefs.DeleteKey("PlayerRecord");for(int i=0;i<10;i++){if(keys[i]<0)PlayerPrefs.DeleteKey("LevelComplete_"+i);else PlayerPrefs.SetInt("LevelComplete_"+i,keys[i]);}PlayerPrefs.Save();};
EditorApplication.CallbackFunction tick=null;tick=()=>{try{
if(!EditorApplication.isPlaying){EditorApplication.update-=tick;finish();return;}if(Time.frameCount==last)return;last=Time.frameCount;
if(index>=commands.Length){System.IO.File.AppendAllText(log,"PASS five optional routes and F exits\n");EditorApplication.update-=tick;finish();return;}
var cmd=commands[index];if(EditorApplication.timeSinceStartup-since>35)throw new Exception("Timeout "+cmd.k+" "+cmd.name);
bool done=false;
if(cmd.k=="Load"){
if(phase==0){if(ui.levels.Current==null||ui.levels.CurrentIndex!=cmd.n-1)ui.PlayLevel(cmd.n-1);phase=1;return;}
if(ui.levels.Current==null||ui.levels.CurrentIndex!=cmd.n-1)return;space=ui.levels.Current.Space;if(space.Map.Data.roads.Count==0)return;mover=(PlaneShift.PlayerController)typeof(PlaneShift.PlayerManager).GetField("controller",flags).GetValue(space.Player);done=true;
}else if(cmd.k=="Compress"){
if(phase==0){space.Data.direction=cmd.d;space.View.ChangeView(cmd.d,false,true);typeof(PlaneShift.SpaceManager).GetMethod("Compress",flags).Invoke(space,null);if(space.Data.mode!=PlaneShift.SpaceMode.Changing)throw new Exception("Compression rejected");phase=1;return;}
done=space.Data.mode==PlaneShift.SpaceMode.Flat&&!space.View.IsChanging;
}else if(cmd.k=="Expand"){
if(phase==0){space.Player.RefreshGround();typeof(PlaneShift.SpaceManager).GetMethod("TryExpand",flags).Invoke(space,null);if(space.Data.mode==PlaneShift.SpaceMode.Flat)throw new Exception("Expand rejected source="+space.Player.Data.road.name);phase=1;return;}done=space.Data.mode==PlaneShift.SpaceMode.World&&!space.View.IsChanging;
}else if(cmd.k=="Use"){
if(phase==0){used=space.Map.GetComponentsInChildren<PlaneShift.MapMechanism>().First(m=>m.name==cmd.name);space.Player.UpdateInteraction(true);if(!(space.Player.Target is PlaneShift.MechanismButton)||((PlaneShift.MechanismButton)space.Player.Target).Mechanism!=used)throw new Exception("Wrong mechanism target "+cmd.name);if(!space.Player.TryInteract())throw new Exception("Use rejected "+cmd.name+" pos "+space.Player.Position);phase=1;return;}done=!space.Map.IsMoving;
}else if(cmd.k=="Exit"){
space.Player.RefreshGround();space.Player.UpdateInteraction(true);if(!(space.Player.Target is PlaneShift.LevelExit))throw new Exception("F selected "+(space.Player.Target==null?"none":space.Player.Target.GetType().Name)+" instead of exit");if(!space.Player.TryInteract())throw new Exception("Player F exit failed");done=true;
}else{
bool flat=cmd.k=="FlatWalk";var delta=cmd.p-space.Player.Position;delta.y=0;if(flat&&space.Data.axis!=PlaneShift.SpaceAxis.Y)delta[(int)space.Data.axis]=0;
if(delta.magnitude<=0.16f){space.Player.RefreshGround();done=space.Player.Data.grounded;if(!flat&&Mathf.Abs(space.Player.Position.y-cmd.p.y)>0.3f)throw new Exception("Wrong world height");}
if(!done){var direction=delta.magnitude>0.12f?delta.normalized:Vector3.zero;var previous=space.Player.Position;mover.Move(direction,space.Player.Data,flat&&space.Data.axis==PlaneShift.SpaceAxis.Y,flat?(PlaneShift.SpaceAxis?)space.Data.axis:null,space.Map.Plane);space.Player.RefreshGround();if(flat&&space.Data.axis==PlaneShift.SpaceAxis.Y&&!space.Player.Data.grounded)space.Player.SetFlat(space.Data.axis,space.Map.Plane,previous-Vector3.up*.025f);}
}
if(done){System.IO.File.AppendAllText(log,"PASS "+cmd.n+" "+index+" "+cmd.k+" "+cmd.name+(space==null?"":" pos="+space.Player.Position+" road="+(space.Player.Data.road==null?"none":space.Player.Data.road.name))+"\n");index++;phase=0;since=EditorApplication.timeSinceStartup;}
}catch(Exception e){System.IO.File.AppendAllText(log,"FAIL index="+index+" "+e.Message+" pos="+(space==null?"none":space.Player.Position.ToString())+"\n");EditorApplication.update-=tick;finish();}};
EditorApplication.update+=tick;return "Started optional route tests";



