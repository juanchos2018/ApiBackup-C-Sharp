using ApiBackup.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApiBackup.Controllers
{
    public class NuevaController : ApiController
    {

        ClsConexion go_Sql;
        string ruta;
        string ruta1;
        string baseplantilla;
        string nbase;
        bool bandera = false;
        string tipo;
        string servidor;
        string basedatos;
        string usuario;
        string password;
        public IHttpActionResult Index([FromBody] Datos datos)
        {
            var path = "";
            path = System.Web.HttpContext.Current.Server.MapPath("~/bak/");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            if (datos.periodo == "")
            {
                return BadRequest("Periodo esta vacio");
            }
            if (datos.razonsocial == "")
            {
                return BadRequest("Razon social vacio");
            }
            if (datos.ruc == "")
            {
                return BadRequest("Ruc esta vacio");
            }
            if (datos.direccion == "")
            {
                return BadRequest("Direccion esta Vacio");
            }
            //DESKTOP-PEKLP69\MSSQLSERVER01
            servidor = datos.servidor;
            basedatos = "master";
            usuario = datos.usuario;
            password = datos.password; 
            string rutax = " select top 1 reverse(right(reverse(physical_name),len(reverse(physical_name))-charindex('\',reverse(physical_name))))as ruta from sys.master_files where name like 'CON%' ";
            go_Sql = new ClsConexion();
            go_Sql.Asignar_Servidor(servidor, usuario, password, basedatos);
            DataTable dtruta = go_Sql.EjecutarConsulta("ruta", rutax, datos.servidor, basedatos, datos.usuario, datos.password).Tables[0];
            if (dtruta.Rows.Count > 0)
                 ruta = dtruta.Rows[0][0].ToString();
                 ruta1 = Path.GetDirectoryName(ruta);
                 DataTable dttr = new DataTable();
                 nbase = "CON" + datos.ruc + datos.periodo;   
                GenerarBackup(datos.baseplantilla, path);               
                RestaurarBackup( datos.ruc, datos.periodo, path);              
                eliminarbackup(path);
                limpiartablas(datos.periodo, datos.razonsocial, datos.direccion, datos.ruc, "");
              
                return Ok("Se genero correctamente el Nuevo Periodo");          
        }

        private void GenerarBackup(object bd, string path)
        {
            try
            {
                var Consulta = "";
              
                Consulta = "backup database " + bd + " to disk = N'" + path + @"\Temporal.bak' with noformat, noinit, name = N'" + bd + "',skip,stats = 10";
                go_Sql.EjecutarConsulta("Me", Consulta, servidor, basedatos, usuario, password);
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void RestaurarBackup( string ruc, string periodo, string path)
        {
            DataSet ds_logicalname = new DataSet();
            string logicalname = " RESTORE FILELISTONLY FROM  DISK = N'" + path + "Temporal.bak' ";
           
            ds_logicalname = go_Sql.EjecutarConsulta("ma", logicalname, servidor, basedatos, usuario, password);

            nbase = "CON" + ruc + periodo;
            string dato = nbase;
            int a = dato.Length;
            string dx = "select * from sys.databases where Name='" + nbase.Trim() + "'";
            if (go_Sql.EjecutarConsulta("de", dx, servidor, basedatos, usuario, password).Tables[0].Rows.Count > 0)
            {                
                eliminarbackup(path);
              
                return;
            }
            try
            {
                var Consulta = "";             
                Consulta = "RESTORE DATABASE  " + nbase + "  FROM  DISK = N'" + path + "Temporal.bak' WITH  FILE = 1, MOVE N'" + ds_logicalname.Tables[0].Rows[0][0].ToString() + "' TO N'" + ruta1 +"\\"+ nbase +".mdf"+ "',MOVE N'" + ds_logicalname.Tables[0].Rows[1][0] + "' TO N'" + ruta1 + "\\" + nbase + ".ldf" + "',  NOUNLOAD,  REPLACE,  STATS = 10 ";             
                go_Sql.EjecutarConsulta("Me", Consulta, servidor, basedatos, usuario, password);
               
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void eliminarbackup(string path)
        {
            if (File.Exists((path + @"\Temporal.bak")) == true)
            {
                File.SetAttributes(path + @"\Temporal.bak", FileAttributes.Normal);
                File.Delete(path + @"\Temporal.bak");
            }            
        }
        private void limpiartablas(string periodo,string razonsocial,string direccion,string ruc,string bd)
        {
            ClsConexion conex = new ClsConexion();
            string txt_consulta;
            txt_consulta = "select Name from " + nbase + ".sys.tables ";
            txt_consulta += "where Name not in( ";
            txt_consulta += "'PlanCuenta','CambioMoneda','PtTipoMoneda','PtUsuarioEntidad','AnexoPrincipal','Ptusuario','transferencia','EstadoxNaturaleza' ";
            txt_consulta += ",'TablaGeneral','Inflacion','PtUsuarioMenu','AnexoComplementario','Cierre','Detallecierre','PtEntidad','CuentaBanco' ";
            txt_consulta += ",'PtMenues','PTNivelUsuario','PtEstado','PtTipoUsuario','Tbl_Usuario_Menu','FormatoFlujoEfectivo','DetalleFormatoFlujoEfectivo','DetalleAmarreCambioPatrimonio' ";
            txt_consulta += " ) ";
          
            DataTable dt_tablas = new DataTable();
          
            go_Sql.EjecutarConsulta("Me", " Disable TRIGGER dbo.tr_EliminaDocreft on comprobante ", servidor, nbase, usuario, password);
            dt_tablas = conex.EjecutarConsulta("me", txt_consulta, servidor, basedatos, usuario, password).Tables[0];
            conex.EjecutarConsulta("deactivar", "USE " + nbase + " EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? DISABLE TRIGGER ALL'", servidor, basedatos, usuario, password);
            conex.EjecutarConsulta("deactivar", "USE " + nbase + " EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? NOCHECK CONSTRAINT ALL'", servidor, basedatos, usuario, password);
            conex.EjecutarConsulta("deactivar", "USE " + nbase + " EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? DISABLE TRIGGER ALL'", servidor, basedatos, usuario, password);

            if (dt_tablas.Rows.Count > 0)
            {
                int i;
                for (i = 0; i <= dt_tablas.Rows.Count - 1; i++)
                {
                    if (dt_tablas.Rows[i][0].ToString().Trim().Length>=3)
                    {
                        if (dt_tablas.Rows[i][0].ToString().Trim().Substring(0, 3) == "REG")
                        {    
                            go_Sql.EjecutarConsulta("me", "Drop table " + nbase + ".." + dt_tablas.Rows[i][0].ToString().Trim(), servidor, basedatos, usuario, password);
                        }
                        else
                        {
                  

                            go_Sql.EjecutarConsulta("me", "Delete from " + nbase + ".." + dt_tablas.Rows[i][0].ToString().Trim(), servidor, basedatos, usuario, password);
                        }
                    }
                    
                } 
                go_Sql.EjecutarConsulta("me", "update " + nbase + "..ptentidad set anioEjercicio='" + periodo + "', Nombre='" + razonsocial + "',Direccion='" + direccion + "',Ruc='" + ruc + "'", servidor, basedatos, usuario, password);
            }
            conex.EjecutarConsulta("deactivar", "USE " + nbase + " EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? CHECK CONSTRAINT ALL'", servidor, basedatos, usuario, password);
            conex.EjecutarConsulta("deactivar", "USE " + nbase + " EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; ALTER TABLE ? ENABLE TRIGGER ALL'", servidor, basedatos, usuario, password);
            go_Sql.EjecutarConsulta("tr", " Enable TRIGGER dbo.tr_EliminaDocreft on comprobante ", servidor, nbase, usuario, password);
        }

        public class Datos
        {
            public string baseplantilla { get; set; }
            public string ruc { get; set; }
            public string periodo { get; set; }
            public string razonsocial { get; set; }
            public string direccion { get; set; }       
           

            //servidor
            public string servidor { get; set; }
         
            public string usuario { get; set; }
            public string password { get; set; }
        }
    }
}
