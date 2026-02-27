using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AEIOU
{
    public abstract class GridViewOperation
    {
        protected String _name;

        public String Name
        {
            get { return _name; }
        }

        public abstract void Execute(GridViewManager manager);
        public abstract void Undo(GridViewManager manager);
        public abstract void Redo(GridViewManager manager);

        public void CopyToBuffer(Rect range, GridViewManager manager)
        {
            manager.CopyRect = range;
            manager.CopyBuffer = new string[range.Height, range.Width];
            for (int i = 0; i < range.Height; i++)
            {
                for (int j = 0; j < range.Width; j++)
                {
                    manager.CopyBuffer[i, j] = manager.GetCellValue(range.X + j, range.Y + i);
                }
            }
        }

        public void PasteFromBuffer(int row, int col, GridViewManager manager)
        {
            if (manager.CopyBuffer != null)
            {
                // コピー範囲のサイズを取得
                int bufferHeight = manager.CopyBuffer.GetLength(0);
                int bufferWidth = manager.CopyBuffer.GetLength(1);

                // DataGridViewの範囲内に収まるように調整
                int maxHeight = Math.Min(bufferHeight, manager.View.RowCount - row);
                int maxWidth = Math.Min(bufferWidth, manager.View.ColumnCount - col);

                // DataGridViewに書き戻す
                for (int i = 0; i < maxHeight; i++)
                {
                    for (int j = 0; j < maxWidth; j++)
                    {
                        manager.SetCellValue(col + j, row + i, manager.CopyBuffer[i, j]);
                    }
                }
            }
        }


        public void ClearSelection(Rect range, GridViewManager manager)
        {
            for (int i = 0; i < range.Height; i++)
            {
                for (int j = 0; j < range.Width; j++)
                {
                    manager.SetCellValue(range.X + j, range.Y + i, "");
                }
            }
        }

    }
    public class SetValueOperation : GridViewOperation
    {
        private int _row;
        private int _column;
        private string _newValue;
        private string _oldValue;

        public SetValueOperation(int row, int column, string newValue)
        {
            _name = "入力";
            _row = row;
            _column = column;
            _newValue = newValue;
        }

        public override void Execute(GridViewManager manager)
        {
            _oldValue = manager.SetCellValueWithUndo(_column, _row, _newValue);
        }

        public override void Undo(GridViewManager manager)
        {
            manager.ApplyUndoCellValue(_column, _row, _oldValue);
        }

        public override void Redo(GridViewManager manager)
        {
            manager.ApplyRedoCellValue(_column, _row, _newValue);
        }
    }

    public class CopyOperation : GridViewOperation
    {
        private Rect _copyRange;

        public CopyOperation(Rect copyRange)
        {
            _name = "複製";
            _copyRange = copyRange;
        }

        public override void Execute(GridViewManager manager)
        {
            CopyToBuffer(_copyRange, manager);
        }

        public override void Undo(GridViewManager manager)
        {
            // (コピーの undo時は、なにもしない)
        }

        public override void Redo(GridViewManager manager)
        {
            Execute(manager);
        }
    }

    public class PasteOperation : GridViewOperation
    {
        private int _col;
        private int _row;
        private String[,] _oldValues;
        private String[,] _newValues;

        public PasteOperation(int row, int col)
        {
            _name = "貼り付け";
            _row = row;
            _col = col;
        }

        public override void Execute(GridViewManager manager)
        {
            if (manager.CopyBuffer == null)
            {
                _oldValues = null;
                _newValues = null;
                return;
            }

            // 範囲外の処理はしないよう、コピー範囲を計算
            Rect copyRect = manager.CopyRect;
            int maxHeight = Math.Min(copyRect.Height, manager.View.RowCount - _row);
            int maxWidth = Math.Min(copyRect.Width, manager.View.ColumnCount - _col);
            _oldValues = new String[maxHeight, maxWidth];
            _newValues = new String[maxHeight, maxWidth];

            // 貼り付け前/貼り付け値を保存して反映
            for (int i = 0; i < maxHeight; i++)
            {
                for (int j = 0; j < maxWidth; j++)
                {
                    _oldValues[i, j] = manager.GetCellValue(_col + j, _row + i);
                    _newValues[i, j] = manager.CopyBuffer[i, j];
                    manager.SetCellValue(_col + j, _row + i, _newValues[i, j]);
                }
            }
        }

        public override void Undo(GridViewManager manager)
        {
            if (_oldValues == null)
            {
                return;
            }

            int rowCount = _oldValues.GetLength(0);
            int columnCount = _oldValues.GetLength(1);
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    manager.SetCellValue(_col + j, _row + i, _oldValues[i, j]);
                }
            }
        }

        public override void Redo(GridViewManager manager)
        {
            if (_newValues == null)
            {
                return;
            }

            int rowCount = _newValues.GetLength(0);
            int columnCount = _newValues.GetLength(1);
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    manager.SetCellValue(_col + j, _row + i, _newValues[i, j]);
                }
            }
        }
    }

    public class CutOperation : GridViewOperation
    {
        private Rect _cutRange;
        private String[,] _oldValues;

        public CutOperation(Rect cutRange)
        {
            _name = "切り取り";
            _cutRange = cutRange;
        }

        public override void Execute(GridViewManager manager)
        {
            _oldValues = new String[_cutRange.Height, _cutRange.Width];
            for (int i = 0; i < _cutRange.Height; i++)
            {
                for (int j = 0; j < _cutRange.Width; j++)
                {
                    _oldValues[i, j] = manager.GetCellValue(_cutRange.X + j, _cutRange.Y + i);
                }
            }
            CopyToBuffer(_cutRange, manager);
            ClearSelection(_cutRange, manager);
        }

        public override void Undo(GridViewManager manager)
        {
            for (int i = 0; i < _cutRange.Height; i++)
            {
                for (int j = 0; j < _cutRange.Width; j++)
                {
                    manager.SetCellValue(_cutRange.X + j, _cutRange.Y + i, _oldValues[i, j]);
                }
            }
        }

        public override void Redo(GridViewManager manager)
        {
            Execute(manager);
        }

    }

    public class DeleteOperation : GridViewOperation
    {
        private Rect _deleteRange;
        private String[,] _oldValues;

        public DeleteOperation(Rect deleteRange)
        {
            _name = "削除";
            _deleteRange = deleteRange;
        }

        public override void Execute(GridViewManager manager)
        {
            _oldValues = new String[_deleteRange.Height, _deleteRange.Width];
            for (int i = 0; i < _deleteRange.Height; i++)
            {
                for (int j = 0; j < _deleteRange.Width; j++)
                {
                    _oldValues[i, j] = manager.GetCellValue(_deleteRange.X + j, _deleteRange.Y + i);
                }
            }
            ClearSelection(_deleteRange, manager);
        }

        public override void Undo(GridViewManager manager)
        {
            for (int i = 0; i < _deleteRange.Height; i++)
            {
                for (int j = 0; j < _deleteRange.Width; j++)
                {
                    manager.SetCellValue(_deleteRange.X + j, _deleteRange.Y + i, _oldValues[i, j]);
                }
            }
        }

        public override void Redo(GridViewManager manager)
        {
            Execute(manager);
        }

    }

}
