var ui=UnityEngine.Object.FindObjectOfType<PlaneShift.UIMgr>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var save=PlayerPrefs.GetString("PlayerRecord","");bool hadSave=PlayerPrefs.HasKey("PlayerRecord");var keys=Enumerable.Range(0,11).Select(i=>PlayerPrefs.GetInt("LevelComplete_"+i,-1)).ToArray();var oldScale=Time.timeScale;var oldBackground=Application.runInBackground;
ui.helpUI.Hide();ui.settingsUI.Hide();ui.guide.enabled=false;ui.guideUI.Hide();Time.timeScale=4;Application.runInBackground=true;
var commands=new[]{new {n=11,k="Load",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.East},
new {n=11,k="FlatWalk",p=new Vector3(-14f,6f,-8f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.South},
new {n=11,k="FlatWalk",p=new Vector3(24f,6f,-8f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Use",p=new Vector3(0f,0f,0f),name="LiftA",d=PlaneShift.ViewDirection.North},
new {n=11,k="Walk",p=new Vector3(34f,12f,14f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.South},
new {n=11,k="FlatWalk",p=new Vector3(18f,12f,14f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.West},
new {n=11,k="FlatWalk",p=new Vector3(18f,12f,38.5f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Walk",p=new Vector3(-38f,12f,38.5f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Walk",p=new Vector3(-38f,12f,39f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Use",p=new Vector3(0f,0f,0f),name="LiftB",d=PlaneShift.ViewDirection.North},
new {n=11,k="Walk",p=new Vector3(-38f,18f,12f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Compress",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="FlatWalk",p=new Vector3(-6f,18f,12f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Expand",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Walk",p=new Vector3(-6f,18f,-38f),name="",d=PlaneShift.ViewDirection.North},
new {n=11,k="Exit",p=new Vector3(0f,0f,0f),name="",d=PlaneShift.ViewDirection.North}};
var log=System.IO.Path.Combine(Application.dataPath,"../Library/LoopLevel-check.log");System.IO.File.WriteAllText(log,"Ten level actual-controller traversal\n");
int index=0,phase=0,last=-1;double since=EditorApplication.timeSinceStartup;PlaneShift.SpaceManager space=null;PlaneShift.PlayerController mover=null;PlaneShift.MapMechanism used=null;
Action finish=()=>{Time.timeScale=oldScale;Application.runInBackground=oldBackground;if(hadSave)PlayerPrefs.SetString("PlayerRecord",save);else PlayerPrefs.DeleteKey("PlayerRecord");for(int i=0;i<11;i++){if(keys[i]<0)PlayerPrefs.DeleteKey("LevelComplete_"+i);else PlayerPrefs.SetInt("LevelComplete_"+i,keys[i]);}PlayerPrefs.Save();};
EditorApplication.CallbackFunction tick=null;tick=()=>{try{
if(!EditorApplication.isPlaying){EditorApplication.update-=tick;finish();return;}if(Time.frameCount==last)return;last=Time.frameCount;
if(index>=commands.Length){System.IO.File.AppendAllText(log,"PASS Level11 main route and F exit\n");EditorApplication.update-=tick;finish();return;}
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
bool flat=cmd.k=="FlatWalk";var delta=cmd.p-space.Player.Position;delta.y=0;if(flat && space.Data.axis!=PlaneShift.SpaceAxis.Y)delta[(int)space.Data.axis]=0;
if(delta.magnitude<=0.16f){space.Player.RefreshGround();done=space.Player.Data.grounded;if(!flat&&Mathf.Abs(space.Player.Position.y-cmd.p.y)>0.3f)throw new Exception("Wrong world height");}
if(!done){var direction=delta.magnitude>0.12f?delta.normalized:Vector3.zero;var previous=space.Player.Position;mover.Move(direction,space.Player.Data,flat&&space.Data.axis==PlaneShift.SpaceAxis.Y,flat?(PlaneShift.SpaceAxis?)space.Data.axis:null,space.Map.Plane);space.Player.RefreshGround();if(flat&&space.Data.axis==PlaneShift.SpaceAxis.Y&&!space.Player.Data.grounded)space.Player.SetFlat(space.Data.axis,space.Map.Plane,previous-Vector3.up*.025f);}
}
if(done){System.IO.File.AppendAllText(log,"PASS "+cmd.n+" "+index+" "+cmd.k+" "+cmd.name+(space==null?"":" pos="+space.Player.Position+" road="+(space.Player.Data.road==null?"none":space.Player.Data.road.name))+"\n");index++;phase=0;since=EditorApplication.timeSinceStartup;}
}catch(Exception e){System.IO.File.AppendAllText(log,"FAIL index="+index+" "+e.Message+" pos="+(space==null?"none":space.Player.Position.ToString())+"\n");EditorApplication.update-=tick;finish();}};
EditorApplication.update+=tick;return "Started ten level route test";
