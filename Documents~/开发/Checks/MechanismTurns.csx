var root = new UnityEngine.GameObject("MechanismCheck");
try
{
    var mechanism = root.AddComponent<PlaneShift.MapMechanism>();
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    var moving = new UnityEngine.GameObject("Moving");
    moving.transform.SetParent(root.transform);
    var first = moving.AddComponent<UnityEngine.BoxCollider>();
    var secondObject = new UnityEngine.GameObject("Second");
    secondObject.transform.SetParent(moving.transform);
    secondObject.transform.localPosition = new UnityEngine.Vector3(2, 0, 0);
    var second = secondObject.AddComponent<UnityEngine.BoxCollider>();
    var button = root.AddComponent<UnityEngine.BoxCollider>();
    typeof(PlaneShift.MapMechanism).GetField("movingRoot", flags).SetValue(mechanism, moving.transform);
    typeof(PlaneShift.MapMechanism).GetField("button", flags).SetValue(mechanism, button);
    typeof(PlaneShift.MapMechanism).GetField("roads", flags).SetValue(mechanism, new[] { first, second });
    mechanism.Data.turnAngle = 90;
    mechanism.Data.duration = 1;
    mechanism.Init();
    for (int i = 1; i <= 4; i++)
    {
        mechanism.StartMove();
        mechanism.Tick(0.5f);
        float expected = (i - 1) * 90 + 45;
        if (UnityEngine.Quaternion.Angle(moving.transform.rotation, UnityEngine.Quaternion.Euler(0, expected, 0)) > 0.01f)
            throw new System.Exception("Rotation midpoint failed at turn " + i);
        mechanism.Tick(0.5f);
    }
    if (UnityEngine.Quaternion.Angle(moving.transform.rotation, UnityEngine.Quaternion.identity) > 0.01f)
        throw new System.Exception("Fourth turn did not return to origin");
    var map = new PlaneShift.MapController();
    var data = new PlaneShift.MapData();
    map.Init(moving.transform, root.transform, null, data, new[] { mechanism });
    PlaneShift.RoadData road;
    if (data.roads.Count != 2 || !map.TryGetRoad(second, out road) || !map.IsOnRoad(mechanism, road))
        throw new System.Exception("Second segment membership failed");
    mechanism.ResetState();
    return "PASS four turns midpoint direction reset and second segment membership";
}
finally
{
    UnityEngine.Object.DestroyImmediate(root);
}
