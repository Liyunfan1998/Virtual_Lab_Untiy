using System;
using UnityEngine;

public class GridPlacement : MonoBehaviour
{
    public GameObject cubePrefab; // The cube prefab to be instantiated
    public int gridSizeX = 10; // Size of the grid in the X direction
    public int gridSizeZ = 10; // Size of the grid in the Z direction

    public GameObject surfaceObject; // The GameObject on which the grid will be drawn

    private GameObject[,] grid; // 2D array to store placed cubes
    private Vector3 gridOrigin; // Origin position of the grid
    private GameObject previewCube; // Preview cube for drag placement
    private bool isDragging; // Flag to indicate if the user is dragging
    public float gridSpacing = 1.0f; // Distance between grid points

    private void Awake()
    {
        grid = new GameObject[gridSizeX, gridSizeZ];

        // Calculate the grid origin based on the surface object's bounds
        Renderer surfaceRenderer = surfaceObject.GetComponent<Renderer>();
        if (surfaceRenderer != null)
        {
            Bounds surfaceBounds = surfaceRenderer.bounds;
            Debug.Log(surfaceBounds.size);
            gridOrigin = new Vector3(surfaceBounds.min.x, surfaceBounds.min.y, surfaceBounds.min.z);
            gridSpacing = surfaceBounds.size.x / gridSizeX;
        }
    }

    private void Update()
    {

        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray();
        bool isHit = false;
        //try
        //{
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            isHit = Physics.Raycast(ray, out hit);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogException(e);
        //    Debug.Log(Camera.main == null);
        //}
        // Check for left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            if (isHit)
            {
                // Get the grid position from the hit point
                Vector3 gridPos = GetGridPosition(hit.point);

                // Check if the grid position is valid
                if (IsValidGridPosition(gridPos))
                {
                    // Start dragging
                    isDragging = true;

                    // Create a preview cube
                    Vector3 cubePosition = GetCubePosition(gridPos);
                    if (previewCube == null)
                    {
                        previewCube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);
                        previewCube.transform.localScale = Vector3.one * gridSpacing;
                    }
                    //previewCube.transform.localScale = Vector3.one * surfaceObject.transform.localScale.x / gridSizeX;
                    SetCubeMaterialColor(previewCube, gridPos);
                    previewCube.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 1.0f, 0.5f); // Set the preview cube color to semi-transparent blue
                }
            }
        }

        // Handle dragging
        if (isDragging)
        {
            // Check if the drag time exceeds the maximum allowed time or if the mouse moves out of the grid space
            previewCube.SetActive(isHit && IsValidGridPosition(GetGridPosition(hit.point)));
            if (isHit)
            {
                Vector3 gridPos = GetGridPosition(hit.point);
                Vector3 cubePosition = GetCubePosition(gridPos);
                previewCube.transform.position = cubePosition;
            }
        }

        // Check for left mouse button up
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // Confirm the placement
            if (isHit)
            {
                Vector3 gridPos = GetGridPosition(hit.point);

                if (IsValidGridPosition(gridPos))
                {
                    int x, z;
                    GetGridIndices(gridPos, out x, out z);
                    grid[x, z] = previewCube;
                    SetCubeMaterialColor(previewCube, x, z); // Set the final material color
                    previewCube = null;
                }
                else
                {
                    Destroy(previewCube);
                }
            }
            else
            {
                Destroy(previewCube);
            }

            isDragging = false;
        }

        //Debug.Log($"isMouseDown:{isDragging} isHit:{isHit} showCube:{previewCube!=null}");

        // Draw the grid on the top surface of the surfaceObject
        DrawGrid();
    }

    private Vector3 GetCubePosition(Vector3 gridPos)
    {
        int x, z;
        GetGridIndices(gridPos, out x, out z);

        // Calculate the center position of the grid slot
        Vector3 slotCenter = gridOrigin + new Vector3(x * gridSpacing + gridSpacing / 2, 0, z * gridSpacing + gridSpacing / 2);

        // Check if there is a cube adjacently under the current grid slot or on the surface
        float highestY = GetHighestPositionInSlot(x, z, slotCenter);

        // Place the cube on top of the highest cube or surface in the grid slot
        return new Vector3(slotCenter.x, highestY, slotCenter.z);
    }

    private float GetHighestPositionInSlot(int x, int z, Vector3 slotCenter)
    {
        // Check if there is a cube in the current grid slot
        if (x >= 0 && z >= 0 && x < gridSizeX && z < gridSizeZ && grid[x, z] != null)
        {
            return grid[x, z].transform.position.y + gridSpacing;
        }
        else
        {
            return slotCenter.y + gridSpacing / 2;
        }
    }


    private Vector3 GetGridPosition(Vector3 position)
    {
        // Snap the position to the grid
        float x = Mathf.Round((position.x - gridOrigin.x) / gridSpacing) * gridSpacing + gridOrigin.x;
        float z = Mathf.Round((position.z - gridOrigin.z) / gridSpacing) * gridSpacing + gridOrigin.z;
        return new Vector3(x, position.y, z);
    }

    private bool IsValidGridPosition(Vector3 gridPos)
    {
        int x, z;
        GetGridIndices(gridPos, out x, out z);

        // Check if the grid indices are within the grid bounds
        if (x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ)
        {
            // Stacking is allowed, so the grid position is always valid
            return true;
        }

        return false;
    }

    private void GetGridIndices(Vector3 gridPos, out int x, out int z)
    {
        // Calculate the grid indices from the grid position
        x = Mathf.FloorToInt((gridPos.x - gridOrigin.x) / gridSpacing);
        z = Mathf.FloorToInt((gridPos.z - gridOrigin.z) / gridSpacing);
    }

    private void SetCubeMaterialColor(GameObject cube, Vector3 gridPos)
    {
        int x, z;
        GetGridIndices(gridPos, out x, out z);
        SetCubeMaterialColor(cube, x, z);
    }

    private void SetCubeMaterialColor(GameObject cube, int x, int z)
    {
        // Set the material color to black or red based on the sum of x and z coordinates
        int sum = x + z;
        Color color = (sum % 2 == 0) ? Color.red : Color.black;

        // Alternate colors for adjacent cubes on different layers
        int y = Mathf.FloorToInt(cube.transform.position.y / gridSpacing);
        if ((x + y + z) % 2 != 0)
            color = (color == Color.red) ? Color.gray : new Color(0.5f, 0.5f, 0.5f);

        cube.GetComponent<Renderer>().material.color = color;
    }

    private void DrawGrid()
    {
        // Find the bounds of the surface object
        Renderer surfaceRenderer = surfaceObject.GetComponent<Renderer>();
        if (surfaceRenderer != null)
        {
            Bounds surfaceBounds = surfaceRenderer.bounds;

            // Calculate the grid bounds
            Vector3 topLeftCorner = surfaceBounds.min;

            // Draw the grid lines
            for (int x = 0; x <= gridSizeX; x++)
            {
                Vector3 start = topLeftCorner + new Vector3(x * gridSpacing, 0, 0);
                Vector3 end = start + new Vector3(0, 0, (gridSizeZ + 1) * gridSpacing);
                Debug.DrawLine(start, end, Color.gray);
            }

            for (int z = 0; z <= gridSizeZ; z++)
            {
                Vector3 start = topLeftCorner + new Vector3(0, 0, z * gridSpacing);
                Vector3 end = start + new Vector3((gridSizeX + 1) * gridSpacing, 0, 0);
                Debug.DrawLine(start, end, Color.gray);
            }
        }
    }
}