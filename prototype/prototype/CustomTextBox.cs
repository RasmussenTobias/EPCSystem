using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class CustomTextBox : TextBox
{
    private Color borderColor = Color.White; // Default border color
    public Color BorderColor
    {
        get { return borderColor; }
        set { borderColor = value; this.Invalidate(); } // Causes control to be redrawn
    }

    public CustomTextBox()
    {
        // Set properties to ensure the custom drawing is visible
        this.BorderStyle = BorderStyle.FixedSingle;
    }

    // A method to update the control, specifically for drawing a border
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == 0xf || m.Msg == 0x133) // WM_PAINT = 0xf, WM_NCPAINT = 0x133
        {
            // Draw the custom border
            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                using (Pen pen = new Pen(borderColor, 2)) // Set the border width here
                {
                    g.DrawRectangle(pen, rect);
                }
            }
        }
    }
}
