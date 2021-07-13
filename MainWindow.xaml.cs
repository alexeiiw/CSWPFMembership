using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarteraSaneada
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Define objetos globales
        string connStringUTILS = "data source=128.1.202.136;initial catalog=UTILS;persist security info=True;user id=usrsap;password=C@nella20$";

        string connStringSTOD = "data source=128.1.200.180;initial catalog=STOD_SAPBONE;persist security info=True;user id=usrSTOD;password=stod20$";

        DataTable dt = null;

        int[] intIndice = null;

        string strSerie = "Ku7JZf9H";

        double dblTotalPagadoD = 0.00;

        bool Logueado = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Evento que se genera al iniciar la aplicacion
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Valida que el número de serie de la aplicación sea válido
            if (Validar_SerieAPP()) { Iniciar_Pantalla(); }
            else
            {
                MessageBox.Show("Su versión de aplicación ya no es válida, favor comunicarse con el Administrador!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        // Evento para exportar los datos a CSV
        private void btnCSV_Click(object sender, RoutedEventArgs e)
        {
            Exportar_CSV();
        }

        // Evento del boton login
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Si el boton esta en modo Login
            if (btnLogin.Content.ToString() == "Login")
            {
                if (Validar_Credenciales())
                {
                    // Deshabilita los controles y login
                    txtUsuario.IsEnabled = false;
                    txtClave.IsEnabled = false;
                    Logueado = true;
                    btnLogin.Content = "Logout";

                    // Dirige el foco al formulario de datos del cliente
                    tabCliente.IsEnabled = true;
                    tabCliente.Focus();
                }
                else { MessageBox.Show("Debe de ingresar sus credenciales de STOD para continuar!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                // Habilita los controles y logout
                txtUsuario.IsEnabled = true;
                txtUsuario.Text = "";
                txtClave.IsEnabled = true;
                txtClave.Password = "";
                Logueado = false;
                btnLogin.Content = "Login";
                lblnombreusuario.Visibility = Visibility.Hidden;
                lblareausuario.Visibility = Visibility.Hidden;
                tabCliente.IsEnabled = false;   
                
                // Limpia la aplicación
                Limpiar_Aplicacion();
            }
        }

        // Evento de cancelar / limpiar
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Limpia la aplicación
            Limpiar_Aplicacion();
        }

        // Evento botón detalle de la cuenta
        private void btnDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Habilita el botón de detalle
            tabDetalle.IsEnabled = true;
            tabDetalle.Focus();

            // Obtiene el boton y sus propiedades
            Button btnBoton = sender as Button;

            // Obtiene el detalle de los documentos
            Obtener_DetalleCuentas(Convert.ToInt32(btnBoton.Tag));
        }

        // Evento de buscar al cliente
        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            // Deshabilita el botón de busqueda
            btnBuscar.IsEnabled = false;

            // Valida que tipo de busqueda se realizará
            if (txtCodigo.Text != "" & txtNombre.Text != "") 
            { 
                MessageBox.Show("La busqueda puede realizarse con solo 1 criterio!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                // Limpia la aplicación
                Limpiar_Aplicacion();
            }
            else if (txtCodigo.Text != "" & txtNombre.Text == "") { Consultar_Codigo(txtCodigo.Text); }
            else if (txtCodigo.Text == "" & txtNombre.Text != "") { Consultar_Codigo(); }
        }

        // Evento para calcular el monto del pago
        private void btnCalcular_Click(object sender, RoutedEventArgs e)
        {
            if (txtMontoP.Text == "") { txtMonto.Text = "0.00"; }
            if (txtInteresP.Text == "") { txtInteresP.Text = "0.00"; }
            if (txtRecargoP.Text == "") { txtRecargoP.Text = "0.00"; }

            // Calcula el monto que va a pagar el cliente
            txtSaldoP.Text = Aplicar_FormatoMoneda((Convert.ToDouble(txtMontoP.Text) + Convert.ToDouble(txtInteresP.Text) + Convert.ToDouble(txtRecargoP.Text)),2);
        }

        // Evento del checkbox para pagar una cuenta
        private void chkPagar_Click(object sender, RoutedEventArgs e)
        {
            // Habilito el boton para realizar el pago
            btnPagar.IsEnabled = true;

            // Obtiene el check y sus propiedades
            CheckBox chkCheck = sender as CheckBox;

            // Si el botón esta chequeado actualiza el indice
            if (chkCheck.IsChecked == true) { intIndice[Convert.ToInt32(chkCheck.Tag)] = Convert.ToInt32(chkCheck.Tag); }
            else { intIndice[Convert.ToInt32(chkCheck.Tag)] = -1; }
        }

        // Evento para tomar los documentos y realizar el pago
        private void btnPagar_Click(object sender, RoutedEventArgs e)
        {
            // Habilita el Tab de pagos
            tabPagos.IsEnabled = true;
            tabPagos.Focus();

            // Habilita el boton para aplicar pagos y deshabilito el boton de pagar, también se deshabilita el tab de las cuentas
            btnAplicar.IsEnabled = true;
            btnPagar.IsEnabled = false;
            tabCuentas.IsEnabled = false;

            // Limpia los documentos
            lblDocumentos.Content = "";

            // Obtiene el formulario para el pago
            Obtener_Pago();
        }

        // Evento para aplicar el pago, crear el recibo
        private void btnAplicar_Click(object sender, RoutedEventArgs e)
        {
            // 
            if (txtSaldoP.Text == "") { MessageBox.Show("El total a pagar no puede ser 0.00!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
            else { Aplicar_Pago(); }           
        }

        // Metodo para exportar a CSV
        private void Exportar_CSV()
        {
            string strSql = @"select c.NO_DOCUMENTO, c.FECHA_DOCUMENTO, C.FECHA_VENCIMIENTO_DOCUMENTO, C.TIPO_DOCUMENTO, case id_empresa when 1 then 'CANELLA' when 2 then 'MR. CREDIT' end as EMPRESA, C.MARCA_EMPRESA, 
                            C.SERIE_DOCUMENTO_FACTURA, C.NO_DOCUMENTO_FACTURA, C.CODIGO_CLIENTE, C.NOMBRE_CLIENTE, C.DIRECCION_CASA_CLIENTE, c.TOTAL_CUOTA as TOTAL_DOCUMENTO, 
                            c.total_cuota - (c.total_pagado + (select SUM(monto_cuota+monto_interes+monto_recargos) as total from CS_Pagos where NO_DOCUMENTO = C.NO_DOCUMENTO)) as SALDO,
                            case cancelada when 0 then 'NO CANCELADA' when 1 then 'CANCELADA' end as CANCELADA,
                            C.FECHA_PERDIDA
                            from CS_Cuentas C";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }
           
            // Genera el reporte y lo deposita en el escritorio
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CarteraSaneada.csv";

            dt.ToCSV(filename);

            MessageBox.Show("Reporte generado y depositado en el Escritorio!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Metodo para validar las credenciales
        private bool Validar_Credenciales()
        {
            string strSql = @"select U.UserId, U.UserName, M.Password, M.PasswordSalt, M.Email from aspnet_Users U, aspnet_Membership M where M.UserId = U.UserId and upper(U.UserName) = '" + txtUsuario.Text.ToUpper() + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSTOD))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["password"].ToString() == EncodePassword(txtClave.Password, dt.Rows[0]["passwordsalt"].ToString()))
                {
                    // Coloca el nombre del usuario y el correo
                    lblnombreusuario.Content = dt.Rows[0]["username"].ToString();
                    lblnombreusuario.Visibility = Visibility.Visible;

                    lblareausuario.Visibility = Visibility.Visible;
                    lblareausuario.Content = dt.Rows[0]["email"].ToString();

                    // Si esta logueado retorna verdadero
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        // Metodo para validar el hash del password del usuario de STOD
        public string EncodePassword(string pass, string saltBase64)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(pass);
            byte[] src = Convert.FromBase64String(saltBase64);
            byte[] dst = new byte[src.Length + bytes.Length];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
            HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
            byte[] inArray = algorithm.ComputeHash(dst);
            return Convert.ToBase64String(inArray);
        }

        // Metodo que inicia la pantalla
        private void Iniciar_Pantalla()
        {
            // Inicia la pantalla y coloca el foco en el formulario de seguridad
            Limpiar_Aplicacion();
            tabCliente.IsEnabled = false;
            tabSeguridad.Focus();
        }

        // Metodo que valida la serie y la vigencia de la aplicacion
        private bool Validar_SerieAPP()
        {
            string strSql = "select * from [128.1.200.136].UTILS.dbo.SeriesAplicaciones where Serie = '" + strSerie + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                if (bool.Parse(dt.Rows[0]["Activo"].ToString()))
                {
                    Title = dt.Rows[0]["Aplicacion"].ToString() + " Versión: " + dt.Rows[0]["Version"].ToString() + " " + dt.Rows[0]["Ambiente"].ToString();
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        // Limpia la aplicación
        private void Limpiar_Aplicacion()
        {
            tabCuentas.IsEnabled = false;
            tabDetalle.IsEnabled = false;
            tabPagos.IsEnabled = false;
            tabHistorial.IsEnabled = false;

            txtNombre.Text = "";
            txtCodigo.Text = "";
            txtComentarios.Text = "";

            lblCodigo.Content = "";
            lblDireccionPrincipal.Content = "";
            lblDireccionTrabajo.Content = "";
            lblTelefonoMovil.Content = "";
            lblTelefonoCasa.Content = "";
            lblTelefonoTrabajo.Content = "";
            lblCorreo.Content = "";
            lblTrabajo.Content = "";
            lblMonto.Content = "";
            lblInteres.Content = "";
            lblRecargo.Content = "";
            lblPagado.Content = "";
            lblSaldo.Content = "";
            lblEmpresa.Content = "";
            lblCuentas.Content = "";
            lblFechaPerdida.Content = "";
            lblEstadoCuenta.Content = "";
            txtSaldoP.Text = "";
            chkCancela.IsChecked = false;

            btnPagar.IsEnabled = false;
            btnAplicar.IsEnabled = false;

            dt = new DataTable();

            btnBuscar.IsEnabled = true;
            tabCliente.Focus();

            dblTotalPagadoD = 0.00;

            intIndice = new int[10] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        }

        // Aplica el pago, crea el recibo
        private void Aplicar_Pago()
        {
            var objResultado = MessageBox.Show("Desea aplicar el pago?", "Datos", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Obtiene el total de documentos
            int intDocumentos = Convert.ToInt32(lblTotalDocumentos.Content);

            // Obtiene el numero de transaccion
            int intTransaccion = Obtener_Transaccion();

            if (objResultado == MessageBoxResult.Yes)
            {
                using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
                {

                    connUtil.Open();

                    SqlCommand command = connUtil.CreateCommand();

                    command.Connection = connUtil;

                    // Recorre todos los indices para saber cuantos pagos aplicar
                    for (int i = 1; i <= 10; i++)
                    {
                        if (intIndice[i - 1] != -1)
                        {
                            // Inserta el pago
                            string strInsert = @"insert into CS_Pagos values ('" + dt.Rows[intIndice[i - 1]]["no_documento"].ToString() + "',getdate()," + Convert.ToDouble(txtMontoP.Text) / intDocumentos + ","
                                + Convert.ToDouble(txtInteresP.Text) / intDocumentos + "," + Convert.ToDouble(txtRecargoP.Text) / intDocumentos + "," + intTransaccion + ",'{JSON}','" + txtUsuario.Text + "','" + txtComentarios.Text + "')";

                            command.CommandText = strInsert;
                            int intValida = command.ExecuteNonQuery();
                            if (intValida == 0) { MessageBox.Show("Hay problemas con insertar el registro!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                            // Revisa si el pago cancela la deuda total del cliente (check) o el total del pago aplicado salda el documento / cuenta
                            if (chkCancela.IsChecked == true ^ Convert.ToDouble(txtSaldo.Text) == Convert.ToDouble(txtSaldoP.Text))
                            {
                                string strUpdate = "update CS_Cuentas set cancelada = 1 where id = " + dt.Rows[intIndice[i - 1]]["id"].ToString();

                                command.CommandText = strUpdate;
                                intValida = command.ExecuteNonQuery();
                                if (intValida == 0) { MessageBox.Show("Hay problemas con actualizar el registro!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
                                else { MessageBox.Show("Se ha cancelado el total del documento!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information); }
                            }
                        }
                    }

                    MessageBox.Show("Se ha generado la transacción " + intTransaccion + " y se ha enviado el pago a la Interfaz SAP!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);

                    connUtil.Close();
                }
            }

            // Limpia la aplicación
            Limpiar_Aplicacion();
        }
        
        // Obtiene el valor total del pago
        private void Obtener_Pago()
        {
            // Define variables a nivel local
            double dblMonto = 0.00;
            double dblInteres = 0.00;
            double dblRecargo = 0.00;
            double dblPagado = 0.00;
            double dblTotalPagadoR = 0.00;
            int intTotalDocumentos = 0;

            // Obtiene los valores para mostrar en el formulario de pagos
            for (int i = 1; i <= 10; i++)
            {
                if (intIndice[i - 1] != -1)
                {
                    lblDocumentos.Content += dt.Rows[intIndice[i - 1]]["no_documento"].ToString() + " - ";
                    dblMonto += Convert.ToDouble(dt.Rows[intIndice[i - 1]]["total_cuota"].ToString());
                    dblInteres += Convert.ToDouble(dt.Rows[intIndice[i - 1]]["total_interes"].ToString());
                    dblRecargo += Convert.ToDouble(dt.Rows[intIndice[i - 1]]["total_recargos"].ToString());
                    dblPagado += Convert.ToDouble(dt.Rows[intIndice[i - 1]]["total_pagado"].ToString());
                    dblTotalPagadoR += Consultar_Pagos("'" + dt.Rows[intIndice[i - 1]]["no_documento"].ToString() + "'");
                    intTotalDocumentos += 1;
                }
            }

            // Aplica el total de lo pagado al recibo
            dblPagado += dblTotalPagadoR;

            txtMonto.Text = Aplicar_FormatoMoneda(dblMonto, 2);
            txtInteres.Text = Aplicar_FormatoMoneda(dblInteres, 2);
            txtRecargo.Text = Aplicar_FormatoMoneda(dblRecargo, 2);
            txtPagado.Text = Aplicar_FormatoMoneda(dblPagado, 2);
            txtSaldo.Text = Aplicar_FormatoMoneda(((dblMonto + dblInteres + dblRecargo) - dblPagado),2);

            txtMontoP.Text = txtSaldo.Text;
            txtInteresP.Text = Aplicar_FormatoMoneda(dblInteres, 2);
            txtRecargoP.Text = Aplicar_FormatoMoneda(dblRecargo, 2);
            lblTotalDocumentos.Content = intTotalDocumentos.ToString();
        }

        // Obtiene el detalle de las cuentas
        private void Obtener_DetalleCuentas(int intIndice)
        {
            // Define variables
            double dblSaldo = 0.00;

            // Rellena el detalle
            lblDocumento.Content = dt.Rows[intIndice]["no_documento"].ToString();
            lblFechaDocumento.Content = dt.Rows[intIndice]["fecha_documento"].ToString();
            lblTipoDocumento.Content = dt.Rows[intIndice]["tipo_documento"].ToString();
            lblFechaVencimiento.Content = dt.Rows[intIndice]["fecha_vencimiento_documento"].ToString();
            lblSubProducto.Content = dt.Rows[intIndice]["sub_producto_documento"].ToString();
            lblMarca.Content = dt.Rows[intIndice]["marca_empresa"].ToString();
            lblSerieFactura.Content = dt.Rows[intIndice]["serie_documento_factura"].ToString();
            lblFactura.Content = dt.Rows[intIndice]["no_documento_factura"].ToString();

            lblMontoD.Content = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[intIndice]["total_cuota"]),1);
            lblInteresD.Content = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[intIndice]["total_interes"]),1);
            lblRecargoD.Content = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[intIndice]["total_recargos"]),1);
            lblPagadoD.Content = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[intIndice]["total_pagado"]),1);
            dblSaldo = (Convert.ToDouble(dt.Rows[intIndice]["total_cuota"]) + Convert.ToDouble(dt.Rows[intIndice]["total_interes"]) + Convert.ToDouble(dt.Rows[intIndice]["total_recargos"])) - Convert.ToDouble(dt.Rows[intIndice]["total_pagado"]);
            lblSaldoD.Content = Aplicar_FormatoMoneda(dblSaldo,1);
            Consultar_PagosDocumento(dt.Rows[intIndice]["no_documento"].ToString());
        }

        // Obtiene el historial de pagos
        private void Obtener_Historial()
        {
            // Create the Grid    
            Grid DynamicGrid = new Grid();
            DynamicGrid.Width = 750;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.Background = new SolidColorBrush(Colors.WhiteSmoke);

            // Create Columns    
            ColumnDefinition gridCol1 = new ColumnDefinition();
            GridLength gridL1 = new GridLength(130, GridUnitType.Pixel);
            gridCol1.Width = gridL1;

            ColumnDefinition gridCol2 = new ColumnDefinition();
            GridLength gridL2 = new GridLength(140, GridUnitType.Pixel);
            gridCol2.Width = gridL2;

            ColumnDefinition gridCol3 = new ColumnDefinition();
            GridLength gridL3 = new GridLength(90, GridUnitType.Pixel);
            gridCol3.Width = gridL3;

            ColumnDefinition gridCol4 = new ColumnDefinition();
            GridLength gridL4 = new GridLength(90, GridUnitType.Pixel);
            gridCol4.Width = gridL4;

            ColumnDefinition gridCol5 = new ColumnDefinition();
            GridLength gridL5 = new GridLength(90, GridUnitType.Pixel);
            gridCol5.Width = gridL5;

            ColumnDefinition gridCol6 = new ColumnDefinition();
            GridLength gridL6 = new GridLength(110, GridUnitType.Pixel);
            gridCol6.Width = gridL6;

            ColumnDefinition gridCol7 = new ColumnDefinition();
            GridLength gridL7 = new GridLength(80, GridUnitType.Pixel);
            gridCol7.Width = gridL7;

            DynamicGrid.ColumnDefinitions.Add(gridCol1);
            DynamicGrid.ColumnDefinitions.Add(gridCol2);
            DynamicGrid.ColumnDefinitions.Add(gridCol3);
            DynamicGrid.ColumnDefinitions.Add(gridCol4);
            DynamicGrid.ColumnDefinitions.Add(gridCol5);
            DynamicGrid.ColumnDefinitions.Add(gridCol6);
            DynamicGrid.ColumnDefinitions.Add(gridCol7);

            // Create Rows    
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(25);
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(25);
            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(25);
            RowDefinition gridRow4 = new RowDefinition();
            gridRow4.Height = new GridLength(25);
            RowDefinition gridRow5 = new RowDefinition();
            gridRow5.Height = new GridLength(25);
            RowDefinition gridRow6 = new RowDefinition();
            gridRow6.Height = new GridLength(25);
            RowDefinition gridRow7 = new RowDefinition();
            gridRow7.Height = new GridLength(25);
            RowDefinition gridRow8 = new RowDefinition();
            gridRow8.Height = new GridLength(25);
            RowDefinition gridRow9 = new RowDefinition();
            gridRow9.Height = new GridLength(25);
            RowDefinition gridRow10 = new RowDefinition();
            gridRow10.Height = new GridLength(25);
            RowDefinition gridRow11 = new RowDefinition();
            gridRow11.Height = new GridLength(25);
            RowDefinition gridRow12 = new RowDefinition();
            gridRow12.Height = new GridLength(25);
            RowDefinition gridRow13 = new RowDefinition();
            gridRow13.Height = new GridLength(25);
            RowDefinition gridRow14 = new RowDefinition();
            gridRow14.Height = new GridLength(25);
            RowDefinition gridRow15 = new RowDefinition();
            gridRow15.Height = new GridLength(25);

            DynamicGrid.RowDefinitions.Add(gridRow1);
            DynamicGrid.RowDefinitions.Add(gridRow2);
            DynamicGrid.RowDefinitions.Add(gridRow3);
            DynamicGrid.RowDefinitions.Add(gridRow4);
            DynamicGrid.RowDefinitions.Add(gridRow5);
            DynamicGrid.RowDefinitions.Add(gridRow6);
            DynamicGrid.RowDefinitions.Add(gridRow7);
            DynamicGrid.RowDefinitions.Add(gridRow8);
            DynamicGrid.RowDefinitions.Add(gridRow9);
            DynamicGrid.RowDefinitions.Add(gridRow10);
            DynamicGrid.RowDefinitions.Add(gridRow11);
            DynamicGrid.RowDefinitions.Add(gridRow12);
            DynamicGrid.RowDefinitions.Add(gridRow13);
            DynamicGrid.RowDefinitions.Add(gridRow14);
            DynamicGrid.RowDefinitions.Add(gridRow15);

            // Add first column header    
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = " NO. DOCUMENTO ";
            txtBlock1.FontSize = 13;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock1.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock1, 0);
            Grid.SetColumn(txtBlock1, 0);

            TextBlock txtBlock2 = new TextBlock();
            txtBlock2.Text = "FECHA";
            txtBlock2.FontSize = 13;
            txtBlock2.FontWeight = FontWeights.Bold;
            txtBlock2.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock2.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock2.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock2, 0);
            Grid.SetColumn(txtBlock2, 1);

            TextBlock txtBlock3 = new TextBlock();
            txtBlock3.Text = "CUOTA";
            txtBlock3.FontSize = 13;
            txtBlock3.FontWeight = FontWeights.Bold;
            txtBlock3.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock3.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock3.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock3, 0);
            Grid.SetColumn(txtBlock3, 2);

            TextBlock txtBlock4 = new TextBlock();
            txtBlock4.Text = "INTERES";
            txtBlock4.FontSize = 13;
            txtBlock4.FontWeight = FontWeights.Bold;
            txtBlock4.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock4.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock4, 0);
            Grid.SetColumn(txtBlock4, 3);

            TextBlock txtBlock5 = new TextBlock();
            txtBlock5.Text = "RECARGOS";
            txtBlock5.FontSize = 13;
            txtBlock5.FontWeight = FontWeights.Bold;
            txtBlock5.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock5.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock5.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock5, 0);
            Grid.SetColumn(txtBlock5, 4);

            TextBlock txtBlock6 = new TextBlock();
            txtBlock6.Text = "TRANSACCION";
            txtBlock6.FontSize = 13;
            txtBlock6.FontWeight = FontWeights.Bold;
            txtBlock6.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock6.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock6.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock6, 0);
            Grid.SetColumn(txtBlock6, 5);

            TextBlock txtBlock7 = new TextBlock();
            txtBlock7.Text = "USUARIO";
            txtBlock7.FontSize = 13;
            txtBlock7.FontWeight = FontWeights.Bold;
            txtBlock7.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock7.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock7.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock7, 0);
            Grid.SetColumn(txtBlock7, 6);

            //// Add column headers to the Grid    
            DynamicGrid.Children.Add(txtBlock1);
            DynamicGrid.Children.Add(txtBlock2);
            DynamicGrid.Children.Add(txtBlock3);
            DynamicGrid.Children.Add(txtBlock4);
            DynamicGrid.Children.Add(txtBlock5);
            DynamicGrid.Children.Add(txtBlock6);
            DynamicGrid.Children.Add(txtBlock7);

            string strDocumentos = "";

            // Obtiene los documentos relacionados al cliente
            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    strDocumentos += ",'" + dt.Rows[i - 1]["no_documento"].ToString() + "'";
                }

                // Define consulta
                strDocumentos = strDocumentos.Substring(1, (strDocumentos.Length - 1));
                string strSql = @"select * from CS_Pagos where no_documento in (" + strDocumentos + ") order by FECHA_PAGO desc";

                DataTable dtHistorial = new DataTable();

                using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                    {
                        connUtil.Open();

                        da.Fill(dtHistorial);

                        connUtil.Close();
                    }
                }

                // Dibujar el detalle
                if (dtHistorial.Rows.Count > 0)
                {
                    for (int i = 1; i <= dtHistorial.Rows.Count; i++)
                    {
                        // Create first Row    
                        TextBlock txtbDocumento = new TextBlock();
                        txtbDocumento.Text = dtHistorial.Rows[i - 1]["no_documento"].ToString();
                        txtbDocumento.FontSize = 12;
                        Grid.SetRow(txtbDocumento, i);
                        Grid.SetColumn(txtbDocumento, 0);

                        TextBlock txtbFecha = new TextBlock();
                        txtbFecha.Text = dtHistorial.Rows[i - 1]["fecha_pago"].ToString();
                        txtbFecha.FontSize = 11;
                        txtbFecha.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetRow(txtbFecha, i);
                        Grid.SetColumn(txtbFecha, 1);

                        TextBlock txtbMonto = new TextBlock();
                        txtbMonto.Text = Aplicar_FormatoMoneda(Convert.ToDouble(dtHistorial.Rows[i - 1]["monto_cuota"]),1);
                        txtbMonto.FontSize = 11;
                        txtbMonto.HorizontalAlignment = HorizontalAlignment.Right;
                        Grid.SetRow(txtbMonto, i);
                        Grid.SetColumn(txtbMonto, 2);

                        TextBlock txtbInteres = new TextBlock();
                        txtbInteres.Text = Aplicar_FormatoMoneda(Convert.ToDouble(dtHistorial.Rows[i - 1]["monto_interes"]),1);
                        txtbInteres.FontSize = 11;
                        txtbInteres.HorizontalAlignment = HorizontalAlignment.Right;
                        Grid.SetRow(txtbInteres, i);
                        Grid.SetColumn(txtbInteres, 3);

                        TextBlock txtbRecargo = new TextBlock();
                        txtbRecargo.Text = Aplicar_FormatoMoneda(Convert.ToDouble(dtHistorial.Rows[i - 1]["monto_recargos"]), 1);
                        txtbRecargo.FontSize = 11;
                        txtbRecargo.HorizontalAlignment = HorizontalAlignment.Right;
                        Grid.SetRow(txtbRecargo, i);
                        Grid.SetColumn(txtbRecargo, 4);

                        TextBlock txtbTransaccion = new TextBlock();
                        txtbTransaccion.Text = dtHistorial.Rows[i - 1]["transaccion"].ToString();
                        txtbTransaccion.FontSize = 11;
                        txtbTransaccion.HorizontalAlignment = HorizontalAlignment.Center;
                        Grid.SetRow(txtbTransaccion, i);
                        Grid.SetColumn(txtbTransaccion, 5);

                        TextBlock txtbUsuario = new TextBlock();
                        txtbUsuario.Text = dtHistorial.Rows[i - 1]["usuario"].ToString();
                        txtbUsuario.FontSize = 11;
                        txtbUsuario.HorizontalAlignment = HorizontalAlignment.Center;
                        Grid.SetRow(txtbUsuario, i);
                        Grid.SetColumn(txtbUsuario, 6);

                        DynamicGrid.Children.Add(txtbDocumento);
                        DynamicGrid.Children.Add(txtbFecha);
                        DynamicGrid.Children.Add(txtbMonto);
                        DynamicGrid.Children.Add(txtbInteres);
                        DynamicGrid.Children.Add(txtbRecargo);
                        DynamicGrid.Children.Add(txtbTransaccion);
                        DynamicGrid.Children.Add(txtbUsuario);
                    }
                }
            }

            // Display grid into a Window    
            tabHistorial.Content = DynamicGrid;
        }

        // Obtiene las cuentas relacioandas al cliente
        private void Obtener_Cuentas()
        {
            // Create the Grid    
            Grid DynamicGrid = new Grid();
            DynamicGrid.Width = 750;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.Background = new SolidColorBrush(Colors.WhiteSmoke);

            // Create Columns    
            ColumnDefinition gridCol1 = new ColumnDefinition();
            GridLength gridL1 = new GridLength(130, GridUnitType.Pixel);
            gridCol1.Width = gridL1;

            ColumnDefinition gridCol2 = new ColumnDefinition();
            GridLength gridL2 = new GridLength(140, GridUnitType.Pixel);
            gridCol2.Width = gridL2;

            ColumnDefinition gridCol3 = new ColumnDefinition();
            GridLength gridL3 = new GridLength(60, GridUnitType.Pixel);
            gridCol3.Width = gridL3;

            ColumnDefinition gridCol4 = new ColumnDefinition();
            GridLength gridL4 = new GridLength(90, GridUnitType.Pixel);
            gridCol4.Width = gridL4;

            ColumnDefinition gridCol5 = new ColumnDefinition();
            GridLength gridL5 = new GridLength(80, GridUnitType.Pixel);
            gridCol5.Width = gridL5;

            ColumnDefinition gridCol6 = new ColumnDefinition();
            GridLength gridL6 = new GridLength(80, GridUnitType.Pixel);
            gridCol6.Width = gridL6;

            ColumnDefinition gridCol7 = new ColumnDefinition();
            GridLength gridL7 = new GridLength(50, GridUnitType.Pixel);
            gridCol7.Width = gridL7;

            ColumnDefinition gridCol8 = new ColumnDefinition();
            GridLength gridL8 = new GridLength(60, GridUnitType.Pixel);
            gridCol8.Width = gridL8;

            ColumnDefinition gridCol9 = new ColumnDefinition();
            GridLength gridL9 = new GridLength(60, GridUnitType.Pixel);
            gridCol9.Width = gridL9;

            DynamicGrid.ColumnDefinitions.Add(gridCol1);
            DynamicGrid.ColumnDefinitions.Add(gridCol2);
            DynamicGrid.ColumnDefinitions.Add(gridCol3);
            DynamicGrid.ColumnDefinitions.Add(gridCol4);
            DynamicGrid.ColumnDefinitions.Add(gridCol5);
            DynamicGrid.ColumnDefinitions.Add(gridCol6);
            DynamicGrid.ColumnDefinitions.Add(gridCol7);
            DynamicGrid.ColumnDefinitions.Add(gridCol8);
            DynamicGrid.ColumnDefinitions.Add(gridCol9);

            // Create Rows    
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(25);
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(25);
            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(25);
            RowDefinition gridRow4 = new RowDefinition();
            gridRow4.Height = new GridLength(25);
            RowDefinition gridRow5 = new RowDefinition();
            gridRow5.Height = new GridLength(25);
            RowDefinition gridRow6 = new RowDefinition();
            gridRow6.Height = new GridLength(25);
            RowDefinition gridRow7 = new RowDefinition();
            gridRow7.Height = new GridLength(25);
            RowDefinition gridRow8 = new RowDefinition();
            gridRow8.Height = new GridLength(25);
            RowDefinition gridRow9 = new RowDefinition();
            gridRow9.Height = new GridLength(25);
            RowDefinition gridRow10 = new RowDefinition();
            gridRow10.Height = new GridLength(25);
            RowDefinition gridRow11 = new RowDefinition();
            gridRow11.Height = new GridLength(25);
            RowDefinition gridRow12 = new RowDefinition();
            gridRow12.Height = new GridLength(25);
            RowDefinition gridRow13 = new RowDefinition();
            gridRow13.Height = new GridLength(25);
            RowDefinition gridRow14 = new RowDefinition();
            gridRow14.Height = new GridLength(25);
            RowDefinition gridRow15 = new RowDefinition();
            gridRow15.Height = new GridLength(25);

            DynamicGrid.RowDefinitions.Add(gridRow1);
            DynamicGrid.RowDefinitions.Add(gridRow2);
            DynamicGrid.RowDefinitions.Add(gridRow3);
            DynamicGrid.RowDefinitions.Add(gridRow4);
            DynamicGrid.RowDefinitions.Add(gridRow5);
            DynamicGrid.RowDefinitions.Add(gridRow6);
            DynamicGrid.RowDefinitions.Add(gridRow7);
            DynamicGrid.RowDefinitions.Add(gridRow8);
            DynamicGrid.RowDefinitions.Add(gridRow9);
            DynamicGrid.RowDefinitions.Add(gridRow10);
            DynamicGrid.RowDefinitions.Add(gridRow11);
            DynamicGrid.RowDefinitions.Add(gridRow12);
            DynamicGrid.RowDefinitions.Add(gridRow13);
            DynamicGrid.RowDefinitions.Add(gridRow14);
            DynamicGrid.RowDefinitions.Add(gridRow15);

            // Add first column header    
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = " NO. DOCUMENTO ";
            txtBlock1.FontSize = 13;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock1.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock1, 0);
            Grid.SetColumn(txtBlock1, 0);

            TextBlock txtBlock2 = new TextBlock();
            txtBlock2.Text = "FECHA";
            txtBlock2.FontSize = 13;
            txtBlock2.FontWeight = FontWeights.Bold;
            txtBlock2.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock2.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock2.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock2, 0);
            Grid.SetColumn(txtBlock2, 1);

            TextBlock txtBlock3 = new TextBlock();
            txtBlock3.Text = "TIPO";
            txtBlock3.FontSize = 13;
            txtBlock3.FontWeight = FontWeights.Bold;
            txtBlock3.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock3.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock3.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock3, 0);
            Grid.SetColumn(txtBlock3, 2);

            TextBlock txtBlock4 = new TextBlock();
            txtBlock4.Text = "MARCA";
            txtBlock4.FontSize = 13;
            txtBlock4.FontWeight = FontWeights.Bold;
            txtBlock4.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock4.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock4, 0);
            Grid.SetColumn(txtBlock4, 3);

            TextBlock txtBlock5 = new TextBlock();
            txtBlock5.Text = "MONTO";
            txtBlock5.FontSize = 13;
            txtBlock5.FontWeight = FontWeights.Bold;
            txtBlock5.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock5.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock5.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock5, 0);
            Grid.SetColumn(txtBlock5, 4);

            TextBlock txtBlock6 = new TextBlock();
            txtBlock6.Text = "SALDO";
            txtBlock6.FontSize = 13;
            txtBlock6.FontWeight = FontWeights.Bold;
            txtBlock6.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock6.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock6.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock6, 0);
            Grid.SetColumn(txtBlock6, 5);


            TextBlock txtBlock7 = new TextBlock();
            txtBlock7.Text = "CUOTA";
            txtBlock7.FontSize = 13;
            txtBlock7.FontWeight = FontWeights.Bold;
            txtBlock7.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock7.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock7.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock7, 0);
            Grid.SetColumn(txtBlock7, 6);

            TextBlock txtBlock8 = new TextBlock();
            txtBlock8.Text = "Acción";
            txtBlock8.FontSize = 13;
            txtBlock8.FontWeight = FontWeights.Bold;
            txtBlock8.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock8.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock8.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock8, 0);
            Grid.SetColumn(txtBlock8, 7);

            TextBlock txtBlock9 = new TextBlock();
            txtBlock9.Text = "Pagar";
            txtBlock9.FontSize = 13;
            txtBlock9.FontWeight = FontWeights.Bold;
            txtBlock9.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock9.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock9.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock9, 0);
            Grid.SetColumn(txtBlock9, 8);

            //// Add column headers to the Grid    
            DynamicGrid.Children.Add(txtBlock1);
            DynamicGrid.Children.Add(txtBlock2);
            DynamicGrid.Children.Add(txtBlock3);
            DynamicGrid.Children.Add(txtBlock4);
            DynamicGrid.Children.Add(txtBlock5);
            DynamicGrid.Children.Add(txtBlock6);
            DynamicGrid.Children.Add(txtBlock7);
            DynamicGrid.Children.Add(txtBlock8);
            DynamicGrid.Children.Add(txtBlock9);

            // Dibujar el detalle
            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    // Create first Row    
                    TextBlock txtbDocumento = new TextBlock();
                    txtbDocumento.Text = dt.Rows[i - 1]["no_documento"].ToString();
                    txtbDocumento.FontSize = 12;
                    Grid.SetRow(txtbDocumento, i);
                    Grid.SetColumn(txtbDocumento, 0);

                    TextBlock txtbFecha = new TextBlock();
                    txtbFecha.Text = dt.Rows[i - 1]["fecha_documento"].ToString();
                    txtbFecha.FontSize = 11;
                    txtbFecha.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbFecha, i);
                    Grid.SetColumn(txtbFecha, 1);

                    TextBlock txtbTipoDocumento = new TextBlock();
                    if (dt.Rows[i - 1]["tipo_documento"].ToString() == "CR") { txtbTipoDocumento.Text = "CREDITO"; }
                    if (dt.Rows[i - 1]["tipo_documento"].ToString() == "FA") { txtbTipoDocumento.Text = "FACTURA"; }
                    txtbTipoDocumento.FontSize = 11;
                    txtbTipoDocumento.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbTipoDocumento, i);
                    Grid.SetColumn(txtbTipoDocumento, 2);

                    TextBlock txtbMarca = new TextBlock();
                    txtbMarca.Text = dt.Rows[i - 1]["marca_empresa"].ToString();
                    txtbMarca.FontSize = 11;
                    txtbMarca.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbMarca, i);
                    Grid.SetColumn(txtbMarca, 3);

                    TextBlock txtbMonto = new TextBlock();
                    txtbMonto.Text = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[i - 1]["total_cuota"]),1);
                    txtbMonto.FontSize = 11;
                    txtbMonto.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtbMonto, i);
                    Grid.SetColumn(txtbMonto, 4);

                    TextBlock txtbSaldo = new TextBlock();
                    txtbSaldo.Text = Aplicar_FormatoMoneda(Convert.ToDouble(dt.Rows[i - 1]["total_cuota"]) - 
                        (Convert.ToDouble(dt.Rows[i - 1]["total_pagado"]) + Consultar_Pagos("'" + dt.Rows[i - 1]["no_documento"].ToString() + "'")),1);
                    txtbSaldo.FontSize = 11;
                    txtbSaldo.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtbSaldo, i);
                    Grid.SetColumn(txtbSaldo, 5);

                    TextBlock txtbCuota = new TextBlock();
                    txtbCuota.Text = dt.Rows[i - 1]["cuota"].ToString();
                    txtbCuota.FontSize = 11;
                    txtbCuota.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtbCuota, i);
                    Grid.SetColumn(txtbCuota, 6);

                    Button btnDetalle = new Button();
                    btnDetalle.Content = "DETALLE";
                    btnDetalle.Click += btnDetalle_Click;
                    btnDetalle.Tag = i - 1;
                    Grid.SetRow(btnDetalle, i);
                    Grid.SetColumn(btnDetalle, 7);

                    TextBlock txtbCancelado = new TextBlock();
                    CheckBox chkPago = new CheckBox();

                    // Si la cuenta / documento / factura esta cancelada, no muestra el check
                    if (dt.Rows[i - 1]["cancelada"].ToString() == "True")
                    {
                        txtbCancelado.Text = "Cancelado";
                        txtbCancelado.FontSize = 10;
                        txtbCancelado.HorizontalAlignment = HorizontalAlignment.Center;
                        txtbCancelado.VerticalAlignment = VerticalAlignment.Center;
                        txtbCancelado.FontWeight = FontWeights.SemiBold;
                        Grid.SetRow(txtbCancelado, i);
                        Grid.SetColumn(txtbCancelado, 8);
                    }
                    else
                    {
                        chkPago.HorizontalAlignment = HorizontalAlignment.Center;
                        chkPago.Click += chkPagar_Click;
                        chkPago.Tag = i - 1;
                        Grid.SetRow(chkPago, i);
                        Grid.SetColumn(chkPago, 8);
                    }

                    DynamicGrid.Children.Add(txtbDocumento);
                    DynamicGrid.Children.Add(txtbFecha);
                    DynamicGrid.Children.Add(txtbTipoDocumento);
                    DynamicGrid.Children.Add(txtbMarca);
                    DynamicGrid.Children.Add(txtbMonto);
                    DynamicGrid.Children.Add(txtbSaldo);
                    DynamicGrid.Children.Add(txtbCuota);
                    DynamicGrid.Children.Add(btnDetalle);
                    if (dt.Rows[i - 1]["cancelada"].ToString() == "True") { DynamicGrid.Children.Add(txtbCancelado); }
                    else { DynamicGrid.Children.Add(chkPago); }
                }
            }

            // Display grid into a Window    
            tabCuentas.Content = DynamicGrid;
        }

        // Obtiene el detalle de pagos por documento
        private void Consultar_PagosDocumento(string strDocumento)
        {
            double dblPagadoD = 0.00;
            double dblPagado = 0.00;
            double dblSaldoD = 0.00;

            // Define consulta
            string strSql = @"select isnull(SUM(monto_cuota),0) as TotalCuota, isnull(SUM(monto_interes),0) as TotalInteres, isnull(SUM(monto_recargos),0) as TotalRecargos from cs_pagos where no_documento in ('" + strDocumento + "')";

            DataTable dtPagos = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dtPagos);

                    connUtil.Close();
                }
            }

            if (dtPagos.Rows.Count > 0)
            {
                lblPagadoD.Content = lblPagadoD.Content.ToString().Replace("---", "0");
                lblPagadoD.Content = lblPagadoD.Content.ToString().Replace("Q", "0");
                lblSaldoD.Content = lblSaldoD.Content.ToString().Replace("Q", "0");

                dblPagado = Convert.ToDouble(lblPagadoD.Content);
                dblSaldoD = Convert.ToDouble(lblSaldoD.Content);

                dblPagadoD = Convert.ToDouble(dtPagos.Rows[0]["totalcuota"]) + Convert.ToDouble(dtPagos.Rows[0]["totalinteres"]) + Convert.ToDouble(dtPagos.Rows[0]["totalrecargos"]);
                if ((Convert.ToDouble(dtPagos.Rows[0]["totalcuota"]) + Convert.ToDouble(dtPagos.Rows[0]["totalinteres"]) + Convert.ToDouble(dtPagos.Rows[0]["totalrecargos"])) != 0) { dblSaldoD = dblSaldoD - dblPagadoD; }

                lblPagadoD.Content = Aplicar_FormatoMoneda(dblPagadoD+dblPagado,1);
                lblSaldoD.Content = Aplicar_FormatoMoneda(dblSaldoD,1);
            }
        }

        // Obtiene los documentos relacionados para el pago
        private double Consultar_Pagos(string strDocumento="")
        {
            string strDocumentos = "";

            double dblTotalPagado = 0.00;
            double dblTotalSaldo = 0.00;

            // Obtiene los documentos relacionados al cliente
            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    strDocumentos += ",'" + dt.Rows[i - 1]["no_documento"].ToString() + "'";
                }

                // Define consulta
                strDocumentos = strDocumentos.Substring(1, (strDocumentos.Length - 1));
                
                // Si viene el parametro de documentos obtiene el valor por un documento
                if (strDocumento != "") { strDocumentos = strDocumento; }
                string strSql = @"select isnull(SUM(monto_cuota),0) as TotalCuota, isnull(SUM(monto_interes),0) as TotalInteres, isnull(SUM(monto_recargos),0) as TotalRecargos from cs_pagos where no_documento in (" + strDocumentos + ")";

                DataTable dtPagos = new DataTable();

                using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                    {
                        connUtil.Open();

                        da.Fill(dtPagos);

                        connUtil.Close();
                    }
                }

                if (strDocumento=="")
                {
                    if (dtPagos.Rows.Count > 0)
                    {
                        // Reemplaza el contenido para quitar el valor moneda
                        lblPagado.Content = lblPagado.Content.ToString().Replace("Q", "0");
                        lblPagado.Content = lblPagado.Content.ToString().Replace("---", "0");
                        lblSaldo.Content = lblSaldo.Content.ToString().Replace("Q", "0");

                        dblTotalPagado = Convert.ToDouble(lblPagado.Content);
                        dblTotalSaldo = Convert.ToDouble(lblSaldo.Content);

                        dblTotalPagadoD += Convert.ToDouble(dtPagos.Rows[0]["totalcuota"]) + Convert.ToDouble(dtPagos.Rows[0]["totalinteres"]) + Convert.ToDouble(dtPagos.Rows[0]["totalrecargos"]);
                        // Si hay pagos
                        if (dblTotalPagadoD != 0)
                        {
                            //lblSaldo.Content = Aplicar_FormatoMoneda((dblTotalSaldo - (dblTotalPagado + dblTotalPagadoD)),1);
                            lblSaldo.Content = Aplicar_FormatoMoneda((dblTotalSaldo - dblTotalPagadoD), 1);
                            lblPagado.Content = Aplicar_FormatoMoneda(dblTotalPagado + dblTotalPagadoD, 1);
                        }
                        else
                        {
                            // Si no hay pagos, agrega el formato a los nuevos valores
                            lblPagado.Content = Aplicar_FormatoMoneda(Convert.ToDouble(lblPagado.Content), 1);
                            lblSaldo.Content = Aplicar_FormatoMoneda(Convert.ToDouble(lblSaldo.Content), 1);
                        }
                    }

                    return 0.00;
                }
                else 
                {
                    return Convert.ToDouble(dtPagos.Rows[0]["totalcuota"]) + Convert.ToDouble(dtPagos.Rows[0]["totalinteres"]) + Convert.ToDouble(dtPagos.Rows[0]["totalrecargos"]);
                }
            }
            return 0.00;
        }

        // Obtener la última transacción
        private int Obtener_Transaccion()
        {
            // Define consulta
            string strSql = "";

            strSql = @"select transaccion from CS_Pagos order by TRANSACCION desc";

            DataTable dtTransaccion = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dtTransaccion);

                    connUtil.Close();
                }
            }

            if (dtTransaccion.Rows.Count > 0) { return Convert.ToInt32(dtTransaccion.Rows[0]["transaccion"]) + 1; }
            else { return 100;  }

        }

        // Consulta el código del cliente
        private void Consultar_Codigo(string strCodigo = "")
        {
            // Define consulta
            string strSql = "";
            if (strCodigo =="") 
            { 
                strSql = @"select codigo_cliente from CS_Cuentas where nombre_cliente like '%" + txtNombre.Text + "%' group by codigo_cliente";

                DataTable dtCodigo = new DataTable();

                using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                    {
                        connUtil.Open();

                        da.Fill(dtCodigo);

                        connUtil.Close();
                    }
                }

                if (dtCodigo.Rows.Count > 1)
                {
                    MessageBox.Show("Existe más de 1 cliente con el criterio de busqueda, buscar por código!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Limpia la pantalla
                    Limpiar_Aplicacion();

                }
                else { Consultar_Cliente(dtCodigo.Rows[0]["codigo_cliente"].ToString()); }
            }
            else 
            {
                Consultar_Cliente(strCodigo);  
            }
        }

        // Elabora el formato para la moneda
        private string Aplicar_FormatoMoneda(double dblValor, byte bytTipo)
        {
            if (bytTipo == 1)
            {
                if (dblValor == 0.00) { return "---"; }
                else { return dblValor.ToString("Q###,###,###.00"); }
            }
            if (bytTipo == 2) 
            {
                if (dblValor == 0.00) { return "0.00"; }
                else { return dblValor.ToString("###,###,###.00"); }
            }
            else { return ""; }
        }

        // Obtiene al cliente 
        private void Consultar_Cliente(string strCodigo)
        {
            // Define variables
            double dblTotalMonto = 0.00;
            double dblTotalInteres = 0.00;
            double dblTotalRecargos = 0.00;
            double dblTotalPagado = 0.00;
            int intTotalCuentas = 0;

            // Define consulta
            string strSql = @"select * from CS_Cuentas where codigo_cliente = '" + strCodigo + "' order by CANCELADA asc";

            using (SqlConnection connUtil = new SqlConnection(connStringUTILS))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                // Rellena el formulario
                txtNombre.Text = dt.Rows[0]["nombre_cliente"].ToString();
                lblCodigo.Content = dt.Rows[0]["codigo_cliente"].ToString();
                lblDireccionPrincipal.Content = dt.Rows[0]["direccion_casa_cliente"].ToString();
                lblDireccionTrabajo.Content = dt.Rows[0]["direccion_trabajo_cliente"].ToString();
                lblTelefonoMovil.Content = dt.Rows[0]["telefono_movil_cliente"].ToString();
                lblTelefonoCasa.Content = dt.Rows[0]["telefono_casa_cliente"].ToString();
                lblTelefonoTrabajo.Content = dt.Rows[0]["telefono_trabajo_cliente"].ToString();
                lblCorreo.Content = dt.Rows[0]["correo_electronico_cliente"].ToString();
                lblTrabajo.Content = dt.Rows[0]["trabajo_cliente"].ToString();

                // Obtiene los montos
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    dblTotalMonto += Convert.ToDouble(dt.Rows[i - 1]["total_cuota"].ToString());
                    dblTotalInteres += Convert.ToDouble(dt.Rows[i - 1]["total_interes"].ToString());
                    dblTotalRecargos += Convert.ToDouble(dt.Rows[i - 1]["total_recargos"].ToString());
                    dblTotalPagado += Convert.ToDouble(dt.Rows[i - 1]["total_pagado"].ToString());
                    intTotalCuentas += 1;
                }

                lblMonto.Content = Aplicar_FormatoMoneda(dblTotalMonto,1);
                lblInteres.Content = Aplicar_FormatoMoneda(dblTotalInteres,1);
                lblRecargo.Content = Aplicar_FormatoMoneda(dblTotalRecargos,1);
                lblPagado.Content = Aplicar_FormatoMoneda(dblTotalPagado,1);
                lblSaldo.Content = Aplicar_FormatoMoneda(((dblTotalMonto + dblTotalInteres + dblTotalRecargos) - dblTotalPagado),1);
                lblCuentas.Content = intTotalCuentas.ToString();

                if (dt.Rows[0]["id_empresa"].ToString() == "1") { lblEmpresa.Content = "CANELLA"; }
                else { lblEmpresa.Content = "MR CREDIT"; }
                lblFechaPerdida.Content = dt.Rows[0]["fecha_perdida"].ToString();
                
                if (dt.Rows[0]["cancelada"].ToString() == "True") { lblEstadoCuenta.Content = "CANCELADA"; }
                else { lblEstadoCuenta.Content = "PENDIENTE"; }

                // Obtener pagos
                Consultar_Pagos();

                // Dibuja el grid de Cuentas
                tabCuentas.IsEnabled = true;
                Obtener_Cuentas();

                // Dibuja el grid de Pagos
                tabHistorial.IsEnabled = true;
                Obtener_Historial();
            }
            else 
            {
                if (strCodigo == "") { MessageBox.Show("Nombre del cliente no existe!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
                else { MessageBox.Show("Código del cliente no existe!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                // Limpia la pantalla
                Limpiar_Aplicacion();
            }
        }
    }

    // Clase que define la extensión del método para el CSV
    public static class CSVUtlity
    {
        public static void ToCSV(this DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
    }
}
