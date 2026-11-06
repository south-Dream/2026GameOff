// Unity MCP execute_code 方法体中的纯数据回归检查
var report = new System.Text.StringBuilder();
int checks = 0;
System.Action<bool, string> check = (passed, name) =>
{
    if (!passed) throw new System.Exception(name);
    report.AppendLine("PASS " + name);
    checks++;
};

foreach (PlaneShift.SpaceAxis axis in System.Enum.GetValues(typeof(PlaneShift.SpaceAxis)))
{
    var road = new PlaneShift.RoadData("LongRoad", new Vector3(4, 8, 5), new Vector3(30, 1, 40));
    var original = new Vector3(-7, road.Top, 17);
    var flat = PlaneShift.MapData.Project(original, axis, road.position[(int)axis]);
    flat[axis == PlaneShift.SpaceAxis.X ? 2 : 0] += 1;
    var restored = PlaneShift.MapData.Restore(flat, road, axis, original[(int)axis]);
    check(Mathf.Abs(restored[(int)axis] - original[(int)axis]) < 0.001f, axis + " keeps original depth on same road");
    int retained = axis == PlaneShift.SpaceAxis.X ? 2 : 0;
    check(Mathf.Abs(restored[retained] - flat[retained]) < 0.001f, axis + " keeps movement in retained plane");
    check(Mathf.Abs(restored.y - road.Top) < 0.001f, axis + " restores road surface height");
}

foreach (PlaneShift.ViewDirection view in System.Enum.GetValues(typeof(PlaneShift.ViewDirection)))
{
    var axis = PlaneShift.CameraData.GetAxis(view);
    var forward = PlaneShift.CameraData.GetForward(view);
    var start = new Vector3(-30, 3, -30);
    var anchor = new PlaneShift.RoadData("Anchor", start, new Vector3(2, 1, 2));
    var nearPoint = new Vector3(0, 3, 0) - forward * 10;
    var farPoint = new Vector3(0, 3, 0) + forward * 10;
    var near = new PlaneShift.RoadData("Near", nearPoint, new Vector3(4, 1, 4));
    var far = new PlaneShift.RoadData("Far", farPoint, new Vector3(4, 1, 4));
    var roads = new[] { anchor, far, near };
    var result = PlaneShift.MapData.CreateProjection(roads, anchor, axis, forward);
    check(result[0].source == anchor, view + " prioritizes standing road");
    check(result.Any(p => p.source == near) && !result.Any(p => p.source == far), view + " selects front source independently of list order");
    check(roads[1] == far && roads[2] == near, view + " does not reorder world road data");
    if (axis == PlaneShift.SpaceAxis.Y)
        check(result.All(p => Mathf.Abs(p.position.y + p.size.y / 2 - anchor.Top) < 0.001f), view + " removes every height difference");
}

var current = new PlaneShift.RoadData("Current", new Vector3(20, 3, 20), new Vector3(2, 1, 2));
var wide = new PlaneShift.RoadData("Wide", new Vector3(5, 3, 0), new Vector3(20, 1, 4));
var narrow = new PlaneShift.RoadData("Narrow", new Vector3(0, 3, 0), new Vector3(2, 1, 4));
var faceOrder = PlaneShift.MapData.CreateProjection(new[] { current, narrow, wide }, current, PlaneShift.SpaceAxis.X, Vector3.right);
check(faceOrder.Any(p => p.source == wide) && !faceOrder.Any(p => p.source == narrow), "sorts by near box face rather than center");

var overlap = new PlaneShift.RoadData("Overlap", current.position - Vector3.right * 20, current.size);
var anchorFirst = PlaneShift.MapData.CreateProjection(new[] { overlap, current }, current, PlaneShift.SpaceAxis.X, Vector3.right);
check(anchorFirst.All(p => p.source == current), "standing road wins even when another face is closer");

var tie = new PlaneShift.RoadData("Tie", wide.position, wide.size);
var equalDepth = PlaneShift.MapData.CreateProjection(new[] { current, wide, tie }, current, PlaneShift.SpaceAxis.X, Vector3.right);
check(equalDepth.Any(p => p.source == wide) && !equalDepth.Any(p => p.source == tie), "equal-depth sources retain stable order");

return report.ToString() + "TOTAL " + checks;
