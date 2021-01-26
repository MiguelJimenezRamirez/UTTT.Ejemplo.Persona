using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UTTT.Ejemplo.Persona.Control
{
	public class CtrlvalidaInyeccion
	{
		public bool htmlInyectionValida(string _informacion, ref string _mensaje, string _etiquetaReferente,
			ref System.Web.UI.WebControls.TextBox _control) {
			Regex tagPegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
			bool respuesta = tagPegex.IsMatch(_informacion);
			if (respuesta) {
				_mensaje = "Caracter no permitido en " + _etiquetaReferente.Replace(":","");
				_control.Focus();
			}
			return respuesta;
		}
		public bool SQLInyectionValida(string _informacion, ref string _mensaje, string _etiquetaReferente,
			ref System.Web.UI.WebControls.TextBox _control)
		{
			Regex tagPegex = new Regex(@"('(''|[^'])*')|(\b(ALTER|alter|Alter|CREATE|create|Create|
				DELETE|Delate|delate|DROP|drop|Drop|EXEC(UTE){0,1}|exec(ute){0,1}|
				Exec(ute){0,1}|INSERT( +INTO){0,1}|insert( +into){0,1}|Insert( +into){0,1}|MERGE|
				union( +all){0,1}|Union( +all){0,1})\b)");
			bool respuesta = tagPegex.IsMatch(_informacion);
			if (respuesta)
			{
				_mensaje = "Caracter no permitido en " + _etiquetaReferente.Replace(":", "");
				_control.Focus();
			}
			return respuesta;
		}
	}
}
