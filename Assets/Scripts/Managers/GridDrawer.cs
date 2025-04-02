using UnityEngine;

[ExecuteAlways]
public class GridDrawer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public Vector2 cellSize = new(1f,1f);
    public Vector3 offset = Vector3.zero;
    public bool showGrid = true;
    public bool usePosition = true;

    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = Color.green;
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(x * cellSize.x, 0, 0) + offset;
            Vector3 end = new Vector3(x * cellSize.x, height * cellSize.y, 0) + offset;

            if (usePosition)
            {
                start += transform.position;
                end += transform.position;
            }

            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(0, y * cellSize.y, 0) + offset;
            Vector3 end = new Vector3(width * cellSize.x, y * cellSize.y, 0) + offset;

            if (usePosition)
            {
                start += transform.position;
                end += transform.position;
            }
            Gizmos.DrawLine(start, end);
        }
    }
}
