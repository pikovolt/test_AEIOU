using System;
using System.Windows.Forms;

namespace AEIOU
{
    public partial class Form1 : Form, IGridShortcutHandler
    {
        public void OnBackSpace()
        {
            if (isFirstEdit || setting.IsAlwaysAppend)
            {
                // Delete selection
                isCellEdit = deleteRect_with_backspace(isCellEdit);
            }
            else
            {
                // 1 char delete logic
                for (int row = selectRange.Top; row <= selectRange.Bottom; row++)
                {
                    for (int col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        string str = GetCellValue(col, row);
                        if (!string.IsNullOrEmpty(str) && str.Length > 0 && str != setting.KaraCell)
                        {
                            str = str.Substring(0, str.Length - 1);
                            gridCellValueService.ApplyValue(new Rect(col, row, 1, 1), str);
                            isCellEdit = true;
                        }
                    }
                }
            }
        }

        public void OnEnter()
        {
            Rect rect = getSelectedRect();
            dataGridView1.ClearSelection();

            int len = cursorMoveWithNakaNuki();
            if (len > 0)
            {
                selectRange = gridSelectionService.MoveSelectionDown(rect, len);
            }

            isFirstEdit = true;
            scrollingForward();
        }
        public void OnPageUp(int keyValue) { gridScrollService.ScrollVertical(-dataGridView1.DisplayedRowCount(true)); }
        public void OnPageDown(int keyValue) { gridScrollService.ScrollVertical(dataGridView1.DisplayedRowCount(true)); }
        public void OnHome() 
        { 
            dataGridView1.ClearSelection();
            selectRange = gridSelectionService.MoveSelection(selectRange, selectRange.X, 0);
        }
        public void OnLeftArrow(int keyValue)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                Rect rect = getSelectedRect();
                if (rect.Width > 1)
                {
                    for (int i = 0; i < rect.Height; i++)
                    {
                        dataGridView1[rect.Right, rect.Y + i].Selected = false;
                    }
                    rect.Width--;
                    selectRange = rect;
                }
                return;
            }

            selectRange = gridSelectionService.MoveLeft(selectRange);
        }

        public void OnUpArrow(int keyValue)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                OnDivideKey();
                return;
            }

            selectRange = gridSelectionService.MoveUp(selectRange);
        }

        public void OnRightArrow(int keyValue)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                Rect rect = getSelectedRect();
                if (rect.Right + 1 < setting.ColLength)
                {
                    for (int i = 0; i < rect.Height; i++)
                    {
                        dataGridView1[rect.Right + 1, rect.Y + i].Selected = true;
                    }
                    rect.Width++;
                    selectRange = rect;
                }
                return;
            }

            selectRange = gridSelectionService.MoveRight(selectRange);
        }

        public void OnDownArrow(int keyValue)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                OnMultiplyKey();
                return;
            }

            selectRange = gridSelectionService.MoveDown(selectRange);
        }
        
        public void OnInsert() 
        { 
            insertToAllCell(selectRange.Top, selectRange.Height);
            calcNakanukiRange(true, selectRange.Top, selectRange.Height);
            calcKiribariRange(true, selectRange.Top, selectRange.Height);
            flushUndoHistory();
            selectRange = gridSelectionService.MoveSelectionDown(selectRange, selectRange.Height);
        }

        public void OnDelete(int keyValue) 
        { 
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                // Shift+Delete: 範囲削除
                cutToAllCell(selectRange.Top, selectRange.Height);
                calcNakanukiRange(false, selectRange.Top, selectRange.Height);
                calcKiribariRange(false, selectRange.Top, selectRange.Height);
                flushUndoHistory();
                int top = selectRange.Y - selectRange.Height;
                if (top < 0)
                {
                    top = 0;
                }
                selectRange = gridSelectionService.MoveSelection(selectRange, selectRange.X, top);
            }
            else
            {
                // Delete: 選択範囲の内容削除（移動なし）
                deleteRect(selectRange);
            }
        }

        public void OnNumberKey(int keyValue, int keyCode)
        {
            int normalizedKey = keyValue & 0x0ff;
            if (normalizedKey >= 96 && normalizedKey <= 105)
            {
                normalizedKey -= 96;
            }
            else if (normalizedKey >= 48 && normalizedKey <= 57)
            {
                normalizedKey -= 48;
            }
            else
            {
                return;
            }

            gridCellValueService.InsertNumber(selectRange, normalizedKey, isFirstEdit, setting.IsAlwaysAppend);
            isCellEdit = true;
        }

        public void OnJOrKKey(int keyCode)
        {
            int value = selectRange.Top;
            int col, row;
            if (keyCode == 74) // J
            {
                for (row = selectRange.Top - 1; row >= 0 && value == selectRange.Top; row--)
                {
                    for (col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        if (GetCellValue(col, row) == "") continue;
                        value = row;
                        break;
                    }
                }
            }
            if (keyCode == 75) // K
            {
                for (row = selectRange.Bottom + 1; row < setting.RowLength && value == selectRange.Top; row++)
                {
                    for (col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        if (GetCellValue(col, row) == "") continue;
                        value = row;
                        break;
                    }
                }
            }
            if (value != selectRange.Top)
            {
                this.dataGridView1.ClearSelection();
                selectRange = gridSelectionService.MoveSelection(selectRange, selectRange.X, value);
            }
        }

        public void OnMultiplyKey() 
        { 
            // 選択範囲の拡大
            Rect rect = getSelectedRect();
            if (rect.Bottom + 1 < setting.RowLength)
            {
                for (int j = 0; j < rect.Width; j++)
                    dataGridView1[rect.X + j, rect.Bottom + 1].Selected = true;
                rect.Height++;
                selectRange = rect;
            }
        }
        public void OnAddKey()
        {
            gridCellValueService.IncrementValue(selectRange, setting.KaraCell);
            this.dataGridView1.ClearSelection();
            int len = cursorMoveWithNakaNuki();
            if (len > 0)
            {
                selectRange = gridSelectionService.MoveSelectionDown(selectRange, len);
            }
            scrollingForward();
        }

        public void OnSubtractKey()
        {
            gridCellValueService.DecrementValue(selectRange, setting.KaraCell);
            this.dataGridView1.ClearSelection();
            int len = cursorMoveWithNakaNuki();
            if (len > 0)
            {
                selectRange = gridSelectionService.MoveSelectionDown(selectRange, len);
            }
            scrollingForward();
        }
        public void OnDivideKey() 
        { 
            // 選択範囲の縮小
            Rect rect = getSelectedRect();
            if (rect.Height > 1)
            {
                for (int j = 0; j < rect.Width; j++)
                    dataGridView1[rect.X + j, rect.Bottom].Selected = false;
                rect.Height--;
                selectRange = rect;
            }
        }
        
        public void OnDecimalKey() 
        {
            gridCellValueService.InsertEmptyCell(selectRange, setting.KaraCell);
            isCellEdit = true;
            this.dataGridView1.ClearSelection();
            if (!setting.IsKaraNoMove) {
                int len = cursorMoveWithNakaNuki();
                if (len > 0)
                {
                    selectRange = gridSelectionService.MoveSelectionDown(selectRange, len);
                }
                scrollingForward();
            }
        }
    }
}
