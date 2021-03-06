﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Simple implementation of DataGridView.
    /// </summary>
    public class TableView : Control
    {
        private HScrollBar hScroll;
        internal TableColumn lastSortedColumn;
        private float maxScrollHeight
        {
            get
            {
                var h = Rows.Last().control.Location.Y + Rows.Last().control.Height;
                if (hScroll != null)
                    h += hScroll.Height;
                return h;
            }
        }
        private float maxScrollWidth
        {
            get
            {
                var w = Columns.Last().control.Location.X + Columns.Last().control.Width;
                if (vScroll != null)
                    w += vScroll.Width;
                return w;
            }
        }
        private TableColumnButton topLeftButton;
        private VScrollBar vScroll;

        public Color BorderColor { get; set; }
        public int CellPadding { get; set; }
        public int ColumnCount { get { return Columns.Count; } }
        public TableColumnCollection Columns { get; private set; }
        public TableButtonStyle ColumnsStyle { get; set; }
        public TableRowCollection Rows { get; private set; }

        public TableView()
        {
            BackColor = Color.FromArgb(171, 171, 171);
            BorderColor = Color.Black;
            CellPadding = 1;
            ColumnsStyle = new TableButtonStyle();
            Padding = new Padding(2);

            Columns = new TableColumnCollection(this);
            Rows = new TableRowCollection(this);

            Size = new Size(240, 150);
        }

        private void CreateTopLeftButton()
        {
            if (topLeftButton == null)
            {
                topLeftButton = new TableColumnButton(ColumnsStyle);
                topLeftButton.Size = new Size(40, 20);
                Controls.Add(topLeftButton);
            }
        }
        private void HScroll_ValueChanged(object sender, EventArgs e)
        {
            int offsetX = -(int)((float)(maxScrollWidth * hScroll.Value) / 100);
            for (int i = 0; i < Controls.Count; i++)
            {
                var c = Controls[i];
                if (c is ScrollBar) continue;

                c.Offset = new Point(offsetX, c.Offset.Y);
            }
        }
        private void ResetHOffset()
        {
            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Offset = new Point(0, Controls[i].Offset.Y);
        }
        private void ResetVOffset()
        {
            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Offset = new Point(Controls[i].Offset.X, 0);
        }
        private void VScroll_ValueChanged(object sender, EventArgs e)
        {
            int offseY = -(int)((float)(maxScrollHeight * vScroll.Value) / 100);
            for (int i = 0; i < Controls.Count; i++)
            {
                var c = Controls[i];
                if (c is ScrollBar) continue;

                c.Offset = new Point(c.Offset.X, offseY);
            }
        }
        private void UpdateScrolls()
        {
            // Create or dispose scrolls.
            if (Rows.Count > 0)
            {
                if (Height < maxScrollHeight)
                {
                    if (vScroll == null)
                    {
                        vScroll = new VScrollBar();
                        vScroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
                        vScroll.Height = Height;
                        vScroll.Location = new Point(Width - vScroll.Width, 0);
                        vScroll.ValueChanged += VScroll_ValueChanged;
                        Controls.Add(vScroll);
                    }
                }
                else if (vScroll != null)
                {
                    vScroll.ValueChanged -= VScroll_ValueChanged;
                    vScroll.Dispose();
                    vScroll = null;
                    ResetVOffset();
                }
            }
            else if (vScroll != null)
            {
                vScroll.ValueChanged -= VScroll_ValueChanged;
                vScroll.Dispose();
                vScroll = null;
                ResetVOffset();
            }

            if (Columns.Count > 0)
            {
                if (Width < maxScrollWidth)
                {
                    if (hScroll == null)
                    {
                        hScroll = new HScrollBar();
                        hScroll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        hScroll.Width = Width;
                        hScroll.Location = new Point(0, Height - hScroll.Height);
                        hScroll.ValueChanged += HScroll_ValueChanged;
                        Controls.Add(hScroll);
                    }
                }
                else if (hScroll != null)
                {
                    hScroll.ValueChanged -= HScroll_ValueChanged;
                    hScroll.Dispose();
                    hScroll = null;
                    ResetHOffset();
                }
            }
            else if (vScroll != null)
            {
                hScroll.ValueChanged -= HScroll_ValueChanged;
                vScroll.Dispose();
                vScroll = null;
                ResetHOffset();
            }

            // Update properties.
            if (vScroll != null)
            {
                vScroll.LargeChange = (int)((float)(vScroll.Maximum - vScroll.Minimum) / ((float)maxScrollHeight / Height));
                vScroll.BringToFront();
            }
            if (hScroll != null)
            {
                if (vScroll != null)
                    hScroll.Width = Width - vScroll.Width;
                else
                    hScroll.Width = Width;

                hScroll.LargeChange = (int)((float)(hScroll.Maximum - hScroll.Minimum) / ((float)maxScrollWidth / Width));
                hScroll.BringToFront();
            }
        }

        internal void AlignColumns()
        {
            int cX = Padding.Left;
            if (topLeftButton != null)
            {
                topLeftButton.Location = new Point(Padding.Left, Padding.Top);
                cX = topLeftButton.Location.X + topLeftButton.Width + CellPadding;
            }
            for (int i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                column.control.Location = new Point(cX, Padding.Top);
                cX += column.control.Width + CellPadding;
            }

            UpdateScrolls();
        }
        internal void AlignRows()
        {
            int cY = Padding.Top;
            if (topLeftButton != null)
            {
                topLeftButton.Location = new Point(Padding.Left, Padding.Top);
                cY = topLeftButton.Location.Y + topLeftButton.Height + CellPadding;
            }
            for (int i = 0, cellIndex = 1; i < Rows.Count; i++)
            {
                var row = Rows[i];
                row.control.Location = new Point(Padding.Left, cY);
                row.control.Text = (i + 1).ToString();
                if (topLeftButton != null)
                    row.control.Width = topLeftButton.Width;

                int cX = row.control.Location.X + row.control.Width + CellPadding;
                for (int k = 0; k < row.ItemsControls.Length; k++)
                {
                    row.ItemsControls[k].Location = new Point(cX, cY);
                    row.ItemsControls[k].TabIndex = cellIndex;
                    row.ItemsControls[k].Size = new Size(Columns[k].Width, row.Height);
                    cellIndex++;
                    cX += row.ItemsControls[k].Width + CellPadding;
                }

                cY += row.control.Height + CellPadding;
            }

            UpdateScrolls();
        }
        internal void UpdateColumn(TableColumn column)
        {
            CreateTopLeftButton();

            if (column.control == null)
            {
                var cButton = new TableColumnButton(ColumnsStyle);
                cButton.column = column;
                cButton.EnableHorizontalResizing = true;
                cButton.table = this;
                cButton.Text = column.HeaderText;

                column.control = cButton;
                Controls.Add(cButton);
            }

            AlignColumns();

            UpdateRows();
        }
        internal void UpdateRow(TableRow row, bool align = true)
        {
            CreateTopLeftButton();

            if (row.control == null)
            {
                var rButton = new TableRowButton(ColumnsStyle);
                rButton.Size = new Size(40, row.Height);
                rButton.Text = Rows.Count.ToString();

                row.control = rButton;
                Controls.Add(rButton);
            }

            if (row.Items.Length != Columns.Count)
                row.AddjustItemsCountTo(Columns.Count);

            if (row.ItemsControls == null)
                row.ItemsControls = new TableRow.TableRowControlsCollection(row, Columns.Count);
            if (row.ItemsControls.Length != row.Items.Length)
            {
                var newControls = new Control[Columns.Count];
                if (row.ItemsControls.Length > newControls.Length) // Dispose unnecessary controls.
                {
                    Array.Copy(row.ItemsControls.items, 0, newControls, 0, newControls.Length);
                    for (int i = newControls.Length; i < row.ItemsControls.Length; i++)
                        row.ItemsControls[i].Dispose();
                }
                else
                    Array.Copy(row.ItemsControls.items, 0, newControls, 0, row.ItemsControls.Length);
                row.ItemsControls.items = newControls;
            }
            for (int i = 0; i < row.Items.Length; i++)
            {
                if (row.ItemsControls[i] != null) continue;

                int controlColumn = i;
                TextBox itemControl = new TextBox();
                itemControl.BorderColor = Color.Transparent;
                itemControl.Size = new Size(Columns[i].Width, row.Height);
                itemControl.TextChanged += (s, a) =>
                {
                    row.Items[controlColumn] = itemControl.Text;
                };
                if (row.Items[i] != null)
                    itemControl.Text = row.Items[i].ToString();

                row.ItemsControls[i] = itemControl;
            }

            if (align)
                AlignRows();
        }
        internal void UpdateRows()
        {
            for (int i = 0; i < Rows.Count; i++)
                UpdateRow(Rows[i], false);
            AlignRows();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (vScroll != null)
                vScroll.RaiseOnMouseWheel(e);
        }
        protected override void OnLatePaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(BorderColor), 0, 0, Width, Height);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(BackColor, 0, 0, Width, Height);
        }
        protected override void OnResize(Point delta)
        {
            base.OnResize(delta);

            UpdateScrolls();
        }

        public virtual void Sort(TableColumn column, ListSortDirection direction)
        {
            int columnIndex = Columns.FindIndex(column);
            Dictionary<TableRow, object[]> items = new Dictionary<TableRow, object[]>();
            for (int i = 0; i < Rows.Count; i++)
            {
                var r = Rows[i];
                items.Add(r, r.Items);
            }

            var itemsList = items.ToList();
            if (direction == ListSortDirection.Ascending)
                itemsList.Sort((x, y) =>
                {
                    var v1 = x.Value[columnIndex];
                    var v2 = y.Value[columnIndex];

                    if (v1 == null && v2 == null)
                        return 0;

                    if (v1 == null)
                        return -1;
                    if (v2 == null)
                        return 1;

                    var we = v1.ToString().CompareTo(v2.ToString());
                    return we;
                });
            else
                itemsList.Sort((x, y) =>
                {
                    var v1 = x.Value[columnIndex];
                    var v2 = y.Value[columnIndex];

                    if (v1 == null && v2 == null)
                        return 0;

                    if (v1 == null)
                        return 1;
                    if (v2 == null)
                        return -1;

                    var we = v1.ToString().CompareTo(v2.ToString());
                    return -we;
                });

            Rows.ClearList();

            for (int i = 0; i < itemsList.Count; i++)
                Rows.Add(itemsList[i].Key);

            AlignRows();

            if (lastSortedColumn != null)
                lastSortedColumn.control.Padding = new Padding(8, 0, 8, 0);
            lastSortedColumn = column;
            lastSortedColumn.control.Padding = new Padding(24, 0, 8, 0);
        }

        internal class TableColumnButton : Button
        {
            private Control prevButton;
            private resizeTypes resizeType = resizeTypes.None;
            private bool resizing = false;
            private Point resizeStartMouseLocation;
            private Point resizeStartLocation;
            private int resizeStartWidth;

            internal TableColumn column;
            internal ListSortDirection lastSortDirection;
            internal TableView table;

            public bool EnableHorizontalResizing { get; set; }
            public int ResizeWidth { get; set; }

            public TableColumnButton(TableButtonStyle style)
            {
                BackColor = style.BackColor;
                HoverColor = style.HoverColor;
                BorderColor = style.BorderColor;
                BorderHoverColor = style.BorderHoverColor;
                BorderSelectColor = style.BorderSelectColor;
                Padding = new Padding(8, 0, 8, 0);
                ResizeWidth = 8;
                Size = new Size(100, 20);
                TextAlign = ContentAlignment.MiddleLeft;

                Owner.UpClick += Owner_UpClick;
            }

            private ListSortDirection GetNextSortDirection()
            {
                if (lastSortDirection == ListSortDirection.Ascending)
                    return ListSortDirection.Descending;
                return ListSortDirection.Ascending;
            }
            private void Owner_UpClick(object sender, MouseEventArgs e)
            {
                resizing = false;
                resizeType = resizeTypes.None;
            }

            public override void Dispose()
            {
                base.Dispose();

                Owner.UpClick -= Owner_UpClick;
            }
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                switch (e.Button)
                {
                    case MouseButtons.Left:
                        if (resizeType != resizeTypes.None)
                        {
                            resizing = true;
                            resizeStartMouseLocation = PointToScreen(e.Location);
                            resizeStartLocation = Location;
                            resizeStartWidth = Width;

                            // Find prev button.
                            var table = Parent as TableView;
                            var button = table.topLeftButton;
                            if (column.Index > 0)
                                button = table.Columns[column.Index - 1].control as TableColumnButton;
                            prevButton = button;
                        }
                        break;
                }
            }
            protected override void OnMouseHover(EventArgs e)
            {
                base.OnMouseHover(e);

                var mclient = PointToClient(MousePosition);
                if (EnableHorizontalResizing)
                {
                    if (mclient.X < ResizeWidth)
                        resizeType = resizeTypes.Left;
                    else if (mclient.X > Width - ResizeWidth)
                        resizeType = resizeTypes.Right;
                    else
                        resizeType = resizeTypes.None;
                }
            }
            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                if (resizing == false)
                    resizeType = resizeTypes.None;
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (resizing)
                {
                    var dif = PointToScreen(e.Location) - resizeStartMouseLocation;
                    switch (resizeType)
                    {
                        case resizeTypes.Left:
                            if (prevButton != null)
                            {
                                var newX = resizeStartLocation.X + dif.X;
                                Location = new Point(newX, Location.Y);
                                prevButton.Width = Location.X - prevButton.Location.X - (Parent as TableView).CellPadding;
                                (Parent as TableView).AlignColumns();
                                (Parent as TableView).AlignRows();
                            }
                            break;
                        case resizeTypes.Right:
                            Width = resizeStartWidth + dif.X;
                            (Parent as TableView).AlignColumns();
                            (Parent as TableView).AlignRows();
                            break;
                    }
                }
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (resizeType != resizeTypes.None) return;

                switch (e.Button)
                {
                    case MouseButtons.Left:
                        // Sort.
                        lastSortDirection = GetNextSortDirection();
                        table.Sort(column, lastSortDirection);
                        break;
                    case MouseButtons.Right:
                        // Create context menu.
                        ContextMenuStrip contextMenu = new ContextMenuStrip();

                        var itemSort = new ToolStripMenuItem("Sort");
                        contextMenu.Items.Add(itemSort);

                        var itemSortAsc = new ToolStripMenuItem("Ascending");
                        itemSortAsc.Click += (s, a) =>
                        {
                            lastSortDirection = ListSortDirection.Ascending;
                            table.Sort(column, lastSortDirection);
                        };
                        var itemSortDesc = new ToolStripMenuItem("Descending");
                        itemSortDesc.Click += (s, a) =>
                        {
                            lastSortDirection = ListSortDirection.Descending;
                            table.Sort(column, lastSortDirection);
                        };

                        itemSort.DropDownItems.Add(itemSortAsc);
                        itemSort.DropDownItems.Add(itemSortDesc);

                        contextMenu.Show(null, MousePosition);
                        break;

                }
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                switch (resizeType)
                {
                    case resizeTypes.Left:
                        e.Graphics.DrawTexture(ApplicationBehaviour.Resources.Images.CurvedArrowLeft, -2, (Height - 16) / 2, 16, 16, Color.Gray);
                        break;
                    case resizeTypes.Right:
                        e.Graphics.DrawTexture(ApplicationBehaviour.Resources.Images.CurvedArrowRight, Width - 14, (Height - 16) / 2, 16, 16, Color.Gray);
                        break;
                }

                if (table != null)
                    if (column == table.lastSortedColumn)
                    {
                        switch (lastSortDirection)
                        {
                            case ListSortDirection.Ascending:
                                e.Graphics.DrawTexture(ApplicationBehaviour.Resources.Images.ArrowUp, 8, Height / 2 - 4, 8, 8, Color.Gray);
                                break;
                            case ListSortDirection.Descending:
                                e.Graphics.DrawTexture(ApplicationBehaviour.Resources.Images.ArrowDown, 8, Height / 2 - 4, 8, 8, Color.Gray);
                                break;
                        }
                    }
            }

            private enum resizeTypes : byte
            {
                None,
                Down,
                Left,
                Right,
                Up
            }
        }
        internal class TableRowButton : Button
        {
            public TableRowButton(TableButtonStyle style)
            {
                BackColor = style.BackColor;
                HoverColor = style.HoverColor;
                BorderColor = style.BorderColor;
                BorderHoverColor = style.BorderHoverColor;
                BorderSelectColor = style.BorderSelectColor;
                Padding = new Padding(8, 0, 8, 0);
                Size = new Size(100, 20);
                TextAlign = ContentAlignment.MiddleRight;
            }
        }
    }

    public class TableRow
    {
        internal Button control;
        internal TableRowCollection owner;

        public object[] Items { get; internal set; }
        public TableRowControlsCollection ItemsControls { get; internal set; }
        public int Height { get; set; }

        public object this[int column]
        {
            get { return Items[column]; }
            set { Items[column] = value; }
        }

        public TableRow(TableRowCollection o)
        {
            owner = o;

            ItemsControls = new TableRowControlsCollection(this, 0);
            Height = 22;
        }

        internal void AddjustItemsCountTo(int n)
        {
            if (Items == null)
                Items = new object[n];
            else if (Items.Length != n)
            {
                var newItems = new object[n];
                if (Items.Length > n)
                    Array.Copy(Items, 0, newItems, 0, newItems.Length);
                else
                    Array.Copy(Items, 0, newItems, 0, Items.Length);
                Items = newItems;
            }
        }

        public class TableRowControlsCollection
        {
            private TableRow owner;
            internal Control[] items;

            public int Length { get { return items.Length; } }

            public Control this[int index]
            {
                get
                {
                    return items[index];
                }
                set
                {
                    if (items[index] != null && items[index].Disposing == false)
                        items[index].Dispose();
                    items[index] = value;
                    owner.owner.owner.Controls.Add(value);
                    owner.owner.owner.UpdateRow(owner);
                }
            }

            public TableRowControlsCollection(TableRow row, int count)
            {
                owner = row;
                items = new Control[count];
            }
        }
    }
    public class TableRowCollection
    {
        private List<TableRow> items = new List<TableRow>();
        internal TableView owner;

        public int Count { get { return items.Count; } }

        public TableRow this[int index]
        {
            get { return items[index]; }
        }

        public TableRowCollection(TableView table)
        {
            owner = table;
        }

        internal void ClearList()
        {
            items.Clear();
        }

        public virtual int Add()
        {
            TableRow row = new TableRow(this);
            row.Items = new object[owner.Columns.Count];
            return Add(row);
        }
        public virtual int Add(TableRow row)
        {
            items.Add(row);
            owner.UpdateRow(row);
            return items.Count - 1;
        }
        public virtual int Add(int count)
        {
            for (int i = 0; i < count; i++)
                Add();
            return items.Count - 1;
        }
        public virtual int Add(params object[] values)
        {
            TableRow row = new TableRow(this);
            row.Items = values;
            return Add(row);
        }
        public void Clear()
        {
            for (; items.Count > 0;)
                Remove(items[0]);
        }
        public TableRow Last()
        {
            return items.Last();
        }
        public void Remove(TableRow row)
        {
            if (row.control != null)
                row.control.Dispose();
            if (row.ItemsControls != null)
                for (int i = 0; i < row.ItemsControls.Length; i++)
                    row.ItemsControls[i].Dispose();

            items.Remove(row);
        }
    }
    public class TableColumn
    {
        internal TableView.TableColumnButton control;
        private TableColumnCollection owner;

        public const int DEFAULT_WIDTH = 40;

        public int Index { get { return owner.owner.Columns.FindIndex(this); } }
        public string HeaderText { get; set; }
        public string Name { get; set; }
        public int Width
        {
            get
            {
                if (control != null)
                    return control.Width;
                return DEFAULT_WIDTH;
            }
            set
            {
                if (control != null)
                {
                    control.Width = value;
                    owner.owner.UpdateColumn(this);
                }
            }
        }

        public TableColumn(TableColumnCollection o)
        {
            owner = o;
        }
    }
    public class TableColumnCollection
    {
        private List<TableColumn> items = new List<TableColumn>();
        internal TableView owner;

        public int Count { get { return items.Count; } }

        public TableColumn this[int index]
        {
            get { return items[index]; }
        }

        public TableColumnCollection(TableView table)
        {
            owner = table;
        }

        public void Add(TableColumn column)
        {
            items.Add(column);
        }
        public void Add(string columnName, string headerText)
        {
            TableColumn column = new TableColumn(this);
            column.Name = columnName;
            column.HeaderText = headerText;
            Add(column);

            owner.UpdateColumn(column);
        }
        public int FindIndex(TableColumn column)
        {
            return items.FindIndex(x => x == column);
        }
        public TableColumn Last()
        {
            return items.Last();
        }
    }
    public class TableButtonStyle
    {
        public Color BackColor { get; set; }
        public Color BorderColor { get; set; }
        public Color BorderHoverColor { get; set; }
        public Color BorderSelectColor { get; set; }
        public Color HoverColor { get; set; }

        public TableButtonStyle()
        {
            BackColor = Color.FromArgb(252, 252, 252);
            BorderColor = Color.FromArgb(232, 241, 251);
            BorderHoverColor = Color.FromArgb(252, 253, 254);
            BorderSelectColor = BorderHoverColor;
            HoverColor = Color.FromArgb(243, 248, 254);
        }
    }
}
