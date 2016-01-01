namespace Gabriel.Cat
{
    partial class VisualitzaTaula
    {
        /// <summary> 
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar 
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvDadesTaulaConsultada = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDadesTaulaConsultada)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvDadesTaulaConsultada
            // 
            this.dgvDadesTaulaConsultada.AllowUserToAddRows = false;
            this.dgvDadesTaulaConsultada.AllowUserToDeleteRows = false;
            this.dgvDadesTaulaConsultada.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDadesTaulaConsultada.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDadesTaulaConsultada.Location = new System.Drawing.Point(0, 0);
            this.dgvDadesTaulaConsultada.Name = "dgvDadesTaulaConsultada";
            this.dgvDadesTaulaConsultada.ReadOnly = true;
            this.dgvDadesTaulaConsultada.Size = new System.Drawing.Size(150, 150);
            this.dgvDadesTaulaConsultada.TabIndex = 0;
            // 
            // VisualitzaTaula
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvDadesTaulaConsultada);
            this.Name = "VisualitzaTaula";
            ((System.ComponentModel.ISupportInitialize)(this.dgvDadesTaulaConsultada)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDadesTaulaConsultada;
    }
}
