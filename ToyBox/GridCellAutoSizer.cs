using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Resizes cells in a grid to match parent grid width/height.
/// </summary>
[ExecuteAlways]
public class GridCellAutoSizer : MonoBehaviour
{
    [SerializeField]
    private GridLayoutGroup _grid;
    public GridLayoutGroup grid
    {
        get
        {
            if (_grid == null)
            {
                _grid = GetComponent<GridLayoutGroup>();
            }
            return _grid;
        }
    }

    public bool onUpdate = true;
    public bool squareCells = true;

    void Update()
    {
        if (!onUpdate)
            return;

        Resize();
    }

    /// <summary>
    /// Call when need immediate resizing...
    /// </summary>
    public void Resize()
    {
        Vector2 gridSize = (grid.transform as RectTransform).rect.size;

        if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            // calcu the width
            var w = gridSize.x;
            w -= grid.spacing.x * (grid.constraintCount - 1);

            w /= grid.constraintCount;

            grid.cellSize = new Vector2(w, squareCells ? w : grid.cellSize.y);
        }
        else if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            var h = gridSize.y;
            h -= grid.spacing.y * (grid.constraintCount - 1);

            h /= grid.constraintCount;

            grid.cellSize = new Vector2(squareCells ? h : grid.cellSize.x, h);
        }

    }
}