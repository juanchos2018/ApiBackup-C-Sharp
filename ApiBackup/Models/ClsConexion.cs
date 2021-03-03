using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ApiBackup.Models
{
    public class ClsConexion
    {

        private string pc_Servidor;
        string pc_BaseDatos;
        string pc_Usuario;
        string pc_Contrasena;

        private SqlConnection po_Conexion = null/* TODO Change to default(_) if this is not a reference type */;
        private SqlDataAdapter po_Adaptador = null/* TODO Change to default(_) if this is not a reference type */;
        private SqlCommand po_Comando = null/* TODO Change to default(_) if this is not a reference type */;
        public void Asignar_Servidor(string aServidor, string aUsuario, string aContrasena)
        {
            pc_Servidor = aServidor;
            pc_Usuario = aUsuario;
            pc_Contrasena = aContrasena;
        }
        public void Asignar_Servidor(string aServidor, string aUsuario, string aContrasena, string aBaseDatos)
        {
            pc_Servidor = aServidor;
            pc_Usuario = aUsuario;
            pc_Contrasena = aContrasena;
            pc_BaseDatos = aBaseDatos;
        }

        private bool Conectar_BD(string server,string bd,string usuario,string password)
        {
            string estadoconex = "";
            //pc_Servidor = server;
            //pc_BaseDatos =bd;
            //pc_Usuario = usuario;
            //pc_Contrasena =password;
            //string sr = "DESKTOP-PEKLP69\\SSQLSERVER01";
            //string b = "CON2056568395121";
            //string usu = "sa";
            //string pa = "123456";

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-PE");
            try
            {
                if (po_Conexion == null)
                {
                    //DESKTOP-PEKLP69\MSSQLSERVER01
                 // po_Conexion = new SqlConnection("Data Source=DESKTOP-PEKLP69\\SSQLSERVER01; Initial Catalog=CON2056568395121; Integrated Security=True;UID=sa; PWD=123456;");
                    po_Conexion = new SqlConnection("Server=" + pc_Servidor + "; DataBase=" + pc_BaseDatos + ";UID=" + pc_Usuario + "; PWD=" + pc_Contrasena + ";");
                   // po_Conexion = new SqlConnection("Server=DESKTOP-PEKLP69\\SSQLSERVER01; DataBase=master ;UID=sa; PWD=123456;");
                    po_Conexion.Open();
                    return true;
                }
                else if (po_Conexion.State.Equals(ConnectionState.Closed))
                    estadoconex = "La Conexión Se encuentra Cerrada.";
                else
                    estadoconex = "La Conexión ya se encuentra abierta.";
            }
            catch (Exception ex)
            {
                estadoconex = "Datos Incorrectos: revise el ID de servidor y la contraseña de usuario";
                po_Conexion = null;
            }
            return false;
        }
        public void Crear_Comando(string SentenciaSQL)
        {
            this.po_Comando = new SqlCommand();
            this.po_Comando.Connection = po_Conexion;
            this.po_Comando.CommandType = CommandType.Text;
            po_Comando.CommandTimeout = 360;
            this.po_Comando.CommandText = SentenciaSQL;
        }
        public DataSet EjecutarConsulta1(string Tabla)
        {
            DataSet ds = new DataSet();
            this.po_Adaptador = new SqlDataAdapter();
            this.po_Adaptador.SelectCommand = po_Comando;
            po_Adaptador.Fill(ds, Tabla);
            return ds;
        }

        public DataSet EjecutarConsulta(string Tabla, string SentenciaSQL, string server, string bd, string usuario, string password)
        {
            DataSet ds = new DataSet();
            string Error;
            this.po_Comando = new SqlCommand();
            this.po_Comando.CommandType = CommandType.Text;
            this.po_Comando.CommandText = SentenciaSQL;
            this.po_Comando.CommandTimeout = 200;

            this.po_Adaptador = new SqlDataAdapter();
           // usuario = "sa";
           // password = "123456";
            if (Conectar_BD(server,bd,usuario,password) == true)
            {
                po_Comando.Connection = po_Conexion;
                this.po_Adaptador.SelectCommand = po_Comando;
                try
                {
                    po_Adaptador.Fill(ds, Tabla);
                }
                catch (Exception EX)
                {
                    Error=("Error en expresion!: " + EX.Message);
                    return null/* TODO Change to default(_) if this is not a reference type */;
                }
                Desconectar_BD();
            }
            return ds;
        }
        public void Desconectar_BD()
        {
            if (!(po_Conexion == null))
            {
                if (po_Conexion.State.Equals(ConnectionState.Open))
                {
                    po_Conexion.Close();
                    po_Conexion = null;
                }
            }
        }
    }
}