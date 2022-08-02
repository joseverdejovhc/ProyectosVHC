using LAB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Helper;
using Presupuestos.Models;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Presupuestos.Controllers
{
    public class MargenBrutoController : Controller
    {

        private Permissions permissions;
        private int nivel = 0;
        private string login;

        public MargenBrutoController(DBContext usrctxt, IConfiguration configuration)
        {
            permissions = new Permissions(usrctxt, configuration);
        }

        // GET: MargenBrutoController
        public IActionResult Index()
        {
            login = GetCookie();

            nivel = Permissions.ComprobarPermiso(login, 1);

            if (nivel == 0)
            {
                return View("NoAccess");
            }
            else
            {
                ViewData["nivel"] = nivel;

                return View("Margen Bruto");
            }

        }


        // POST: MargenBrutoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MargenBrutoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }


        // GET:MargenBruto/Query
        public ActionResult Query()
        {
/*
            List<SqlParameter> parameters = new List<SqlParameter>();

            SqlDataReader reader = BaseDataAccess.GetDataReader("select * from vw_margen_bruto where ejercicio= '2022'", parameters, CommandType.Text);

            List<ReferenciaBAK> referencias = new List<ReferenciaBAK>();

            while (reader.Read())
            {
                ReferenciaBAK referencia = new ReferenciaBAK(reader["CODIRE"].ToString(), reader["CODALT"].ToString(), reader["TIPPRD"].ToString(), reader["TIPREF"].ToString(), reader["NOMTIPREF"].ToString(), reader["linea"].ToString(), reader["kilos_ventas"].ToString(), reader["euros_ventas"].ToString(), reader["stock_previsto"].ToString(), reader["cdv_almacen"].ToString(), reader["cdf_almacen"].ToString(), reader["ejercicio"].ToString());

                if (referencia.kilos_venta == 0)
                {
                    referencia.pmv_kilos = referencia.euros_venta / 1;
                }
                else
                {
                    referencia.pmv_kilos = referencia.euros_venta / referencia.kilos_venta;
                }

                if(referencia.kilos_venta >= referencia.stock_previsto)
                {
                    referencia.kilos_cargo_almacen = referencia.stock_previsto;
                }
                else
                {
                    referencia.kilos_cargo_almacen = referencia.kilos_venta;
                }

                referencia.cdv_cargo_almacen = referencia.kilos_cargo_almacen * referencia.cdv_almacen;

                referencia.cdf_cargo_almacen = referencia.kilos_cargo_almacen * referencia.cdf_almacen;

                referencia.kilos_cargo_produccion = referencia.kilos_venta - referencia.kilos_cargo_almacen;

                referencias.Add(referencia);
            }
        */
            return View();

        }


        // POST: MargenBrutoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MargenBrutoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MargenBrutoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private string GetCookie()
        {
            if (Request.Cookies["credencialesIntranetMVC"] != null)
            {
                login = "vegenat0\\" + Request.Cookies["credencialesIntranetMVC"].ToString();
            }
            else
            {
                Response.Redirect("http://dev.vegenat.net/Intranet2/Login?ReturnUrl=dev.vegenat.net%2LABVHCTEST%2&out=1");
            }

            return login;
        }
    }
}
