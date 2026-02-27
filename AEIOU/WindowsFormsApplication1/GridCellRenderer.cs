using System.Windows.Forms;

namespace AEIOU
{
    class GridCellRenderer
    {
        private readonly DataGridView _view;
        private readonly Settings _setting;

        public GridCellRenderer(DataGridView view, Settings setting)
        {
            _view = view;
            _setting = setting;
        }

        public void ApplyTimingCellState(DataGridViewCellPaintingEventArgs e, SheetBorder borderState, bool isContinuousLine)
        {
            TimingCell cell = _view[e.ColumnIndex, e.RowIndex] as TimingCell;
            if (cell == null)
            {
                return;
            }

            string value = _view[e.ColumnIndex, e.RowIndex].Value.ToString();
            cell.BorderState = borderState;
            cell.IsKaraCell = (value == _setting.KaraCell);
            cell.IsContinuousLine = (value == "" && isContinuousLine);
        }

        public void PaintCell(DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.ClipBounds, e.PaintParts);
            e.Handled = true;
        }
    }
}
