var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var saved = UnityEngine.PlayerPrefs.GetString("PlayerRecord", "");
var existed = UnityEngine.PlayerPrefs.HasKey("PlayerRecord");
var root = new UnityEngine.GameObject("GuideChecks");
root.SetActive(false);
var defaults = UnityEngine.ScriptableObject.CreateInstance<PlaneShift.RecordData>();
try
{
    UnityEngine.PlayerPrefs.DeleteKey("PlayerRecord");
    var record = root.AddComponent<PlaneShift.RecordManager>();
    typeof(PlaneShift.RecordManager).GetField("defaults", flags).SetValue(record, defaults);
    record.Init(); record.ReadHelp(); record.Finish(PlaneShift.GuideStep.Move); record.Finish(PlaneShift.GuideStep.Move);
    var second = root.AddComponent<PlaneShift.RecordManager>();
    typeof(PlaneShift.RecordManager).GetField("defaults", flags).SetValue(second, defaults);
    second.Init();
    if (!second.Data.helpRead || !second.HasStep(PlaneShift.GuideStep.Move) || second.Data.steps.Count != 1 || defaults.helpRead)
        throw new System.Exception("Record persistence or default isolation failed");
    var input = UnityLightFrame.InputManager.Instance;
    foreach (UnityLightFrame.E_InputEventType key in System.Enum.GetValues(typeof(UnityLightFrame.E_InputEventType)))
        input.SetInput(key, UnityLightFrame.InputInfo.E_TriggerType.Press, UnityEngine.KeyCode.A);
    var levels = root.AddComponent<PlaneShift.LevelManager>();
    var guide = root.AddComponent<PlaneShift.GuideManager>();
    guide.levels = levels;
    guide.ui = root.AddComponent<PlaneShift.GuideUI>();
    guide.SetHelp(true);
    if (input.totalToggle) throw new System.Exception("Help did not suppress gameplay");
    guide.SetHelp(false);
    typeof(PlaneShift.GuideManager).GetField("release", flags).SetValue(guide, true);
    guide.Apply();
    foreach (UnityLightFrame.E_InputEventType key in System.Enum.GetValues(typeof(UnityLightFrame.E_InputEventType)))
        if (input.GetInput(key).isActive) throw new System.Exception("Release guard leaked a binding");
    typeof(PlaneShift.GuideManager).GetField("release", flags).SetValue(guide, false);
    guide.Apply();
    foreach (UnityLightFrame.E_InputEventType key in System.Enum.GetValues(typeof(UnityLightFrame.E_InputEventType)))
        if (!input.GetInput(key).isActive) throw new System.Exception("Idle guide failed to restore binding");
    foreach (PlaneShift.GuideStep value in new[] { PlaneShift.GuideStep.ViewSide, PlaneShift.GuideStep.ViewTop, PlaneShift.GuideStep.Compress }) record.Finish(value);
    if (PlaneShift.GuideManager.NextStep(record, PlaneShift.SpaceMode.Flat) != PlaneShift.GuideStep.FlatMove)
        throw new System.Exception("FlatMove prerequisite failed");
    if (PlaneShift.GuideManager.NextStep(record, PlaneShift.SpaceMode.World) != PlaneShift.GuideStep.Compress)
        throw new System.Exception("Restart prerequisite recovery failed");
    record.Finish(PlaneShift.GuideStep.FlatMove);
    if (PlaneShift.GuideManager.NextStep(record, PlaneShift.SpaceMode.Flat) != PlaneShift.GuideStep.Expand)
        throw new System.Exception("Expand prerequisite failed");
    if (PlaneShift.GuideManager.NextStep(record, PlaneShift.SpaceMode.World) != PlaneShift.GuideStep.Compress)
        throw new System.Exception("Expand must not begin in World");
    levels.SetPaused(true); levels.SetHelpPaused(true); levels.SetHelpPaused(false);
    if (!levels.Paused) throw new System.Exception("Help overrode settings pause");
    levels.SetPaused(false);
    if (levels.Paused) throw new System.Exception("Pause failed to restore");
    UnityEngine.Debug.Log("Record input and guide lifecycle checks passed");
}
finally
{
    UnityEngine.Object.DestroyImmediate(root);
    UnityEngine.Object.DestroyImmediate(defaults);
    if (existed) UnityEngine.PlayerPrefs.SetString("PlayerRecord", saved); else UnityEngine.PlayerPrefs.DeleteKey("PlayerRecord");
    UnityEngine.PlayerPrefs.Save();
}
return "PASS record persistence defaults isolation input masks prerequisite recovery and independent pauses";
