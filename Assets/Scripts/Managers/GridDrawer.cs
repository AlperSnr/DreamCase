using UnityEngine;

[ExecuteAlways]
public class GridGizmoDrawer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Vector3 offset = Vector3.zero;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0) + offset;
            Vector3 end = new Vector3(x * cellSize, height * cellSize, 0) + offset;
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(0, y * cellSize, 0) + offset;
            Vector3 end = new Vector3(width * cellSize, y * cellSize, 0) + offset;
            Gizmos.DrawLine(start, end);
        }
    }
}
