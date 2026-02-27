using System;
using System.Windows.Forms;

namespace AEIOU
{
    public class GridSelectionService
    {
        private readonly DataGridView _view;
        private readonly Settings _setting;

        public GridSelectionService(DataGridView view, Settings setting)
        {
            _view = view;
            _setting = setting;
        }

        public Rect GetSelectedRect()
        {
            Rect r = new Rect(_setting.ColLength, _setting.RowLength, 0, 0);
            int count = _view.SelectedCells.Count;

            for (int i = 0; i < count; i++)
            {
                int rowIndex = _view.SelectedCells[i].RowIndex;
                int colIndex = _view.SelectedCells[i].ColumnIndex;
                if (rowIndex < r.Y)
                {
                    r.Y = rowIndex;
                }
                if (colIndex < r.X)
                {
                    r.X = colIndex;
                }
                if (rowIndex > r.Height)
                {
                    r.Height = rowIndex;
                }
                if (colIndex > r.Width)
                {
                    r.Width = colIndex;
                }
            }

            r.Height = r.Height - r.Y + 1;
            r.Width = r.Width - r.X + 1;

            return r;
        }

        public Rect MoveSelectionDown(Rect rect, int moveLength)
        {
            if (moveLength <= 0)
            {
                return rect;
            }

            int top = rect.Y + moveLength;
            int btm = _setting.RowLength - rect.Height;
            top = (top > btm) ? btm : top;

            _view.ClearSelection();
            _view.CurrentCell = _view[rect.X, top];

            for (int i = 0; i < rect.Height; i++)
            {
                for (int j = 0; j < rect.Width; j++)
                {
                    _view[rect.X + j, top + i].Selected = true;
                }
            }

            rect.Y = top;
            return rect;
        }
    }
}
